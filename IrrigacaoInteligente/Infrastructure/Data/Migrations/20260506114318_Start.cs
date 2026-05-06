using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IrrigacaoInteligente.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Start : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "configuracoes_sistema",
                columns: table => new
                {
                    Chave = table.Column<string>(type: "TEXT", nullable: false),
                    Valor = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_configuracoes_sistema", x => x.Chave);
                });

            migrationBuilder.CreateTable(
                name: "Controladores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Configuracao = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Controladores", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_configuracoes_sistema_Chave",
                table: "configuracoes_sistema",
                column: "Chave",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "configuracoes_sistema");

            migrationBuilder.DropTable(
                name: "Controladores");
        }
    }
}
