using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Colegio> Colegios { get; set; } = null!;
        public DbSet<Persona> Personas { get; set; } = null!;
        public DbSet<Usuario> Usuarios { get; set; } = null!;
        public DbSet<Estudiante> Estudiantes { get; set; } = null!;
        public DbSet<Curso> Cursos { get; set; } = null!;
        public DbSet<RegistroCaso> RegistrosCasos { get; set; } = null!;
        public DbSet<Encuesta> Encuestas { get; set; } = null!;
        public DbSet<Pregunta> Preguntas { get; set; } = null!;
        public DbSet<Opcion> Opciones { get; set; } = null!;
        public DbSet<RespuestaEncuesta> RespuestasEncuestas { get; set; } = null!;
        public DbSet<PreguntaRespuesta> PreguntasRespuestas { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Relación Persona -> Usuario
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Persona)
                .WithMany()
                .HasForeignKey(u => u.PersonaId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación Persona -> Estudiante
            modelBuilder.Entity<Estudiante>()
                .HasOne(e => e.Persona)
                .WithMany()
                .HasForeignKey(e => e.PersonaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relaciones SaaS Colegio
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Colegio)
                .WithMany()
                .HasForeignKey(u => u.ColegioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Estudiante>()
                .HasOne(e => e.Colegio)
                .WithMany()
                .HasForeignKey(e => e.ColegioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Curso>()
                .HasOne(c => c.Colegio)
                .WithMany()
                .HasForeignKey(c => c.ColegioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Estudiante>()
                .HasOne(e => e.Usuario)
                .WithMany()
                .HasForeignKey(e => e.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RegistroCaso>()
                .HasOne(r => r.Colegio)
                .WithMany()
                .HasForeignKey(r => r.ColegioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RegistroCaso>()
                .HasOne(r => r.Estudiante)
                .WithMany()
                .HasForeignKey(r => r.EstudianteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RegistroCaso>()
                .HasOne(r => r.CreadoPorUsuario)
                .WithMany()
                .HasForeignKey(r => r.CreadoPorUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Encuesta>()
                .HasOne(e => e.Colegio)
                .WithMany()
                .HasForeignKey(e => e.ColegioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Encuesta>()
                .HasOne(e => e.CreadoPorUsuario)
                .WithMany()
                .HasForeignKey(e => e.CreadoPorUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Pregunta>()
                .HasOne<Encuesta>()
                .WithMany(e => e.Preguntas)
                .HasForeignKey(p => p.EncuestaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Opcion>()
                .HasOne<Pregunta>()
                .WithMany(p => p.Opciones)
                .HasForeignKey(o => o.PreguntaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RespuestaEncuesta>()
                .HasOne(r => r.Encuesta)
                .WithMany()
                .HasForeignKey(r => r.EncuestaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RespuestaEncuesta>()
                .HasOne(r => r.Estudiante)
                .WithMany()
                .HasForeignKey(r => r.EstudianteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PreguntaRespuesta>()
                .HasOne(pr => pr.Pregunta)
                .WithMany()
                .HasForeignKey(pr => pr.PreguntaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PreguntaRespuesta>()
                .HasOne(pr => pr.OpcionSeleccionada)
                .WithMany()
                .HasForeignKey(pr => pr.OpcionSeleccionadaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PreguntaRespuesta>()
                .HasOne<RespuestaEncuesta>()
                .WithMany(re => re.RespuestasDetalle)
                .HasForeignKey(pr => pr.RespuestaEncuestaId)
                .OnDelete(DeleteBehavior.Cascade);

            // ==========================================
            // SEMILLA DE DATOS COMPLETA (SEED DATA)
            // ==========================================

            // 1. Colegio por defecto
            modelBuilder.Entity<Colegio>().HasData(
                new Colegio
                {
                    Id = 1,
                    Nombre = "Institución Educativa Cuarta Poza de Manga",
                    Nit = "800123456-1",
                    CodigoDane = "113837000123",
                    Direccion = "Turbaco, Bolívar",
                    Telefono = "(605) 6789012",
                    EmailContacto = "contacto@cuartapozademanga.edu.co",
                    FechaRegistro = new DateTime(2026, 1, 1),
                    Activo = true
                }
            );
            // Cursos por defecto
            modelBuilder.Entity<Curso>().HasData(
                new Curso { Id = 1, ColegioId = 1, Nombre = "10-A" },
                new Curso { Id = 2, ColegioId = 1, Nombre = "11-A" },
                new Curso { Id = 3, ColegioId = 1, Nombre = "11-B" }
            );
            // 2. Personas (Identidades humanas desacopladas)
            modelBuilder.Entity<Persona>().HasData(
                new Persona
                {
                    Id = 999,
                    Nombre = "Gybram (SuperAdmin)",
                    Apellido = "Llamas",
                    TipoIdentificacion = "CC",
                    NumeroIdentificacion = "0000000000",
                    Email = "admin@siae.com",
                    Telefono = "3000000000",
                    Sexo = "Masculino",
                    Direccion = "Sede Central SIAE"
                },
                new Persona
                {
                    Id = 1,
                    Nombre = "Pedro",
                    Apellido = "Pérez",
                    TipoIdentificacion = "CC",
                    NumeroIdentificacion = "73123456",
                    Email = "rector@cuartapozademanga.edu.co",
                    Telefono = "3001112233",
                    Sexo = "Masculino",
                    Direccion = "Turbaco - Centro"
                },
                new Persona
                {
                    Id = 2,
                    Nombre = "Sofía",
                    Apellido = "Rodríguez",
                    TipoIdentificacion = "CC",
                    NumeroIdentificacion = "45987654",
                    Email = "orientacion@cuartapozademanga.edu.co",
                    Telefono = "3004445566",
                    Sexo = "Femenino",
                    Direccion = "Turbaco - El Carmen"
                },
                new Persona
                {
                    Id = 3,
                    Nombre = "Carlos",
                    Apellido = "Sánchez",
                    TipoIdentificacion = "CC",
                    NumeroIdentificacion = "92456789",
                    Email = "coordinacion@cuartapozademanga.edu.co",
                    Telefono = "3007778899",
                    Sexo = "Masculino",
                    Direccion = "Turbaco - La Granja"
                },
                new Persona
                {
                    Id = 4,
                    Nombre = "Gybram",
                    Apellido = "Llamas",
                    TipoIdentificacion = "TI",
                    NumeroIdentificacion = "1098765432",
                    Email = "gybram@estudiante.edu.co",
                    Telefono = "3101234567",
                    Sexo = "Masculino",
                    Direccion = "Turbaco - Sector Manga",
                    FechaNacimiento = new DateTime(2009, 5, 12),
                    LugarNacimiento = "Cartagena"
                },
                new Persona
                {
                    Id = 5,
                    Nombre = "María Fernanda",
                    Apellido = "Gómez Ruiz",
                    TipoIdentificacion = "TI",
                    NumeroIdentificacion = "1045678901",
                    Email = "maria.gomez@estudiante.edu.co",
                    Telefono = "3129876543",
                    Sexo = "Femenino",
                    Direccion = "Turbaco - Centro",
                    FechaNacimiento = new DateTime(2010, 8, 20),
                    LugarNacimiento = "Turbaco"
                },
                new Persona
                {
                    Id = 6,
                    Nombre = "Carlos Andrés",
                    Apellido = "Ruiz Martínez",
                    TipoIdentificacion = "TI",
                    NumeroIdentificacion = "1076543210",
                    Email = "carlos.ruiz@estudiante.edu.co",
                    Telefono = "3154567890",
                    Sexo = "Masculino",
                    Direccion = "Turbaco - La Granja",
                    FechaNacimiento = new DateTime(2011, 3, 15),
                    LugarNacimiento = "Turbaco"
                },
                new Persona
                {
                    Id = 7,
                    Nombre = "Valentina",
                    Apellido = "Torres Blanco",
                    TipoIdentificacion = "TI",
                    NumeroIdentificacion = "1087654321",
                    Email = "valentina.torres@estudiante.edu.co",
                    Telefono = "3186543210",
                    Sexo = "Femenino",
                    Direccion = "Turbaco - El Carmen",
                    FechaNacimiento = new DateTime(2012, 11, 5),
                    LugarNacimiento = "Cartagena"
                }
            );

            // 3. Usuarios (Vinculados a PersonaId)
            modelBuilder.Entity<Usuario>().HasData(
                new Usuario { Id = 999, ColegioId = 1, PersonaId = 999, Username = "superadmin", PasswordHash = "superadmin123", Rol = "SUPER_ADMIN", Jornada = "Global" },
                new Usuario { Id = 1, ColegioId = 1, PersonaId = 1, Username = "rector", PasswordHash = "rector123", Rol = "Rector", Jornada = "Mañana" },
                new Usuario { Id = 2, ColegioId = 1, PersonaId = 2, Username = "orientador", PasswordHash = "orientador123", Rol = "Orientador", Jornada = "Mañana" },
                new Usuario { Id = 3, ColegioId = 1, PersonaId = 3, Username = "coordinador", PasswordHash = "coordinador123", Rol = "Coordinador", Jornada = "Tarde" },
                new Usuario { Id = 4, ColegioId = 1, PersonaId = 4, Username = "gybram", PasswordHash = "gybram123", Rol = "Estudiante", Jornada = "Tarde" },
                new Usuario { Id = 5, ColegioId = 1, PersonaId = 5, Username = "maria.gomez", PasswordHash = "1045678901", Rol = "Estudiante", Jornada = "Mañana" },
                new Usuario { Id = 6, ColegioId = 1, PersonaId = 6, Username = "carlos.ruiz", PasswordHash = "1076543210", Rol = "Estudiante", Jornada = "Mañana" },
                new Usuario { Id = 7, ColegioId = 1, PersonaId = 7, Username = "valentina.torres", PasswordHash = "1087654321", Rol = "Estudiante", Jornada = "Tarde" }
            );

            // 4. Estudiantes (Vinculados a UsuarioId y PersonaId)
            modelBuilder.Entity<Estudiante>().HasData(
                new Estudiante { Id = 1, ColegioId = 1, UsuarioId = 4, PersonaId = 4, Curso = "11-A", Eps = "Coosalud" },
                new Estudiante { Id = 2, ColegioId = 1, UsuarioId = 5, PersonaId = 5, Curso = "10-B", Eps = "Sura" },
                new Estudiante { Id = 3, ColegioId = 1, UsuarioId = 6, PersonaId = 6, Curso = "9-C", Eps = "Sanitas" },
                new Estudiante { Id = 4, ColegioId = 1, UsuarioId = 7, PersonaId = 7, Curso = "8-A", Eps = "Nueva EPS" }
            );

            // 5. Registros de Casos
            modelBuilder.Entity<RegistroCaso>().HasData(
                new RegistroCaso
                {
                    Id = 1,
                    ColegioId = 1,
                    EstudianteId = 1,
                    CreadoPorUsuarioId = 2,
                    Tipo = "Convivencia",
                    Descripcion = "Acompañamiento por adaptación escolar y estrategias socioemocionales de liderazgo positivo en el aula.",
                    RequierePIAR = false,
                    RequiereDUA = true,
                    Discapacidades = "Ninguna",
                    FechaCreacion = new DateTime(2026, 2, 10, 9, 30, 0),
                    Estado = "Iniciado"
                },
                new RegistroCaso
                {
                    Id = 2,
                    ColegioId = 1,
                    EstudianteId = 2,
                    CreadoPorUsuarioId = 3,
                    Tipo = "Disciplinario",
                    Descripcion = "Infracción al manual de convivencia por altercado verbal reiterado durante el descanso.",
                    RequierePIAR = false,
                    RequiereDUA = false,
                    Discapacidades = "",
                    FechaCreacion = new DateTime(2026, 3, 5, 14, 15, 0),
                    Estado = "EnProceso"
                },
                new RegistroCaso
                {
                    Id = 3,
                    ColegioId = 1,
                    EstudianteId = 3,
                    CreadoPorUsuarioId = 2,
                    Tipo = "Convivencia",
                    Descripcion = "Atención por dificultad de concentración y ansiedad en evaluaciones. Se aprueba Plan PIAR de adaptación en tiempos de entrega.",
                    RequierePIAR = true,
                    RequiereDUA = true,
                    Discapacidades = "TDAH, Ansiedad Generalizada",
                    FechaCreacion = new DateTime(2026, 3, 12, 10, 0, 0),
                    Estado = "EnProceso"
                },
                new RegistroCaso
                {
                    Id = 4,
                    ColegioId = 1,
                    EstudianteId = 4,
                    CreadoPorUsuarioId = 2,
                    Tipo = "Convivencia",
                    Descripcion = "Aplicación de Primeros Auxilios Psicológicos (PAP) por episodio de hiperventilación previo a examen. Estabilizada exitosamente.",
                    RequierePIAR = false,
                    RequiereDUA = true,
                    Discapacidades = "Ansiedad Aguda",
                    FechaCreacion = new DateTime(2026, 4, 1, 11, 45, 0),
                    Estado = "Cerrado"
                }
            );

            // 6. Encuestas Semilla
            modelBuilder.Entity<Encuesta>().HasData(
                new Encuesta
                {
                    Id = 1,
                    ColegioId = 1,
                    CreadoPorUsuarioId = 2,
                    Titulo = "Diagnóstico de Clima Escolar y Convivencia 2026",
                    Descripcion = "Encuesta institucional para evaluar las relaciones interpersonales, seguridad emocional y respeto en el aula.",
                    FechaCreacion = new DateTime(2026, 2, 1, 8, 0, 0)
                }
            );

            modelBuilder.Entity<Pregunta>().HasData(
                new Pregunta { Id = 1, EncuestaId = 1, TextoPregunta = "¿Cómo evalúas el nivel de respeto y trato entre compañeros en tu salón de clases?", TipoRespuesta = "SeleccionUnica" },
                new Pregunta { Id = 2, EncuestaId = 1, TextoPregunta = "¿Has presenciado o experimentado situaciones de acoso o discriminación en la institución?", TipoRespuesta = "SeleccionUnica" },
                new Pregunta { Id = 3, EncuestaId = 1, TextoPregunta = "¿Qué propuestas o sugerencias tienes para fortalecer la convivencia pacífica en tu grado?", TipoRespuesta = "TextoAbierto" }
            );

            modelBuilder.Entity<Opcion>().HasData(
                new Opcion { Id = 1, PreguntaId = 1, TextoOpcion = "Excelente" },
                new Opcion { Id = 2, PreguntaId = 1, TextoOpcion = "Bueno" },
                new Opcion { Id = 3, PreguntaId = 1, TextoOpcion = "Regular" },
                new Opcion { Id = 4, PreguntaId = 1, TextoOpcion = "Deficiente" },
                new Opcion { Id = 5, PreguntaId = 2, TextoOpcion = "Nunca" },
                new Opcion { Id = 6, PreguntaId = 2, TextoOpcion = "Rara vez" },
                new Opcion { Id = 7, PreguntaId = 2, TextoOpcion = "Frecuentemente" }
            );

            modelBuilder.Entity<RespuestaEncuesta>().HasData(
                new RespuestaEncuesta { Id = 1, EncuestaId = 1, EstudianteId = 1, FechaRespuesta = new DateTime(2026, 2, 15, 10, 30, 0) }
            );

            modelBuilder.Entity<PreguntaRespuesta>().HasData(
                new PreguntaRespuesta { Id = 1, RespuestaEncuestaId = 1, PreguntaId = 1, OpcionSeleccionadaId = 2, RespuestaTexto = "" },
                new PreguntaRespuesta { Id = 2, RespuestaEncuestaId = 1, PreguntaId = 2, OpcionSeleccionadaId = 5, RespuestaTexto = "" },
                new PreguntaRespuesta { Id = 3, RespuestaEncuestaId = 1, PreguntaId = 3, OpcionSeleccionadaId = null, RespuestaTexto = "Realizar más talleres grupales de integración y deportes durante los descansos." }
            );
        }
    }
}
