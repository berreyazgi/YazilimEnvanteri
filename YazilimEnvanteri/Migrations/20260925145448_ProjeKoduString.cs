using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YazilimEnvanteri.Migrations
{
    /// <inheritdoc />
    public partial class ProjeKoduString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // integer -> varchar keeps every value (9999 becomes '9999'); nothing is dropped.
            migrationBuilder.AlterColumn<string>(
                name: "ProjeKodu",
                schema: "Proje",
                table: "Proje",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            // Refuse (instead of silently deleting/rewriting) if existing codes break the new rule,
            // e.g. negative numbers - they must be fixed by hand first.
            migrationBuilder.Sql("""
                DO $$
                DECLARE gecersizler text;
                BEGIN
                    SELECT string_agg(format('Id=%s Kod=%s', "Id", "ProjeKodu"), ', ' ORDER BY "Id") INTO gecersizler
                    FROM "Proje"."Proje"
                    WHERE "ProjeKodu" !~ '^[A-Z0-9ÇĞİÖŞÜ][A-Z0-9ÇĞİÖŞÜ_-]*$';

                    IF gecersizler IS NOT NULL THEN
                        RAISE EXCEPTION 'Kurala uymayan proje kodları var, önce düzeltin: %', gecersizler;
                    END IF;
                END $$;
                """);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Proje_ProjeKodu_Format",
                schema: "Proje",
                table: "Proje",
                sql: "\"ProjeKodu\" ~ '^[A-Z0-9ÇĞİÖŞÜ][A-Z0-9ÇĞİÖŞÜ_-]*$'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Going back to integer is only possible while every code is still purely numeric.
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM "Proje"."Proje" WHERE "ProjeKodu" !~ '^[0-9]{1,9}$') THEN
                        RAISE EXCEPTION 'Sayısal olmayan proje kodları varken geri alınamaz.';
                    END IF;
                END $$;
                """);

            migrationBuilder.DropCheckConstraint(
                name: "CK_Proje_ProjeKodu_Format",
                schema: "Proje",
                table: "Proje");

            // varchar -> integer needs an explicit USING cast in PostgreSQL.
            migrationBuilder.Sql("""
                ALTER TABLE "Proje"."Proje" ALTER COLUMN "ProjeKodu" TYPE integer USING "ProjeKodu"::integer;
                """);
        }
    }
}
