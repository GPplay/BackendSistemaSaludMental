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
        public DbSet<Usuario> Usuarios { get; set; } = null!;
        public DbSet<Estudiante> Estudiantes { get; set; } = null!;
        public DbSet<RegistroCaso> RegistrosCasos { get; set; } = null!;
        public DbSet<Encuesta> Encuestas { get; set; } = null!;
        public DbSet<Pregunta> Preguntas { get; set; } = null!;
        public DbSet<Opcion> Opciones { get; set; } = null!;
        public DbSet<RespuestaEncuesta> RespuestasEncuestas { get; set; } = null!;
        public DbSet<PreguntaRespuesta> PreguntasRespuestas { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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

            // Semilla de Colegio por defecto
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

            // Semilla de Datos de Prueba (Seed Data)
            modelBuilder.Entity<Usuario>().HasData(
                new Usuario 
                { 
                    Id = 1, 
                    ColegioId = 1,
                    Username = "rector", 
                    PasswordHash = "rector123", 
                    Nombre = "Pedro", 
                    Apellido = "Pérez", 
                    Email = "rector@cuartapozademanga.edu.co",
                    Rol = "Rector", 
                    Jornada = "Mañana" 
                },
                new Usuario 
                { 
                    Id = 2, 
                    ColegioId = 1,
                    Username = "orientador", 
                    PasswordHash = "orientador123", 
                    Nombre = "Sofía", 
                    Apellido = "Rodríguez", 
                    Email = "orientacion@cuartapozademanga.edu.co",
                    Rol = "Orientador", 
                    Jornada = "Mañana" 
                },
                new Usuario 
                { 
                    Id = 3, 
                    ColegioId = 1,
                    Username = "coordinador", 
                    PasswordHash = "coordinador123", 
                    Nombre = "Carlos", 
                    Apellido = "Sánchez", 
                    Email = "coordinacion@cuartapozademanga.edu.co",
                    Rol = "Coordinador", 
                    Jornada = "Tarde" 
                },
                new Usuario 
                { 
                    Id = 4, 
                    ColegioId = 1,
                    Username = "gybram", 
                    PasswordHash = "gybram123", 
                    Nombre = "Gybram", 
                    Apellido = "Llamas", 
                    NumeroIdentificacion = "1098765432",
                    TipoIdentificacion = "TI",
                    Email = "gybram@estudiante.edu.co",
                    Rol = "Estudiante", 
                    Jornada = "Tarde" 
                }
            );

            modelBuilder.Entity<Estudiante>().HasData(
                new Estudiante 
                { 
                    Id = 1, 
                    ColegioId = 1,
                    UsuarioId = 4, 
                    Curso = "11-A", 
                    LugarNacimiento = "Cartagena", 
                    FechaNacimiento = new DateTime(2009, 5, 12), 
                    Sexo = "Masculino", 
                    Eps = "Coosalud", 
                    Direccion = "Turbaco - Sector Manga" 
                }
            );
        }
    }
}
