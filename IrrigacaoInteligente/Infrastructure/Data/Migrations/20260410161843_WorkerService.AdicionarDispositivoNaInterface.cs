using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IrrigacaoInteligente.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class IrrigacaoInteligenteAdicionarDispositivoNaInterface : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DispositivoConectadoId",
                table: "interfaces",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_interfaces_DispositivoConectadoId",
                table: "interfaces",
                column: "DispositivoConectadoId");

            migrationBuilder.AddForeignKey(
                name: "FK_interfaces_dispositivos_DispositivoConectadoId",
                table: "interfaces",
                column: "DispositivoConectadoId",
                principalTable: "dispositivos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_interfaces_dispositivos_DispositivoConectadoId",
                table: "interfaces");

            migrationBuilder.DropIndex(
                name: "IX_interfaces_DispositivoConectadoId",
                table: "interfaces");

            migrationBuilder.DropColumn(
                name: "DispositivoConectadoId",
                table: "interfaces");
        }
    }
}
