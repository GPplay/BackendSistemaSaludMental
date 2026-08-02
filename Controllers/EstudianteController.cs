using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ExcelDataReader;
using Backend.Data;
using Backend.Models;
using Backend.Services;

namespace Backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EstudianteController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public EstudianteController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetEstudiantes([FromQuery] string? jornada, [FromQuery] string? curso)
        {
            var colegioIdClaim = ClaimHelper.GetColegioId(User);
            if (colegioIdClaim == null)
            {
                return Unauthorized(new { success = false, message = "Token JWT inválido o incompleto." });
            }

            int colegioId = int.Parse(colegioIdClaim);

            IQueryable<Estudiante> query = _dbContext.Estudiantes
                .Include(e => e.Usuario)
                .ThenInclude(u => u.Persona)
                .Where(e => e.ColegioId == colegioId);

            if (!string.IsNullOrWhiteSpace(jornada))
            {
                query = query.Where(e => e.Usuario.Jornada == jornada);
            }

            if (!string.IsNullOrWhiteSpace(curso) && !string.Equals(curso, "Todos", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(e => e.Curso == curso);
            }

            var lista = await query.ToListAsync();

            var result = lista.Select(e => new
            {
                id = e.Id,
                usuarioId = e.UsuarioId,
                nombre = e.Usuario.Nombre,
                apellido = e.Usuario.Apellido,
                tipoIdentificacion = e.Usuario.TipoIdentificacion,
                numeroIdentificacion = e.Usuario.NumeroIdentificacion,
                email = e.Usuario.Email,
                telefono = e.Usuario.Telefono,
                curso = e.Curso,
                jornada = e.Usuario.Jornada,
                sexo = e.Sexo,
                eps = e.Eps,
                direccion = e.Direccion
            });

            return Ok(result);
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadEstudiantesExcel(IFormFile file)
        {
            var colegioIdClaim = ClaimHelper.GetColegioId(User);
            if (colegioIdClaim == null)
            {
                return Unauthorized(new { success = false, message = "Token JWT inválido o incompleto." });
            }

            int colegioId = int.Parse(colegioIdClaim);

            if (file == null || file.Length == 0)
            {
                return BadRequest(new { success = false, message = "El archivo enviado está vacío o es nulo." });
            }

            try
            {
                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0;

                using var reader = ExcelReaderFactory.CreateReader(stream);

                var dataset = reader.AsDataSet(new ExcelDataSetConfiguration()
                {
                    ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                    {
                        UseHeaderRow = true
                    }
                });

                if (dataset.Tables.Count == 0 || dataset.Tables[0].Rows.Count == 0)
                {
                    return BadRequest(new { success = false, message = "El archivo Excel no contiene hojas de datos o filas válidas." });
                }

                var table = dataset.Tables[0];
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
                        string telefonoEstudiante = GetVal(row, colMap, "telefonoestudiante", "telefono", "celular");
                        string nombreAcudiente = GetVal(row, colMap, "nombreacudiente", "acudiente", "padre", "madre");
                        string telefonoAcudiente = GetVal(row, colMap, "telefonoacudiente", "celularacudiente", "contactopadre");
                        string parentescoAcudiente = GetVal(row, colMap, "parentescoacudiente", "parentesco");
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

                        string normalizedCurso = curso.Trim().Replace(" ", "").ToUpperInvariant();
                        bool cursoExiste = await _dbContext.Cursos.AnyAsync(c => c.ColegioId == colegioId && c.Nombre == normalizedCurso);
                        if (!cursoExiste)
                        {
                            var nuevoCurso = new Curso
                            {
                                ColegioId = colegioId,
                                Nombre = normalizedCurso
                            };
                            _dbContext.Cursos.Add(nuevoCurso);
                            await _dbContext.SaveChangesAsync();
                        }

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

                        var persona = new Persona
                        {
                            Nombre = nombre.Trim(),
                            Apellido = apellido?.Trim() ?? "",
                            TipoIdentificacion = tipoDoc.Trim(),
                            NumeroIdentificacion = numDoc.Trim(),
                            Email = !string.IsNullOrWhiteSpace(email) ? email.Trim() : $"{username}@estudiante.siae.edu.co",
                            Telefono = telefonoEstudiante?.Trim() ?? "",
                            Sexo = sexo?.Trim() ?? "No especificado",
                            Direccion = direccion?.Trim() ?? "No registrada",
                            FechaNacimiento = fechaNacimiento,
                            LugarNacimiento = "No registrado"
                        };

                        _dbContext.Personas.Add(persona);
                        await _dbContext.SaveChangesAsync();

                        var usuario = new Usuario
                        {
                            ColegioId = colegioId,
                            PersonaId = persona.Id,
                            Username = username,
                            PasswordHash = PasswordHasher.HashPassword(numDoc.Trim()),
                            Rol = "Estudiante",
                            Jornada = jornada.Trim()
                        };

                        _dbContext.Usuarios.Add(usuario);
                        await _dbContext.SaveChangesAsync();

                        var estudiante = new Estudiante
                        {
                            ColegioId = colegioId,
                            UsuarioId = usuario.Id,
                            PersonaId = persona.Id,
                            Curso = normalizedCurso,
                            Eps = eps?.Trim() ?? "Sin asignación",
                            NombreAcudiente = nombreAcudiente?.Trim() ?? "No registrado",
                            TelefonoAcudiente = telefonoAcudiente?.Trim() ?? "No registrado",
                            ParentescoAcudiente = parentescoAcudiente?.Trim() ?? "Acudiente Legal"
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

                return Ok(new
                {
                    success = true,
                    totalProcesados = table.Rows.Count,
                    creadosExitosos = creados,
                    omitidosDuplicados = duplicados,
                    errores = errores,
                    message = $"Carga finalizada: {creados} estudiantes creados exitosamente, {duplicados} omitidos por estar duplicados."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error al leer el archivo Excel: {ex.Message}" });
            }
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
