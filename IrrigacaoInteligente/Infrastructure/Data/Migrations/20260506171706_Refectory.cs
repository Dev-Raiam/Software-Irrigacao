using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IrrigacaoInteligente.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Refectory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_controlador",
                table: "controlador");

            migrationBuilder.RenameTable(
                name: "controlador",
                newName: "controladores");

            migrationBuilder.AddPrimaryKey(
                name: "PK_controladores",
                table: "controladores",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_controladores",
                table: "controladores");

            migrationBuilder.RenameTable(
                name: "controladores",
                newName: "controlador");

            migrationBuilder.AddPrimaryKey(
                name: "PK_controlador",
                table: "controlador",
                column: "Id");
        }
    }
}
