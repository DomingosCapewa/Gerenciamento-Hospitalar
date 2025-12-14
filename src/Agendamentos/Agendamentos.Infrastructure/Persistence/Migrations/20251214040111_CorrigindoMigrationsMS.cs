using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Agendamentos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CorrigindoMigrationsMS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PacienteCpf",
                table: "Exames",
                newName: "CpfPaciente");

            migrationBuilder.RenameColumn(
                name: "DataHora",
                table: "Exames",
                newName: "Horario");

            migrationBuilder.RenameIndex(
                name: "IX_Exames_PacienteCpf_DataHora",
                table: "Exames",
                newName: "IX_Exames_CpfPaciente_Horario");

            migrationBuilder.RenameColumn(
                name: "PacienteCpf",
                table: "Consultas",
                newName: "CpfPaciente");

            migrationBuilder.RenameColumn(
                name: "DataHora",
                table: "Consultas",
                newName: "Horario");

            migrationBuilder.RenameIndex(
                name: "IX_Consultas_PacienteCpf_DataHora",
                table: "Consultas",
                newName: "IX_Consultas_CpfPaciente_Horario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Horario",
                table: "Exames",
                newName: "DataHora");

            migrationBuilder.RenameColumn(
                name: "CpfPaciente",
                table: "Exames",
                newName: "PacienteCpf");

            migrationBuilder.RenameIndex(
                name: "IX_Exames_CpfPaciente_Horario",
                table: "Exames",
                newName: "IX_Exames_PacienteCpf_DataHora");

            migrationBuilder.RenameColumn(
                name: "Horario",
                table: "Consultas",
                newName: "DataHora");

            migrationBuilder.RenameColumn(
                name: "CpfPaciente",
                table: "Consultas",
                newName: "PacienteCpf");

            migrationBuilder.RenameIndex(
                name: "IX_Consultas_CpfPaciente_Horario",
                table: "Consultas",
                newName: "IX_Consultas_PacienteCpf_DataHora");
        }
    }
}
