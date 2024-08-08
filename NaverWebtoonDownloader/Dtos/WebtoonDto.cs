using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaverWebtoonDownloader.Dtos
{
    public class WebtoonDto
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Provider { get; set; }
        public List<string> UpdateDays { get; set; }
        public string Url { get; set; }
        public List<string> Thumbnail { get; set; }
        public bool IsEnd { get; set; }
        public bool IsFree { get; set; }
        public bool IsUpdated { get; set; }
        public int AgeGrade { get; set; }
        public object FreeWaitHour { get; set; }
        public List<string> Authors { get; set; }
    }
}
