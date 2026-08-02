using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Colegios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CodigoDane = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmailContacto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Colegios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Personas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Apellido = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TipoIdentificacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumeroIdentificacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sexo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaNacimiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LugarNacimiento = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cursos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ColegioId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cursos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cursos_Colegios_ColegioId",
                        column: x => x.ColegioId,
                        principalTable: "Colegios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ColegioId = table.Column<int>(type: "int", nullable: false),
                    PersonaId = table.Column<int>(type: "int", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Jornada = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Usuarios_Colegios_ColegioId",
                        column: x => x.ColegioId,
                        principalTable: "Colegios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Usuarios_Personas_PersonaId",
                        column: x => x.PersonaId,
                        principalTable: "Personas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Encuestas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ColegioId = table.Column<int>(type: "int", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreadoPorUsuarioId = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Encuestas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Encuestas_Colegios_ColegioId",
                        column: x => x.ColegioId,
                        principalTable: "Colegios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Encuestas_Usuarios_CreadoPorUsuarioId",
                        column: x => x.CreadoPorUsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Estudiantes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ColegioId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    PersonaId = table.Column<int>(type: "int", nullable: false),
                    Curso = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Eps = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NombreAcudiente = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TelefonoAcudiente = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParentescoAcudiente = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estudiantes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Estudiantes_Colegios_ColegioId",
                        column: x => x.ColegioId,
                        principalTable: "Colegios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Estudiantes_Personas_PersonaId",
                        column: x => x.PersonaId,
                        principalTable: "Personas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Estudiantes_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Preguntas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EncuestaId = table.Column<int>(type: "int", nullable: false),
                    TextoPregunta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TipoRespuesta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EsObligatoria = table.Column<bool>(type: "bit", nullable: false),
                    Restriccion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrefijoPais = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Preguntas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Preguntas_Encuestas_EncuestaId",
                        column: x => x.EncuestaId,
                        principalTable: "Encuestas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegistrosCasos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ColegioId = table.Column<int>(type: "int", nullable: false),
                    EstudianteId = table.Column<int>(type: "int", nullable: false),
                    CreadoPorUsuarioId = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequierePIAR = table.Column<bool>(type: "bit", nullable: false),
                    RequiereDUA = table.Column<bool>(type: "bit", nullable: false),
                    Discapacidades = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosCasos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegistrosCasos_Colegios_ColegioId",
                        column: x => x.ColegioId,
                        principalTable: "Colegios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegistrosCasos_Estudiantes_EstudianteId",
                        column: x => x.EstudianteId,
                        principalTable: "Estudiantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RegistrosCasos_Usuarios_CreadoPorUsuarioId",
                        column: x => x.CreadoPorUsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RespuestasEncuestas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EncuestaId = table.Column<int>(type: "int", nullable: false),
                    EstudianteId = table.Column<int>(type: "int", nullable: false),
                    FechaRespuesta = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RespuestasEncuestas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RespuestasEncuestas_Encuestas_EncuestaId",
                        column: x => x.EncuestaId,
                        principalTable: "Encuestas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RespuestasEncuestas_Estudiantes_EstudianteId",
                        column: x => x.EstudianteId,
                        principalTable: "Estudiantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Opciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PreguntaId = table.Column<int>(type: "int", nullable: false),
                    TextoOpcion = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Opciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Opciones_Preguntas_PreguntaId",
                        column: x => x.PreguntaId,
                        principalTable: "Preguntas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PreguntasRespuestas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RespuestaEncuestaId = table.Column<int>(type: "int", nullable: false),
                    PreguntaId = table.Column<int>(type: "int", nullable: false),
                    RespuestaTexto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OpcionSeleccionadaId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreguntasRespuestas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreguntasRespuestas_Opciones_OpcionSeleccionadaId",
                        column: x => x.OpcionSeleccionadaId,
                        principalTable: "Opciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PreguntasRespuestas_Preguntas_PreguntaId",
                        column: x => x.PreguntaId,
                        principalTable: "Preguntas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PreguntasRespuestas_RespuestasEncuestas_RespuestaEncuestaId",
                        column: x => x.RespuestaEncuestaId,
                        principalTable: "RespuestasEncuestas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Colegios",
                columns: new[] { "Id", "Activo", "CodigoDane", "Direccion", "EmailContacto", "FechaRegistro", "Nit", "Nombre", "Telefono" },
                values: new object[] { 1, true, "113837000123", "Turbaco, Bolívar", "contacto@cuartapozademanga.edu.co", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "800123456-1", "Institución Educativa Cuarta Poza de Manga", "(605) 6789012" });

            migrationBuilder.InsertData(
                table: "Personas",
                columns: new[] { "Id", "Apellido", "Direccion", "Email", "FechaNacimiento", "LugarNacimiento", "Nombre", "NumeroIdentificacion", "Sexo", "Telefono", "TipoIdentificacion" },
                values: new object[,]
                {
                    { 1, "Pérez", "Turbaco - Centro", "rector@cuartapozademanga.edu.co", null, "", "Pedro", "73123456", "Masculino", "3001112233", "CC" },
                    { 2, "Rodríguez", "Turbaco - El Carmen", "orientacion@cuartapozademanga.edu.co", null, "", "Sofía", "45987654", "Femenino", "3004445566", "CC" },
                    { 3, "Sánchez", "Turbaco - La Granja", "coordinacion@cuartapozademanga.edu.co", null, "", "Carlos", "92456789", "Masculino", "3007778899", "CC" },
                    { 4, "Llamas", "Turbaco - Sector Manga", "gybram@estudiante.edu.co", new DateTime(2009, 5, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Cartagena", "Gybram", "1098765432", "Masculino", "3101234567", "TI" },
                    { 5, "Gómez Ruiz", "Turbaco - Centro", "maria.gomez@estudiante.edu.co", new DateTime(2010, 8, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Turbaco", "María Fernanda", "1045678901", "Femenino", "3129876543", "TI" },
                    { 6, "Ruiz Martínez", "Turbaco - La Granja", "carlos.ruiz@estudiante.edu.co", new DateTime(2011, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Turbaco", "Carlos Andrés", "1076543210", "Masculino", "3154567890", "TI" },
                    { 7, "Torres Blanco", "Turbaco - El Carmen", "valentina.torres@estudiante.edu.co", new DateTime(2012, 11, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Cartagena", "Valentina", "1087654321", "Femenino", "3186543210", "TI" },
                    { 999, "Llamas", "Sede Central SIAE", "admin@siae.com", null, "", "Gybram (SuperAdmin)", "0000000000", "Masculino", "3000000000", "CC" }
                });

            migrationBuilder.InsertData(
                table: "Cursos",
                columns: new[] { "Id", "ColegioId", "Nombre" },
                values: new object[,]
                {
                    { 1, 1, "10-A" },
                    { 2, 1, "11-A" },
                    { 3, 1, "11-B" }
                });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "ColegioId", "Jornada", "PasswordHash", "PersonaId", "Rol", "Username" },
                values: new object[,]
                {
                    { 1, 1, "Mañana", "rector123", 1, "Rector", "rector" },
                    { 2, 1, "Mañana", "orientador123", 2, "Orientador", "orientador" },
                    { 3, 1, "Tarde", "coordinador123", 3, "Coordinador", "coordinador" },
                    { 4, 1, "Tarde", "gybram123", 4, "Estudiante", "gybram" },
                    { 5, 1, "Mañana", "1045678901", 5, "Estudiante", "maria.gomez" },
                    { 6, 1, "Mañana", "1076543210", 6, "Estudiante", "carlos.ruiz" },
                    { 7, 1, "Tarde", "1087654321", 7, "Estudiante", "valentina.torres" },
                    { 999, 1, "Global", "superadmin123", 999, "SUPER_ADMIN", "superadmin" }
                });

            migrationBuilder.InsertData(
                table: "Encuestas",
                columns: new[] { "Id", "ColegioId", "CreadoPorUsuarioId", "Descripcion", "FechaCreacion", "Titulo" },
                values: new object[] { 1, 1, 2, "Encuesta institucional para evaluar las relaciones interpersonales, seguridad emocional y respeto en el aula.", new DateTime(2026, 2, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), "Diagnóstico de Clima Escolar y Convivencia 2026" });

            migrationBuilder.InsertData(
                table: "Estudiantes",
                columns: new[] { "Id", "ColegioId", "Curso", "Eps", "NombreAcudiente", "ParentescoAcudiente", "PersonaId", "TelefonoAcudiente", "UsuarioId" },
                values: new object[,]
                {
                    { 1, 1, "11-A", "Coosalud", "", "", 4, "", 4 },
                    { 2, 1, "10-B", "Sura", "", "", 5, "", 5 },
                    { 3, 1, "9-C", "Sanitas", "", "", 6, "", 6 },
                    { 4, 1, "8-A", "Nueva EPS", "", "", 7, "", 7 }
                });

            migrationBuilder.InsertData(
                table: "Preguntas",
                columns: new[] { "Id", "EncuestaId", "EsObligatoria", "PrefijoPais", "Restriccion", "TextoPregunta", "TipoRespuesta" },
                values: new object[,]
                {
                    { 1, 1, true, "+57", "", "¿Cómo evalúas el nivel de respeto y trato entre compañeros en tu salón de clases?", "SeleccionUnica" },
                    { 2, 1, true, "+57", "", "¿Has presenciado o experimentado situaciones de acoso o discriminación en la institución?", "SeleccionUnica" },
                    { 3, 1, true, "+57", "", "¿Qué propuestas o sugerencias tienes para fortalecer la convivencia pacífica en tu grado?", "TextoAbierto" }
                });

            migrationBuilder.InsertData(
                table: "RegistrosCasos",
                columns: new[] { "Id", "ColegioId", "CreadoPorUsuarioId", "Descripcion", "Discapacidades", "Estado", "EstudianteId", "FechaCreacion", "RequiereDUA", "RequierePIAR", "Tipo" },
                values: new object[,]
                {
                    { 1, 1, 2, "Acompañamiento por adaptación escolar y estrategias socioemocionales de liderazgo positivo en el aula.", "Ninguna", "Iniciado", 1, new DateTime(2026, 2, 10, 9, 30, 0, 0, DateTimeKind.Unspecified), true, false, "Convivencia" },
                    { 2, 1, 3, "Infracción al manual de convivencia por altercado verbal reiterado durante el descanso.", "", "EnProceso", 2, new DateTime(2026, 3, 5, 14, 15, 0, 0, DateTimeKind.Unspecified), false, false, "Disciplinario" },
                    { 3, 1, 2, "Atención por dificultad de concentración y ansiedad en evaluaciones. Se aprueba Plan PIAR de adaptación en tiempos de entrega.", "TDAH, Ansiedad Generalizada", "EnProceso", 3, new DateTime(2026, 3, 12, 10, 0, 0, 0, DateTimeKind.Unspecified), true, true, "Convivencia" },
                    { 4, 1, 2, "Aplicación de Primeros Auxilios Psicológicos (PAP) por episodio de hiperventilación previo a examen. Estabilizada exitosamente.", "Ansiedad Aguda", "Cerrado", 4, new DateTime(2026, 4, 1, 11, 45, 0, 0, DateTimeKind.Unspecified), true, false, "Convivencia" }
                });

            migrationBuilder.InsertData(
                table: "RespuestasEncuestas",
                columns: new[] { "Id", "EncuestaId", "EstudianteId", "FechaRespuesta" },
                values: new object[] { 1, 1, 1, new DateTime(2026, 2, 15, 10, 30, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                table: "Opciones",
                columns: new[] { "Id", "PreguntaId", "TextoOpcion" },
                values: new object[,]
                {
                    { 1, 1, "Excelente" },
                    { 2, 1, "Bueno" },
                    { 3, 1, "Regular" },
                    { 4, 1, "Deficiente" },
                    { 5, 2, "Nunca" },
                    { 6, 2, "Rara vez" },
                    { 7, 2, "Frecuentemente" }
                });

            migrationBuilder.InsertData(
                table: "PreguntasRespuestas",
                columns: new[] { "Id", "OpcionSeleccionadaId", "PreguntaId", "RespuestaEncuestaId", "RespuestaTexto" },
                values: new object[,]
                {
                    { 3, null, 3, 1, "Realizar más talleres grupales de integración y deportes durante los descansos." },
                    { 1, 2, 1, 1, "" },
                    { 2, 5, 2, 1, "" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cursos_ColegioId",
                table: "Cursos",
                column: "ColegioId");

            migrationBuilder.CreateIndex(
                name: "IX_Encuestas_ColegioId",
                table: "Encuestas",
                column: "ColegioId");

            migrationBuilder.CreateIndex(
                name: "IX_Encuestas_CreadoPorUsuarioId",
                table: "Encuestas",
                column: "CreadoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Estudiantes_ColegioId",
                table: "Estudiantes",
                column: "ColegioId");

            migrationBuilder.CreateIndex(
                name: "IX_Estudiantes_PersonaId",
                table: "Estudiantes",
                column: "PersonaId");

            migrationBuilder.CreateIndex(
                name: "IX_Estudiantes_UsuarioId",
                table: "Estudiantes",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Opciones_PreguntaId",
                table: "Opciones",
                column: "PreguntaId");

            migrationBuilder.CreateIndex(
                name: "IX_Preguntas_EncuestaId",
                table: "Preguntas",
                column: "EncuestaId");

            migrationBuilder.CreateIndex(
                name: "IX_PreguntasRespuestas_OpcionSeleccionadaId",
                table: "PreguntasRespuestas",
                column: "OpcionSeleccionadaId");

            migrationBuilder.CreateIndex(
                name: "IX_PreguntasRespuestas_PreguntaId",
                table: "PreguntasRespuestas",
                column: "PreguntaId");

            migrationBuilder.CreateIndex(
                name: "IX_PreguntasRespuestas_RespuestaEncuestaId",
                table: "PreguntasRespuestas",
                column: "RespuestaEncuestaId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosCasos_ColegioId",
                table: "RegistrosCasos",
                column: "ColegioId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosCasos_CreadoPorUsuarioId",
                table: "RegistrosCasos",
                column: "CreadoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosCasos_EstudianteId",
                table: "RegistrosCasos",
                column: "EstudianteId");

            migrationBuilder.CreateIndex(
                name: "IX_RespuestasEncuestas_EncuestaId",
                table: "RespuestasEncuestas",
                column: "EncuestaId");

            migrationBuilder.CreateIndex(
                name: "IX_RespuestasEncuestas_EstudianteId",
                table: "RespuestasEncuestas",
                column: "EstudianteId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_ColegioId",
                table: "Usuarios",
                column: "ColegioId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_PersonaId",
                table: "Usuarios",
                column: "PersonaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cursos");

            migrationBuilder.DropTable(
                name: "PreguntasRespuestas");

            migrationBuilder.DropTable(
                name: "RegistrosCasos");

            migrationBuilder.DropTable(
                name: "Opciones");

            migrationBuilder.DropTable(
                name: "RespuestasEncuestas");

            migrationBuilder.DropTable(
                name: "Preguntas");

            migrationBuilder.DropTable(
                name: "Estudiantes");

            migrationBuilder.DropTable(
                name: "Encuestas");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Colegios");

            migrationBuilder.DropTable(
                name: "Personas");
        }
    }
}
