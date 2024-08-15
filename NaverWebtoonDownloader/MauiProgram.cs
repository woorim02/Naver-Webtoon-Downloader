using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MudBlazor;
using MudBlazor.Services;
using NaverWebtoonDownloader.Apis;
using NaverWebtoonDownloader.Data;
using NaverWebtoonDownloader.Services;

namespace NaverWebtoonDownloader
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            // Preferences 설정
            SetPreferences();
            Directory.CreateDirectory(Preferences.Default.Get("DOWNLOAD_FOLDER_PATH", ""));
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddMudServices();

            // 서비스 추가
            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(Constants.CONNECTION_STRING);
            });
            builder.Services.AddScoped(sp => new HttpClient());
            builder.Services.AddScoped<NaverWebtoonApi>();
            builder.Services.AddSingleton<IDialogService, DialogService>();
            builder.Services.AddSingleton<WebtoonDownloadService>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            var app = builder.Build();

            // 앱이 빌드된 후 서비스에서 DbContext를 가져와서 초기화
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // 데이터베이스 존재 여부 확인후 생성
                var databasePath = Path.Combine(Constants.DataSource);
                var databaseExists = File.Exists(databasePath);
                if (!databaseExists)
                {
                    context.Database.Migrate();
                    context.Database.ExecuteSqlRaw("PRAGMA journal_mode = WAL;");
                }
            }

            return app;
        }


        public static void SetPreferences()
        {
            string DOWNLOAD_FOLDER_PATH;
#if ANDROID
            DOWNLOAD_FOLDER_PATH = Path.Combine(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDcim).AbsolutePath, "NaverWebtoonDownloader");
#else
            DOWNLOAD_FOLDER_PATH = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Pictures\NaverWebtoonDownloader";
#endif
            Preferences.Default.Set("DOWNLOAD_FOLDER_PATH", DOWNLOAD_FOLDER_PATH);
        }
    }
}
