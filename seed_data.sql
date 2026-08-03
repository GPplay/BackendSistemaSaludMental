-- =========================================================================
-- SCRIPT DE SEMILLA OFICIAL (SEED DATA) - SISTEMA SIAE DE SALUD MENTAL
-- Instancia: SQL Server (.\SQLEXPRESS o Docker container)
-- Base de Datos: EncuestasSaludMental
-- Usuario Admin: Gybram3 / gybram1202
-- SuperAdmin App: superadmin / superadmin123
-- Arquitectura: Tabla 'Personas' (Abstracción de Identidad Humana)
-- =========================================================================

USE master;
GO

-- 1. Crear Login y Base de Datos si no existen
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'Gybram3')
BEGIN
    CREATE LOGIN Gybram3 WITH PASSWORD = 'gybram1202', DEFAULT_DATABASE = master, CHECK_POLICY = OFF;
    ALTER SERVER ROLE sysadmin ADD MEMBER Gybram3;
    PRINT 'Login Gybram3 creado con éxito y asignado a sysadmin.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'EncuestasSaludMental')
BEGIN
    CREATE DATABASE EncuestasSaludMental;
    PRINT 'Base de datos EncuestasSaludMental creada.';
END
GO

USE EncuestasSaludMental;
GO

-- 2. Inserción Semilla de Colegios (Verificando existencia de la tabla)
IF OBJECT_ID('Colegios', 'U') IS NOT NULL
BEGIN
    SET IDENTITY_INSERT Colegios ON;
    IF NOT EXISTS (SELECT * FROM Colegios WHERE Id = 1)
    BEGIN
        INSERT INTO Colegios (Id, Nombre, Nit, CodigoDane, Direccion, Telefono, EmailContacto, FechaRegistro, Activo)
        VALUES (1, 'Institución Educativa Cuarta Poza de Manga', '800123456-1', '113837000123', 'Turbaco, Bolívar', '(605) 6789012', 'contacto@cuartapozademanga.edu.co', '2026-01-01 00:00:00', 1);
    END
    SET IDENTITY_INSERT Colegios OFF;
END
GO

-- 3. Inserción Semilla de Personas (Verificando existencia de la tabla)
IF OBJECT_ID('Personas', 'U') IS NOT NULL
BEGIN
    SET IDENTITY_INSERT Personas ON;
    IF NOT EXISTS (SELECT * FROM Personas WHERE Id = 999)
        INSERT INTO Personas (Id, Nombre, Apellido, TipoIdentificacion, NumeroIdentificacion, Email, Telefono, Sexo, Direccion, FechaNacimiento, LugarNacimiento)
        VALUES (999, 'Gybram (SuperAdmin)', 'Llamas', 'CC', '0000000000', 'admin@siae.com', '3000000000', 'Masculino', 'Sede Central SIAE', NULL, '');

    IF NOT EXISTS (SELECT * FROM Personas WHERE Id = 1)
        INSERT INTO Personas (Id, Nombre, Apellido, TipoIdentificacion, NumeroIdentificacion, Email, Telefono, Sexo, Direccion, FechaNacimiento, LugarNacimiento)
        VALUES (1, 'Pedro', 'Pérez', 'CC', '73123456', 'rector@cuartapozademanga.edu.co', '3001112233', 'Masculino', 'Turbaco - Centro', NULL, '');

    IF NOT EXISTS (SELECT * FROM Personas WHERE Id = 2)
        INSERT INTO Personas (Id, Nombre, Apellido, TipoIdentificacion, NumeroIdentificacion, Email, Telefono, Sexo, Direccion, FechaNacimiento, LugarNacimiento)
        VALUES (2, 'Sofía', 'Rodríguez', 'CC', '45987654', 'orientacion@cuartapozademanga.edu.co', '3004445566', 'Femenino', 'Turbaco - El Carmen', NULL, '');

    IF NOT EXISTS (SELECT * FROM Personas WHERE Id = 3)
        INSERT INTO Personas (Id, Nombre, Apellido, TipoIdentificacion, NumeroIdentificacion, Email, Telefono, Sexo, Direccion, FechaNacimiento, LugarNacimiento)
        VALUES (3, 'Carlos', 'Sánchez', 'CC', '92456789', 'coordinacion@cuartapozademanga.edu.co', '3007778899', 'Masculino', 'Turbaco - La Granja', NULL, '');

    IF NOT EXISTS (SELECT * FROM Personas WHERE Id = 4)
        INSERT INTO Personas (Id, Nombre, Apellido, TipoIdentificacion, NumeroIdentificacion, Email, Telefono, Sexo, Direccion, FechaNacimiento, LugarNacimiento)
        VALUES (4, 'Gybram', 'Llamas', 'TI', '1098765432', 'gybram@estudiante.edu.co', '3101234567', 'Masculino', 'Turbaco - Sector Manga', '2009-05-12 00:00:00', 'Cartagena');

    IF NOT EXISTS (SELECT * FROM Personas WHERE Id = 5)
        INSERT INTO Personas (Id, Nombre, Apellido, TipoIdentificacion, NumeroIdentificacion, Email, Telefono, Sexo, Direccion, FechaNacimiento, LugarNacimiento)
        VALUES (5, 'María Fernanda', 'Gómez Ruiz', 'TI', '1045678901', 'maria.gomez@estudiante.edu.co', '3129876543', 'Femenino', 'Turbaco - Centro', '2010-08-20 00:00:00', 'Turbaco');

    IF NOT EXISTS (SELECT * FROM Personas WHERE Id = 6)
        INSERT INTO Personas (Id, Nombre, Apellido, TipoIdentificacion, NumeroIdentificacion, Email, Telefono, Sexo, Direccion, FechaNacimiento, LugarNacimiento)
        VALUES (6, 'Carlos Andrés', 'Ruiz Martínez', 'TI', '1076543210', 'carlos.ruiz@estudiante.edu.co', '3154567890', 'Masculino', 'Turbaco - La Granja', '2011-03-15 00:00:00', 'Turbaco');

    IF NOT EXISTS (SELECT * FROM Personas WHERE Id = 7)
        INSERT INTO Personas (Id, Nombre, Apellido, TipoIdentificacion, NumeroIdentificacion, Email, Telefono, Sexo, Direccion, FechaNacimiento, LugarNacimiento)
        VALUES (7, 'Valentina', 'Torres Blanco', 'TI', '1087654321', 'valentina.torres@estudiante.edu.co', '3186543210', 'Femenino', 'Turbaco - El Carmen', '2012-11-05 00:00:00', 'Cartagena');
    SET IDENTITY_INSERT Personas OFF;
