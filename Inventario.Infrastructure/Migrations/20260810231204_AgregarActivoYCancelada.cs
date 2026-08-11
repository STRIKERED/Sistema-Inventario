using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventario.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarActivoYCancelada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Cancelada",
                table: "Ventas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Productos",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cancelada",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Productos");
        }
    }
}
