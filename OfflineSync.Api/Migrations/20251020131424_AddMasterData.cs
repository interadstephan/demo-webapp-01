using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OfflineSync.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMasterData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MasterData",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasterData", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MasterData_Category_Key",
                table: "MasterData",
                columns: new[] { "Category", "Key" });

            migrationBuilder.CreateIndex(
                name: "IX_MasterData_UpdatedAt",
                table: "MasterData",
                column: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MasterData_Version",
                table: "MasterData",
                column: "Version");

            // Seed sample Settings data
            var now = DateTime.UtcNow;
            var version = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            migrationBuilder.InsertData(
                table: "MasterData",
                columns: new[] { "Id", "Key", "Value", "Category", "Description", "CreatedAt", "UpdatedAt", "IsDeleted", "Version" },
                values: new object[,]
                {
                    { Guid.Parse("11111111-1111-1111-1111-111111111111"), "AppName", "Offline Sync Demo", "Settings", "Application display name", now, now, false, version },
                    { Guid.Parse("22222222-2222-2222-2222-222222222222"), "MaxFileSize", "10485760", "Settings", "Maximum file size in bytes (10MB)", now, now, false, version },
                    { Guid.Parse("33333333-3333-3333-3333-333333333333"), "SyncIntervalSeconds", "30", "Settings", "Default sync interval in seconds", now, now, false, version },
                    { Guid.Parse("44444444-4444-4444-4444-444444444444"), "Theme", "light", "Settings", "Default UI theme", now, now, false, version },
                    { Guid.Parse("55555555-5555-5555-5555-555555555555"), "EnableNotifications", "true", "Settings", "Enable push notifications", now, now, false, version }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MasterData");
        }
    }
}
