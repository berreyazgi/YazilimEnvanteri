using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YazilimEnvanteri.Migrations
{
    /// <inheritdoc />
    public partial class LinkPersonelAppUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AppUserId",
                table: "Personeller",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Personeller_AppUserId",
                table: "Personeller",
                column: "AppUserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Personeller_AspNetUsers_AppUserId",
                table: "Personeller",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Personeller_AspNetUsers_AppUserId",
                table: "Personeller");

            migrationBuilder.DropIndex(
                name: "IX_Personeller_AppUserId",
                table: "Personeller");

            migrationBuilder.DropColumn(
                name: "AppUserId",
                table: "Personeller");
        }
    }
}
