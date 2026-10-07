using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YazilimEnvanteri.Migrations
{
    /// <inheritdoc />
    public partial class ProjeSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Proje_ProjeKodu",
                schema: "Proje",
                table: "Proje");

            migrationBuilder.AddColumn<bool>(
                name: "SilindiMi",
                schema: "Proje",
                table: "Proje",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Proje_ProjeKodu",
                schema: "Proje",
                table: "Proje",
                column: "ProjeKodu",
                unique: true,
                filter: "\"SilindiMi\" = FALSE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Proje_ProjeKodu",
                schema: "Proje",
                table: "Proje");

            migrationBuilder.DropColumn(
                name: "SilindiMi",
                schema: "Proje",
                table: "Proje");

            migrationBuilder.CreateIndex(
                name: "IX_Proje_ProjeKodu",
                schema: "Proje",
                table: "Proje",
                column: "ProjeKodu",
                unique: true);
        }
    }
}
