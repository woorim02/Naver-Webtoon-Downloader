using NaverWebtoonDownloader.Apis;
using NaverWebtoonDownloader.Data;
using NaverWebtoonDownloader.Entities.Naver;
using Image = NaverWebtoonDownloader.Entities.Naver.Image;

namespace NaverWebtoonDownloader.Services
{
    public class WebtoonDownloadService
    {
        public WebtoonDownloadService(NaverWebtoonApi api, IServiceProvider serviceProvider)
        {
            this.api = api;
            this.serviceProvider = serviceProvider;
        }

        private readonly NaverWebtoonApi api;
        private readonly IServiceProvider serviceProvider;

        public async Task FetchWebtoonDbAsync(Webtoon webtoon, CancellationToken token, Func<Webtoon, string, Task> downloadStatusChanged, Func<Task> StateHasChanged)
        {
            await downloadStatusChanged(webtoon, "회차 데이터베이스 로딩중...");

            var maxEpisodeNo = await api.GetMaxEpisodeNo(webtoon.ID);
            var localLatestEpisodeNo = webtoon.Episodes.Count == 0 ? 0 : webtoon.Episodes.Select(x => x.No).Max();
            // 신규 회차가 있을때 회차 정보 업데이트 진행
            if (maxEpisodeNo > localLatestEpisodeNo)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                //에피소드 목록 업데이트
                for (int i = (localLatestEpisodeNo / 20) + 1; i <= maxEpisodeNo / 20 + 1; i++)
                {
                    var episodeList = await api.GetEpisodesAsync(webtoon.ID, i);
                    using (var scope = serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                        await dbContext.NaverEpisodes.AddRangeAsync(episodeList);
                        await dbContext.SaveChangesAsync();
                    }
                    webtoon.Episodes.AddRange(episodeList);
                    await StateHasChanged();
                }
            }
            await downloadStatusChanged(webtoon, "회차 데이터베이스 로딩 완료");

            await downloadStatusChanged(webtoon, "이미지 데이터베이스 로딩중...");
            // 로컬 데이터베이스에 이미지가 없는 에피소드만 업데이트 진행
            var noImageEpisodes = webtoon.Episodes.Where(x => x.Images.Count == 0);
            foreach (var episode in noImageEpisodes)
            {
                if (token.IsCancellationRequested)
                {
                    break;
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
                await StateHasChanged();
            }
            await downloadStatusChanged(webtoon, "이미지 데이터베이스 로딩 완료");
        }


        public async Task DownloadImagesAsync(Webtoon webtoon, CancellationToken token, Func<Webtoon, string, Task> downloadStatusChanged, Func<Task> StateHasChanged)
        {
            foreach (var episode in webtoon.Episodes)
            {
                foreach (var image in episode.Images)
                {
                    image.Episode = episode;
                    image.Webtoon = webtoon;
                }
            }
            await downloadStatusChanged(webtoon, "이미지 다운로드중...");

            var toDownloadImages = webtoon.Episodes.SelectMany(x => x.Images).Where(x => !x.IsDownloaded);
            await Parallel.ForEachAsync(toDownloadImages, new ParallelOptions { MaxDegreeOfParallelism = 10, CancellationToken = token }, async (image, token) =>
            {
                // 이미지 다운로드 작업 실행
                await DownloadImageAsync(image, token, StateHasChanged);
            });

            await downloadStatusChanged(webtoon, "마무리 작업 실행중...");
            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbContext.NaverImages.UpdateRange(webtoon.Episodes.SelectMany(x => x.Images));
                await dbContext.SaveChangesAsync();
            }
            await downloadStatusChanged(webtoon, "다운로드 완료");

            await StateHasChanged();
        }

        private object lockObj = new object();
        private async Task DownloadImageAsync(Image image, CancellationToken token, Func<Task> StateHasChanged)
        {
            if (token.IsCancellationRequested)
                return;
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
            await StateHasChanged();
        }
    }
}
