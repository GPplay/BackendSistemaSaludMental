using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
using Backend.Services;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public AuthController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { success = false, message = "Datos de entrada inválidos." });
            }

            var usuario = await _dbContext.Usuarios
                .Include(u => u.Persona)
                .Include(u => u.Colegio)
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (usuario == null || !PasswordHasher.VerifyPassword(usuario.PasswordHash, request.Password))
            {
                return Unauthorized(new { success = false, message = "Usuario o contraseña incorrectos." });
            }

            int estudianteId = 0;
            if (usuario.Rol == "Estudiante")
            {
                var estudiante = await _dbContext.Estudiantes
                    .FirstOrDefaultAsync(e => e.UsuarioId == usuario.Id);
                if (estudiante != null)
                {
                    estudianteId = estudiante.Id;
                }
            }

            // Generar Token JWT conteniendo los datos clave de sesión
            var token = JwtUtils.GenerateToken(usuario.Id, usuario.Username, usuario.Rol, usuario.ColegioId);

            return Ok(new
            {
                success = true,
                message = "Autenticación exitosa.",
                token = token,
                userId = usuario.Id,
                nombre = usuario.Nombre,
                apellido = usuario.Apellido,
                rol = usuario.Rol,
                jornada = usuario.Jornada,
                estudianteId = estudianteId,
                colegioId = usuario.ColegioId,
                colegioNombre = usuario.Colegio?.Nombre ?? "Institución Educativa"
            });
        }
    }

    public class LoginDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
