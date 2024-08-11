using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaverWebtoonDownloader;

public class Constants
{
#if ANDROID
    public readonly static string CONNECTION_STRING
        = $"Data Source = {Path.Combine(FileSystem.AppDataDirectory, "appdb.sqlite")}; Mode=ReadWriteCreate; Cache=Shared;";
#else
    public static string CONNECTION_STRING
        = $"Data Source={Path.Combine(Preferences.Default.Get("DOWNLOAD_FOLDER_PATH", ""), "appdb.sqlite")}; Mode=ReadWriteCreate; Cache=Shared;";
#endif
}
