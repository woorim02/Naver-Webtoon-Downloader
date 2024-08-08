namespace NaverWebtoonDownloader.Entities.Naver;

public class Episode
{
    public int WebtoonID { get; set; }
    public Webtoon Webtoon { get; set; }
    public int No { get; set; }
    public string Thumbnail { get; set; }
    public string Title { get; set; }
    public double StarScore { get; set; }
    public DateTime Date { get; set; }
    public List<Image> Images { get; set; }
}
