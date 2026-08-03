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

                IExcelDataReader reader;
                if (file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase) || file.FileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                {
                    reader = ExcelReaderFactory.CreateCsvReader(stream, new ExcelReaderConfiguration()
                    {
                        FallbackEncoding = System.Text.Encoding.UTF8,
                        AutodetectSeparators = new char[] { ',', ';', '\t', '|' }
                    });
                }
                else
                {
                    reader = ExcelReaderFactory.CreateReader(stream);
                }

                using (reader)
                {
                    var dataset = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                        {
                            UseHeaderRow = true
                        }
                    });

                    if (dataset.Tables.Count == 0 || dataset.Tables[0].Rows.Count == 0)
                    {
                        return BadRequest(new { success = false, message = "El archivo no contiene hojas de datos o filas válidas." });
                    }

                    var table = dataset.Tables[0];
                    var colMap = GetColumnMapping(table);

                    // Detección especial si todo el CSV / Excel está concatenado en la Columna A (Separado por Comas o Punto y Coma)
                    bool isSingleColumnCsv = false;
                    var singleColumnHeaderMap = new Dictionary<string, int>();

                    if (table.Columns.Count == 1 || (!colMap.ContainsKey("nombre") && table.Columns.Count > 0 && (table.Columns[0].ColumnName.Contains(",") || table.Columns[0].ColumnName.Contains(";"))))
                    {
                        string firstColName = table.Columns[0].ColumnName;
                        if (firstColName.Contains(",") || firstColName.Contains(";"))
                        {
                            isSingleColumnCsv = true;
                            var headers = ParseCsvLine(firstColName);
                            for (int h = 0; h < headers.Count; h++)
                            {
                                string cleanH = headers[h].ToLowerInvariant().Replace(" ", "").Replace("_", "").Replace("-", "");
                                if (!singleColumnHeaderMap.ContainsKey(cleanH))
                                {
                                    singleColumnHeaderMap[cleanH] = h;
                                }
                            }
                        }
                    }

                    int creados = 0;
                    int duplicados = 0;
                    var errores = new List<string>();

                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        var row = table.Rows[i];
                        try
                        {
                            string nombre = "";
                            string apellido = "";
                            string numDoc = "";
                            string tipoDoc = "";
                            string email = "";
                            string telefonoEstudiante = "";
                            string nombreAcudiente = "";
                            string telefonoAcudiente = "";
                            string parentescoAcudiente = "";
                            string curso = "";
                            string jornada = "";
                            string sexo = "";
                            string eps = "";
                            string direccion = "";
                            string fechaNacStr = "";

                            if (isSingleColumnCsv)
                            {
                                string cellValue = row[0]?.ToString() ?? "";
                                var rowValues = ParseCsvLine(cellValue);

                                nombre = GetValFromList(rowValues, singleColumnHeaderMap, "nombre");
                                apellido = GetValFromList(rowValues, singleColumnHeaderMap, "apellido");
                                numDoc = GetValFromList(rowValues, singleColumnHeaderMap, "numeroidentificacion", "documento", "identificacion", "cedula", "tarjeta");
                                tipoDoc = GetValFromList(rowValues, singleColumnHeaderMap, "tipoidentificacion", "tipodoc", "tipo");
                                email = GetValFromList(rowValues, singleColumnHeaderMap, "email", "correo");
                                telefonoEstudiante = GetValFromList(rowValues, singleColumnHeaderMap, "telefonoestudiante", "telefono", "celular");
                                nombreAcudiente = GetValFromList(rowValues, singleColumnHeaderMap, "nombreacudiente", "acudiente", "padre", "madre");
                                telefonoAcudiente = GetValFromList(rowValues, singleColumnHeaderMap, "telefonoacudiente", "celularacudiente", "contactopadre");
                                parentescoAcudiente = GetValFromList(rowValues, singleColumnHeaderMap, "parentescoacudiente", "parentesco");
                                curso = GetValFromList(rowValues, singleColumnHeaderMap, "curso", "grado");
                                jornada = GetValFromList(rowValues, singleColumnHeaderMap, "jornada");
                                sexo = GetValFromList(rowValues, singleColumnHeaderMap, "sexo", "genero");
                                eps = GetValFromList(rowValues, singleColumnHeaderMap, "eps");
                                direccion = GetValFromList(rowValues, singleColumnHeaderMap, "direccion");
                                fechaNacStr = GetValFromList(rowValues, singleColumnHeaderMap, "fechanacimiento", "nacimiento");
                            }
                            else
                            {
                                nombre = GetVal(row, colMap, "nombre");
                                apellido = GetVal(row, colMap, "apellido");
                                numDoc = GetVal(row, colMap, "numeroidentificacion", "documento", "identificacion", "cedula", "tarjeta");
                                tipoDoc = GetVal(row, colMap, "tipoidentificacion", "tipodoc", "tipo");
                                email = GetVal(row, colMap, "email", "correo");
                                telefonoEstudiante = GetVal(row, colMap, "telefonoestudiante", "telefono", "celular");
                                nombreAcudiente = GetVal(row, colMap, "nombreacudiente", "acudiente", "padre", "madre");
                                telefonoAcudiente = GetVal(row, colMap, "telefonoacudiente", "celularacudiente", "contactopadre");
                                parentescoAcudiente = GetVal(row, colMap, "parentescoacudiente", "parentesco");
                                curso = GetVal(row, colMap, "curso", "grado");
                                jornada = GetVal(row, colMap, "jornada");
                                sexo = GetVal(row, colMap, "sexo", "genero");
                                eps = GetVal(row, colMap, "eps");
                                direccion = GetVal(row, colMap, "direccion");
                                fechaNacStr = GetVal(row, colMap, "fechanacimiento", "nacimiento");
                            }

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
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error al procesar el archivo: {ex.Message}" });
            }
        }

        [HttpPost("individual")]
        public async Task<IActionResult> CreateEstudianteIndividual([FromBody] CreateEstudianteIndividualDto dto)
        {
            var colegioIdClaim = ClaimHelper.GetColegioId(User);
            if (colegioIdClaim == null)
            {
                return Unauthorized(new { success = false, message = "Token JWT inválido o incompleto." });
            }

            int colegioId = int.Parse(colegioIdClaim);

            if (string.IsNullOrWhiteSpace(dto.Nombre) || string.IsNullOrWhiteSpace(dto.NumeroIdentificacion))
            {
                return BadRequest(new { success = false, message = "Nombre y número de identificación son obligatorios." });
            }

            string numDoc = dto.NumeroIdentificacion.Trim();
            string username = numDoc;

            bool existe = await _dbContext.Usuarios
                .AnyAsync(u => u.NumeroIdentificacion == numDoc || u.Username == username);

            if (existe)
            {
                return BadRequest(new { success = false, message = $"Ya existe un estudiante o usuario registrado con el documento '{numDoc}'." });
            }

            string curso = string.IsNullOrWhiteSpace(dto.Curso) ? "Sin Asignar" : dto.Curso.Trim();
            string normalizedCurso = curso.Replace(" ", "").ToUpperInvariant();

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

            DateTime fechaNacimiento = DateTime.UtcNow.AddYears(-15);
            if (!string.IsNullOrWhiteSpace(dto.FechaNacimiento) && DateTime.TryParse(dto.FechaNacimiento, out var parsedDate))
            {
                fechaNacimiento = parsedDate;
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var persona = new Persona
                {
                    Nombre = dto.Nombre.Trim(),
                    Apellido = dto.Apellido?.Trim() ?? "",
                    TipoIdentificacion = string.IsNullOrWhiteSpace(dto.TipoIdentificacion) ? "TI" : dto.TipoIdentificacion.Trim(),
                    NumeroIdentificacion = numDoc,
                    Email = !string.IsNullOrWhiteSpace(dto.Email) ? dto.Email.Trim() : $"{username}@estudiante.siae.edu.co",
                    Telefono = dto.Telefono?.Trim() ?? "",
                    Sexo = string.IsNullOrWhiteSpace(dto.Sexo) ? "Masculino" : dto.Sexo.Trim(),
                    Direccion = dto.Direccion?.Trim() ?? "No registrada",
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
                    PasswordHash = PasswordHasher.HashPassword(numDoc),
                    Rol = "Estudiante",
                    Jornada = string.IsNullOrWhiteSpace(dto.Jornada) ? "Mañana" : dto.Jornada.Trim()
                };

                _dbContext.Usuarios.Add(usuario);
                await _dbContext.SaveChangesAsync();

                var estudiante = new Estudiante
                {
                    ColegioId = colegioId,
                    UsuarioId = usuario.Id,
                    PersonaId = persona.Id,
                    Curso = normalizedCurso,
                    Eps = string.IsNullOrWhiteSpace(dto.Eps) ? "Sin asignación" : dto.Eps.Trim(),
                    NombreAcudiente = string.IsNullOrWhiteSpace(dto.NombreAcudiente) ? "No registrado" : dto.NombreAcudiente.Trim(),
                    TelefonoAcudiente = string.IsNullOrWhiteSpace(dto.TelefonoAcudiente) ? "No registrado" : dto.TelefonoAcudiente.Trim(),
                    ParentescoAcudiente = string.IsNullOrWhiteSpace(dto.ParentescoAcudiente) ? "Acudiente Legal" : dto.ParentescoAcudiente.Trim()
                };

                _dbContext.Estudiantes.Add(estudiante);
                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new
                {
                    success = true,
                    message = $"Estudiante '{persona.Nombre} {persona.Apellido}' matriculado exitosamente en el curso {normalizedCurso}.",
                    estudianteId = estudiante.Id
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { success = false, message = $"Error al registrar el estudiante: {ex.Message}" });
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

        private string GetValFromList(List<string> rowValues, Dictionary<string, int> map, params string[] possibleKeys)
        {
            foreach (var key in possibleKeys)
            {
                string cleanKey = key.ToLowerInvariant().Replace(" ", "").Replace("_", "").Replace("-", "");
                if (map.TryGetValue(cleanKey, out int colIdx) && colIdx < rowValues.Count)
                {
                    return rowValues[colIdx] ?? "";
                }
            }
            return "";
        }

        private static List<string> ParseCsvLine(string line)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(line)) return result;

            bool inQuotes = false;
            var currentToken = new System.Text.StringBuilder();
            char delimiter = line.Contains(';') && !line.Contains(',') ? ';' : ',';

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == delimiter && !inQuotes)
                {
                    result.Add(currentToken.ToString().Trim().Trim('"'));
                    currentToken.Clear();
                }
                else
                {
                    currentToken.Append(c);
                }
            }
            result.Add(currentToken.ToString().Trim().Trim('"'));
            return result;
        }
    }
}
