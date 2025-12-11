using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Agendamentos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial_Agendamentos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Exames_PacienteId_DataHora",
                table: "Exames");

            migrationBuilder.DropIndex(
                name: "IX_Consultas_PacienteId_DataHora",
                table: "Consultas");

            migrationBuilder.DropColumn(
                name: "PacienteId",
                table: "Exames");

            migrationBuilder.DropColumn(
                name: "ExameId",
                table: "Consultas");

            migrationBuilder.DropColumn(
                name: "PacienteId",
                table: "Consultas");

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "Consultas");

            migrationBuilder.RenameColumn(
                name: "Tipo",
                table: "Exames",
                newName: "TipoExame");

            migrationBuilder.RenameColumn(
                name: "Medico",
                table: "Consultas",
                newName: "Status");

            migrationBuilder.AddColumn<int>(
                name: "Idade",
                table: "Pacientes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Sexo",
                table: "Pacientes",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PacienteCpf",
                table: "Exames",
                type: "varchar(255)",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Exames",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "MedicoEspecialidade",
                table: "Consultas",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PacienteCpf",
                table: "Consultas",
                type: "varchar(255)",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Exames_PacienteCpf_DataHora",
                table: "Exames",
                columns: new[] { "PacienteCpf", "DataHora" });

            migrationBuilder.CreateIndex(
                name: "IX_Consultas_PacienteCpf_DataHora",
                table: "Consultas",
                columns: new[] { "PacienteCpf", "DataHora" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Exames_PacienteCpf_DataHora",
                table: "Exames");

            migrationBuilder.DropIndex(
                name: "IX_Consultas_PacienteCpf_DataHora",
                table: "Consultas");

            migrationBuilder.DropColumn(
                name: "Idade",
                table: "Pacientes");

            migrationBuilder.DropColumn(
                name: "Sexo",
                table: "Pacientes");

            migrationBuilder.DropColumn(
                name: "PacienteCpf",
                table: "Exames");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Exames");

            migrationBuilder.DropColumn(
                name: "MedicoEspecialidade",
                table: "Consultas");

            migrationBuilder.DropColumn(
                name: "PacienteCpf",
                table: "Consultas");

            migrationBuilder.RenameColumn(
                name: "TipoExame",
                table: "Exames",
                newName: "Tipo");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Consultas",
                newName: "Medico");

            migrationBuilder.AddColumn<int>(
                name: "PacienteId",
                table: "Exames",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ExameId",
                table: "Consultas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PacienteId",
                table: "Consultas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Tipo",
                table: "Consultas",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Exames_PacienteId_DataHora",
                table: "Exames",
                columns: new[] { "PacienteId", "DataHora" });

            migrationBuilder.CreateIndex(
                name: "IX_Consultas_PacienteId_DataHora",
                table: "Consultas",
                columns: new[] { "PacienteId", "DataHora" });
        }
    }
}
