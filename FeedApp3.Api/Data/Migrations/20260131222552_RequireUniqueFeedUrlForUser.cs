using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FeedApp3.Api.Migrations
{
    /// <inheritdoc />
    public partial class RequireUniqueFeedUrlForUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "FeedUrl",
                table: "Feeds",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Feeds_UserId_FeedUrl",
                table: "Feeds",
                columns: new[] { "UserId", "FeedUrl" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Feeds_UserId_FeedUrl",
                table: "Feeds");

            migrationBuilder.AlterColumn<string>(
                name: "FeedUrl",
                table: "Feeds",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
