using System;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Protos;

namespace Backend.Services
{
    public class AdminGrpcService : AdminService.AdminServiceBase
    {
        private readonly ApplicationDbContext _dbContext;

        public AdminGrpcService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public override async Task<GetAllUsersReply> GetAllUsers(GetAllUsersRequest request, ServerCallContext context)
        {
            var adminUser = await _dbContext.Usuarios.FindAsync(request.SuperAdminId);
            if (adminUser == null || adminUser.Rol != "SUPER_ADMIN")
            {
                throw new RpcException(new Status(StatusCode.PermissionDenied, "Acceso denegado. Solo el SUPER_ADMIN puede consultar todos los usuarios de la plataforma."));
            }

            IQueryable<Models.Usuario> query = _dbContext.Usuarios
                .Include(u => u.Persona)
                .Include(u => u.Colegio);

            if (request.ColegioIdFilter > 0)
            {
                query = query.Where(u => u.ColegioId == request.ColegioIdFilter);
            }

            var usuarios = await query.ToListAsync();
            var reply = new GetAllUsersReply();

            foreach (var u in usuarios)
            {
                reply.Usuarios.Add(new UserDetailMessage
                {
                    Id = u.Id,
                    ColegioId = u.ColegioId,
                    ColegioNombre = u.Colegio?.Nombre ?? "Institución Educativa",
                    Username = u.Username,
                    PasswordHash = "", // Nunca exponer hashes de contraseña en las respuestas gRPC
                    Nombre = u.Nombre,
                    Apellido = u.Apellido,
                    Email = u.Email,
                    Telefono = u.Telefono,
                    TipoIdentificacion = u.TipoIdentificacion,
                    NumeroIdentificacion = u.NumeroIdentificacion,
                    Rol = u.Rol,
                    Jornada = u.Jornada
                });
            }

            return reply;
        }

        public override async Task<ResetUserPasswordReply> ResetUserPassword(ResetUserPasswordRequest request, ServerCallContext context)
        {
            var adminUser = await _dbContext.Usuarios.FindAsync(request.SuperAdminId);
            if (adminUser == null || adminUser.Rol != "SUPER_ADMIN")
            {
                throw new RpcException(new Status(StatusCode.PermissionDenied, "Acceso denegado. Solo el SUPER_ADMIN puede restablecer contraseñas."));
            }

            var targetUser = await _dbContext.Usuarios.FindAsync(request.TargetUserId);
            if (targetUser == null)
            {
                return new ResetUserPasswordReply
                {
                    Success = false,
                    Message = "Usuario no encontrado."
                };
            }

            targetUser.PasswordHash = PasswordHasher.HashPassword(request.NewPassword);
            await _dbContext.SaveChangesAsync();

            return new ResetUserPasswordReply
            {
                Success = true,
                Message = $"Contraseña para '{targetUser.Username}' actualizada correctamente por el SUPER_ADMIN."
            };
        }

        public override async Task<UpdateUserProfileReply> UpdateUserProfile(UpdateUserProfileRequest request, ServerCallContext context)
        {
            try
            {
                var user = await _dbContext.Usuarios
                    .Include(u => u.Persona)
                    .FirstOrDefaultAsync(u => u.Id == request.UserId);

                if (user == null)
                {
                    return new UpdateUserProfileReply
                    {
                        Success = false,
                        Message = "Usuario no encontrado."
                    };
                }

                // Actualizar campos mutables (El ID no se modifica)
                user.Persona.Nombre = request.Nombre;
                user.Persona.Apellido = request.Apellido;
                user.Persona.Email = request.Email;
                user.Username = request.Email; // El email se utiliza como username de ingreso

                if (!string.IsNullOrEmpty(request.Password))
                {
                    user.PasswordHash = PasswordHasher.HashPassword(request.Password);
                }

                await _dbContext.SaveChangesAsync();

                return new UpdateUserProfileReply
                {
                    Success = true,
                    Message = "Datos de perfil actualizados exitosamente."
                };
            }
            catch (Exception ex)
            {
                return new UpdateUserProfileReply
                {
                    Success = false,
                    Message = $"Error al actualizar el perfil: {ex.Message}"
                };
            }
        }
    }
}
