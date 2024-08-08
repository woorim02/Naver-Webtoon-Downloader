namespace NaverWebtoonDownloader.Entities.Naver;

public class Webtoon
{
    public int ID { get; set; }
    public string Title { get; set; }
    public List<string> UpdateDays { get; set; }
    public string Url { get; set; }
    public string Thumbnail { get; set; }
    public bool IsEnd { get; set; }
    public bool IsFree { get; set; }
    public List<string> Authors { get; set; }
    public List<Episode> Episodes { get; set; }
}
