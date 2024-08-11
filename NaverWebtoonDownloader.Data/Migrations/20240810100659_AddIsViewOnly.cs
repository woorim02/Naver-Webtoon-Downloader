using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NaverWebtoonDownloader.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIsViewOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsViewOnly",
                table: "NaverWebtoons",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsViewOnly",
                table: "NaverWebtoons");
        }
    }
}
