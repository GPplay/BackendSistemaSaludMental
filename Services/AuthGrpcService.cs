using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Protos;

namespace Backend.Services
{
    public class AuthGrpcService : AuthService.AuthServiceBase
    {
        private readonly ApplicationDbContext _dbContext;

        public AuthGrpcService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public override async Task<LoginReply> Login(LoginRequest request, ServerCallContext context)
        {
            var usuario = await _dbContext.Usuarios
                .Include(u => u.Persona)
                .Include(u => u.Colegio)
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (usuario == null || !PasswordHasher.VerifyPassword(usuario.PasswordHash, request.Password))
            {
                return new LoginReply
                {
                    Success = false,
                    Message = "Usuario o contraseña incorrectos."
                };
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

            return new LoginReply
            {
                Success = true,
                Message = "Autenticación exitosa.",
                UserId = usuario.Id,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Rol = usuario.Rol,
                Jornada = usuario.Jornada,
                EstudianteId = estudianteId,
                ColegioId = usuario.ColegioId,
                ColegioNombre = usuario.Colegio?.Nombre ?? "Institución Educativa"
            };
        }
    }
}
