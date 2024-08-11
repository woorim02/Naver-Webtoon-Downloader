namespace NaverWebtoonDownloader.Entities.Naver;

public class Image
{
    public int WebtoonID { get; set; }
    public Webtoon Webtoon { get; set; }
    public int EpisodeNo { get; set; }
    public Episode Episode { get; set; }
    public int No { get; set; }
    public string ImageUrl { get; set; }
    public long Size { get; set; }
    public bool IsDownloaded { get; set; }
}
