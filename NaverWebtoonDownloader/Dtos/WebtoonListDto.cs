using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaverWebtoonDownloader.Dtos;

public class WebtoonListDto
{
    public List<WebtoonDto> Webtoons { get; set; }
    public int Total { get; set; }
    public bool IsLastPage { get; set; }
}
