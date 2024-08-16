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

    /// <summary>
    /// 공식 API에서 불러온 웹툰 엔티티를 반환합니다.
    /// </summary>
    /// <param name="webtoonId"></param>
    /// <returns></returns>
    public async Task<Webtoon> GetWebtoonAsync(int webtoonId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"https://comic.naver.com/api/article/list/info?titleId={webtoonId}");
        var response = await _client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        var webtoonDtoContent = JsonConvert.DeserializeObject<WebtoonDetailDto>(content);
        return webtoonDtoContent.ToEntity();
    }

    /// <summary>
    /// 사설 API에서 불러온 웹툰 엔티티 목록을 반환합니다. <br/>
    /// 리턴값의 episode가 Empty List이니 주의
    /// </summary>
    /// <param name="day"></param>
    /// <returns></returns>
    public async Task<List<Webtoon>> GetWebtoonsAsync(DayOfWeek day, int page = 1)
    {
        // 페이지당 100개, ASC 정렬
        var request = new HttpRequestMessage(HttpMethod.Get, $"https://korea-webtoon-api-cc7dda2f0d77.herokuapp.com/webtoons?provider=NAVER&page={page}&perPage=100&sort=ASC&updateDay={day.ToString().Substring(0, 3).ToUpper()}");
        var response = await _client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        // dto 변환
        var webtoonDtoContent = JsonConvert.DeserializeObject<WebtoonListDto>(content);
        var webtoonDtos = webtoonDtoContent.Webtoons;

        // 엔티티 변환
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
    /// 사설 API에서 불러온 웹툰 엔티티 목록을 반환합니다. <br/>
    /// 리턴값의 episode가 Empty List이니 주의
    /// </summary>
    /// <returns></returns>
    public async Task<List<Webtoon>> GetWebtoonsAsync(string keyword)
    {
        // 페이지당 100개, 키워드 검색
        var request = new HttpRequestMessage(HttpMethod.Get,
            $"https://korea-webtoon-api-cc7dda2f0d77.herokuapp.com/webtoons" +
            $"?provider=NAVER&page=1&perPage=100&sort=ASC&keyword={Uri.UnescapeDataString(keyword)}");
        var response = await _client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        // dto 변환
        var webtoonDtoContent = JsonConvert.DeserializeObject<WebtoonListDto>(content);
        var webtoonDtos = webtoonDtoContent.Webtoons;

        // 엔티티 변환
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
    /// 공식 API에서 불러온 애피소드 리스트를 반환합니다. 미리보기 회차 제외 <br/>
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

        // dto 변환
        var articleList = JsonConvert.DeserializeObject<ArticleListDto>(content);

        // 엔티티 변환
        var episodes = new List<Episode>();
        foreach (var article in articleList.articleList)
        {
            // 미리보기 회차 제외
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
        // 회차 목록 불러오기
        var request = new HttpRequestMessage(HttpMethod.Get, $"https://comic.naver.com/api/article/list?titleId={webtoonId}&page=1&sort=DESC");
        var response = await _client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        var articleList = JsonConvert.DeserializeObject<ArticleListDto>(content);
        // 회차 목록에서 가장 최신회차 번호 불러오기
        return articleList.articleList.Where(x => !x.serviceDateDescription.Contains("무료")).Select(x => x.no).Max();
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
        // user-agent, referer 필수
        request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36");
        request.Headers.Add("Referer", "https://comic.naver.com/webtoon/detail");
        var response = await _client.SendAsync(request);

        var content = await response.Content.ReadAsByteArrayAsync();
        return content;
    }
}