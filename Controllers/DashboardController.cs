using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Backend.Data;

namespace Backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public DashboardController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboard()
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

            if (rol == "SUPER_ADMIN")
            {
                int globalEstudiantes = await _dbContext.Estudiantes.CountAsync();
                int globalEncuestas = await _dbContext.Encuestas.CountAsync();
                int globalRespuestas = await _dbContext.RespuestasEncuestas.CountAsync();
                int globalCasosIniciados = await _dbContext.RegistrosCasos.CountAsync(c => c.Estado == "Iniciado");
                int globalCasosEnProceso = await _dbContext.RegistrosCasos.CountAsync(c => c.Estado == "EnProceso");
                int globalCasosCerrados = await _dbContext.RegistrosCasos.CountAsync(c => c.Estado == "Cerrado");
                int globalColegios = await _dbContext.Colegios.CountAsync();
                int globalUsuarios = await _dbContext.Usuarios.CountAsync();

                return Ok(new
                {
                    totalEstudiantes = globalEstudiantes,
                    casosIniciados = globalCasosIniciados,
                    casosEnProceso = globalCasosEnProceso,
                    casosCerrados = globalCasosCerrados,
                    totalEncuestas = globalEncuestas,
                    totalRespuestas = globalRespuestas,
                    totalColegios = globalColegios,
                    totalUsuarios = globalUsuarios
                });
            }

            int totalEstudiantes = await _dbContext.Estudiantes.CountAsync(e => e.ColegioId == colegioId);
            int totalEncuestas = await _dbContext.Encuestas.CountAsync(e => e.ColegioId == colegioId);
            int totalRespuestas = await _dbContext.RespuestasEncuestas
                .Include(r => r.Encuesta)
                .CountAsync(r => r.Encuesta.ColegioId == colegioId);

            int casosIniciados = await _dbContext.RegistrosCasos.CountAsync(c => c.ColegioId == colegioId && c.Estado == "Iniciado");
            int casosEnProceso = await _dbContext.RegistrosCasos.CountAsync(c => c.ColegioId == colegioId && c.Estado == "EnProceso");
            int casosCerrados = await _dbContext.RegistrosCasos.CountAsync(c => c.ColegioId == colegioId && c.Estado == "Cerrado");

            if (rol == "Estudiante")
            {
                var estudiante = await _dbContext.Estudiantes
                    .FirstOrDefaultAsync(e => e.UsuarioId == userId && e.ColegioId == colegioId);

                if (estudiante != null)
                {
                    totalRespuestas = await _dbContext.RespuestasEncuestas
                        .CountAsync(r => r.EstudianteId == estudiante.Id);
                    
                    casosIniciados = await _dbContext.RegistrosCasos
                        .CountAsync(c => c.EstudianteId == estudiante.Id && c.Estado == "Iniciado");
                    casosEnProceso = await _dbContext.RegistrosCasos
                        .CountAsync(c => c.EstudianteId == estudiante.Id && c.Estado == "EnProceso");
                    casosCerrados = await _dbContext.RegistrosCasos
                        .CountAsync(c => c.EstudianteId == estudiante.Id && c.Estado == "Cerrado");
                }
            }

            return Ok(new
            {
                totalEstudiantes = totalEstudiantes,
                casosIniciados = casosIniciados,
                casosEnProceso = casosEnProceso,
                casosCerrados = casosCerrados,
                totalEncuestas = totalEncuestas,
                totalRespuestas = totalRespuestas,
                totalColegios = 1,
                totalUsuarios = await _dbContext.Usuarios.CountAsync(u => u.ColegioId == colegioId)
            });
        }
    }
}
