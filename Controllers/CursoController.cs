using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
using Backend.Services;

namespace Backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CursoController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public CursoController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetCursos([FromQuery] int? colegioId)
        {
            var roleClaim = ClaimHelper.GetRole(User);
            var userColegioIdClaim = ClaimHelper.GetColegioId(User);

            int targetColegioId = 1;
            if (roleClaim == "SUPER_ADMIN")
            {
                if (colegioId.HasValue && colegioId.Value > 0)
                {
                    targetColegioId = colegioId.Value;
                }
                else
                {
                    // Si no se pasa, devolver todos los cursos
                    var todosCursos = await _dbContext.Cursos
                        .OrderBy(c => c.ColegioId).ThenBy(c => c.Nombre)
                        .Select(c => new
                        {
                            id = c.Id,
                            colegioId = c.ColegioId,
                            nombre = c.Nombre
                        })
                        .ToListAsync();
                    return Ok(todosCursos);
                }
            }
            else
            {
                if (userColegioIdClaim == null)
                {
                    return Unauthorized(new { success = false, message = "Token JWT inválido o incompleto." });
                }
                targetColegioId = int.Parse(userColegioIdClaim);
            }

            var cursos = await _dbContext.Cursos
                .Where(c => c.ColegioId == targetColegioId)
                .OrderBy(c => c.Nombre)
                .Select(c => new
                {
                    id = c.Id,
                    colegioId = c.ColegioId,
                    nombre = c.Nombre
                })
                .ToListAsync();

            return Ok(cursos);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCurso([FromBody] CreateCursoDto dto)
        {
            var roleClaim = ClaimHelper.GetRole(User);
            var userColegioIdClaim = ClaimHelper.GetColegioId(User);

            if (roleClaim != "SUPER_ADMIN" && roleClaim != "Rector")
            {
                return Forbid("Solo el SUPER_ADMIN o el Rector pueden crear nuevos cursos.");
            }

            int targetColegioId = 0;
            if (roleClaim == "SUPER_ADMIN")
            {
                if (dto.ColegioId <= 0)
                {
                    return BadRequest(new { success = false, message = "El SUPER_ADMIN debe proveer un ColegioId válido." });
                }
                targetColegioId = dto.ColegioId;
            }
            else
            {
                if (userColegioIdClaim == null)
                {
                    return Unauthorized(new { success = false, message = "Token JWT inválido." });
                }
                targetColegioId = int.Parse(userColegioIdClaim);
            }

            if (string.IsNullOrWhiteSpace(dto.Nombre))
            {
                return BadRequest(new { success = false, message = "El nombre del curso no puede estar vacío." });
            }

            string cleanNombre = dto.Nombre.Trim().Replace(" ", "").ToUpperInvariant();

            // Validar si ya existe
            bool existe = await _dbContext.Cursos
                .AnyAsync(c => c.ColegioId == targetColegioId && c.Nombre == cleanNombre);

            if (existe)
            {
                return BadRequest(new { success = false, message = $"El curso '{cleanNombre}' ya está registrado en este colegio." });
            }

            var curso = new Curso
            {
                ColegioId = targetColegioId,
                Nombre = cleanNombre
            };

            _dbContext.Cursos.Add(curso);
            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Curso creado exitosamente.",
                id = curso.Id,
                nombre = curso.Nombre,
                colegioId = curso.ColegioId
            });
        }
    }

    public class CreateCursoDto
    {
        public string Nombre { get; set; } = string.Empty;
        public int ColegioId { get; set; }
    }
}
