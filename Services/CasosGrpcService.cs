using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
using Backend.Protos;

namespace Backend.Services
{
    public class CasosGrpcService : CasosService.CasosServiceBase
    {
        private readonly ApplicationDbContext _dbContext;

        public CasosGrpcService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public override async Task<CreateCasoReply> CreateCaso(CreateCasoRequest request, ServerCallContext context)
        {
            try
            {
                int colegioId = request.ColegioId > 0 ? request.ColegioId : 1;

                var caso = new RegistroCaso
                {
                    ColegioId = colegioId,
                    EstudianteId = request.EstudianteId,
                    CreadoPorUsuarioId = request.CreadoPorId,
                    Tipo = request.Tipo,
                    Descripcion = request.Descripcion,
                    RequierePIAR = request.RequierePiar,
                    RequiereDUA = request.RequiereDua,
                    Discapacidades = request.Discapacidades,
                    FechaCreacion = DateTime.UtcNow,
                    Estado = "Iniciado"
                };

                _dbContext.RegistrosCasos.Add(caso);
                await _dbContext.SaveChangesAsync();

                return new CreateCasoReply
                {
                    Success = true,
                    Message = "Registro de caso creado exitosamente.",
                    CasoId = caso.Id
                };
            }
            catch (Exception ex)
            {
                return new CreateCasoReply
                {
                    Success = false,
                    Message = $"Error al registrar el caso: {ex.Message}"
                };
            }
        }

        public override async Task<CasoListReply> GetCasos(CasoListRequest request, ServerCallContext context)
        {
            int colegioId = request.ColegioId > 0 ? request.ColegioId : 1;

            IQueryable<RegistroCaso> query = _dbContext.RegistrosCasos
                .Include(r => r.Estudiante)
                .ThenInclude(e => e.Usuario)
                .Include(r => r.CreadoPorUsuario)
                .Where(r => r.ColegioId == colegioId);

            if (request.Rol == "Estudiante")
            {
                query = query.Where(r => r.Estudiante.UsuarioId == request.UserId);
            }

            var casos = await query.ToListAsync();
            var reply = new CasoListReply();

            foreach (var c in casos)
            {
                reply.Casos.Add(new CasoMessage
                {
                    Id = c.Id,
                    EstudianteNombre = $"{c.Estudiante.Usuario.Nombre} {c.Estudiante.Usuario.Apellido}",
                    EstudianteCurso = c.Estudiante.Curso,
                    CreadoPorNombre = $"{c.CreadoPorUsuario.Nombre} {c.CreadoPorUsuario.Apellido}",
                    Tipo = c.Tipo,
                    Descripcion = c.Descripcion,
                    RequierePiar = c.RequierePIAR,
                    RequiereDua = c.RequiereDUA,
                    Discapacidades = c.Discapacidades,
                    FechaCreacion = c.FechaCreacion.ToString("yyyy-MM-dd HH:mm:ss"),
                    Estado = c.Estado
                });
            }

            return reply;
        }

        public override async Task<UpdateCasoEstadoReply> UpdateCasoEstado(UpdateCasoEstadoRequest request, ServerCallContext context)
        {
            try
            {
                var caso = await _dbContext.RegistrosCasos.FindAsync(request.CasoId);

                if (caso == null)
                {
                    return new UpdateCasoEstadoReply
                    {
                        Success = false,
                        Message = "Caso no encontrado."
                    };
                }

                caso.Estado = request.NuevoEstado;
                await _dbContext.SaveChangesAsync();

                return new UpdateCasoEstadoReply
                {
                    Success = true,
                    Message = $"Estado del caso actualizado a '{request.NuevoEstado}'."
                };
            }
            catch (Exception ex)
            {
                return new UpdateCasoEstadoReply
                {
                    Success = false,
                    Message = $"Error al actualizar el estado: {ex.Message}"
                };
            }
        }
    }
}
