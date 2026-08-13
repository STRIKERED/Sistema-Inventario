using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventario.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarConfiguracionImpresion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConfiguracionesImpresion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    InventarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    NombreImpresora = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    AnchoTicketMm = table.Column<int>(type: "INTEGER", nullable: false),
                    EncabezadoTicket = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    PiePaginaTicket = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    LogoRutaPdf = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionesImpresion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfiguracionesImpresion_Inventarios_InventarioId",
                        column: x => x.InventarioId,
                        principalTable: "Inventarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesImpresion_InventarioId",
                table: "ConfiguracionesImpresion",
                column: "InventarioId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracionesImpresion");
        }
    }
}
