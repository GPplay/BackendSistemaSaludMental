using System;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
using Backend.Protos;

namespace Backend.Services
{
    public class ColegioGrpcService : ColegioService.ColegioServiceBase
    {
        private readonly ApplicationDbContext _dbContext;

        public ColegioGrpcService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public override async Task<GetColegiosReply> GetColegios(GetColegiosRequest request, ServerCallContext context)
        {
            var user = await _dbContext.Usuarios.FindAsync(request.UserId);
            if (user == null || user.Rol != "SUPER_ADMIN")
            {
                throw new RpcException(new Status(StatusCode.PermissionDenied, "Solo el SUPER_ADMIN tiene acceso a la administración global de colegios."));
            }

            var colegios = await _dbContext.Colegios.ToListAsync();
            var reply = new GetColegiosReply();

            foreach (var c in colegios)
            {
                int totalEstud = await _dbContext.Estudiantes.CountAsync(e => e.ColegioId == c.Id);
                int totalUsers = await _dbContext.Usuarios.CountAsync(u => u.ColegioId == c.Id);

                reply.Colegios.Add(new ColegioMessage
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Nit = c.Nit,
                    CodigoDane = c.CodigoDane,
                    Direccion = c.Direccion,
                    Telefono = c.Telefono,
                    EmailContacto = c.EmailContacto,
                    FechaRegistro = c.FechaRegistro.ToString("yyyy-MM-dd HH:mm:ss"),
                    Activo = c.Activo,
                    TotalEstudiantes = totalEstud,
                    TotalUsuarios = totalUsers
                });
            }

            return reply;
        }

        public override async Task<CreateColegioReply> CreateColegio(CreateColegioRequest request, ServerCallContext context)
        {
            try
            {
                var colegio = new Colegio
                {
                    Nombre = request.Nombre,
                    Nit = request.Nit,
                    CodigoDane = request.CodigoDane,
                    Direccion = request.Direccion,
                    Telefono = request.Telefono,
                    EmailContacto = request.EmailContacto,
                    FechaRegistro = DateTime.UtcNow,
                    Activo = true
                };

                _dbContext.Colegios.Add(colegio);
                await _dbContext.SaveChangesAsync();

                return new CreateColegioReply
                {
                    Success = true,
                    Message = "Colegio creado exitosamente en la plataforma SaaS.",
                    ColegioId = colegio.Id
                };
            }
            catch (Exception ex)
            {
                return new CreateColegioReply
                {
                    Success = false,
                    Message = $"Error al crear el colegio: {ex.Message}"
                };
            }
        }
    }
}
