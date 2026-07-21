using System.Text;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Services;

// Registrar soporte de páginas de código (requerido por ExcelDataReader)
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var builder = WebApplication.CreateBuilder(args);

// Configurar Entity Framework con SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

// Configurar gRPC con límite de mensaje amplio para cargas masivas
builder.Services.AddGrpc(options =>
{
    options.MaxReceiveMessageSize = 16 * 1024 * 1024; // 16 MB para cargas de Excel
    options.MaxSendMessageSize = 16 * 1024 * 1024;
});

var app = builder.Build();

// Asegurar la creación de la base de datos y la inserción de datos iniciales
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated();
}

app.UseRouting();
app.UseCors("AllowAngular");

// Habilitar gRPC-Web middleware
app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });

// Registrar los endpoints de gRPC habilitados con gRPC-Web y CORS
app.MapGrpcService<AuthGrpcService>().EnableGrpcWeb().RequireCors("AllowAngular");
app.MapGrpcService<EncuestaGrpcService>().EnableGrpcWeb().RequireCors("AllowAngular");
app.MapGrpcService<CasosGrpcService>().EnableGrpcWeb().RequireCors("AllowAngular");
app.MapGrpcService<EstudianteGrpcService>().EnableGrpcWeb().RequireCors("AllowAngular");
app.MapGrpcService<DashboardGrpcService>().EnableGrpcWeb().RequireCors("AllowAngular");

app.MapGet("/", () => "API gRPC SaaS con soporte gRPC-Web para Encuestas de Salud Mental está corriendo.");

app.Run();
