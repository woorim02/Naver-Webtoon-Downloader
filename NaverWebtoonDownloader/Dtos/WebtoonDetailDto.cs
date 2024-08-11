using NaverWebtoonDownloader.Entities.Naver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaverWebtoonDownloader.Dtos;

public class Age
{
    public string Type { get; set; }
    public string Description { get; set; }
}

public class CommunityArtist
{
    public int ArtistId { get; set; }
    public string Name { get; set; }
    public List<string> ArtistTypeList { get; set; }
    public string CurationPageUrl { get; set; }
}

public class CurationTagList
{
    public int Id { get; set; }
    public string TagName { get; set; }
    public string UrlPath { get; set; }
    public string CurationType { get; set; }
}

public class FirstArticle
{
    public int No { get; set; }
    public string Subtitle { get; set; }
    public bool Charge { get; set; }
}

public class GfpAdCustomParam
{
    public int TitleId { get; set; }
    public int Cid { get; set; }
    public string WebtoonLevelCode { get; set; }
    public string TitleName { get; set; }
    public string DisplayAuthor { get; set; }
    public string Cpid { get; set; }
    public string CpName { get; set; }
    public List<string> GenreTypes { get; set; }
    public List<string> RankGenreTypes { get; set; }
    public List<string> Tags { get; set; }
    public List<string> Weekdays { get; set; }
    public string FinishedYn { get; set; }
    public string AdultYn { get; set; }
    public string DailyPlusYn { get; set; }
    public string DailyFreeYn { get; set; }
}

public class WebtoonDetailDto
{
    public int TitleId { get; set; }
    public string ThumbnailUrl { get; set; }
    public string PosterThumbnailUrl { get; set; }
    public string SharedThumbnailUrl { get; set; }
    public string TitleName { get; set; }
    public int ContentsNo { get; set; }
    public string WebtoonLevelCode { get; set; }
    public bool Rest { get; set; }
    public bool Finished { get; set; }
    public bool DailyPass { get; set; }
    public List<string> PublishDayOfWeekList { get; set; }
    public bool ChargeBestChallenge { get; set; }
    public List<CommunityArtist> CommunityArtists { get; set; }
    public string Synopsis { get; set; }
    public bool Favorite { get; set; }
    public int FavoriteCount { get; set; }
    public Age Age { get; set; }
    public string PublishDescription { get; set; }
    public List<CurationTagList> CurationTagList { get; set; }
    public List<object> ThumbnailBadgeList { get; set; }
    public List<object> AdBannerList { get; set; }
    public FirstArticle FirstArticle { get; set; }
    public GfpAdCustomParam GfpAdCustomParam { get; set; }
    public bool New { get; set; }

    public Webtoon ToEntity()
    {
        var webtoon = new Webtoon
        {
            ID = this.TitleId,
            Title = this.TitleName,
            UpdateDays = this.PublishDayOfWeekList,
            Url = $"https://comic.naver.com/webtoon/list?titleId={this.TitleId}", 
            Thumbnail = this.ThumbnailUrl,
            IsEnd = this.Finished,
            IsFree = !this.DailyPass, 
            Authors = new List<string>(),
        };

        // Add authors from CommunityArtists
        foreach (var artist in this.CommunityArtists)
        {
            webtoon.Authors.Add(artist.Name);
        }

        return webtoon;
    }
}

