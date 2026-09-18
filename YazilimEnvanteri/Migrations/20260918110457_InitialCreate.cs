using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace YazilimEnvanteri.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Personeller",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BirimId = table.Column<int>(type: "integer", nullable: false),
                    Ad = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Soyad = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Telefon = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Gorev = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SorumluPersonel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personeller", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Teknolojiler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EntegrasyonDurum = table.Column<bool>(type: "boolean", nullable: false),
                    Veritabani = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FrontendTeknoloji = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BackendTeknoloji = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    eimzaDurum = table.Column<bool>(type: "boolean", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teknolojiler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Birimler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    YazilimUzmaniId = table.Column<int>(type: "integer", nullable: false),
                    UstBirim = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    AltBirim = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Birim = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Birimler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Proje",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    YazilimUzmaniId = table.Column<int>(type: "integer", nullable: false),
                    TeknolojiId = table.Column<int>(type: "integer", nullable: false),
                    BirimId = table.Column<int>(type: "integer", nullable: false),
                    ProjeKodu = table.Column<int>(type: "integer", nullable: false),
                    ProjeAdi = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ProjeHizmetAlani = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Projenin hizmet alanını belirtir. Örneğin: Yazılım, Altyapı, Uygulama vb."),
                    ProjeAciklamasi = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ProjeAktifMi = table.Column<bool>(type: "boolean", nullable: false),
                    Sunucu = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    websiteUrl = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    ProjeDurum = table.Column<int>(type: "integer", nullable: false),
                    ProjeKritiklik = table.Column<int>(type: "integer", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proje", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Proje_Birimler_BirimId",
                        column: x => x.BirimId,
                        principalTable: "Birimler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Proje_Teknolojiler_TeknolojiId",
                        column: x => x.TeknolojiId,
                        principalTable: "Teknolojiler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "YazilimUzmanlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjeId = table.Column<int>(type: "integer", nullable: false),
                    Birim = table.Column<int>(type: "integer", nullable: false),
                    KullanıcıAdi = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, computedColumnSql: "SUBSTRING(Email, 1, CHARINDEX('@', Email) - 1)", stored: true),
                    Ad = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Soyad = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Gorev = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Eposta = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Telefon = table.Column<int>(type: "integer", maxLength: 20, nullable: false),
                    SorumluFirma = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YazilimUzmanlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YazilimUzmanlari_Birimler_Birim",
                        column: x => x.Birim,
                        principalTable: "Birimler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_YazilimUzmanlari_Proje_ProjeId",
                        column: x => x.ProjeId,
                        principalTable: "Proje",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Birimler_YazilimUzmaniId",
                table: "Birimler",
                column: "YazilimUzmaniId");

            migrationBuilder.CreateIndex(
                name: "IX_Personeller_BirimId",
                table: "Personeller",
                column: "BirimId");

            migrationBuilder.CreateIndex(
                name: "IX_Proje_BirimId",
                table: "Proje",
                column: "BirimId");

            migrationBuilder.CreateIndex(
                name: "IX_Proje_ProjeKodu",
                table: "Proje",
                column: "ProjeKodu",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Proje_TeknolojiId",
                table: "Proje",
                column: "TeknolojiId");

            migrationBuilder.CreateIndex(
                name: "IX_Proje_YazilimUzmaniId",
                table: "Proje",
                column: "YazilimUzmaniId");

            migrationBuilder.CreateIndex(
                name: "IX_YazilimUzmanlari_Birim",
                table: "YazilimUzmanlari",
                column: "Birim");

            migrationBuilder.CreateIndex(
                name: "IX_YazilimUzmanlari_Eposta",
                table: "YazilimUzmanlari",
                column: "Eposta");

            migrationBuilder.CreateIndex(
                name: "IX_YazilimUzmanlari_KullanıcıAdi",
                table: "YazilimUzmanlari",
                column: "KullanıcıAdi");

            migrationBuilder.CreateIndex(
                name: "IX_YazilimUzmanlari_ProjeId",
                table: "YazilimUzmanlari",
                column: "ProjeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Birimler_YazilimUzmanlari_YazilimUzmaniId",
                table: "Birimler",
                column: "YazilimUzmaniId",
                principalTable: "YazilimUzmanlari",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Proje_YazilimUzmanlari_YazilimUzmaniId",
                table: "Proje",
                column: "YazilimUzmaniId",
                principalTable: "YazilimUzmanlari",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Birimler_YazilimUzmanlari_YazilimUzmaniId",
                table: "Birimler");

            migrationBuilder.DropForeignKey(
                name: "FK_Proje_YazilimUzmanlari_YazilimUzmaniId",
                table: "Proje");

            migrationBuilder.DropTable(
                name: "Personeller");

            migrationBuilder.DropTable(
                name: "YazilimUzmanlari");

            migrationBuilder.DropTable(
                name: "Proje");

            migrationBuilder.DropTable(
                name: "Birimler");

            migrationBuilder.DropTable(
                name: "Teknolojiler");
        }
    }
}