END
GO

-- 4. Inserción Semilla de Usuarios (Verificando existencia de la tabla)
IF OBJECT_ID('Usuarios', 'U') IS NOT NULL
BEGIN
    SET IDENTITY_INSERT Usuarios ON;
    IF NOT EXISTS (SELECT * FROM Usuarios WHERE Id = 999)
        INSERT INTO Usuarios (Id, ColegioId, PersonaId, Username, PasswordHash, Rol, Jornada)
        VALUES (999, 1, 999, 'superadmin', 'superadmin123', 'SUPER_ADMIN', 'Global');

    IF NOT EXISTS (SELECT * FROM Usuarios WHERE Id = 1)
        INSERT INTO Usuarios (Id, ColegioId, PersonaId, Username, PasswordHash, Rol, Jornada)
        VALUES (1, 1, 1, 'rector', 'rector123', 'Rector', 'Mañana');

    IF NOT EXISTS (SELECT * FROM Usuarios WHERE Id = 2)
        INSERT INTO Usuarios (Id, ColegioId, PersonaId, Username, PasswordHash, Rol, Jornada)
        VALUES (2, 1, 2, 'orientador', 'orientador123', 'Orientador', 'Mañana');

    IF NOT EXISTS (SELECT * FROM Usuarios WHERE Id = 3)
        INSERT INTO Usuarios (Id, ColegioId, PersonaId, Username, PasswordHash, Rol, Jornada)
        VALUES (3, 1, 3, 'coordinador', 'coordinador123', 'Coordinador', 'Tarde');

    IF NOT EXISTS (SELECT * FROM Usuarios WHERE Id = 4)
        INSERT INTO Usuarios (Id, ColegioId, PersonaId, Username, PasswordHash, Rol, Jornada)
        VALUES (4, 1, 4, 'gybram', 'gybram123', 'Estudiante', 'Tarde');

    IF NOT EXISTS (SELECT * FROM Usuarios WHERE Id = 5)
        INSERT INTO Usuarios (Id, ColegioId, PersonaId, Username, PasswordHash, Rol, Jornada)
        VALUES (5, 1, 5, 'maria.gomez', '1045678901', 'Estudiante', 'Mañana');

    IF NOT EXISTS (SELECT * FROM Usuarios WHERE Id = 6)
        INSERT INTO Usuarios (Id, ColegioId, PersonaId, Username, PasswordHash, Rol, Jornada)
        VALUES (6, 1, 6, 'carlos.ruiz', '1076543210', 'Estudiante', 'Mañana');

    IF NOT EXISTS (SELECT * FROM Usuarios WHERE Id = 7)
        INSERT INTO Usuarios (Id, ColegioId, PersonaId, Username, PasswordHash, Rol, Jornada)
        VALUES (7, 1, 7, 'valentina.torres', '1087654321', 'Estudiante', 'Tarde');
    SET IDENTITY_INSERT Usuarios OFF;
END
GO

-- 5. Inserción Semilla de Estudiantes (Verificando existencia de la tabla)
IF OBJECT_ID('Estudiantes', 'U') IS NOT NULL
BEGIN
    SET IDENTITY_INSERT Estudiantes ON;
    IF NOT EXISTS (SELECT * FROM Estudiantes WHERE Id = 1)
        INSERT INTO Estudiantes (Id, ColegioId, UsuarioId, PersonaId, Curso, Eps)
        VALUES (1, 1, 4, 4, '11-A', 'Coosalud');

    IF NOT EXISTS (SELECT * FROM Estudiantes WHERE Id = 2)
        INSERT INTO Estudiantes (Id, ColegioId, UsuarioId, PersonaId, Curso, Eps)
        VALUES (2, 1, 5, 5, '10-B', 'Sura');

    IF NOT EXISTS (SELECT * FROM Estudiantes WHERE Id = 3)
        INSERT INTO Estudiantes (Id, ColegioId, UsuarioId, PersonaId, Curso, Eps)
        VALUES (3, 1, 6, 6, '9-C', 'Sanitas');

    IF NOT EXISTS (SELECT * FROM Estudiantes WHERE Id = 4)
        INSERT INTO Estudiantes (Id, ColegioId, UsuarioId, PersonaId, Curso, Eps)
        VALUES (4, 1, 7, 7, '8-A', 'Nueva EPS');
    SET IDENTITY_INSERT Estudiantes OFF;
END
GO

PRINT '=======================================================';
PRINT '  SEMILLA SQL CON TABLA PERSONAS EJECUTADA CON ÉXITO';
PRINT '=======================================================';
GO
