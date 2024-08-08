using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaverWebtoonDownloader.Dtos;

public class ArticleList
{
    public int no { get; set; }
    public string thumbnailUrl { get; set; }
    public string subtitle { get; set; }
    public double starScore { get; set; }
    public bool bgm { get; set; }
    public bool up { get; set; }
    public bool charge { get; set; }
    public string serviceDateDescription { get; set; }
    public int volumeNo { get; set; }
    public bool hasReadLog { get; set; }
    public bool recentlyReadLog { get; set; }
    public bool thumbnailClock { get; set; }
    public bool thumbnailLock { get; set; }
}

public class ChargeFolderArticleList
{
    public int no { get; set; }
    public string thumbnailUrl { get; set; }
    public string subtitle { get; set; }
    public double starScore { get; set; }
    public bool bgm { get; set; }
    public bool up { get; set; }
    public bool charge { get; set; }
    public string serviceDateDescription { get; set; }
    public int volumeNo { get; set; }
    public bool hasReadLog { get; set; }
    public bool recentlyReadLog { get; set; }
    public bool thumbnailClock { get; set; }
    public bool thumbnailLock { get; set; }
}

public class PageInfo
{
    public int totalRows { get; set; }
    public int pageSize { get; set; }
    public int indexSize { get; set; }
    public int page { get; set; }
    public int firstPage { get; set; }
    public int prevPage { get; set; }
    public int rawPage { get; set; }
    public int totalPages { get; set; }
    public int startRowNum { get; set; }
    public int lastPage { get; set; }
    public int nextPage { get; set; }
    public int endRowNum { get; set; }
}

public class ArticleListDto
{
    public int titleId { get; set; }
    public string webtoonLevelCode { get; set; }
    public int totalCount { get; set; }
    public int contentsNo { get; set; }
    public bool finished { get; set; }
    public bool dailyPass { get; set; }
    public bool chargeBestChallenge { get; set; }
    public List<ArticleList> articleList { get; set; }
    public List<ChargeFolderArticleList> chargeFolderArticleList { get; set; }
    public bool chargeFolderUp { get; set; }
    public PageInfo pageInfo { get; set; }
    public string sort { get; set; }
}
