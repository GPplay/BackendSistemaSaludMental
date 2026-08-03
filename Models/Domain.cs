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

    /// <summary>
    /// Entidad Base 'Persona' que abstrae los datos demográficos e identidad humana
    /// compartidos por Estudiantes, Rectores, Psicólogos, Trabajadores Sociales, Coordinadores y SuperAdmin.
    /// </summary>
    public class Persona
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string TipoIdentificacion { get; set; } = string.Empty; // CC, TI, RC, CE
        public string NumeroIdentificacion { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Sexo { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public DateTime? FechaNacimiento { get; set; }
        public string LugarNacimiento { get; set; } = string.Empty;
    }

    /// <summary>
    /// Cuenta de Usuario en el sistema SaaS (Asociada a una Persona)
    /// </summary>
    public class Usuario
    {
        public int Id { get; set; }
        public int ColegioId { get; set; } = 1;
        public Colegio Colegio { get; set; } = null!;
        public int PersonaId { get; set; }
        public Persona Persona { get; set; } = null!;
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty; // SUPER_ADMIN, Rector, Orientador, Psicologo, TrabajadorSocial, Coordinador, Estudiante
        public string Jornada { get; set; } = string.Empty; // Mañana, Tarde, Noche, Global

        // Propiedades de conveniencia delegadas a Persona
        public string Nombre => Persona?.Nombre ?? string.Empty;
        public string Apellido => Persona?.Apellido ?? string.Empty;
        public string Email => Persona?.Email ?? string.Empty;
        public string Telefono => Persona?.Telefono ?? string.Empty;
        public string TipoIdentificacion => Persona?.TipoIdentificacion ?? string.Empty;
        public string NumeroIdentificacion => Persona?.NumeroIdentificacion ?? string.Empty;
    }

    /// <summary>
    /// Expediente Académico y Clínico del Estudiante (Asociado a una Persona y Usuario)
    /// </summary>
    public class Estudiante
    {
        public int Id { get; set; }
        public int ColegioId { get; set; } = 1;
        public Colegio Colegio { get; set; } = null!;
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;
        public int PersonaId { get; set; }
        public Persona Persona { get; set; } = null!;
        public string Curso { get; set; } = string.Empty;
        public string Eps { get; set; } = string.Empty;
        
        // Datos del Acudiente / Padre de Familia
        public string NombreAcudiente { get; set; } = string.Empty;
        public string TelefonoAcudiente { get; set; } = string.Empty;
        public string ParentescoAcudiente { get; set; } = string.Empty; // Madre, Padre, Acudiente Legal
        
        // Propiedades delegadas a Persona para retrocompatibilidad
        public string LugarNacimiento => Persona?.LugarNacimiento ?? string.Empty;
        public DateTime FechaNacimiento => Persona?.FechaNacimiento ?? DateTime.MinValue;
        public string Sexo => Persona?.Sexo ?? string.Empty;
        public string Direccion => Persona?.Direccion ?? string.Empty;
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
        public string TipoAsignacion { get; set; } = "Global"; // Global, Curso, Estudiante
        public string? CursoAsignado { get; set; }
        public int? EstudianteAsignadoId { get; set; }
        public Estudiante? EstudianteAsignado { get; set; }
        public List<Pregunta> Preguntas { get; set; } = new();
    }

    public class Pregunta
    {
        public int Id { get; set; }
        public int EncuestaId { get; set; }
        public string TextoPregunta { get; set; } = string.Empty;
        public string TipoRespuesta { get; set; } = string.Empty; // SeleccionUnica, TextoAbierto, CorreoElectronico, NumeroTelefono, EscalaNumerica
        public bool EsObligatoria { get; set; } = true;
        public string Restriccion { get; set; } = string.Empty; // Email, Phone, MinMax_1_10
        public string PrefijoPais { get; set; } = "+57"; // +57 (Colombia), +1 (USA), +52 (México), +34 (España), etc.
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

    public class CreateUsuarioColegioDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string TipoIdentificacion { get; set; } = "CC";
        public string NumeroIdentificacion { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Rol { get; set; } = "Orientador"; // Orientador, Psicologo, TrabajadorSocial, Coordinador, Docente
        public string Jornada { get; set; } = "Mañana";
        public int? ColegioId { get; set; }
    }

    public class CreateEstudianteIndividualDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string TipoIdentificacion { get; set; } = "TI";
        public string NumeroIdentificacion { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Sexo { get; set; } = "Masculino";
        public string Direccion { get; set; } = string.Empty;
        public string FechaNacimiento { get; set; } = string.Empty;
        public string Curso { get; set; } = string.Empty;
        public string Jornada { get; set; } = "Mañana";
        public string Eps { get; set; } = "Sin Asignar";
        public string NombreAcudiente { get; set; } = string.Empty;
        public string TelefonoAcudiente { get; set; } = string.Empty;
        public string ParentescoAcudiente { get; set; } = "Acudiente Legal";
    }
}
