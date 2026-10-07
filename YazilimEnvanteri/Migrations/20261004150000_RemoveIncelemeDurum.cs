using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using YazilimEnvanteri.Data;

#nullable disable

namespace YazilimEnvanteri.Migrations
{
    // Data-only: "İnceleme" (3) was removed from Enums.ProjeDurum, so those projects become "Test" (4).
    // No model change, hence no Designer/snapshot update.
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261004150000_RemoveIncelemeDurum")]
    public partial class RemoveIncelemeDurum : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"Proje\".\"Proje\" SET \"ProjeDurum\" = 4 WHERE \"ProjeDurum\" = 3;");
        }

        // One-way: which rows were İnceleme before is not recorded.
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
