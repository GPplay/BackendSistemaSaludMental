using System.Text;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

// Registrar soporte de páginas de código (requerido por ExcelDataReader)
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var builder = WebApplication.CreateBuilder(args);

// Configurar Entity Framework con SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar Controladores REST
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configurar Swagger UI con soporte para JWT Bearer
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SIAE REST API",
        Version = "v1",
        Description = "API RESTful con autenticación JWT para el Sistema de Atención Integral Escolar (SIAE)"
    });

    // Definición de esquema de seguridad JWT Bearer
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresa el token JWT en el formato: Bearer {tu_token_aqui}"
    });

    options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", doc),
            new List<string>()
        }
    });
});

// Configurar Autenticación basada en JWT
var key = Encoding.ASCII.GetBytes(JwtUtils.Secret);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Permitir desarrollo HTTP local
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = "SIAE_Backend",
        ValidateAudience = true,
        ValidAudience = "SIAE_Frontend",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Configurar CORS para permitir Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding");
    });
});



var app = builder.Build();

// Asegurar la creación de la base de datos y la inserción de datos iniciales con reintentos para soportar el arranque lento en Docker
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    int retries = 6;
    while (retries > 0)
    {
        try
        {
            dbContext.Database.EnsureCreated();
            break;
        }
        catch (Exception ex)
        {
            retries--;
            Console.WriteLine($"[WARNING] No se pudo conectar a la base de datos. Reintentando en 5 segundos... ({retries} intentos restantes). Error: {ex.Message}");
            if (retries == 0)
            {
                Console.WriteLine("[ERROR] Se agotaron los intentos de conexión. Deteniendo el servidor.");
                throw;
            }
            Thread.Sleep(5000);
        }
    }
}

app.UseRouting();
app.UseCors("AllowAngular");

// Habilitar Swagger UI
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "SIAE REST API v1");
    options.RoutePrefix = "swagger";
});

// Middleware de Autenticación y Autorización
app.UseAuthentication();
app.UseAuthorization();

// Mapear Controladores REST
app.MapControllers();



app.MapGet("/proto", async (context) =>
{
    var path = Path.Combine(app.Environment.ContentRootPath, "Protos", "encuestas.proto");
    if (File.Exists(path))
    {
        context.Response.ContentType = "text/plain; charset=utf-8";
        await context.Response.WriteAsync(await File.ReadAllTextAsync(path));
    }
    else
    {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsync("Archivo .proto no encontrado.");
    }
});

