using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddAsignacionEncuestas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CursoAsignado",
                table: "Encuestas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EstudianteAsignadoId",
                table: "Encuestas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoAsignacion",
                table: "Encuestas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Encuestas",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CursoAsignado", "EstudianteAsignadoId", "TipoAsignacion" },
                values: new object[] { null, null, "Global" });

            migrationBuilder.CreateIndex(
                name: "IX_Encuestas_EstudianteAsignadoId",
                table: "Encuestas",
                column: "EstudianteAsignadoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Encuestas_Estudiantes_EstudianteAsignadoId",
                table: "Encuestas",
                column: "EstudianteAsignadoId",
                principalTable: "Estudiantes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Encuestas_Estudiantes_EstudianteAsignadoId",
                table: "Encuestas");

            migrationBuilder.DropIndex(
                name: "IX_Encuestas_EstudianteAsignadoId",
                table: "Encuestas");

            migrationBuilder.DropColumn(
                name: "CursoAsignado",
                table: "Encuestas");

            migrationBuilder.DropColumn(
                name: "EstudianteAsignadoId",
                table: "Encuestas");

            migrationBuilder.DropColumn(
                name: "TipoAsignacion",
                table: "Encuestas");
        }
    }
}
