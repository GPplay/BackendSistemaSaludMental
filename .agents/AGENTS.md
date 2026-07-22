# Reglas de Juego del Backend (.NET 10 gRPC + SQL Server)

1. **Framework:** .NET 10 C# con comunicación **gRPC** y **gRPC-Web**.
2. **Base de Datos:** SQL Server (`.\SQLEXPRESS`), Base de datos `EncuestasSaludMental`, Usuario `Gybram3` / `gybram1202`.
3. **Multi-Tenancy:** Filtrar siempre todas las entidades por `ColegioId`.
4. **Carga Masiva:** Procesamiento de Excel con `ExcelDataReader` registrando `CodePagesEncodingProvider.Instance`.
5. **Verificación:** Prohibido finalizar sin comprobar `dotnet build` con 0 errores.