app.MapGet("/", async (context) =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    var html = @"<!DOCTYPE html>
<html lang=""es"">
<head>
    <meta charset=""UTF-8"">
    <title>Portal de Control SIAE Backend</title>
    <link href=""https://fonts.googleapis.com/css2?family=Outfit:wght@300;400;600;800&display=swap"" rel=""stylesheet"">
    <link href=""https://unpkg.com/boxicons@2.1.4/css/boxicons.min.css"" rel=""stylesheet"">
    <style>
        :root {
            --bg-color: #0a0a0f;
            --card-bg: rgba(255, 255, 255, 0.03);
            --border-glass: rgba(255, 255, 255, 0.08);
            --accent-purple: #8b5cf6;
            --accent-purple-glow: rgba(139, 92, 246, 0.15);
            --text-primary: #f3f4f6;
            --text-secondary: #9ca3af;
            --success: #10b981;
        }
        body {
            margin: 0;
            padding: 0;
            background-color: var(--bg-color);
            color: var(--text-primary);
            font-family: 'Outfit', sans-serif;
            display: flex;
            justify-content: center;
            align-items: center;
            min-height: 100vh;
            background-image: radial-gradient(circle at 10% 20%, rgba(90, 40, 200, 0.1) 0%, transparent 40%),
                              radial-gradient(circle at 90% 80%, rgba(30, 100, 220, 0.1) 0%, transparent 40%);
        }
        .portal-card {
            width: 100%;
            max-width: 600px;
            background: var(--card-bg);
            backdrop-filter: blur(16px);
            border: 1px solid var(--border-glass);
            border-radius: 20px;
            padding: 2.5rem;
            box-shadow: 0 20px 40px rgba(0, 0, 0, 0.5);
            text-align: center;
        }
        .logo-icon {
            font-size: 3.5rem;
            color: var(--accent-purple);
            margin-bottom: 1rem;
            display: inline-block;
            text-shadow: 0 0 20px rgba(139, 92, 246, 0.5);
        }
        h1 {
            margin: 0 0 0.5rem 0;
            font-weight: 800;
            font-size: 2rem;
            letter-spacing: -0.025em;
        }
        .subtitle {
            color: var(--text-secondary);
            font-size: 1.1rem;
            margin-bottom: 2rem;
        }
        .status-grid {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 1.25rem;
            margin-bottom: 2.5rem;
        }
        .status-item {
            background: rgba(255, 255, 255, 0.02);
            border: 1px solid var(--border-glass);
            border-radius: 12px;
            padding: 1rem;
            text-align: left;
        }
        .status-label {
            font-size: 0.8rem;
            color: var(--text-secondary);
            text-transform: uppercase;
            letter-spacing: 0.05em;
            margin-bottom: 0.25rem;
        }
        .status-value {
            font-weight: 600;
            font-size: 1rem;
            display: flex;
            align-items: center;
            gap: 0.5rem;
        }
        .status-indicator {
            width: 8px;
            height: 8px;
            border-radius: 50%;
            background-color: var(--success);
            box-shadow: 0 0 8px var(--success);
        }
        .services-list {
            text-align: left;
            background: rgba(0, 0, 0, 0.2);
            border: 1px solid var(--border-glass);
            border-radius: 12px;
            padding: 1.25rem;
            margin-bottom: 2rem;
        }
        .services-title {
            font-size: 0.9rem;
            font-weight: 600;
            color: var(--accent-purple);
            margin-bottom: 0.75rem;
            display: flex;
            align-items: center;
            gap: 0.5rem;
        }
        .service-badge {
            display: inline-block;
            background: rgba(255, 255, 255, 0.05);
            border: 1px solid var(--border-glass);
            padding: 0.35rem 0.75rem;
            border-radius: 20px;
            font-size: 0.8rem;
            margin: 0.25rem;
            color: var(--text-primary);
        }
        .btn-proto {
            display: inline-flex;
            align-items: center;
            gap: 0.5rem;
            background: var(--accent-purple);
            color: white;
            border: none;
            padding: 0.8rem 1.8rem;
            border-radius: 10px;
            font-size: 0.95rem;
            font-weight: 600;
            text-decoration: none;
            cursor: pointer;
            transition: all 0.3s ease;
            box-shadow: 0 4px 15px rgba(139, 92, 246, 0.4);
        }
        .btn-proto:hover {
            transform: translateY(-2px);
            box-shadow: 0 6px 20px rgba(139, 92, 246, 0.6);
        }
    </style>
</head>
<body>
    <div class=""portal-card"">
        <i class=""bx bx-cube-alt logo-icon""></i>
        <h1>SIAE Backend</h1>
        <p class=""subtitle"">API RESTful con JWT para Salud Mental Escolar</p>
        
        <div class=""status-grid"">
            <div class=""status-item"">
                <div class=""status-label"">Servidor REST</div>
                <div class=""status-value"">
                    <span class=""status-indicator""></span> Activo
                </div>
            </div>
            <div class=""status-item"">
                <div class=""status-label"">Motor de API</div>
                <div class=""status-value"">.NET 10.0</div>
            </div>
            <div class=""status-item"">
                <div class=""status-label"">Autenticación</div>
                <div class=""status-value"">JWT Bearer</div>
            </div>
            <div class=""status-item"">
                <div class=""status-label"">Base de Datos</div>
                <div class=""status-value"">SQL Server (SaaS)</div>
            </div>
        </div>

        <div class=""services-list"">
            <div class=""services-title"">
                <i class=""bx bx-git-branch""></i> Controladores REST Activos
            </div>
            <div>
                <span class=""service-badge"">AuthController</span>
                <span class=""service-badge"">DashboardController</span>
                <span class=""service-badge"">CasosController</span>
                <span class=""service-badge"">EstudianteController</span>
                <span class=""service-badge"">EncuestaController</span>
                <span class=""service-badge"">AdminController</span>
            </div>
        </div>

        <a href=""/swagger"" target=""_blank"" class=""btn-proto"">
            <i class=""bx bx-code-block""></i> Abrir Swagger UI (API Docs)
        </a>
    </div>
</body>
</html>";
    await context.Response.WriteAsync(html);
});

app.Run();
