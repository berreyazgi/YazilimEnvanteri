using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YazilimEnvanteri.Migrations
{
    /// <summary>
    /// YazilimUzmanlari'daki kişisel bilgileri (KullanıcıAdi, Ad, Soyad, Email, Gorev, Telefon)
    /// Personeller tablosuna taşır ve yerine bir PersonelId referansı koyar. Mevcut kayıtlar
    /// korunur: her uzman e-posta ile eşleşen personele bağlanır, eşleşme yoksa uzmanın
    /// bilgilerinden yeni bir personel kaydı oluşturulur. Eski kolonlar ancak veri taşındıktan
    /// sonra silinir. Ayrıca "Birim" FK kolonu, projedeki isimlendirmeyle uyumlu olarak
    /// "BirimId" olarak yeniden adlandırılır.
    /// </summary>
    public partial class MoveYazilimUzmaniPersonelBilgileri : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Birim -> BirimId (same data, FK/index renamed to match).
            migrationBuilder.DropForeignKey(
                name: "FK_YazilimUzmanlari_Birimler_Birim",
                table: "YazilimUzmanlari");

            migrationBuilder.RenameColumn(
                name: "Birim",
                table: "YazilimUzmanlari",
                newName: "BirimId");

            migrationBuilder.RenameIndex(
                name: "IX_YazilimUzmanlari_Birim",
                table: "YazilimUzmanlari",
                newName: "IX_YazilimUzmanlari_BirimId");

            migrationBuilder.AddForeignKey(
                name: "FK_YazilimUzmanlari_Birimler_BirimId",
                table: "YazilimUzmanlari",
                column: "BirimId",
                principalTable: "Birimler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // 2) Personeller gains the one identity field it was missing.
            migrationBuilder.AddColumn<string>(
                name: "KullanıcıAdi",
                table: "Personeller",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Personeller_Email",
                table: "Personeller",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Personeller_KullanıcıAdi",
                table: "Personeller",
                column: "KullanıcıAdi");

            // 3) PersonelId starts nullable so it can be back-filled before becoming required.
            migrationBuilder.AddColumn<int>(
                name: "PersonelId",
                table: "YazilimUzmanlari",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql("""
                DO $$
                DECLARE
                    uzman RECORD;
                    hedef_personel_id integer;
                BEGIN
                    FOR uzman IN SELECT * FROM "YazilimUzmanlari" ORDER BY "Id" LOOP
                        hedef_personel_id := NULL;

                        -- Same person already in Personeller? E-mail is the identifying field;
                        -- blank e-mails are never matched so distinct people aren't merged.
                        IF btrim(uzman."Email") <> '' THEN
                            SELECT p."Id" INTO hedef_personel_id
                            FROM "Personeller" p
                            WHERE lower(p."Email") = lower(uzman."Email")
                            ORDER BY p."Id"
                            LIMIT 1;
                        END IF;

                        IF hedef_personel_id IS NULL THEN
                            INSERT INTO "Personeller"
                                ("BirimId", "KullanıcıAdi", "Ad", "Soyad", "Email", "Telefon", "Gorev", "OlusturmaTarihi")
                            VALUES
                                (uzman."BirimId", NULLIF(btrim(uzman."KullanıcıAdi"), ''), uzman."Ad", uzman."Soyad", uzman."Email",
                                 CASE WHEN uzman."Telefon" = 0 THEN '' ELSE uzman."Telefon"::text END,
                                 uzman."Gorev", uzman."OlusturmaTarihi")
                            RETURNING "Id" INTO hedef_personel_id;
                        ELSE
                            -- Personeller stays the source of truth; only fill the gap it had.
                            UPDATE "Personeller"
                            SET "KullanıcıAdi" = NULLIF(btrim(uzman."KullanıcıAdi"), '')
                            WHERE "Id" = hedef_personel_id AND "KullanıcıAdi" IS NULL;
                        END IF;

                        UPDATE "YazilimUzmanlari" SET "PersonelId" = hedef_personel_id WHERE "Id" = uzman."Id";
                    END LOOP;
                END $$;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "PersonelId",
                table: "YazilimUzmanlari",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_YazilimUzmanlari_PersonelId",
                table: "YazilimUzmanlari",
                column: "PersonelId");

            migrationBuilder.AddForeignKey(
                name: "FK_YazilimUzmanlari_Personeller_PersonelId",
                table: "YazilimUzmanlari",
                column: "PersonelId",
                principalTable: "Personeller",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // 4) Only now - with every row linked to a Personel - drop the duplicated columns.
            migrationBuilder.DropIndex(
                name: "IX_YazilimUzmanlari_Email",
                table: "YazilimUzmanlari");

            migrationBuilder.DropIndex(
                name: "IX_YazilimUzmanlari_KullanıcıAdi",
                table: "YazilimUzmanlari");

            migrationBuilder.DropColumn(name: "KullanıcıAdi", table: "YazilimUzmanlari");
            migrationBuilder.DropColumn(name: "Ad", table: "YazilimUzmanlari");
            migrationBuilder.DropColumn(name: "Soyad", table: "YazilimUzmanlari");
            migrationBuilder.DropColumn(name: "Email", table: "YazilimUzmanlari");
            migrationBuilder.DropColumn(name: "Gorev", table: "YazilimUzmanlari");
            migrationBuilder.DropColumn(name: "Telefon", table: "YazilimUzmanlari");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore the denormalized columns and refill them from the linked Personel. Personel
            // rows created by Up are left in place - they are valid personnel records by now.
            migrationBuilder.AddColumn<string>(
                name: "KullanıcıAdi",
                table: "YazilimUzmanlari",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Ad",
                table: "YazilimUzmanlari",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Soyad",
                table: "YazilimUzmanlari",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Gorev",
                table: "YazilimUzmanlari",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "YazilimUzmanlari",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Telefon",
                table: "YazilimUzmanlari",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("""
                UPDATE "YazilimUzmanlari" y
                SET "KullanıcıAdi" = COALESCE(p."KullanıcıAdi", ''),
                    "Ad" = p."Ad",
                    "Soyad" = p."Soyad",
                    "Gorev" = p."Gorev",
                    "Email" = p."Email",
                    "Telefon" = CASE
                        WHEN regexp_replace(p."Telefon", '\D', '', 'g') ~ '^[0-9]{1,9}$'
                            THEN regexp_replace(p."Telefon", '\D', '', 'g')::integer
                        ELSE 0
                    END
                FROM "Personeller" p
                WHERE p."Id" = y."PersonelId";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_YazilimUzmanlari_Email",
                table: "YazilimUzmanlari",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_YazilimUzmanlari_KullanıcıAdi",
                table: "YazilimUzmanlari",
                column: "KullanıcıAdi");

            migrationBuilder.DropForeignKey(
                name: "FK_YazilimUzmanlari_Personeller_PersonelId",
                table: "YazilimUzmanlari");

            migrationBuilder.DropIndex(
                name: "IX_YazilimUzmanlari_PersonelId",
                table: "YazilimUzmanlari");

            migrationBuilder.DropColumn(
                name: "PersonelId",
                table: "YazilimUzmanlari");

            migrationBuilder.DropIndex(
                name: "IX_Personeller_Email",
                table: "Personeller");

            migrationBuilder.DropIndex(
                name: "IX_Personeller_KullanıcıAdi",
                table: "Personeller");

            migrationBuilder.DropColumn(
                name: "KullanıcıAdi",
                table: "Personeller");

            migrationBuilder.DropForeignKey(
                name: "FK_YazilimUzmanlari_Birimler_BirimId",
                table: "YazilimUzmanlari");

            migrationBuilder.RenameIndex(
                name: "IX_YazilimUzmanlari_BirimId",
                table: "YazilimUzmanlari",
                newName: "IX_YazilimUzmanlari_Birim");

            migrationBuilder.RenameColumn(
                name: "BirimId",
                table: "YazilimUzmanlari",
                newName: "Birim");

            migrationBuilder.AddForeignKey(
                name: "FK_YazilimUzmanlari_Birimler_Birim",
                table: "YazilimUzmanlari",
                column: "Birim",
                principalTable: "Birimler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
