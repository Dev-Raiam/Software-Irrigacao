using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IrrigacaoInteligente.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AjustandoNomeTabela : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Controladores",
                table: "Controladores");

            migrationBuilder.RenameTable(
                name: "Controladores",
                newName: "controlador");

            migrationBuilder.AddPrimaryKey(
                name: "PK_controlador",
                table: "controlador",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_controlador",
                table: "controlador");

            migrationBuilder.RenameTable(
                name: "controlador",
                newName: "Controladores");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Controladores",
                table: "Controladores",
                column: "Id");
        }
    }
}
