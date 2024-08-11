using NaverWebtoonDownloader.Entities.Naver;
using System.Web;
using Image = NaverWebtoonDownloader.Entities.Naver.Image;

namespace NaverWebtoonDownloader
{
    public static class NameFormater
    {
        /// <summary>
        /// 저장할 이미지의 파일 이름 포맷을 설정합니다. {0~4}는 중복되거나 누락시킬 수 있습니다. {0~4}이외의 다른 숫자는 올 수 없습니다.
        /// <code>포맷 : [{5}] {3} - {4} ({2:D3}).jpg</code>
        /// <code>기본값 : [{5}] {3} - {4} ({2:D3}).jpg</code>
        /// <c>{0} : 웹툰의 titleId 입니다.</c>
        /// <code>{1} : 회차 번호(episodeNo)입니다. / ":Dn" : n자리수가 되도록 0을 패딩합니다.(episodeNo = 9, n = 4 => ex:0009)</code>
        /// <code>{2} : 이미지 인덱스(imageIndex)입니다. / ":Dn" : n자리수가 되도록 0을 패딩합니다.(imageIndex = 3, n = 3 => ex:003)</code>
        /// <code>{3} : 웹툰 제목(title)입니다.</code>
        /// <code>{4} : 회차 제목(episodeTitle)입니다.</code>
        /// <code>{5} : (episodeDate)입니다.</code>
        /// </summary>
        public static string ImageFileNameFormat { get; set; } = "[{5:yyyy.MM.dd}] {3} - {4} ({2:D4}).jpg";

        public static string BuildImageFileName(Image image)
        {
            return ReplaceFileName(string.Format(ImageFileNameFormat,
                image.WebtoonID,
                image.EpisodeNo,
                image.No,
                image.Webtoon.Title,
                image.Episode.Title,
                image.Episode.Date));
        }

        /// <summary>
        /// 저장할 회차의 폴더 이름 포맷을 설정합니다. {0~5}은/는 중복되거나 누락시킬 수 있습니다. {0~5}이외의 다른 숫자는 올 수 없습니다.
        /// <code>포맷 : {0}-{1:D4}-{2}-{3}-{4}</code>
        /// <code>기본값 : [{2}] {4}</code>
        /// <c>{0} : 웹툰의 titleId 입니다.</c>
        /// <code>{1} : 회차 번호(episodeNo)입니다. / ":Dn" : n자리수가 되도록 0을 패딩합니다.(episodeNo = 9, n = 4 => ex:0009)</code>
        /// <code>{2} : 회차 업로드 날짜(date)입니다</code>
        /// <code>{4} : 회차 제목(episodeTitle)입니다.</code>
        /// </summary>
        public static string EpisodeFolderNameFormat { get; set; } = "[{2:yyyy.MM.dd}] {3}";

        public static string BuildEpisodeFolderName(Episode episode)
        {
            return ReplaceFolderName(string.Format(EpisodeFolderNameFormat,
                episode.WebtoonID,
                episode.No,
                episode.Date,
                episode.Title));
        }

        /// <summary>
        /// 저장할 웹툰의 폴더 이름 포맷을 설정합니다. {0~2}은/는 중복되거나 누락시킬 수 있습니다. {0~2}이외의 다른 숫자는 올 수 없습니다.
        /// <code>포맷 : {0}-{1}</code>
        /// <code>기본값 : {1}</code>
        /// <c>{0} : 웹툰의 titleId 입니다.</c>
        /// <code>{1} : 웹툰 제목(title)입니다.</code>
        /// </summary>
        public static string WebtoonFolderNameFormat { get; set; } = "{1}";

        public static string BuildWebtoonFolderName(Webtoon webtoon)
        {
            return ReplaceFolderName(string.Format(WebtoonFolderNameFormat,
                webtoon.ID,
                webtoon.Title,
                webtoon.Authors.FirstOrDefault()));
        }

        private static string ReplaceFolderName(string name)
        {
            if (name[name.Length - 1] == '.')
                name = name.Substring(0, name.Length - 1) + "．";
            return HttpUtility.HtmlDecode(name)
                              .TrimEnd()
                              .Replace('/', '／')
                              .Replace('?', '？')
                              .Replace('*', '＊')
                              .Replace(':', '：')
                              .Replace('|', '｜')
                              .Replace('\"', '＂')
                              .Replace("<", "＜")
                              .Replace(">", "＞");
        }

        private static string ReplaceFileName(string filename)
        {
            if (filename[filename.Length - 1] == '.')
                filename = filename.Substring(0, filename.Length - 1) + "．";
            return HttpUtility.HtmlDecode(filename)
                              .TrimEnd()
                              .Replace('/', '／')
                              .Replace('?', '？')
                              .Replace('*', '＊')
                              .Replace(':', '：')
                              .Replace('|', '｜')
                              .Replace('\"', '＂')
                              .Replace("<", "＜")
                              .Replace(">", "＞");
        }
    }
}