using NaverWebtoonDownloader.Entities.Naver;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaverWebtoonDownloader.Models;


public class WebtoonDownloadStatus : Webtoon, INotifyPropertyChanged
{
    public string LatestEpisodeTitle => Episodes.OrderBy(x => x.No).LastOrDefault()?.Title ?? "-";
    private string _statusText = "";
    public string StatusText
    {
        get
        {
            if (!string.IsNullOrEmpty(_statusText))
                return _statusText;
            return "다운로드 대기중";
        }
        set
        {
            _statusText = value;
            OnPropertyChanged(nameof(StatusText));
        }
    }
    public int DownLoadedImages
        => Episodes.SelectMany(x => x.Images)
                   .Where(x => x.IsDownloaded)
    .Count();
    public int TotalImages
        => Episodes.Select(x => x.Images.Count).Sum();

    public double Progress
    {
        get
        {
            if (TotalImages == 0)
            {
                return 0;
            }
            else
            {
                return (double)DownLoadedImages / TotalImages;
            }
        }
    }


    public WebtoonDownloadStatus(Webtoon webtoon)
    {
        this.IsViewOnly = webtoon.IsViewOnly;
        this.ID = webtoon.ID;
        this.Title = webtoon.Title;
        this.UpdateDays = new List<string>(webtoon.UpdateDays);
        this.Url = webtoon.Url;
        this.Thumbnail = webtoon.Thumbnail;
        this.IsEnd = webtoon.IsEnd;
        this.IsFree = webtoon.IsFree;
        this.Authors = new List<string>(webtoon.Authors);
        this.Episodes = new List<Episode>(webtoon.Episodes);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
