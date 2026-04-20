using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkerService.Infrastrucure.Data.Migrations
{
    /// <inheritdoc />
    public partial class WorkerServiceInitial : Migration
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
                name: "paineis",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Arquivado = table.Column<bool>(type: "INTEGER", nullable: false),
                    Primario = table.Column<bool>(type: "INTEGER", nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", nullable: false),
                    Referencia = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_paineis", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "dispositivos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PainelId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Arquivado = table.Column<bool>(type: "INTEGER", nullable: false),
                    Tipo = table.Column<long>(type: "INTEGER", nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", nullable: false),
                    Parametros = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dispositivos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_dispositivos_paineis_PainelId",
                        column: x => x.PainelId,
                        principalTable: "paineis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "modulos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PainelId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Arquivado = table.Column<bool>(type: "INTEGER", nullable: false),
                    Master = table.Column<bool>(type: "INTEGER", nullable: false),
                    Controlador = table.Column<bool>(type: "INTEGER", nullable: false),
                    Estagio = table.Column<int>(type: "INTEGER", nullable: false),
                    Marca = table.Column<long>(type: "INTEGER", nullable: false),
                    Modelo = table.Column<long>(type: "INTEGER", nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_modulos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_modulos_paineis_PainelId",
                        column: x => x.PainelId,
                        principalTable: "paineis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "interfaces",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModuloId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModuloConectadoId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Tipo = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Marca = table.Column<long>(type: "INTEGER", nullable: false),
                    Modelo = table.Column<long>(type: "INTEGER", nullable: false),
                    Categoria = table.Column<long>(type: "INTEGER", nullable: false),
                    IndiceModbus = table.Column<int>(type: "INTEGER", nullable: true),
                    EnderecoModbus = table.Column<int>(type: "INTEGER", nullable: true),
                    Porta = table.Column<string>(type: "TEXT", nullable: false),
                    EnderecoBorne = table.Column<string>(type: "TEXT", nullable: true),
                    EnderecoLogico = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_interfaces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_interfaces_modulos_ModuloId",
                        column: x => x.ModuloId,
                        principalTable: "modulos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "portas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModuloId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DispositivoConectadoId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Tipo = table.Column<int>(type: "INTEGER", nullable: false),
                    Sinal = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    EnderecoBorne = table.Column<string>(type: "TEXT", nullable: true),
                    EnderecoLogico = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_portas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_portas_dispositivos_DispositivoConectadoId",
                        column: x => x.DispositivoConectadoId,
                        principalTable: "dispositivos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_portas_modulos_ModuloId",
                        column: x => x.ModuloId,
                        principalTable: "modulos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_configuracoes_sistema_Chave",
                table: "configuracoes_sistema",
                column: "Chave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_dispositivos_Id",
                table: "dispositivos",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_dispositivos_PainelId",
                table: "dispositivos",
                column: "PainelId");

            migrationBuilder.CreateIndex(
                name: "IX_interfaces_Id",
                table: "interfaces",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_interfaces_ModuloConectadoId",
                table: "interfaces",
                column: "ModuloConectadoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_interfaces_ModuloId",
                table: "interfaces",
                column: "ModuloId");

            migrationBuilder.CreateIndex(
                name: "IX_modulos_Id",
                table: "modulos",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_modulos_PainelId",
                table: "modulos",
                column: "PainelId");

            migrationBuilder.CreateIndex(
                name: "IX_paineis_Id",
                table: "paineis",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_portas_DispositivoConectadoId",
                table: "portas",
                column: "DispositivoConectadoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_portas_EnderecoLogico",
                table: "portas",
                column: "EnderecoLogico");

            migrationBuilder.CreateIndex(
                name: "IX_portas_Id",
                table: "portas",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_portas_ModuloId",
                table: "portas",
                column: "ModuloId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "configuracoes_sistema");

            migrationBuilder.DropTable(
                name: "interfaces");

            migrationBuilder.DropTable(
                name: "portas");

            migrationBuilder.DropTable(
                name: "dispositivos");

            migrationBuilder.DropTable(
                name: "modulos");

            migrationBuilder.DropTable(
                name: "paineis");
        }
    }
}
