using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ExcelDataReader;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
using Backend.Protos;

namespace Backend.Services
{
    public class EstudianteGrpcService : EstudianteService.EstudianteServiceBase
    {
        private readonly ApplicationDbContext _dbContext;

        public EstudianteGrpcService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public override async Task<UploadExcelReply> UploadEstudiantesExcel(UploadExcelRequest request, ServerCallContext context)
        {
            var reply = new UploadExcelReply
            {
                Success = false,
                TotalProcesados = 0,
                CreadosExitosos = 0,
                OmitidosDuplicados = 0
            };

            if (request.FileBytes == null || request.FileBytes.Length == 0)
            {
                reply.Message = "El archivo enviado está vacío.";
                return reply;
            }

            int colegioId = request.ColegioId > 0 ? request.ColegioId : 1;

            try
            {
                using var stream = new MemoryStream(request.FileBytes.ToByteArray());
                using var reader = ExcelReaderFactory.CreateReader(stream);

                var dataset = reader.AsDataSet(new ExcelDataSetConfiguration()
                {
                    ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                    {
                        UseHeaderRow = true // La primera fila contiene encabezados
                    }
                });

                if (dataset.Tables.Count == 0 || dataset.Tables[0].Rows.Count == 0)
                {
                    reply.Message = "El archivo Excel no contiene hojas de datos o filas válidas.";
                    return reply;
                }

                var table = dataset.Tables[0];
                reply.TotalProcesados = table.Rows.Count;

                // Mapeo flexible de nombres de columnas
                var colMap = GetColumnMapping(table);

                int creados = 0;
                int duplicados = 0;
                var errores = new List<string>();

                for (int i = 0; i < table.Rows.Count; i++)
                {
                    var row = table.Rows[i];
                    try
                    {
                        string nombre = GetVal(row, colMap, "nombre");
                        string apellido = GetVal(row, colMap, "apellido");
                        string numDoc = GetVal(row, colMap, "numeroidentificacion", "documento", "identificacion", "cedula", "tarjeta");
                        string tipoDoc = GetVal(row, colMap, "tipoidentificacion", "tipodoc", "tipo");
                        string email = GetVal(row, colMap, "email", "correo");
                        string telefono = GetVal(row, colMap, "telefono", "celular");
                        string curso = GetVal(row, colMap, "curso", "grado");
                        string jornada = GetVal(row, colMap, "jornada");
                        string sexo = GetVal(row, colMap, "sexo", "genero");
                        string eps = GetVal(row, colMap, "eps");
                        string direccion = GetVal(row, colMap, "direccion");
                        string fechaNacStr = GetVal(row, colMap, "fechanacimiento", "nacimiento");

                        if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(numDoc))
                        {
                            errores.Add($"Fila {i + 2}: Nombre o número de identificación vacío.");
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(tipoDoc)) tipoDoc = "TI";
                        if (string.IsNullOrWhiteSpace(jornada)) jornada = "Mañana";
                        if (string.IsNullOrWhiteSpace(curso)) curso = "Sin Asignar";

                        // Verificar si ya existe el usuario por número de identificación o username
                        string username = numDoc.Trim();
                        bool existe = await _dbContext.Usuarios
                            .AnyAsync(u => u.NumeroIdentificacion == numDoc || u.Username == username);

                        if (existe)
                        {
                            duplicados++;
                            continue;
                        }

                        DateTime fechaNacimiento = DateTime.UtcNow.AddYears(-15);
                        if (!string.IsNullOrWhiteSpace(fechaNacStr) && DateTime.TryParse(fechaNacStr, out var parsedDate))
                        {
                            fechaNacimiento = parsedDate;
                        }

                        // Crear Usuario
                        var usuario = new Usuario
                        {
                            ColegioId = colegioId,
                            Username = username,
                            PasswordHash = numDoc.Trim(), // Contraseña por defecto es el número de documento
                            Nombre = nombre.Trim(),
                            Apellido = apellido?.Trim() ?? "",
                            Email = !string.IsNullOrWhiteSpace(email) ? email.Trim() : $"{username}@estudiante.siae.edu.co",
                            Telefono = telefono?.Trim() ?? "",
                            TipoIdentificacion = tipoDoc.Trim(),
                            NumeroIdentificacion = numDoc.Trim(),
                            Rol = "Estudiante",
                            Jornada = jornada.Trim()
                        };

                        _dbContext.Usuarios.Add(usuario);
                        await _dbContext.SaveChangesAsync();

                        // Crear Estudiante vinculado
                        var estudiante = new Estudiante
                        {
                            ColegioId = colegioId,
                            UsuarioId = usuario.Id,
                            Curso = curso.Trim(),
                            LugarNacimiento = "No registrado",
                            FechaNacimiento = fechaNacimiento,
                            Sexo = sexo?.Trim() ?? "No especificado",
                            Eps = eps?.Trim() ?? "Sin asignación",
                            Direccion = direccion?.Trim() ?? "No registrada"
                        };

                        _dbContext.Estudiantes.Add(estudiante);
                        await _dbContext.SaveChangesAsync();

                        creados++;
                    }
                    catch (Exception ex)
                    {
                        errores.Add($"Fila {i + 2}: Error al procesar ({ex.Message}).");
                    }
                }

                reply.Success = true;
                reply.CreadosExitosos = creados;
                reply.OmitidosDuplicados = duplicados;
                reply.Errores.AddRange(errores);
                reply.Message = $"Carga finalizada: {creados} estudiantes creados exitosamente, {duplicados} omitidos por estar duplicados.";

                return reply;
            }
            catch (Exception ex)
            {
                reply.Success = false;
                reply.Message = $"Error al leer el archivo Excel: {ex.Message}";
                return reply;
            }
        }

