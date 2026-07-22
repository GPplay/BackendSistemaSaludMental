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
    public class EncuestaGrpcService : EncuestaService.EncuestaServiceBase
    {
        private readonly ApplicationDbContext _dbContext;

        public EncuestaGrpcService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public override async Task<SurveyListReply> GetSurveys(SurveyListRequest request, ServerCallContext context)
        {
            int colegioId = request.ColegioId > 0 ? request.ColegioId : 1;

            var encuestas = await _dbContext.Encuestas
                .Where(e => e.ColegioId == colegioId)
                .ToListAsync();

            var reply = new SurveyListReply();

            foreach (var e in encuestas)
            {
                int totalResp = await _dbContext.RespuestasEncuestas.CountAsync(r => r.EncuestaId == e.Id);

                reply.Encuestas.Add(new SurveyMessage
                {
                    Id = e.Id,
                    Titulo = e.Titulo,
                    Descripcion = e.Descripcion,
                    FechaCreacion = e.FechaCreacion.ToString("yyyy-MM-dd HH:mm:ss"),
                    TotalRespuestas = totalResp
                });
            }

            return reply;
        }

        public override async Task<SurveyDetailsReply> GetSurveyDetails(SurveyDetailsRequest request, ServerCallContext context)
        {
            var encuesta = await _dbContext.Encuestas
                .Include(e => e.Preguntas)
                .ThenInclude(p => p.Opciones)
                .FirstOrDefaultAsync(e => e.Id == request.EncuestaId);

            if (encuesta == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Encuesta no encontrada."));
            }

            var reply = new SurveyDetailsReply
            {
                Id = encuesta.Id,
                Titulo = encuesta.Titulo,
                Descripcion = encuesta.Descripcion
            };

            foreach (var p in encuesta.Preguntas)
            {
                var pregMsg = new PreguntaMessage
                {
                    Id = p.Id,
                    Texto = p.TextoPregunta,
                    TipoRespuesta = p.TipoRespuesta,
                    EsObligatoria = p.EsObligatoria,
                    Restriccion = p.Restriccion ?? "",
                    PrefijoPais = p.PrefijoPais ?? "+57"
                };

                foreach (var o in p.Opciones)
                {
                    pregMsg.Opciones.Add(new OpcionMessage
                    {
                        Id = o.Id,
                        Texto = o.TextoOpcion
                    });
                }

                reply.Preguntas.Add(pregMsg);
            }

            return reply;
        }

        public override async Task<SubmitResponseReply> SubmitSurveyResponse(SubmitResponseRequest request, ServerCallContext context)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var respuesta = new RespuestaEncuesta
                {
                    EncuestaId = request.EncuestaId,
                    EstudianteId = request.EstudianteId,
                    FechaRespuesta = DateTime.UtcNow
                };

                _dbContext.RespuestasEncuestas.Add(respuesta);
                await _dbContext.SaveChangesAsync();

                foreach (var r in request.Respuestas)
                {
                    var pr = new PreguntaRespuesta
                    {
                        RespuestaEncuestaId = respuesta.Id,
                        PreguntaId = r.PreguntaId,
                        RespuestaTexto = r.RespuestaTexto ?? ""
                    };

                    if (r.OpcionSeleccionadaId > 0)
                    {
                        pr.OpcionSeleccionadaId = r.OpcionSeleccionadaId;
                    }

                    _dbContext.PreguntasRespuestas.Add(pr);
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return new SubmitResponseReply
                {
                    Success = true,
                    Message = "Respuestas enviadas con éxito."
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new SubmitResponseReply
                {
                    Success = false,
                    Message = $"Error al enviar respuestas: {ex.Message}"
                };
            }
        }

        public override async Task<CreateSurveyReply> CreateSurvey(CreateSurveyRequest request, ServerCallContext context)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                int colegioId = request.ColegioId > 0 ? request.ColegioId : 1;

                var encuesta = new Encuesta
                {
                    ColegioId = colegioId,
                    Titulo = request.Titulo,
                    Descripcion = request.Descripcion,
                    CreadoPorUsuarioId = request.CreadoPorId,
                    FechaCreacion = DateTime.UtcNow
                };

                _dbContext.Encuestas.Add(encuesta);
                await _dbContext.SaveChangesAsync();

                foreach (var p in request.Preguntas)
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

                return new CreateSurveyReply
                {
                    Success = true,
                    Message = "Encuesta creada con éxito.",
                    EncuestaId = encuesta.Id
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new CreateSurveyReply
                {
                    Success = false,
                    Message = $"Error al crear encuesta: {ex.Message}"
                };
            }
        }

        public override async Task<SurveyAnalyticsReply> GetSurveyAnalytics(SurveyAnalyticsRequest request, ServerCallContext context)
        {
            var encuesta = await _dbContext.Encuestas
                .Include(e => e.Preguntas)
                .ThenInclude(p => p.Opciones)
                .FirstOrDefaultAsync(e => e.Id == request.EncuestaId);

            if (encuesta == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Encuesta no encontrada."));
            }

            int totalRespuestasEncuesta = await _dbContext.RespuestasEncuestas
                .CountAsync(r => r.EncuestaId == request.EncuestaId);

            var reply = new SurveyAnalyticsReply
            {
                EncuestaId = encuesta.Id,
                Titulo = encuesta.Titulo,
                Descripcion = encuesta.Descripcion,
                TotalRespuestas = totalRespuestasEncuesta
            };

            foreach (var p in encuesta.Preguntas)
            {
                var pAnalytics = new PreguntaAnalyticsMessage
                {
                    PreguntaId = p.Id,
                    TextoPregunta = p.TextoPregunta,
                    TipoRespuesta = p.TipoRespuesta,
                    EsObligatoria = p.EsObligatoria,
                    Restriccion = p.Restriccion ?? "",
                    PrefijoPais = p.PrefijoPais ?? "+57"
                };

                if (p.TipoRespuesta == "SeleccionUnica")
                {
                    int totalRespuestasPregunta = await _dbContext.PreguntasRespuestas
                        .CountAsync(pr => pr.PreguntaId == p.Id && pr.OpcionSeleccionadaId.HasValue);

                    foreach (var o in p.Opciones)
                    {
                        int votos = await _dbContext.PreguntasRespuestas
                            .CountAsync(pr => pr.PreguntaId == p.Id && pr.OpcionSeleccionadaId == o.Id);

                        double pct = totalRespuestasPregunta > 0 ? (double)votos / totalRespuestasPregunta * 100.0 : 0;

                        pAnalytics.EstadisticasOpciones.Add(new OpcionStatMessage
                        {
                            OpcionId = o.Id,
                            TextoOpcion = o.TextoOpcion,
                            CantidadVotos = votos,
                            Porcentaje = Math.Round(pct, 1)
                        });
                    }
                }
                else
                {
                    var respuestasTexto = await _dbContext.PreguntasRespuestas
                        .Where(pr => pr.PreguntaId == p.Id && !string.IsNullOrWhiteSpace(pr.RespuestaTexto))
                        .Select(pr => pr.RespuestaTexto)
                        .ToListAsync();

                    pAnalytics.RespuestasTexto.AddRange(respuestasTexto);
                }

                reply.PreguntasAnalitica.Add(pAnalytics);
            }

            return reply;
        }

        public override async Task StreamSurveyAnalytics(
            SurveyAnalyticsRequest request, 
            IServerStreamWriter<SurveyAnalyticsReply> responseStream, 
            ServerCallContext context)
        {
            while (!context.CancellationToken.IsCancellationRequested)
            {
                var reply = await GetSurveyAnalytics(request, context);
                await responseStream.WriteAsync(reply);
                await Task.Delay(2000, context.CancellationToken); // Transmisión gRPC en vivo cada 2 segundos
            }
        }
    }
}
