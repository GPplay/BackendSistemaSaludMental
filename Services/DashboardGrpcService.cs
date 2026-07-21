using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Protos;

namespace Backend.Services
{
    public class DashboardGrpcService : DashboardService.DashboardServiceBase
    {
        private readonly ApplicationDbContext _dbContext;

        public DashboardGrpcService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public override async Task<DashboardReply> GetDashboard(DashboardRequest request, ServerCallContext context)
        {
            int colegioId = request.ColegioId > 0 ? request.ColegioId : 1;

            int totalEstudiantes = await _dbContext.Estudiantes.CountAsync(e => e.ColegioId == colegioId);
            int totalEncuestas = await _dbContext.Encuestas.CountAsync(e => e.ColegioId == colegioId);
            int totalRespuestas = await _dbContext.RespuestasEncuestas
                .Include(r => r.Encuesta)
                .CountAsync(r => r.Encuesta.ColegioId == colegioId);

            int casosIniciados = await _dbContext.RegistrosCasos.CountAsync(c => c.ColegioId == colegioId && c.Estado == "Iniciado");
            int casosEnProceso = await _dbContext.RegistrosCasos.CountAsync(c => c.ColegioId == colegioId && c.Estado == "EnProceso");
            int casosCerrados = await _dbContext.RegistrosCasos.CountAsync(c => c.ColegioId == colegioId && c.Estado == "Cerrado");

            if (request.Rol == "Estudiante")
            {
                var estudiante = await _dbContext.Estudiantes
                    .FirstOrDefaultAsync(e => e.UsuarioId == request.UserId && e.ColegioId == colegioId);

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

            return new DashboardReply
            {
                TotalEstudiantes = totalEstudiantes,
                CasosIniciados = casosIniciados,
                CasosEnProceso = casosEnProceso,
                CasosCerrados = casosCerrados,
                TotalEncuestas = totalEncuestas,
                TotalRespuestas = totalRespuestas
            };
        }
    }
}
