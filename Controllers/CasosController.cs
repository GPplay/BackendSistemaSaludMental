using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
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
    public class CasosController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public CasosController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCaso([FromBody] CreateCasoDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var colegioIdClaim = User.FindFirst("ColegioId")?.Value;

            if (userIdClaim == null || colegioIdClaim == null)
            {
                return Unauthorized(new { success = false, message = "Token JWT inválido o incompleto." });
            }

            int userId = int.Parse(userIdClaim);
            int colegioId = int.Parse(colegioIdClaim);

            try
            {
                var caso = new RegistroCaso
                {
                    ColegioId = colegioId,
                    EstudianteId = dto.EstudianteId,
                    CreadoPorUsuarioId = userId,
                    Tipo = dto.Tipo,
                    Descripcion = dto.Descripcion,
                    RequierePIAR = dto.RequierePiar,
                    RequiereDUA = dto.RequiereDua,
                    Discapacidades = dto.Discapacidades,
                    FechaCreacion = DateTime.UtcNow,
                    Estado = "Iniciado"
                };

                _dbContext.RegistrosCasos.Add(caso);
                await _dbContext.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Registro de caso creado exitosamente.",
                    casoId = caso.Id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error al registrar el caso: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCasos()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            var colegioIdClaim = User.FindFirst("ColegioId")?.Value;

            if (userIdClaim == null || roleClaim == null || colegioIdClaim == null)
            {
                return Unauthorized(new { success = false, message = "Token JWT inválido o incompleto." });
            }

            int userId = int.Parse(userIdClaim);
            string rol = roleClaim;
            int colegioId = int.Parse(colegioIdClaim);

            IQueryable<RegistroCaso> query = _dbContext.RegistrosCasos
                .Include(r => r.Estudiante)
                .ThenInclude(e => e.Usuario)
                .ThenInclude(u => u.Persona)
                .Include(r => r.CreadoPorUsuario)
                .ThenInclude(u => u.Persona)
                .Where(r => r.ColegioId == colegioId);

            if (rol == "Estudiante")
            {
                query = query.Where(r => r.Estudiante.UsuarioId == userId);
            }

            var casos = await query.ToListAsync();

            var result = casos.Select(c => new
            {
                id = c.Id,
                estudianteNombre = $"{c.Estudiante.Usuario.Nombre} {c.Estudiante.Usuario.Apellido}",
                estudianteCurso = c.Estudiante.Curso,
                creadoPorNombre = $"{c.CreadoPorUsuario.Nombre} {c.CreadoPorUsuario.Apellido}",
                tipo = c.Tipo,
                descripcion = c.Descripcion,
                requierePiar = c.RequierePIAR,
                requiereDua = c.RequiereDUA,
                discapacidades = c.Discapacidades,
                fechaCreacion = c.FechaCreacion.ToString("yyyy-MM-dd HH:mm:ss"),
                estado = c.Estado
            });

            return Ok(result);
        }

        [HttpPut("{id}/estado")]
        public async Task<IActionResult> UpdateCasoEstado(int id, [FromBody] UpdateCasoEstadoDto dto)
        {
            try
            {
                var caso = await _dbContext.RegistrosCasos.FindAsync(id);

                if (caso == null)
                {
                    return NotFound(new { success = false, message = "Caso no encontrado." });
                }

                caso.Estado = dto.NuevoEstado;
                await _dbContext.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = $"Estado del caso actualizado a '{dto.NuevoEstado}'."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error al actualizar el estado: {ex.Message}" });
            }
        }
    }

    public class CreateCasoDto
    {
        public int EstudianteId { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool RequierePiar { get; set; }
        public bool RequiereDua { get; set; }
        public string Discapacidades { get; set; } = string.Empty;
    }

    public class UpdateCasoEstadoDto
    {
        public string NuevoEstado { get; set; } = string.Empty;
    }
}
