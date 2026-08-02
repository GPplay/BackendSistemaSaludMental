using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
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
    public class EncuestaController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public EncuestaController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetSurveys()
        {
            var colegioIdClaim = ClaimHelper.GetColegioId(User);
            if (colegioIdClaim == null)
            {
                return Unauthorized(new { success = false, message = "Token JWT inválido o incompleto." });
            }

            int colegioId = int.Parse(colegioIdClaim);

            var encuestas = await _dbContext.Encuestas
                .Where(e => e.ColegioId == colegioId)
                .ToListAsync();

            var result = new List<object>();
            foreach (var e in encuestas)
            {
                int totalResp = await _dbContext.RespuestasEncuestas.CountAsync(r => r.EncuestaId == e.Id);
                result.Add(new
                {
                    id = e.Id,
                    titulo = e.Titulo,
                    descripcion = e.Descripcion,
                    fechaCreacion = e.FechaCreacion.ToString("yyyy-MM-dd HH:mm:ss"),
                    totalRespuestas = totalResp
                });
            }

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSurveyDetails(int id)
        {
            var encuesta = await _dbContext.Encuestas
                .Include(e => e.Preguntas)
                .ThenInclude(p => p.Opciones)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (encuesta == null)
            {
                return NotFound(new { success = false, message = "Encuesta no encontrada." });
            }

            var result = new
            {
                id = encuesta.Id,
                titulo = encuesta.Titulo,
                descripcion = encuesta.Descripcion,
                preguntas = encuesta.Preguntas.Select(p => new
                {
                    id = p.Id,
                    texto = p.TextoPregunta,
                    tipoRespuesta = p.TipoRespuesta,
                    esObligatoria = p.EsObligatoria,
                    restriccion = p.Restriccion ?? "",
                    prefijoPais = p.PrefijoPais ?? "+57",
                    opciones = p.Opciones.Select(o => new
                    {
                        id = o.Id,
                        texto = o.TextoOpcion
                    }).ToList()
                }).ToList()
            };

            return Ok(result);
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitSurveyResponse([FromBody] SubmitResponseDto dto)
        {
            var userIdClaim = ClaimHelper.GetUserId(User);
            var roleClaim = ClaimHelper.GetRole(User);

            if (userIdClaim == null || roleClaim == null)
            {
                return Unauthorized(new { success = false, message = "Token JWT inválido o incompleto." });
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var respuesta = new RespuestaEncuesta
                {
                    EncuestaId = dto.EncuestaId,
                    EstudianteId = dto.EstudianteId,
                    FechaRespuesta = DateTime.UtcNow
                };

                _dbContext.RespuestasEncuestas.Add(respuesta);
                await _dbContext.SaveChangesAsync();

                foreach (var r in dto.Respuestas)
                {
                    var pr = new PreguntaRespuesta
                    {
                        RespuestaEncuestaId = respuesta.Id,
                        PreguntaId = r.PreguntaId,
                        RespuestaTexto = r.RespuestaTexto ?? ""
                    };

                    if (r.OpcionSeleccionadaId.HasValue && r.OpcionSeleccionadaId.Value > 0)
                    {
                        pr.OpcionSeleccionadaId = r.OpcionSeleccionadaId.Value;
                    }

                    _dbContext.PreguntasRespuestas.Add(pr);
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { success = true, message = "Respuestas enviadas con éxito." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { success = false, message = $"Error al enviar respuestas: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateSurvey([FromBody] CreateSurveyDto dto)
        {
            var roleClaim = ClaimHelper.GetRole(User);
            if (roleClaim == "SUPER_ADMIN")
            {
                return Forbid("El SUPER_ADMIN no tiene permitido crear encuestas para garantizar la transparencia de los datos.");
            }

            var userIdClaim = ClaimHelper.GetUserId(User);
            var colegioIdClaim = ClaimHelper.GetColegioId(User);

            if (userIdClaim == null || colegioIdClaim == null)
            {
                return Unauthorized(new { success = false, message = "Token JWT inválido o incompleto." });
            }

            int userId = int.Parse(userIdClaim);
            int colegioId = int.Parse(colegioIdClaim);

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var encuesta = new Encuesta
                {
                    ColegioId = colegioId,
                    Titulo = dto.Titulo,
                    Descripcion = dto.Descripcion,
                    CreadoPorUsuarioId = userId,
                    FechaCreacion = DateTime.UtcNow
                };

                _dbContext.Encuestas.Add(encuesta);
                await _dbContext.SaveChangesAsync();

                foreach (var p in dto.Preguntas)
                {
                    var pregunta = new Pregunta
                    {
                        EncuestaId = encuesta.Id,
                        TextoPregunta = p.Texto,
                        TipoRespuesta = p.TipoRespuesta,
                        EsObligatoria = p.EsObligatoria,
                        Restriccion = p.Restriccion ?? "",
                        PrefijoPais = string.IsNullOrWhiteSpace(p.PrefijoPais) ? "+57" : p.PrefijoPais
                    };

                    _dbContext.Preguntas.Add(pregunta);
                    await _dbContext.SaveChangesAsync();

                    if (p.TipoRespuesta == "SeleccionUnica" && p.Opciones != null)
                    {
                        foreach (var oText in p.Opciones)
                        {
                            var opcion = new Opcion
                            {
                                PreguntaId = pregunta.Id,
                                TextoOpcion = oText
                            };
                            _dbContext.Opciones.Add(opcion);
                        }
                    }
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    success = true,
                    message = "Encuesta creada con éxito.",
                    encuestaId = encuesta.Id
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { success = false, message = $"Error al crear encuesta: {ex.Message}" });
            }
        }

        [HttpGet("{id}/analytics")]
        public async Task<IActionResult> GetSurveyAnalytics(int id)
        {
            var encuesta = await _dbContext.Encuestas
                .Include(e => e.Preguntas)
                .ThenInclude(p => p.Opciones)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (encuesta == null)
            {
                return NotFound(new { success = false, message = "Encuesta no encontrada." });
            }

            int totalRespuestasEncuesta = await _dbContext.RespuestasEncuestas
                .CountAsync(r => r.EncuestaId == id);

            var preguntasAnalitica = new List<object>();

            foreach (var p in encuesta.Preguntas)
            {
                var estadisticasOpciones = new List<object>();
                var respuestasTexto = new List<string>();

                if (p.TipoRespuesta == "SeleccionUnica")
                {
                    int totalRespuestasPregunta = await _dbContext.PreguntasRespuestas
                        .CountAsync(pr => pr.PreguntaId == p.Id && pr.OpcionSeleccionadaId.HasValue);

                    foreach (var o in p.Opciones)
                    {
                        int votos = await _dbContext.PreguntasRespuestas
                            .CountAsync(pr => pr.PreguntaId == p.Id && pr.OpcionSeleccionadaId == o.Id);

                        double pct = totalRespuestasPregunta > 0 ? (double)votos / totalRespuestasPregunta * 100.0 : 0;

                        estadisticasOpciones.Add(new
                        {
                            opcionId = o.Id,
                            textoOpcion = o.TextoOpcion,
                            cantidadVotos = votos,
                            porcentaje = Math.Round(pct, 1)
                        });
                    }
                }
                else
                {
                    respuestasTexto = await _dbContext.PreguntasRespuestas
                        .Where(pr => pr.PreguntaId == p.Id && !string.IsNullOrWhiteSpace(pr.RespuestaTexto))
                        .Select(pr => pr.RespuestaTexto)
                        .ToListAsync();
                }

                preguntasAnalitica.Add(new
                {
                    preguntaId = p.Id,
                    textoPregunta = p.TextoPregunta,
                    tipoRespuesta = p.TipoRespuesta,
                    esObligatoria = p.EsObligatoria,
                    restriccion = p.Restriccion ?? "",
                    prefijoPais = p.PrefijoPais ?? "+57",
                    estadisticasOpciones = estadisticasOpciones,
                    respuestasTexto = respuestasTexto
                });
            }

            var result = new
            {
                encuestaId = encuesta.Id,
                titulo = encuesta.Titulo,
                descripcion = encuesta.Descripcion,
                totalRespuestas = totalRespuestasEncuesta,
                preguntasAnalitica = preguntasAnalitica
            };

            return Ok(result);
        }
    }

    public class SubmitResponseDto
    {
        public int EncuestaId { get; set; }
        public int EstudianteId { get; set; }
        public List<PreguntaRespuestaDto> Respuestas { get; set; } = new();
    }

    public class PreguntaRespuestaDto
    {
        public int PreguntaId { get; set; }
        public string? RespuestaTexto { get; set; }
        public int? OpcionSeleccionadaId { get; set; }
    }

    public class CreateSurveyDto
    {
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public List<CreatePreguntaDto> Preguntas { get; set; } = new();
    }

    public class CreatePreguntaDto
    {
        public string Texto { get; set; } = string.Empty;
        public string TipoRespuesta { get; set; } = string.Empty;
        public bool EsObligatoria { get; set; }
        public string? Restriccion { get; set; }
        public string? PrefijoPais { get; set; }
        public List<string>? Opciones { get; set; }
    }
}
