using System;
using System.Collections.Generic;

namespace Backend.Models
{
    public class Colegio
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Nit { get; set; } = string.Empty;
        public string CodigoDane { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string EmailContacto { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
        public bool Activo { get; set; } = true;
    }

    public class Usuario
    {
        public int Id { get; set; }
        public int ColegioId { get; set; } = 1;
        public Colegio Colegio { get; set; } = null!;
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string TipoIdentificacion { get; set; } = string.Empty;
        public string NumeroIdentificacion { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty; // SuperAdmin, Rector, Orientador, Coordinador, Estudiante
        public string Jornada { get; set; } = string.Empty; // Mañana, Tarde, Noche
    }

    public class Estudiante
    {
        public int Id { get; set; }
        public int ColegioId { get; set; } = 1;
        public Colegio Colegio { get; set; } = null!;
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;
        public string Curso { get; set; } = string.Empty;
        public string LugarNacimiento { get; set; } = string.Empty;
        public DateTime FechaNacimiento { get; set; }
        public string Sexo { get; set; } = string.Empty;
        public string Eps { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
    }

    public class RegistroCaso
    {
        public int Id { get; set; }
        public int ColegioId { get; set; } = 1;
        public Colegio Colegio { get; set; } = null!;
        public int EstudianteId { get; set; }
        public Estudiante Estudiante { get; set; } = null!;
        public int CreadoPorUsuarioId { get; set; }
        public Usuario CreadoPorUsuario { get; set; } = null!;
        public string Tipo { get; set; } = string.Empty; // Disciplinario, Convivencia
        public string Descripcion { get; set; } = string.Empty;
        public bool RequierePIAR { get; set; }
        public bool RequiereDUA { get; set; }
        public string Discapacidades { get; set; } = string.Empty; // Autismo, TDAH, etc.
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public string Estado { get; set; } = "Iniciado"; // Iniciado, EnProceso, Cerrado
    }

    public class Encuesta
    {
        public int Id { get; set; }
        public int ColegioId { get; set; } = 1;
        public Colegio Colegio { get; set; } = null!;
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int CreadoPorUsuarioId { get; set; }
        public Usuario CreadoPorUsuario { get; set; } = null!;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public List<Pregunta> Preguntas { get; set; } = new();
    }

    public class Pregunta
    {
        public int Id { get; set; }
        public int EncuestaId { get; set; }
        public string TextoPregunta { get; set; } = string.Empty;
        public string TipoRespuesta { get; set; } = string.Empty; // SeleccionUnica, TextoAbierto
        public List<Opcion> Opciones { get; set; } = new();
    }

    public class Opcion
    {
        public int Id { get; set; }
        public int PreguntaId { get; set; }
        public string TextoOpcion { get; set; } = string.Empty;
    }

    public class RespuestaEncuesta
    {
        public int Id { get; set; }
        public int EncuestaId { get; set; }
        public Encuesta Encuesta { get; set; } = null!;
        public int EstudianteId { get; set; }
        public Estudiante Estudiante { get; set; } = null!;
        public DateTime FechaRespuesta { get; set; } = DateTime.UtcNow;
        public List<PreguntaRespuesta> RespuestasDetalle { get; set; } = new();
    }

    public class PreguntaRespuesta
    {
        public int Id { get; set; }
        public int RespuestaEncuestaId { get; set; }
        public int PreguntaId { get; set; }
        public Pregunta Pregunta { get; set; } = null!;
        public string RespuestaTexto { get; set; } = string.Empty;
        public int? OpcionSeleccionadaId { get; set; }
        public Opcion? OpcionSeleccionada { get; set; }
    }
}
