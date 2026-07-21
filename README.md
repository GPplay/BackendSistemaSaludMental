# SIAE Backend - API gRPC Multi-Tenant (.NET 10)

Backend desarrollado en **.NET 10** con **gRPC** y soporte **gRPC-Web** para la plataforma de atención psicoemocional y encuestas de salud mental en instituciones educativas (**SIAE - Sistema de Atención Integral Escolar**).

---

## 🚀 Tecnologías Utilizadas

* **Framework:** .NET 10 (ASP.NET Core Web API)
* **Comunicación RPC:** gRPC + `Grpc.AspNetCore.Web` (Soporte binario gRPC-Web para navegadores)
* **ORM:** Entity Framework Core 10 (Code-First)
* **Motor de Base de Datos:** Microsoft SQL Server
* **Procesamiento de Archivos:** `ExcelDataReader` (Para carga masiva de estudiantes desde Excel/CSV)

---

## 🗄️ Configuración de la Base de Datos

El backend se conecta a la instancia local de SQL Server mediante el usuario `Gybram3`:

* **Instancia:** `.\SQLEXPRESS`
* **Base de Datos:** `EncuestasSaludMental`
* **Usuario:** `Gybram3`
* **Contraseña:** `gybram1202`
* **Cadena de conexión (`appsettings.json`):**
  ```json
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=EncuestasSaludMental;User Id=Gybram3;Password=gybram1202;TrustServerCertificate=True;"
  }
  ```

*Al iniciar la aplicación con `dotnet run`, Entity Framework Core creará automáticamente las tablas y la semilla de datos si no existen.*

---

## 🏛️ Modelo de Dominio SaaS (Multi-Colegio)

* **`Colegio`**: Institución educativa (Tenant).
* **`Usuario`**: Perfiles con roles (`Rector`, `Orientador`, `Coordinador`, `Estudiante`).
* **`Estudiante`**: Expediente estudiantil vinculado a su cuenta de usuario y colegio.
* **`RegistroCaso`**: Seguimiento de alertas emocionales, convivencia, incidentes disciplinarios y planes PIAR/DUA.
* **`Encuesta`**, **`Pregunta`**, **`Opciones`**: Formularios dinámicos de salud mental y convivencia.
* **`RespuestaEncuesta`**, **`PreguntaRespuesta`**: Respuestas serializadas enviadas por los estudiantes.

---

## ⚡ Servicios gRPC Definidos (`Protos/encuestas.proto`)

1. **`AuthService`**: Autenticación e inicio de sesión por rol.
2. **`EstudianteService`**: 
   * `UploadEstudiantesExcel`: Carga masiva de estudiantes desde Excel con generación automática de cuentas de usuario.
   * `GetEstudiantes`: Consulta de estudiantes matriculados por colegio, curso y jornada.
3. **`EncuestaService`**: Creación de cuestionarios, consulta de listados y envío de respuestas.
4. **`CasosService`**: Registro, consulta y actualización de estado de casos psicoemocionales (Iniciado, EnProceso, Cerrado).
5. **`DashboardService`**: Métricas consolidadas en tiempo real para paneles administrativos y de orientadores.

---

## 📋 Formato Esperado para Carga Masiva en Excel

Para utilizar la función `UploadEstudiantesExcel`, el archivo `.xlsx` o `.csv` debe incluir los siguientes encabezados en la primera fila:

| Encabezado | Descripción | Ejemplo |
|---|---|---|
| `Nombre` | Nombres del estudiante | Juan Carlos |
| `Apellido` | Apellidos del estudiante | Pérez Gómez |
| `NumeroIdentificacion` | Documento de identidad (Se asigna como Usuario y Contraseña) | 1098765432 |
| `TipoIdentificacion` | TI, CC, RC, CE | TI |
| `Curso` | Grado escolar | 10-A |
| `Jornada` | Mañana, Tarde, Noche | Tarde |
| `Email` | Correo electrónico | juan@estudiante.edu.co |
| `Telefono` | Teléfono de contacto | 3001234567 |
| `Sexo` | Masculino, Femenino, Otro | Masculino |
| `EPS` | Coosalud, Sura, etc. | Coosalud |
| `Direccion` | Dirección de residencia | Calle 15 # 4-20 |

---

## 🛠️ Ejecución Local

1. Restaurar y compilar el proyecto:
   ```bash
   dotnet build
   ```
2. Ejecutar la API backend:
   ```bash
   dotnet run
   ```
3. El servidor gRPC se iniciará en `http://localhost:5059` listo para recibir peticiones gRPC-Web desde Angular.
