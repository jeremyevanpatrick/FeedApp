using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FeedApp3.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddFeedUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FeedUpdates",
                columns: table => new
                {
                    FeedUpdateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedUpdates", x => x.FeedUpdateId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeedUpdates");
        }
    }
}
