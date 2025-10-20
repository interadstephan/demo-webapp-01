using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OfflineSync.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddArticles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Articles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Author = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ImageData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageContentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Articles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Articles_PublishedAt",
                table: "Articles",
                column: "PublishedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_UpdatedAt",
                table: "Articles",
                column: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_Version",
                table: "Articles",
                column: "Version");

            // Seed sample articles with small placeholder images
            var now = DateTime.UtcNow;
            var version = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            
            // Small 1x1 red pixel PNG (for demonstration - in production use actual images)
            var redPixel = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8DwHwAFBQIAX8jx0gAAAABJRU5ErkJggg==";
            var bluePixel = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPj/HwADBwIAMCbHYQAAAABJRU5ErkJggg==";
            var greenPixel = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M/wHwAEBgIApD5fRAAAAABJRU5ErkJggg==";

            migrationBuilder.InsertData(
                table: "Articles",
                columns: new[] { "Id", "Title", "Content", "Author", "ImageData", "ImageContentType", "PublishedAt", "CreatedAt", "UpdatedAt", "IsDeleted", "Version" },
                values: new object[,]
                {
                    { 
                        Guid.Parse("a1111111-1111-1111-1111-111111111111"), 
                        "Getting Started with Offline Sync", 
                        "Learn how to build robust offline-first applications using modern web technologies. This comprehensive guide covers the fundamentals of offline sync, conflict resolution, and data persistence strategies. We'll explore RxDB, IndexedDB, and best practices for creating seamless user experiences even when connectivity is unreliable.", 
                        "John Developer", 
                        redPixel, 
                        "image/png", 
                        now.AddDays(-30), 
                        now, 
                        now, 
                        false, 
                        version 
                    },
                    { 
                        Guid.Parse("a2222222-2222-2222-2222-222222222222"), 
                        "Understanding Data Synchronization Patterns", 
                        "Explore different synchronization patterns including push, pull, and bidirectional sync. This article delves into version-based conflict resolution, optimistic locking, and how to handle concurrent updates from multiple devices. Real-world examples demonstrate practical implementations.", 
                        "Sarah Engineer", 
                        bluePixel, 
                        "image/png", 
                        now.AddDays(-25), 
                        now, 
                        now, 
                        false, 
                        version 
                    },
                    { 
                        Guid.Parse("a3333333-3333-3333-3333-333333333333"), 
                        "Optimizing Performance for Large Datasets", 
                        "When dealing with thousands of records, performance becomes critical. This article covers pagination strategies, lazy loading techniques, and database indexing for optimal query performance. Learn how to keep your offline app snappy even with extensive data.", 
                        "Mike Architect", 
                        greenPixel, 
                        "image/png", 
                        now.AddDays(-20), 
                        now, 
                        now, 
                        false, 
                        version 
                    },
                    { 
                        Guid.Parse("a4444444-4444-4444-4444-444444444444"), 
                        "Building Progressive Web Apps", 
                        "Progressive Web Apps (PWAs) provide app-like experiences on the web with offline capabilities. This guide covers service workers, caching strategies, background sync, and creating installable web applications that work seamlessly offline.", 
                        "Emily Designer", 
                        redPixel, 
                        "image/png", 
                        now.AddDays(-15), 
                        now, 
                        now, 
                        false, 
                        version 
                    },
                    { 
                        Guid.Parse("a5555555-5555-5555-5555-555555555555"), 
                        "Security Best Practices for Offline Apps", 
                        "Securing offline applications requires special considerations. Learn about data encryption at rest, secure synchronization protocols, authentication token management, and protecting sensitive data in browser storage. Essential reading for building production-ready apps.", 
                        "David Security", 
                        bluePixel, 
                        "image/png", 
                        now.AddDays(-10), 
                        now, 
                        now, 
                        false, 
                        version 
                    },
                    { 
                        Guid.Parse("a6666666-6666-6666-6666-666666666666"), 
                        "Testing Offline Functionality", 
                        "Comprehensive testing strategies for offline-first applications. Learn how to simulate offline conditions, test sync conflicts, and validate data consistency across devices. Includes examples using popular testing frameworks and tools.", 
                        "Lisa Tester", 
                        greenPixel, 
                        "image/png", 
                        now.AddDays(-5), 
                        now, 
                        now, 
                        false, 
                        version 
                    },
                    { 
                        Guid.Parse("a7777777-7777-7777-7777-777777777777"), 
                        "Real-time Collaboration in Offline Apps", 
                        "Enable multiple users to collaborate even when offline. This advanced topic covers operational transformation, CRDTs (Conflict-free Replicated Data Types), and synchronization strategies for collaborative editing scenarios.", 
                        "Robert Researcher", 
                        redPixel, 
                        "image/png", 
                        now.AddDays(-3), 
                        now, 
                        now, 
                        false, 
                        version 
                    },
                    { 
                        Guid.Parse("a8888888-8888-8888-8888-888888888888"), 
                        "Mobile-First Design Principles", 
                        "Design principles for creating responsive offline-first applications. Cover touch interactions, responsive layouts, performance optimization for mobile devices, and creating intuitive user interfaces that work across all screen sizes.", 
                        "Anna Designer", 
                        bluePixel, 
                        "image/png", 
                        now.AddDays(-1), 
                        now, 
                        now, 
                        false, 
                        version 
                    }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Articles");
        }
    }
}
