using HtmlAgilityPack;
using NaverWebtoonDownloader.Dtos;
using NaverWebtoonDownloader.Entities.Naver;
using Newtonsoft.Json;
using Image = NaverWebtoonDownloader.Entities.Naver.Image;

namespace NaverWebtoonDownloader.Apis;

public class NaverWebtoonApi
{
    public NaverWebtoonApi(HttpClient client)
    {
        _client = client;
    }

    private readonly HttpClient _client;

    public async Task<Webtoon> GetWebtoonAsync(int webtoonId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"https://comic.naver.com/api/article/list/info?titleId={webtoonId}");
        var response = await _client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        var webtoonDtoContent = JsonConvert.DeserializeObject<WebtoonDetailDto>(content);
        return webtoonDtoContent.ToEntity();
    }

    /// <summary>
    /// 리턴값의 episode가 Empty List이니 주의
    /// </summary>
    /// <param name="day"></param>
    /// <returns></returns>
    public async Task<List<Webtoon>> GetWebtoonsAsync(DayOfWeek day)
    {
        var webtoonDtos = new List<WebtoonDto>();
        const int maxPages = 100; // 최대 페이지 수 설정
        for (int i = 1; i < maxPages; i++)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://korea-webtoon-api-cc7dda2f0d77.herokuapp.com/webtoons?provider=NAVER&page={i}&perPage=100&sort=ASC&updateDay={day.ToString().Substring(0, 3).ToUpper()}");
            var response = await _client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            var webtoonDtoContent = JsonConvert.DeserializeObject<WebtoonListDto>(content);
            webtoonDtos.AddRange(webtoonDtoContent.Webtoons);
            if (webtoonDtoContent!.IsLastPage)
                break;
        }

        var webtoons = new List<Webtoon>();
        foreach (var dto in webtoonDtos)
        {
            var webtoon = new Webtoon()
            {
                ID = int.Parse(dto.Id.Split('_')[1]),
                Title = dto.Title,
                UpdateDays = dto.UpdateDays,
                Url = dto.Url,
                Thumbnail = dto.Thumbnail.First(),
                IsEnd = dto.IsEnd,
                IsFree = dto.IsFree,
                Authors = dto.Authors,
                Episodes = new List<Episode>()
            };
            webtoons.Add(webtoon);
        }
        return webtoons;
    }

    public async Task<List<Webtoon>> GetWebtoonsAsync(string keyword)
    {
        var webtoonDtos = new List<WebtoonDto>();
        const int maxPages = 100; // 최대 페이지 수 설정

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"https://korea-webtoon-api-cc7dda2f0d77.herokuapp.com/webtoons" +
            $"?provider=NAVER&page=1&perPage=100&sort=ASC&keyword={Uri.UnescapeDataString(keyword)}");
        var response = await _client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        var webtoonDtoContent = JsonConvert.DeserializeObject<WebtoonListDto>(content);
        webtoonDtos.AddRange(webtoonDtoContent.Webtoons);

        var webtoons = new List<Webtoon>();
        foreach (var dto in webtoonDtos)
        {
            var webtoon = new Webtoon()
            {
                ID = int.Parse(dto.Id.Split('_')[1]),
                Title = dto.Title,
                UpdateDays = dto.UpdateDays,
                Url = dto.Url,
                Thumbnail = dto.Thumbnail.First(),
                IsEnd = dto.IsEnd,
                IsFree = dto.IsFree,
                Authors = dto.Authors,
                Episodes = new List<Episode>()
            };
            webtoons.Add(webtoon);
        }
        return webtoons;
    }

    /// <summary>
    /// ASC정렬, webtoon = null, images = empty List
    /// </summary>
    /// <param name="webtoonId"></param>
    /// <param name="pageNo"></param>
    /// <returns></returns>
    public async Task<List<Episode>> GetEpisodesAsync(int webtoonId, int pageNo)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"https://comic.naver.com/api/article/list?titleId={webtoonId}&page={pageNo}&sort=ASC");
        var response = await _client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        var articleList = JsonConvert.DeserializeObject<ArticleListDto>(content);

        var episodes = new List<Episode>();
        foreach (var article in articleList.articleList)
        {
            if(article.serviceDateDescription.Contains("무료"))
                continue;
            var episode = new Episode()
            {
                No = article.no,
                Thumbnail = article.thumbnailUrl,
                Title = article.subtitle,
                StarScore = article.starScore,
                Date = DateTime.ParseExact(article.serviceDateDescription, "yy.MM.dd", null),
                WebtoonID = webtoonId,
                Images = new List<Image>()
            };
            episodes.Add(episode);
        }
        return episodes;
    }

    public async Task<int> GetMaxEpisodeNo(int webtoonId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"https://comic.naver.com/api/article/list?titleId={webtoonId}");
        var response = await _client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        var articleList = JsonConvert.DeserializeObject<ArticleListDto>(content);
        return articleList.pageInfo.totalRows;
    }

    public async Task<List<Image>> GetImagesAsync(int webtoonId, int episodeNo)
    {
        // 페이지 리스트 불러오기
        var request = new HttpRequestMessage(HttpMethod.Get, $"https://comic.naver.com/webtoon/detail?titleId={webtoonId}&no={episodeNo}");
        request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36");
        var response = await _client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        // html 로드
        var document = new HtmlDocument();
        document.LoadHtml(content);

        // 이미지노드 추출
        var imageNodes = document.DocumentNode.SelectNodes("//img[@alt='comic content']");

        // 이미지 리스트 생성
        var images = new List<Image>();
        for (int i = 0; i < imageNodes.Count; i++) 
        {
            var imageNode = imageNodes[i];
            var image = new Image()
            {
                WebtoonID = webtoonId,
                ImageUrl = imageNode.Attributes["src"].Value,
                IsDownloaded = false,
                EpisodeNo = episodeNo,
                No = i,
                Size = 0
            };
            images.Add(image);
        }
        return images;
    }

    public async Task<byte[]> GetImageAsync(string url)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36");
        request.Headers.Add("Referer", "https://comic.naver.com/webtoon/detail");
        var response = await _client.SendAsync(request);

        var content = await response.Content.ReadAsByteArrayAsync();
        return content;
    }
}