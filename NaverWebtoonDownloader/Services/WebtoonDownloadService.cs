using Microsoft.EntityFrameworkCore;
using NaverWebtoonDownloader.Apis;
using NaverWebtoonDownloader.Data;
using NaverWebtoonDownloader.Entities.Naver;
using NaverWebtoonDownloader.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Image = NaverWebtoonDownloader.Entities.Naver.Image;

namespace NaverWebtoonDownloader.Services
{
    public class WebtoonDownloadService : INotifyPropertyChanged
    {
        public WebtoonDownloadService(NaverWebtoonApi api, IServiceProvider serviceProvider)
        {
            this.api = api;
            this.serviceProvider = serviceProvider;
        }

        private readonly NaverWebtoonApi api;
        private readonly IServiceProvider serviceProvider;
        private CancellationTokenSource? cts;
        private Thread thread;

        public ObservableCollection<WebtoonDownloadStatus> WebtoonDownloadStatuses = new ObservableCollection<WebtoonDownloadStatus>();
        public bool IsRunning {
            get
            {
                return !cts?.IsCancellationRequested ?? false;
            }
        }

        private bool isFirstLoad = true;
        public async Task OnInitializedAsync()
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetService<AppDbContext>();
            if (WebtoonDownloadStatuses.Count != 0)
            {
                var webtoonList = dbContext.NaverWebtoons
                    .Where(x => !x.IsViewOnly)
                    .Where(w => !WebtoonDownloadStatuses
                        .Select(x => x.ID)
                        .Contains(w.ID))
                    .Include(w => w.Episodes)
                    .ThenInclude(e => e.Images)
                    .ToList();
                if (webtoonList.Count > 0)
                {
                    foreach (var item in webtoonList.Select(x => new WebtoonDownloadStatus(x)))
                    {
                        WebtoonDownloadStatuses.Add(item);
                    }
                }
                return;
            }
            var webtoons = await dbContext.NaverWebtoons
                .Where(x => !x.IsViewOnly)
                .Include(w => w.Episodes)
                .ThenInclude(e => e.Images)
                .ToListAsync();
            foreach (var item in webtoons.Select(x => new WebtoonDownloadStatus(x)))
            {
                WebtoonDownloadStatuses.Add(item);
            }
            isFirstLoad = false;
        }

        public void StartDonwload()
        {
            if (IsRunning)
                return;
            thread = new Thread(async () =>
            {
                cts = new CancellationTokenSource();
                for (int i = 0; i< WebtoonDownloadStatuses.Count; i++)
                {
                    var webtoon = WebtoonDownloadStatuses[i];
                    try
                    {
                        if (!IsRunning) throw new TaskCanceledException();
                        await FetchWebtoonDbAsync(webtoon, cts.Token);
                        await DownloadImagesAsync(webtoon, cts.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        webtoon.StatusText = "작업 취소됨";
                        break;
                    }
                    catch (Exception e)
                    {
                        webtoon.StatusText = "오류 발생";
                        File.WriteAllText(Constants.ErrorLogPath, e.ToString());
                        break;
                    }
                }
            cts?.Cancel();
            cts = null;
            });
            thread.Start();
        }

        public void StopDownload()
        {
            cts?.Cancel();
            cts = null;
        }

        private async Task FetchWebtoonDbAsync(WebtoonDownloadStatus webtoon, CancellationToken token)
        {
            webtoon.StatusText = "회차 데이터베이스 로딩중...";

            var maxEpisodeNo = await api.GetMaxEpisodeNo(webtoon.ID);
            var localLatestEpisodeNo = webtoon.Episodes.Count == 0 ? 0 : webtoon.Episodes.Select(x => x.No).Max();
            // 신규 회차가 있을때 회차 정보 업데이트 진행
            if (maxEpisodeNo > localLatestEpisodeNo)
            {
                //에피소드 목록 업데이트
                for (int i = (localLatestEpisodeNo / 20) + 1; i <= maxEpisodeNo / 20 + 1; i++)
                {
                    if (token.IsCancellationRequested)
                    {
                        throw new TaskCanceledException();
                    }
                    var episodeList = await api.GetEpisodesAsync(webtoon.ID, i);
                    using (var scope = serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                        await dbContext.NaverEpisodes.AddRangeAsync(episodeList);
                        await dbContext.SaveChangesAsync(token);
                    }
                    webtoon.Episodes.AddRange(episodeList);
                }
            }
            webtoon.StatusText = "회차 데이터베이스 로딩 완료";

            webtoon.StatusText = "이미지 데이터베이스 로딩중...";
            // 로컬 데이터베이스에 이미지가 없는 에피소드만 업데이트 진행
            var noImageEpisodes = webtoon.Episodes.Where(x => x.Images.Count == 0);
            foreach (var episode in noImageEpisodes)
            {
                if (token.IsCancellationRequested)
                {
                    throw new TaskCanceledException();
                }
                // 이미지 불러오기
                var images = await api.GetImagesAsync(episode.WebtoonID, episode.No);
                // 데이터베이스에 이미지 정보 저장
                using (var scope = serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    await dbContext.NaverImages.AddRangeAsync(images);
                    await dbContext.SaveChangesAsync();
                }
                episode.Images = images;
                webtoon.OnPropertyChanged(nameof(webtoon.TotalImages));
            }
            webtoon.StatusText = "이미지 데이터베이스 로딩 완료";
        }

        private async Task DownloadImagesAsync(WebtoonDownloadStatus webtoon, CancellationToken token)
        {
            foreach (var episode in webtoon.Episodes)
            {
                foreach (var image in episode.Images)
                {
                    image.Episode = episode;
                    image.Webtoon = webtoon;
                }
            }
            webtoon.StatusText = "이미지 다운로드중...";

            var toDownloadImages = webtoon.Episodes.SelectMany(x => x.Images).Where(x => !x.IsDownloaded);
            await Parallel.ForEachAsync(toDownloadImages, new ParallelOptions { MaxDegreeOfParallelism = 10, CancellationToken = token }, async (image, token) =>
            {
                if (token.IsCancellationRequested)
                {
                    throw new TaskCanceledException();
                }
                // 이미지 다운로드 작업 실행
                await DownloadImageAsync(image, token);
                webtoon.OnPropertyChanged(propertyName: nameof(webtoon.TotalImages));
            });

            webtoon.StatusText = "마무리 작업 실행중...";
            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbContext.NaverImages.UpdateRange(webtoon.Episodes.SelectMany(x => x.Images));
                await dbContext.SaveChangesAsync();
            }
            webtoon.StatusText = "다운로드 완료";
        }

        private async Task DownloadImageAsync(Image image, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                throw new TaskCanceledException();
            // 디렉토리 없을시 생성
            var webtoonDir = Path.Combine(Preferences.Default.Get("DOWNLOAD_FOLDER_PATH", ""), NameFormater.BuildWebtoonFolderName(image.Webtoon));
            var episodeFullDir = Path.Combine(webtoonDir, NameFormater.BuildEpisodeFolderName(image.Episode));
            if (!Directory.Exists(episodeFullDir))
            {
                Directory.CreateDirectory(episodeFullDir);
            }

            var imageFullDir = Path.Combine(episodeFullDir, NameFormater.BuildImageFileName(image));
            var imageData = await api.GetImageAsync(image.ImageUrl);

            // 이미지 저장
            await File.WriteAllBytesAsync(imageFullDir, imageData, token);

            // 상태 업데이트
            image.Size = imageData.Length;
            image.IsDownloaded = true;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