        public override async Task<EstudianteListReply> GetEstudiantes(EstudianteListRequest request, ServerCallContext context)
        {
            int colegioId = request.ColegioId > 0 ? request.ColegioId : 1;

            IQueryable<Estudiante> query = _dbContext.Estudiantes
                .Include(e => e.Usuario)
                .Where(e => e.ColegioId == colegioId);

            if (!string.IsNullOrWhiteSpace(request.Jornada))
            {
                query = query.Where(e => e.Usuario.Jornada == request.Jornada);
            }

            if (!string.IsNullOrWhiteSpace(request.Curso))
            {
                query = query.Where(e => e.Curso == request.Curso);
            }

            var lista = await query.ToListAsync();
            var reply = new EstudianteListReply();

            foreach (var e in lista)
            {
                reply.Estudiantes.Add(new EstudianteMessage
                {
                    Id = e.Id,
                    UsuarioId = e.UsuarioId,
                    Nombre = e.Usuario.Nombre,
                    Apellido = e.Usuario.Apellido,
                    TipoIdentificacion = e.Usuario.TipoIdentificacion,
                    NumeroIdentificacion = e.Usuario.NumeroIdentificacion,
                    Email = e.Usuario.Email,
                    Telefono = e.Usuario.Telefono,
                    Curso = e.Curso,
                    Jornada = e.Usuario.Jornada,
                    Sexo = e.Sexo,
                    Eps = e.Eps,
                    Direccion = e.Direccion
                });
            }

            return reply;
        }

        private Dictionary<string, int> GetColumnMapping(System.Data.DataTable table)
        {
            var map = new Dictionary<string, int>();
            for (int i = 0; i < table.Columns.Count; i++)
            {
                string colName = table.Columns[i].ColumnName.ToLowerInvariant().Replace(" ", "").Replace("_", "").Replace("-", "");
                if (!map.ContainsKey(colName))
                {
                    map[colName] = i;
                }
            }
            return map;
        }

        private string GetVal(System.Data.DataRow row, Dictionary<string, int> map, params string[] possibleKeys)
        {
            foreach (var key in possibleKeys)
            {
                string cleanKey = key.ToLowerInvariant().Replace(" ", "").Replace("_", "").Replace("-", "");
                if (map.TryGetValue(cleanKey, out int colIdx))
                {
                    var val = row[colIdx];
                    if (val != DBNull.Value && val != null)
                    {
                        return val.ToString() ?? "";
                    }
                }
            }
            return "";
        }
    }
}
