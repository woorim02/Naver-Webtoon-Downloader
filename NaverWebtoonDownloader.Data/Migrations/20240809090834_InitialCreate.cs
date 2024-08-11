using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NaverWebtoonDownloader.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NaverWebtoons",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    UpdateDays = table.Column<string>(type: "TEXT", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    Thumbnail = table.Column<string>(type: "TEXT", nullable: false),
                    IsEnd = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsFree = table.Column<bool>(type: "INTEGER", nullable: false),
                    Authors = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NaverWebtoons", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "NaverEpisodes",
                columns: table => new
                {
                    WebtoonID = table.Column<int>(type: "INTEGER", nullable: false),
                    No = table.Column<int>(type: "INTEGER", nullable: false),
                    Thumbnail = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    StarScore = table.Column<double>(type: "REAL", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NaverEpisodes", x => new { x.WebtoonID, x.No });
                    table.ForeignKey(
                        name: "FK_NaverEpisodes_NaverWebtoons_WebtoonID",
                        column: x => x.WebtoonID,
                        principalTable: "NaverWebtoons",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NaverImages",
                columns: table => new
                {
                    WebtoonID = table.Column<int>(type: "INTEGER", nullable: false),
                    EpisodeNo = table.Column<int>(type: "INTEGER", nullable: false),
                    No = table.Column<int>(type: "INTEGER", nullable: false),
                    ImageUrl = table.Column<string>(type: "TEXT", nullable: false),
                    Size = table.Column<long>(type: "INTEGER", nullable: false),
                    IsDownloaded = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NaverImages", x => new { x.WebtoonID, x.EpisodeNo, x.No });
                    table.ForeignKey(
                        name: "FK_NaverImages_NaverEpisodes_WebtoonID_EpisodeNo",
                        columns: x => new { x.WebtoonID, x.EpisodeNo },
                        principalTable: "NaverEpisodes",
                        principalColumns: new[] { "WebtoonID", "No" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NaverImages_NaverWebtoons_WebtoonID",
                        column: x => x.WebtoonID,
                        principalTable: "NaverWebtoons",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NaverImages");

            migrationBuilder.DropTable(
                name: "NaverEpisodes");

            migrationBuilder.DropTable(
                name: "NaverWebtoons");
        }
    }
}
