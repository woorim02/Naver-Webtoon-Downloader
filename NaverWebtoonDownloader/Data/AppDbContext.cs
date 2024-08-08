using Microsoft.EntityFrameworkCore;

using NaverWebtoon = NaverWebtoonDownloader.Entities.Naver.Webtoon;
using NaverEpisode = NaverWebtoonDownloader.Entities.Naver.Episode;
using NaverImage = NaverWebtoonDownloader.Entities.Naver.Image;

namespace NaverWebtoonDownloader.Data;

public class AppDbContext : DbContext
{
    public AppDbContext()
    {

    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite(Constants.CONNECTION_STRING);
    }

    public DbSet<NaverWebtoon> NaverWebtoons { get; set; }

    public DbSet<NaverEpisode> NaverEpisodes { get; set; }

    public DbSet<NaverImage> NaverImages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NaverWebtoon>()
            .HasKey(w => w.ID);

        modelBuilder.Entity<NaverEpisode>()
            .HasKey(e => new { e.WebtoonID, e.No });

        modelBuilder.Entity<NaverImage>()
            .HasKey(i => new { i.WebtoonID, i.EpisodeNo, i.No });

        modelBuilder.Entity<NaverEpisode>()
            .HasOne(e => e.Webtoon)
            .WithMany(w => w.Episodes)
            .HasForeignKey(e => e.WebtoonID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<NaverImage>()
            .HasOne(i => i.Webtoon)
            .WithMany()
            .HasForeignKey(i => i.WebtoonID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<NaverImage>()
            .HasOne(i => i.Episode)
            .WithMany(e => e.Images)
            .HasForeignKey(i => new { i.WebtoonID, i.EpisodeNo })
            .OnDelete(DeleteBehavior.Cascade);
    }
}
