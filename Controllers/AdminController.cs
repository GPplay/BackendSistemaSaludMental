using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;

namespace Backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public AdminController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // --- GESTIÓN DE COLEGIOS (Solo SUPER_ADMIN) ---
        [HttpGet("colegios")]
        public async Task<IActionResult> GetColegios()
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            if (roleClaim != "SUPER_ADMIN")
            {
                return Forbid("Solo el SUPER_ADMIN tiene acceso a la administración global de colegios.");
            }

            var colegios = await _dbContext.Colegios.ToListAsync();
            var result = new List<object>();

            foreach (var c in colegios)
            {
                int totalEstud = await _dbContext.Estudiantes.CountAsync(e => e.ColegioId == c.Id);
                int totalUsers = await _dbContext.Usuarios.CountAsync(u => u.ColegioId == c.Id);

                result.Add(new
                {
                    id = c.Id,
                    nombre = c.Nombre,
                    nit = c.Nit,
                    codigoDane = c.CodigoDane,
                    direccion = c.Direccion,
                    telefono = c.Telefono,
                    emailContacto = c.EmailContacto,
                    fechaRegistro = c.FechaRegistro.ToString("yyyy-MM-dd HH:mm:ss"),
                    activo = c.Activo,
                    totalEstudiantes = totalEstud,
                    totalUsuarios = totalUsers
                });
            }

            return Ok(result);
        }

        [HttpPost("colegios")]
        public async Task<IActionResult> CreateColegio([FromBody] CreateColegioDto dto)
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            if (roleClaim != "SUPER_ADMIN")
            {
                return Forbid("Solo el SUPER_ADMIN tiene acceso a crear colegios.");
            }

            try
            {
                var colegio = new Colegio
                {
                    Nombre = dto.Nombre,
                    Nit = dto.Nit,
                    CodigoDane = dto.CodigoDane,
                    Direccion = dto.Direccion,
                    Telefono = dto.Telefono,
                    EmailContacto = dto.EmailContacto,
                    FechaRegistro = DateTime.UtcNow,
                    Activo = true
                };

                _dbContext.Colegios.Add(colegio);
                await _dbContext.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Colegio creado exitosamente en la plataforma SaaS.",
                    colegioId = colegio.Id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error al crear el colegio: {ex.Message}" });
            }
        }

        // --- GESTIÓN DE USUARIOS (Solo SUPER_ADMIN) ---
        [HttpGet("usuarios")]
        public async Task<IActionResult> GetAllUsers([FromQuery] int? colegioIdFilter)
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            if (roleClaim != "SUPER_ADMIN")
            {
                return Forbid("Solo el SUPER_ADMIN puede consultar todos los usuarios de la plataforma.");
            }

            IQueryable<Usuario> query = _dbContext.Usuarios
                .Include(u => u.Persona)
                .Include(u => u.Colegio);

            if (colegioIdFilter.HasValue && colegioIdFilter.Value > 0)
            {
                query = query.Where(u => u.ColegioId == colegioIdFilter.Value);
            }

            var usuarios = await query.ToListAsync();
            var result = usuarios.Select(u => new
            {
                id = u.Id,
                colegioId = u.ColegioId,
                colegioNombre = u.Colegio?.Nombre ?? "Institución Educativa",
                username = u.Username,
                nombre = u.Nombre,
                apellido = u.Apellido,
                email = u.Email,
                telefono = u.Telefono,
                tipoIdentificacion = u.TipoIdentificacion,
                numeroIdentificacion = u.NumeroIdentificacion,
                rol = u.Rol,
                jornada = u.Jornada
            });

            return Ok(result);
        }

        [HttpPost("usuarios/{id}/reset-password")]
        public async Task<IActionResult> ResetUserPassword(int id, [FromBody] ResetPasswordDto dto)
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            if (roleClaim != "SUPER_ADMIN")
            {
                return Forbid("Solo el SUPER_ADMIN puede restablecer contraseñas globales.");
            }

            var targetUser = await _dbContext.Usuarios.FindAsync(id);
            if (targetUser == null)
            {
                return NotFound(new { success = false, message = "Usuario no encontrado." });
            }

            targetUser.PasswordHash = PasswordHasher.HashPassword(dto.NewPassword);
            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = $"Contraseña para '{targetUser.Username}' actualizada correctamente por el SUPER_ADMIN."
            });
        }

        // --- ACTUALIZACIÓN DE PERFIL (Cualquier usuario autenticado) ---
        [HttpPut("perfil")]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateProfileDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized(new { success = false, message = "Token JWT inválido o incompleto." });
            }

            int userId = int.Parse(userIdClaim);

            try
            {
                var user = await _dbContext.Usuarios
                    .Include(u => u.Persona)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return NotFound(new { success = false, message = "Usuario no encontrado." });
                }

                user.Persona.Nombre = dto.Nombre;
                user.Persona.Apellido = dto.Apellido;
                user.Persona.Email = dto.Email;
                user.Username = dto.Email; // El email se utiliza como username de ingreso para Rectores/Orientadores

                if (!string.IsNullOrEmpty(dto.Password))
                {
                    user.PasswordHash = PasswordHasher.HashPassword(dto.Password);
                }

                await _dbContext.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Datos de perfil actualizados exitosamente."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error al actualizar el perfil: {ex.Message}" });
            }
        }
    }

    public class CreateColegioDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Nit { get; set; } = string.Empty;
        public string CodigoDane { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string EmailContacto { get; set; } = string.Empty;
    }

    public class ResetPasswordDto
    {
        public string NewPassword { get; set; } = string.Empty;
    }

    public class UpdateProfileDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Password { get; set; }
    }
}
