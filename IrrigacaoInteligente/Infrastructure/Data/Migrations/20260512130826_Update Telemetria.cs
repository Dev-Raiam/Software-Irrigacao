using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IrrigacaoInteligente.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTelemetria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "configuracoes",
                columns: table => new
                {
                    Chave = table.Column<string>(type: "TEXT", nullable: false),
                    Valor = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_configuracoes", x => x.Chave);
                });

            migrationBuilder.CreateTable(
                name: "controladores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Configuracao = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_controladores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "telemetrias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ControladorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DispositivoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Dados = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_telemetrias", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_configuracoes_Chave",
                table: "configuracoes",
                column: "Chave",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "configuracoes");

            migrationBuilder.DropTable(
                name: "controladores");

            migrationBuilder.DropTable(
                name: "telemetrias");
        }
    }
}
