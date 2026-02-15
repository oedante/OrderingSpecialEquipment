-- =============================================
-- ПОЛНЫЙ СКРИПТ СОЗДАНИЯ БАЗЫ ДАННЫХ OrderingSpecialEquipment
-- ВЕРСИЯ 2.0 (со всеми новыми полями: блокировки, актировка, настройки)
-- =============================================

-- Создание базы данных (выполняется отдельно, вне транзакции)
-- CREATE DATABASE "OrderingSpecialEquipment";
-- \c "OrderingSpecialEquipment";

-- Установка кодировки и параметров
SET client_encoding = 'UTF8';
SET timezone = 'UTC';

-- =============================================
-- 1. ТАБЛИЦА ОТДЕЛОВ (Departments)
-- =============================================
CREATE TABLE IF NOT EXISTS "Departments" (
    "Key"        SERIAL          NOT NULL,
    "Id"         VARCHAR(10)     NOT NULL,
    "Name"       VARCHAR(100)    NOT NULL,
    "IsActive"   BOOLEAN         DEFAULT true NOT NULL,
    "CreatedAt"  TIMESTAMP       DEFAULT CURRENT_TIMESTAMP NOT NULL,
    
    CONSTRAINT "PK_Departments" PRIMARY KEY ("Id"),
    CONSTRAINT "UQ_Departments_Key" UNIQUE ("Key")
);

COMMENT ON TABLE "Departments" IS 'Таблица отделов';
COMMENT ON COLUMN "Departments"."Id" IS 'Уникальный идентификатор формата DE000001';
COMMENT ON COLUMN "Departments"."Name" IS 'Наименование отдела';
COMMENT ON COLUMN "Departments"."IsActive" IS 'Активен ли отдел';

-- =============================================
-- 2. ТАБЛИЦА ОРГАНИЗАЦИЙ-АРЕНДОДАТЕЛЕЙ (LessorOrganizations)
-- =============================================
CREATE TABLE IF NOT EXISTS "LessorOrganizations" (
    "Key"           SERIAL          NOT NULL,
    "Id"            VARCHAR(10)     NOT NULL,
    "Name"          VARCHAR(200)    NOT NULL,
    "INN"           VARCHAR(12)     NULL,
    "ContactPerson" VARCHAR(150)    NULL,
    "Phone"         VARCHAR(20)     NULL,
    "Email"         VARCHAR(100)    NULL,
    "Address"       VARCHAR(500)    NULL,
    "IsActive"      BOOLEAN         DEFAULT true NOT NULL,
    "CreatedAt"     TIMESTAMP       DEFAULT CURRENT_TIMESTAMP NOT NULL,
    
    CONSTRAINT "PK_LessorOrganizations" PRIMARY KEY ("Id"),
    CONSTRAINT "UQ_LessorOrganizations_Key" UNIQUE ("Key"),
    CONSTRAINT "UQ_LessorOrganizations_INN" UNIQUE ("INN")
);

COMMENT ON TABLE "LessorOrganizations" IS 'Организации-арендодатели техники';
COMMENT ON COLUMN "LessorOrganizations"."Id" IS 'Уникальный идентификатор формата LO000001';

-- =============================================
-- 3. ТАБЛИЦА ТЕХНИКИ (Equipments)
-- =============================================
CREATE TABLE IF NOT EXISTS "Equipments" (
    "Key"              SERIAL          NOT NULL,
    "Id"               VARCHAR(10)     NOT NULL,
    "Name"             VARCHAR(200)    NOT NULL,
    "Category"         VARCHAR(50)     NULL,
    "CanOrderMultiple" BOOLEAN         DEFAULT false NOT NULL,
    "HourlyCost"       DECIMAL(10,2)   NULL,
    "RequiresOperator" BOOLEAN         DEFAULT false NOT NULL,
    "Description"      VARCHAR(500)    NULL,
    "IsActive"         BOOLEAN         DEFAULT true NOT NULL,
    "CreatedAt"        TIMESTAMP       DEFAULT CURRENT_TIMESTAMP NOT NULL,
    
    CONSTRAINT "PK_Equipments" PRIMARY KEY ("Id"),
    CONSTRAINT "UQ_Equipments_Key" UNIQUE ("Key")
);

COMMENT ON TABLE "Equipments" IS 'Справочник техники и оборудования';
COMMENT ON COLUMN "Equipments"."Id" IS 'Уникальный идентификатор формата EQ000001';
COMMENT ON COLUMN "Equipments"."CanOrderMultiple" IS 'Можно ли заказать несколько единиц в одной заявке';
COMMENT ON COLUMN "Equipments"."RequiresOperator" IS 'Требуется ли оператор для работы';

-- =============================================
-- 4. ТАБЛИЦА ГОСНОМЕРОВ (LicensePlates)
-- =============================================
CREATE TABLE IF NOT EXISTS "LicensePlates" (
    "Key"                  SERIAL          NOT NULL,
    "Id"                   VARCHAR(10)     NOT NULL,
    "PlateNumber"          VARCHAR(20)     NOT NULL,
    "EquipmentId"          VARCHAR(10)     NOT NULL,
    "LessorOrganizationId" VARCHAR(10)     NOT NULL,
    "Brand"                VARCHAR(100)    NULL,
    "Year"                 INTEGER         NULL,
    "Capacity"             VARCHAR(50)     NULL,
    "VIN"                  VARCHAR(50)     NULL,
    "IsActive"             BOOLEAN         DEFAULT true NOT NULL,
    "CreatedAt"            TIMESTAMP       DEFAULT CURRENT_TIMESTAMP NOT NULL,
    
    CONSTRAINT "PK_LicensePlates" PRIMARY KEY ("Id"),
    CONSTRAINT "UQ_LicensePlates_Key" UNIQUE ("Key"),
    CONSTRAINT "UQ_LicensePlates_PlateNumber" UNIQUE ("PlateNumber"),
    CONSTRAINT "FK_LicensePlates_Equipments" FOREIGN KEY ("EquipmentId") 
        REFERENCES "Equipments" ("Id"),
    CONSTRAINT "FK_LicensePlates_LessorOrganizations" FOREIGN KEY ("LessorOrganizationId") 
        REFERENCES "LessorOrganizations" ("Id")
);

COMMENT ON TABLE "LicensePlates" IS 'Государственные номера техники';
COMMENT ON COLUMN "LicensePlates"."Id" IS 'Уникальный идентификатор формата LP000001';

-- =============================================
-- 5. ТАБЛИЦА ЗАВИСИМОСТЕЙ ТЕХНИКИ (EquipmentDependencies)
-- =============================================
CREATE TABLE IF NOT EXISTS "EquipmentDependencies" (
    "Key"                  SERIAL          NOT NULL,
    "MainEquipmentId"      VARCHAR(10)     NOT NULL,
    "DependentEquipmentId" VARCHAR(10)     NOT NULL,
    "RequiredCount"        INTEGER         DEFAULT 1 NOT NULL,
    "IsMandatory"          BOOLEAN         DEFAULT true NOT NULL,
    "Description"          VARCHAR(200)    NULL,
    "CreatedAt"            TIMESTAMP       DEFAULT CURRENT_TIMESTAMP NOT NULL,
    
    CONSTRAINT "PK_EquipmentDependencies" PRIMARY KEY ("Key"),
    CONSTRAINT "FK_EquipmentDependencies_MainEquipment" FOREIGN KEY ("MainEquipmentId") 
        REFERENCES "Equipments" ("Id"),
    CONSTRAINT "FK_EquipmentDependencies_DependentEquipment" FOREIGN KEY ("DependentEquipmentId") 
        REFERENCES "Equipments" ("Id"),
    CONSTRAINT "UQ_EquipmentDependencies_Main_Dependent" UNIQUE ("MainEquipmentId", "DependentEquipmentId"),
    CONSTRAINT "CHK_EquipmentDependencies_Different" CHECK ("MainEquipmentId" != "DependentEquipmentId")
);

COMMENT ON TABLE "EquipmentDependencies" IS 'Зависимости техники (например, для крана нужны стропальщики)';

-- =============================================
-- 6. ТАБЛИЦА ТРАНСПОРТНОЙ ПРОГРАММЫ (TransportProgram)
-- =============================================
CREATE TABLE IF NOT EXISTS "TransportProgram" (
    "Key"              SERIAL          NOT NULL,
    "DepartmentId"     VARCHAR(10)     NOT NULL,
    "Year"             INTEGER         NOT NULL,
    "EquipmentId"      VARCHAR(10)     NOT NULL,
    "HourlyCost"       DECIMAL(10,2)   NOT NULL,
    "JanuaryHours"     DECIMAL(8,2)    DEFAULT 0 NOT NULL,
    "FebruaryHours"    DECIMAL(8,2)    DEFAULT 0 NOT NULL,
    "MarchHours"       DECIMAL(8,2)    DEFAULT 0 NOT NULL,
    "AprilHours"       DECIMAL(8,2)    DEFAULT 0 NOT NULL,
    "MayHours"         DECIMAL(8,2)    DEFAULT 0 NOT NULL,
    "JuneHours"        DECIMAL(8,2)    DEFAULT 0 NOT NULL,
    "JulyHours"        DECIMAL(8,2)    DEFAULT 0 NOT NULL,
    "AugustHours"      DECIMAL(8,2)    DEFAULT 0 NOT NULL,
    "SeptemberHours"   DECIMAL(8,2)    DEFAULT 0 NOT NULL,
    "OctoberHours"     DECIMAL(8,2)    DEFAULT 0 NOT NULL,
    "NovemberHours"    DECIMAL(8,2)    DEFAULT 0 NOT NULL,
    "DecemberHours"    DECIMAL(8,2)    DEFAULT 0 NOT NULL,
    "TotalYearHours"   DECIMAL(10,2)   GENERATED ALWAYS AS (
        "JanuaryHours" + "FebruaryHours" + "MarchHours" + 
        "AprilHours" + "MayHours" + "JuneHours" + "JulyHours" + 
        "AugustHours" + "SeptemberHours" + "OctoberHours" + 
        "NovemberHours" + "DecemberHours"
    ) STORED,
    "TotalYearCost"    DECIMAL(12,2)   GENERATED ALWAYS AS (
        ("JanuaryHours" + "FebruaryHours" + "MarchHours" + 
         "AprilHours" + "MayHours" + "JuneHours" + "JulyHours" + 
         "AugustHours" + "SeptemberHours" + "OctoberHours" + 
         "NovemberHours" + "DecemberHours") * "HourlyCost"
    ) STORED,
    "CreatedAt"        TIMESTAMP       DEFAULT CURRENT_TIMESTAMP NOT NULL,
    
    CONSTRAINT "PK_TransportProgram" PRIMARY KEY ("Key"),
    CONSTRAINT "UQ_TransportProgram_Dept_Year_Equipment" UNIQUE ("DepartmentId", "Year", "EquipmentId"),
    CONSTRAINT "FK_TransportProgram_Departments" FOREIGN KEY ("DepartmentId") 
        REFERENCES "Departments" ("Id"),
    CONSTRAINT "FK_TransportProgram_Equipments" FOREIGN KEY ("EquipmentId") 
        REFERENCES "Equipments" ("Id"),
    CONSTRAINT "CHK_TransportProgram_Year" CHECK ("Year" >= 2020 AND "Year" <= 2100),
    CONSTRAINT "CHK_TransportProgram_Hours" CHECK (
        "JanuaryHours" >= 0 AND "FebruaryHours" >= 0 AND "MarchHours" >= 0 AND
        "AprilHours" >= 0 AND "MayHours" >= 0 AND "JuneHours" >= 0 AND
        "JulyHours" >= 0 AND "AugustHours" >= 0 AND "SeptemberHours" >= 0 AND
        "OctoberHours" >= 0 AND "NovemberHours" >= 0 AND "DecemberHours" >= 0
    )
);

COMMENT ON TABLE "TransportProgram" IS 'Плановые часы работы техники по отделам на год';

-- =============================================
-- 7. ТАБЛИЦА РОЛЕЙ (Roles)
-- =============================================
CREATE TABLE IF NOT EXISTS "Roles" (
    "Key"                        SERIAL          NOT NULL,
    "Id"                         VARCHAR(10)     NOT NULL,
    "Name"                       VARCHAR(50)     NOT NULL,
    "Code"                       VARCHAR(20)     NOT NULL,
    "Description"                VARCHAR(200)    NULL,
    
    -- Права доступа к таблицам (0-нет, 1-чтение, 2-запись)
    "TAB_AuditLogs"              SMALLINT        DEFAULT 0 NOT NULL,
    "TAB_Departments"            SMALLINT        DEFAULT 0 NOT NULL,
    "TAB_EquipmentDependencies"  SMALLINT        DEFAULT 0 NOT NULL,
    "TAB_Equipments"             SMALLINT        DEFAULT 0 NOT NULL,
    "TAB_LessorOrganizations"    SMALLINT        DEFAULT 0 NOT NULL,
    "TAB_LicensePlates"          SMALLINT        DEFAULT 0 NOT NULL,
    "TAB_Roles"                  SMALLINT        DEFAULT 0 NOT NULL,
    "TAB_ShiftRequests"          SMALLINT        DEFAULT 0 NOT NULL,
    "TAB_TransportProgram"       SMALLINT        DEFAULT 0 NOT NULL,
    "TAB_UserDepartmentAccess"   SMALLINT        DEFAULT 0 NOT NULL,
    "TAB_UserFavorites"          SMALLINT        DEFAULT 0 NOT NULL,
    "TAB_Users"                  SMALLINT        DEFAULT 0 NOT NULL,
    "TAB_UserWarehouseAccess"    SMALLINT        DEFAULT 0 NOT NULL,
    "TAB_WarehouseAreas"         SMALLINT        DEFAULT 0 NOT NULL,
    "TAB_Warehouses"             SMALLINT        DEFAULT 0 NOT NULL,
    
    -- Специальные права
    "SPEC_ExportData"            BOOLEAN         DEFAULT false NOT NULL,
    "SPEC_ViewReports"           BOOLEAN         DEFAULT false NOT NULL,
    "SPEC_ManageAllDepartments"  BOOLEAN         DEFAULT false NOT NULL,
    "SPEC_ManageUsers"           BOOLEAN         DEFAULT false NOT NULL,
    "SPEC_SystemAdmin"           BOOLEAN         DEFAULT false NOT NULL,
    "SPEC_ConfigureConnection"   BOOLEAN         DEFAULT false NOT NULL, -- НОВОЕ: право настройки подключения к БД
    
    "IsSystem"                   BOOLEAN         DEFAULT false NOT NULL,
    "IsActive"                   BOOLEAN         DEFAULT true NOT NULL,
    "CreatedAt"                  TIMESTAMP       DEFAULT CURRENT_TIMESTAMP NOT NULL,
    
    CONSTRAINT "PK_Roles_Key" PRIMARY KEY ("Key"),
    CONSTRAINT "UQ_Roles_Id" UNIQUE ("Id"),
    CONSTRAINT "UQ_Roles_Code" UNIQUE ("Code"),
    CONSTRAINT "CHK_Roles_Permissions" CHECK (
        "TAB_AuditLogs" IN (0, 1, 2) AND
        "TAB_Departments" IN (0, 1, 2) AND
        "TAB_EquipmentDependencies" IN (0, 1, 2) AND
        "TAB_Equipments" IN (0, 1, 2) AND
        "TAB_LessorOrganizations" IN (0, 1, 2) AND
        "TAB_LicensePlates" IN (0, 1, 2) AND
        "TAB_Roles" IN (0, 1, 2) AND
        "TAB_ShiftRequests" IN (0, 1, 2) AND
        "TAB_TransportProgram" IN (0, 1, 2) AND
        "TAB_UserDepartmentAccess" IN (0, 1, 2) AND
        "TAB_UserFavorites" IN (0, 1, 2) AND
        "TAB_Users" IN (0, 1, 2) AND
        "TAB_UserWarehouseAccess" IN (0, 1, 2) AND
        "TAB_WarehouseAreas" IN (0, 1, 2) AND
        "TAB_Warehouses" IN (0, 1, 2)
    )
);

-- =============================================
-- 8. ТАБЛИЦА ПОЛЬЗОВАТЕЛЕЙ (Users)
-- =============================================
CREATE TABLE IF NOT EXISTS "Users" (
    "Key"                 SERIAL          NOT NULL,
    "Id"                  VARCHAR(10)     NOT NULL,
    "WindowsLogin"        VARCHAR(100)    NOT NULL,
    "FullName"            VARCHAR(150)    NOT NULL,
    "Email"               VARCHAR(100)    NULL,
    "Phone"               VARCHAR(20)     NULL,
    "RoleId"              VARCHAR(10)     NOT NULL,
    "DefaultDepartmentId" VARCHAR(10)     NULL,
    "HasAllDepartments"   BOOLEAN         DEFAULT false NOT NULL,
    "IsActive"            BOOLEAN         DEFAULT true NOT NULL,
    "CreatedAt"           TIMESTAMP       DEFAULT CURRENT_TIMESTAMP NOT NULL,
    
    CONSTRAINT "PK_Users" PRIMARY KEY ("Id"),
    CONSTRAINT "UQ_Users_Key" UNIQUE ("Key"),
    CONSTRAINT "UQ_Users_WindowsLogin" UNIQUE ("WindowsLogin"),
    CONSTRAINT "FK_Users_Roles" FOREIGN KEY ("RoleId") 
        REFERENCES "Roles" ("Id"),
    CONSTRAINT "FK_Users_Departments" FOREIGN KEY ("DefaultDepartmentId") 
        REFERENCES "Departments" ("Id")
);

COMMENT ON TABLE "Users" IS 'Пользователи системы (аутентификация по Windows логину)';
COMMENT ON COLUMN "Users"."WindowsLogin" IS 'Логин Windows без домена (нормализованный)';

-- =============================================
-- 9. ТАБЛИЦА ДОСТУПА К ОТДЕЛАМ (UserDepartmentAccess)
-- =============================================
CREATE TABLE IF NOT EXISTS "UserDepartmentAccess" (
    "Key"                SERIAL          NOT NULL,
    "UserId"             VARCHAR(10)     NOT NULL,
    "DepartmentId"       VARCHAR(10)     NOT NULL,
    "HasAllWarehouses"   BOOLEAN         DEFAULT false NOT NULL,
    "CreatedAt"          TIMESTAMP       DEFAULT CURRENT_TIMESTAMP NOT NULL,
    
    CONSTRAINT "PK_UserDepartmentAccess" PRIMARY KEY ("Key"),
    CONSTRAINT "UQ_UserDepartmentAccess_User_Dept" UNIQUE ("UserId", "DepartmentId"),
    CONSTRAINT "FK_UserDepartmentAccess_Users" FOREIGN KEY ("UserId") 
        REFERENCES "Users" ("Id"),
    CONSTRAINT "FK_UserDepartmentAccess_Departments" FOREIGN KEY ("DepartmentId") 
        REFERENCES "Departments" ("Id")
);

-- =============================================
-- 10. ТАБЛИЦА СКЛАДОВ (Warehouses)
-- =============================================
CREATE TABLE IF NOT EXISTS "Warehouses" (
    "Key"          SERIAL          NOT NULL,
    "Id"           VARCHAR(10)     NOT NULL,
    "Name"         VARCHAR(100)    NOT NULL,
    "DepartmentId" VARCHAR(10)     NOT NULL,
    "Address"      VARCHAR(500)    NULL,
    "IsActive"     BOOLEAN         DEFAULT true NOT NULL,
    "CreatedAt"    TIMESTAMP       DEFAULT CURRENT_TIMESTAMP NOT NULL,
    
    CONSTRAINT "PK_Warehouses" PRIMARY KEY ("Id"),
    CONSTRAINT "UQ_Warehouses_Key" UNIQUE ("Key"),
    CONSTRAINT "FK_Warehouses_Departments" FOREIGN KEY ("DepartmentId") 
        REFERENCES "Departments" ("Id")
);

-- =============================================
-- 11. ТАБЛИЦА ДОСТУПА К СКЛАДАМ (UserWarehouseAccess)
-- =============================================
CREATE TABLE IF NOT EXISTS "UserWarehouseAccess" (
    "Key"                      SERIAL          NOT NULL,
    "UserDepartmentAccessKey"  INTEGER         NOT NULL,
    "WarehouseId"              VARCHAR(10)     NOT NULL,
    "CreatedAt"                TIMESTAMP       DEFAULT CURRENT_TIMESTAMP NOT NULL,
    
    CONSTRAINT "PK_UserWarehouseAccess" PRIMARY KEY ("Key"),
    CONSTRAINT "UQ_UserWarehouseAccess_DeptAccess_Warehouse" UNIQUE ("UserDepartmentAccessKey", "WarehouseId"),
    CONSTRAINT "FK_UserWarehouseAccess_UserDepartmentAccess" FOREIGN KEY ("UserDepartmentAccessKey") 
        REFERENCES "UserDepartmentAccess" ("Key") ON DELETE CASCADE,
    CONSTRAINT "FK_UserWarehouseAccess_Warehouses" FOREIGN KEY ("WarehouseId") 
        REFERENCES "Warehouses" ("Id")
);

-- =============================================
-- 12. ТАБЛИЦА ТЕРРИТОРИЙ СКЛАДОВ (WarehouseAreas)
-- =============================================
CREATE TABLE IF NOT EXISTS "WarehouseAreas" (
    "Key"         SERIAL          NOT NULL,
    "Id"          VARCHAR(10)     NOT NULL,
    "Name"        VARCHAR(100)    NOT NULL,
    "WarehouseId" VARCHAR(10)     NOT NULL,
    "AreaType"    VARCHAR(50)     NULL,
    "MaxCapacity" INTEGER         NULL,
    "IsActive"    BOOLEAN         DEFAULT true NOT NULL,
    "CreatedAt"   TIMESTAMP       DEFAULT CURRENT_TIMESTAMP NOT NULL,
    
    CONSTRAINT "PK_WarehouseAreas" PRIMARY KEY ("Id"),
    CONSTRAINT "UQ_WarehouseAreas_Key" UNIQUE ("Key"),
    CONSTRAINT "FK_WarehouseAreas_Warehouses" FOREIGN KEY ("WarehouseId") 
        REFERENCES "Warehouses" ("Id") ON DELETE CASCADE
);

-- =============================================
-- 13. ТАБЛИЦА ЗАЯВОК (ShiftRequests) - ПОЛНАЯ ВЕРСИЯ СО ВСЕМИ НОВЫМИ ПОЛЯМИ
-- =============================================
CREATE TABLE IF NOT EXISTS "ShiftRequests" (
    "Key"                    SERIAL           NOT NULL,
    "Date"                   DATE             NOT NULL,
    "Shift"                  INTEGER          NOT NULL,
    "EquipmentId"            VARCHAR(10)      NOT NULL,
    "LicensePlateId"         VARCHAR(10)      NULL,
    "WarehouseId"            VARCHAR(10)      NOT NULL,
    "AreaId"                 VARCHAR(10)      NULL,
    "VehicleNumber"          VARCHAR(50)      NULL,
    "VehicleBrand"           VARCHAR(50)      NULL,
    "LessorOrganizationId"   VARCHAR(10)      NULL,
    "RequestedCount"         INTEGER          DEFAULT 1 NOT NULL,
    "WorkedHours"            DECIMAL(8,2)     NULL,
    "ActualCost"             DECIMAL(10,2)    NULL,
    "IsWorked"               BOOLEAN          DEFAULT false NOT NULL,
    "IsBlocked"              BOOLEAN          DEFAULT false NOT NULL,
    
    -- НОВЫЕ ПОЛЯ (ДОБАВЛЕНЫ)
    "IsNotProvided"          BOOLEAN          DEFAULT false NOT NULL,
    "IsWeatherCancellation"  BOOLEAN          DEFAULT false NOT NULL,
    "CancellationReason"     VARCHAR(200)     NULL,
    "LockedByUserId"         VARCHAR(10)      NULL,
    "LockedAt"              TIMESTAMP        NULL,
    
    "Comment"                TEXT             NULL,
    "CreatedByUserId"        VARCHAR(10)      NOT NULL,
    "CreatedAt"              TIMESTAMP        DEFAULT CURRENT_TIMESTAMP NOT NULL,
    "DepartmentId"           VARCHAR(10)      NULL,
    
    -- Эти поля УДАЛЯЕМ (как вы просили)
    -- "ProgramYear"         INTEGER          NULL,
    -- "ProgramMonth"        INTEGER          NULL,
    
    CONSTRAINT "PK_ShiftRequests" PRIMARY KEY ("Key"),
    CONSTRAINT "FK_ShiftRequests_Equipments" FOREIGN KEY ("EquipmentId") 
        REFERENCES "Equipments" ("Id"),
    CONSTRAINT "FK_ShiftRequests_LicensePlates" FOREIGN KEY ("LicensePlateId") 
        REFERENCES "LicensePlates" ("Id"),
    CONSTRAINT "FK_ShiftRequests_Warehouses" FOREIGN KEY ("WarehouseId") 
        REFERENCES "Warehouses" ("Id"),
    CONSTRAINT "FK_ShiftRequests_WarehouseAreas" FOREIGN KEY ("AreaId") 
        REFERENCES "WarehouseAreas" ("Id"),
    CONSTRAINT "FK_ShiftRequests_Users" FOREIGN KEY ("CreatedByUserId") 
        REFERENCES "Users" ("Id"),
    CONSTRAINT "FK_ShiftRequests_Departments" FOREIGN KEY ("DepartmentId") 
        REFERENCES "Departments" ("Id"),
    CONSTRAINT "FK_ShiftRequests_LessorOrganizations" FOREIGN KEY ("LessorOrganizationId") 
        REFERENCES "LessorOrganizations" ("Id"),
    CONSTRAINT "FK_ShiftRequests_LockedByUser" FOREIGN KEY ("LockedByUserId") 
        REFERENCES "Users" ("Id"),
    
    -- Проверка: если актировка или отказ, то часы = 0
    CONSTRAINT "CHK_ShiftRequests_CancellationHours" CHECK (
        (NOT ("IsNotProvided" = true OR "IsWeatherCancellation" = true)) OR 
        ("WorkedHours" = 0 OR "WorkedHours" IS NULL)
    ),
    
    CONSTRAINT "CHK_ShiftRequests_Shift" CHECK ("Shift" IN (0, 1))
);

COMMENT ON TABLE "ShiftRequests" IS 'Заявки на технику (основная таблица)';
COMMENT ON COLUMN "ShiftRequests"."Shift" IS '0 - дневная смена (07:30-18:30), 1 - ночная смена (19:30-06:30)';
COMMENT ON COLUMN "ShiftRequests"."IsNotProvided" IS 'Техника не была предоставлена арендодателем';
COMMENT ON COLUMN "ShiftRequests"."IsWeatherCancellation" IS 'Актировка (отмена по погодным условиям)';
COMMENT ON COLUMN "ShiftRequests"."LockedByUserId" IS 'ID пользователя, редактирующего запись';
COMMENT ON COLUMN "ShiftRequests"."LockedAt" IS 'Время начала блокировки записи';

-- =============================================
-- 14. ТАБЛИЦА ИЗБРАННОГО (UserFavorites)
-- =============================================
CREATE TABLE IF NOT EXISTS "UserFavorites" (
    "Key"         SERIAL          NOT NULL,
    "UserId"      VARCHAR(10)     NOT NULL,
    "EquipmentId" VARCHAR(10)     NOT NULL,
    "SortOrder"   INTEGER         DEFAULT 0 NOT NULL,
    "CreatedAt"   TIMESTAMP       DEFAULT CURRENT_TIMESTAMP NOT NULL,
    
    CONSTRAINT "PK_UserFavorites" PRIMARY KEY ("Key"),
    CONSTRAINT "UQ_UserFavorites_User_Equipment" UNIQUE ("UserId", "EquipmentId"),
    CONSTRAINT "FK_UserFavorites_Users" FOREIGN KEY ("UserId") 
        REFERENCES "Users" ("Id"),
    CONSTRAINT "FK_UserFavorites_Equipments" FOREIGN KEY ("EquipmentId") 
        REFERENCES "Equipments" ("Id")
);

-- =============================================
-- 15. ТАБЛИЦА НАСТРОЕК ПОЛЬЗОВАТЕЛЕЙ (UserSettings) - НОВАЯ
-- =============================================
CREATE TABLE IF NOT EXISTS "UserSettings" (
    "Key"         SERIAL          NOT NULL,
    "UserId"      VARCHAR(10)     NOT NULL,
    "SettingKey"  VARCHAR(50)     NOT NULL,
    "SettingValue" JSONB          NOT NULL,  -- Храним любые настройки в JSON формате
    "CreatedAt"   TIMESTAMP       DEFAULT CURRENT_TIMESTAMP NOT NULL,
    "UpdatedAt"   TIMESTAMP       DEFAULT CURRENT_TIMESTAMP NOT NULL,
    
    CONSTRAINT "PK_UserSettings" PRIMARY KEY ("Key"),
    CONSTRAINT "UQ_UserSettings_User_Key" UNIQUE ("UserId", "SettingKey"),
    CONSTRAINT "FK_UserSettings_Users" FOREIGN KEY ("UserId") 
        REFERENCES "Users" ("Id") ON DELETE CASCADE
);

COMMENT ON TABLE "UserSettings" IS 'Настройки пользователей (тема, горячие клавиши, etc)';

-- =============================================
-- 16. ТАБЛИЦА ЛОГОВ АУДИТА (AuditLogs)
-- =============================================
CREATE TABLE IF NOT EXISTS "AuditLogs" (
    "Key"             SERIAL          NOT NULL,
    "TableName"       VARCHAR(50)     NOT NULL,
    "RecordId"        VARCHAR(50)     NOT NULL,
    "Action"          VARCHAR(20)     NOT NULL,
    "OldValues"       JSONB           NULL,  -- JSON формат
    "NewValues"       JSONB           NULL,  -- JSON формат
    "ChangedByUserId" VARCHAR(10)     NOT NULL,
    "ChangedAt"       TIMESTAMP       DEFAULT CURRENT_TIMESTAMP NOT NULL,
    "IPAddress"       VARCHAR(50)     NULL,
    "UserAgent"       VARCHAR(500)    NULL,
    
    CONSTRAINT "PK_AuditLogs" PRIMARY KEY ("Key"),
    CONSTRAINT "FK_AuditLogs_Users" FOREIGN KEY ("ChangedByUserId") 
        REFERENCES "Users" ("Id")
);

-- =============================================
-- ФУНКЦИИ ДЛЯ ГЕНЕРАЦИИ ID (ТРИГГЕРЫ)
-- =============================================

-- Функция для генерации ID отделов
CREATE OR REPLACE FUNCTION trg_departments_generate_id()
RETURNS TRIGGER AS $$
DECLARE
    max_id INT;
BEGIN
    IF NEW."Id" IS NULL OR NEW."Id" = '' THEN
        SELECT COALESCE(MAX(CAST(substring("Id" from 3) AS INTEGER)), 0)
        INTO max_id
        FROM "Departments"
        WHERE "Id" LIKE 'DE%'
        AND substring("Id" from 3) ~ '^[0-9]+$';
        
        NEW."Id" := 'DE' || LPAD((max_id + 1)::TEXT, 6, '0');
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS "TR_Departments_GenerateId" ON "Departments";
CREATE TRIGGER "TR_Departments_GenerateId"
    BEFORE INSERT ON "Departments"
    FOR EACH ROW
    EXECUTE FUNCTION trg_departments_generate_id();

-- Функция для генерации ID организаций
CREATE OR REPLACE FUNCTION trg_lessororganizations_generate_id()
RETURNS TRIGGER AS $$
DECLARE
    max_id INT;
BEGIN
    IF NEW."Id" IS NULL OR NEW."Id" = '' THEN
        SELECT COALESCE(MAX(CAST(substring("Id" from 3) AS INTEGER)), 0)
        INTO max_id
        FROM "LessorOrganizations"
        WHERE "Id" LIKE 'LO%'
        AND substring("Id" from 3) ~ '^[0-9]+$';
        
        NEW."Id" := 'LO' || LPAD((max_id + 1)::TEXT, 6, '0');
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS "TR_LessorOrganizations_GenerateId" ON "LessorOrganizations";
CREATE TRIGGER "TR_LessorOrganizations_GenerateId"
    BEFORE INSERT ON "LessorOrganizations"
    FOR EACH ROW
    EXECUTE FUNCTION trg_lessororganizations_generate_id();

-- Функция для генерации ID техники
CREATE OR REPLACE FUNCTION trg_equipments_generate_id()
RETURNS TRIGGER AS $$
DECLARE
    max_id INT;
BEGIN
    IF NEW."Id" IS NULL OR NEW."Id" = '' THEN
        SELECT COALESCE(MAX(CAST(substring("Id" from 3) AS INTEGER)), 0)
        INTO max_id
        FROM "Equipments"
        WHERE "Id" LIKE 'EQ%'
        AND substring("Id" from 3) ~ '^[0-9]+$';
        
        NEW."Id" := 'EQ' || LPAD((max_id + 1)::TEXT, 6, '0');
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS "TR_Equipments_GenerateId" ON "Equipments";
CREATE TRIGGER "TR_Equipments_GenerateId"
    BEFORE INSERT ON "Equipments"
    FOR EACH ROW
    EXECUTE FUNCTION trg_equipments_generate_id();

-- Функция для генерации ID госномеров
CREATE OR REPLACE FUNCTION trg_licenseplates_generate_id()
RETURNS TRIGGER AS $$
DECLARE
    max_id INT;
BEGIN
    IF NEW."Id" IS NULL OR NEW."Id" = '' THEN
        SELECT COALESCE(MAX(CAST(substring("Id" from 3) AS INTEGER)), 0)
        INTO max_id
        FROM "LicensePlates"
        WHERE "Id" LIKE 'LP%'
        AND substring("Id" from 3) ~ '^[0-9]+$';
        
        NEW."Id" := 'LP' || LPAD((max_id + 1)::TEXT, 6, '0');
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS "TR_LicensePlates_GenerateId" ON "LicensePlates";
CREATE TRIGGER "TR_LicensePlates_GenerateId"
    BEFORE INSERT ON "LicensePlates"
    FOR EACH ROW
    EXECUTE FUNCTION trg_licenseplates_generate_id();

-- Функция для генерации ID ролей
CREATE OR REPLACE FUNCTION trg_roles_generate_id()
RETURNS TRIGGER AS $$
DECLARE
    max_id INT;
BEGIN
    IF NEW."Id" IS NULL OR NEW."Id" = '' THEN
        SELECT COALESCE(MAX(CAST(substring("Id" from 3) AS INTEGER)), 0)
        INTO max_id
        FROM "Roles"
        WHERE "Id" LIKE 'RL%'
        AND substring("Id" from 3) ~ '^[0-9]+$';
        
        NEW."Id" := 'RL' || LPAD((max_id + 1)::TEXT, 6, '0');
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS "TR_Roles_GenerateId" ON "Roles";
CREATE TRIGGER "TR_Roles_GenerateId"
    BEFORE INSERT ON "Roles"
    FOR EACH ROW
    EXECUTE FUNCTION trg_roles_generate_id();

-- Функция для генерации ID пользователей
CREATE OR REPLACE FUNCTION trg_users_generate_id()
RETURNS TRIGGER AS $$
DECLARE
    max_id INT;
BEGIN
    IF NEW."Id" IS NULL OR NEW."Id" = '' THEN
        SELECT COALESCE(MAX(CAST(substring("Id" from 3) AS INTEGER)), 0)
        INTO max_id
        FROM "Users"
        WHERE "Id" LIKE 'US%'
        AND substring("Id" from 3) ~ '^[0-9]+$';
        
        NEW."Id" := 'US' || LPAD((max_id + 1)::TEXT, 6, '0');
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS "TR_Users_GenerateId" ON "Users";
CREATE TRIGGER "TR_Users_GenerateId"
    BEFORE INSERT ON "Users"
    FOR EACH ROW
    EXECUTE FUNCTION trg_users_generate_id();

-- Функция для генерации ID складов
CREATE OR REPLACE FUNCTION trg_warehouses_generate_id()
RETURNS TRIGGER AS $$
DECLARE
    max_id INT;
BEGIN
    IF NEW."Id" IS NULL OR NEW."Id" = '' THEN
        SELECT COALESCE(MAX(CAST(substring("Id" from 3) AS INTEGER)), 0)
        INTO max_id
        FROM "Warehouses"
        WHERE "Id" LIKE 'WH%'
        AND substring("Id" from 3) ~ '^[0-9]+$';
        
        NEW."Id" := 'WH' || LPAD((max_id + 1)::TEXT, 6, '0');
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS "TR_Warehouses_GenerateId" ON "Warehouses";
CREATE TRIGGER "TR_Warehouses_GenerateId"
    BEFORE INSERT ON "Warehouses"
    FOR EACH ROW
    EXECUTE FUNCTION trg_warehouses_generate_id();

-- Функция для генерации ID территорий
CREATE OR REPLACE FUNCTION trg_warehouseareas_generate_id()
RETURNS TRIGGER AS $$
DECLARE
    max_id INT;
BEGIN
    IF NEW."Id" IS NULL OR NEW."Id" = '' THEN
        SELECT COALESCE(MAX(CAST(substring("Id" from 3) AS INTEGER)), 0)
        INTO max_id
        FROM "WarehouseAreas"
        WHERE "Id" LIKE 'WA%'
        AND substring("Id" from 3) ~ '^[0-9]+$';
        
        NEW."Id" := 'WA' || LPAD((max_id + 1)::TEXT, 6, '0');
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS "TR_WarehouseAreas_GenerateId" ON "WarehouseAreas";
CREATE TRIGGER "TR_WarehouseAreas_GenerateId"
    BEFORE INSERT ON "WarehouseAreas"
    FOR EACH ROW
    EXECUTE FUNCTION trg_warehouseareas_generate_id();

-- Функция автоматической очистки старых блокировок
CREATE OR REPLACE FUNCTION cleanup_expired_locks()
RETURNS void AS $$
BEGIN
    -- Снимаем блокировки старше 30 минут
    UPDATE "ShiftRequests"
    SET "LockedByUserId" = NULL, "LockedAt" = NULL
    WHERE "LockedAt" < (CURRENT_TIMESTAMP - INTERVAL '30 minutes');
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- ИНДЕКСЫ ДЛЯ ПРОИЗВОДИТЕЛЬНОСТИ
-- =============================================

-- ShiftRequests
CREATE INDEX IF NOT EXISTS "IX_ShiftRequests_Date_Shift" ON "ShiftRequests" ("Date", "Shift");
CREATE INDEX IF NOT EXISTS "IX_ShiftRequests_EquipmentId" ON "ShiftRequests" ("EquipmentId");
CREATE INDEX IF NOT EXISTS "IX_ShiftRequests_WarehouseId" ON "ShiftRequests" ("WarehouseId");
CREATE INDEX IF NOT EXISTS "IX_ShiftRequests_DepartmentId" ON "ShiftRequests" ("DepartmentId");
CREATE INDEX IF NOT EXISTS "IX_ShiftRequests_CreatedByUserId" ON "ShiftRequests" ("CreatedByUserId");
CREATE INDEX IF NOT EXISTS "IX_ShiftRequests_LicensePlateId" ON "ShiftRequests" ("LicensePlateId");
CREATE INDEX IF NOT EXISTS "IX_ShiftRequests_LockedByUserId" ON "ShiftRequests" ("LockedByUserId");
CREATE INDEX IF NOT EXISTS "IX_ShiftRequests_LockedAt" ON "ShiftRequests" ("LockedAt");
CREATE INDEX IF NOT EXISTS "IX_ShiftRequests_NotProvided" ON "ShiftRequests" ("IsNotProvided") WHERE "IsNotProvided" = true;
CREATE INDEX IF NOT EXISTS "IX_ShiftRequests_WeatherCancellation" ON "ShiftRequests" ("IsWeatherCancellation") WHERE "IsWeatherCancellation" = true;

-- TransportProgram
CREATE INDEX IF NOT EXISTS "IX_TransportProgram_Dept_Year" ON "TransportProgram" ("DepartmentId", "Year");
CREATE INDEX IF NOT EXISTS "IX_TransportProgram_EquipmentId" ON "TransportProgram" ("EquipmentId");

-- LicensePlates
CREATE INDEX IF NOT EXISTS "IX_LicensePlates_EquipmentId" ON "LicensePlates" ("EquipmentId");
CREATE INDEX IF NOT EXISTS "IX_LicensePlates_LessorOrgId" ON "LicensePlates" ("LessorOrganizationId");
CREATE INDEX IF NOT EXISTS "IX_LicensePlates_PlateNumber" ON "LicensePlates" ("PlateNumber");

-- Users
CREATE INDEX IF NOT EXISTS "IX_Users_WindowsLogin" ON "Users" ("WindowsLogin");
CREATE INDEX IF NOT EXISTS "IX_Users_RoleId" ON "Users" ("RoleId");

-- UserDepartmentAccess
CREATE INDEX IF NOT EXISTS "IX_UserDepartmentAccess_UserId" ON "UserDepartmentAccess" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_UserDepartmentAccess_DepartmentId" ON "UserDepartmentAccess" ("DepartmentId");

-- UserWarehouseAccess
CREATE INDEX IF NOT EXISTS "IX_UserWarehouseAccess_UserDeptAccessKey" ON "UserWarehouseAccess" ("UserDepartmentAccessKey");
CREATE INDEX IF NOT EXISTS "IX_UserWarehouseAccess_WarehouseId" ON "UserWarehouseAccess" ("WarehouseId");

-- EquipmentDependencies
CREATE INDEX IF NOT EXISTS "IX_EquipmentDependencies_MainEquipmentId" ON "EquipmentDependencies" ("MainEquipmentId");
CREATE INDEX IF NOT EXISTS "IX_EquipmentDependencies_DependentEquipmentId" ON "EquipmentDependencies" ("DependentEquipmentId");

-- UserSettings
CREATE INDEX IF NOT EXISTS "IX_UserSettings_UserId" ON "UserSettings" ("UserId");

-- AuditLogs
CREATE INDEX IF NOT EXISTS "IX_AuditLogs_TableName_RecordId" ON "AuditLogs" ("TableName", "RecordId");
CREATE INDEX IF NOT EXISTS "IX_AuditLogs_ChangedByUserId" ON "AuditLogs" ("ChangedByUserId");
CREATE INDEX IF NOT EXISTS "IX_AuditLogs_ChangedAt" ON "AuditLogs" ("ChangedAt");

-- =============================================
-- ПРЕДСТАВЛЕНИЯ ДЛЯ УДОБСТВА РАБОТЫ
-- =============================================

-- Представление для заявок с полными данными
CREATE OR REPLACE VIEW "View_ShiftRequests_Full" AS
SELECT 
    sr."Key",
    sr."Date",
    sr."Shift",
    CASE WHEN sr."Shift" = 0 THEN 'Дневная' ELSE 'Ночная' END AS "ShiftName",
    sr."EquipmentId",
    e."Name" AS "EquipmentName",
    e."Category" AS "EquipmentCategory",
    sr."LicensePlateId",
    lp."PlateNumber",
    lp."Brand" AS "VehicleBrand",
    sr."WarehouseId",
    w."Name" AS "WarehouseName",
    sr."AreaId",
    wa."Name" AS "AreaName",
    sr."LessorOrganizationId",
    lo."Name" AS "LessorName",
    sr."RequestedCount",
    sr."WorkedHours",
    sr."ActualCost",
    sr."IsWorked",
    sr."IsNotProvided",
    sr."IsWeatherCancellation",
    sr."CancellationReason",
    sr."IsBlocked",
    sr."Comment",
    sr."CreatedByUserId",
    u."FullName" AS "CreatedByUser",
    sr."DepartmentId",
    d."Name" AS "DepartmentName",
    sr."CreatedAt"
FROM "ShiftRequests" sr
LEFT JOIN "Equipments" e ON sr."EquipmentId" = e."Id"
LEFT JOIN "LicensePlates" lp ON sr."LicensePlateId" = lp."Id"
LEFT JOIN "Warehouses" w ON sr."WarehouseId" = w."Id"
LEFT JOIN "WarehouseAreas" wa ON sr."AreaId" = wa."Id"
LEFT JOIN "LessorOrganizations" lo ON sr."LessorOrganizationId" = lo."Id"
LEFT JOIN "Users" u ON sr."CreatedByUserId" = u."Id"
LEFT JOIN "Departments" d ON sr."DepartmentId" = d."Id";

-- Представление для выполнения транспортной программы
CREATE OR REPLACE VIEW "View_TransportProgram_Execution" AS
WITH actual_hours AS (
    SELECT 
        sr."DepartmentId",
        EXTRACT(YEAR FROM sr."Date") AS "Year",
        EXTRACT(MONTH FROM sr."Date") AS "Month",
        sr."EquipmentId",
        COALESCE(SUM(sr."WorkedHours"), 0) AS "ActualHours",
        COUNT(*) AS "RequestsCount"
    FROM "ShiftRequests" sr
    WHERE sr."IsWorked" = true 
      AND sr."IsNotProvided" = false 
      AND sr."IsWeatherCancellation" = false
    GROUP BY sr."DepartmentId", EXTRACT(YEAR FROM sr."Date"), 
             EXTRACT(MONTH FROM sr."Date"), sr."EquipmentId"
)
SELECT 
    tp."DepartmentId",
    d."Name" AS "DepartmentName",
    tp."Year",
    tp."EquipmentId",
    e."Name" AS "EquipmentName",
    tp."HourlyCost",
    tp."JanuaryHours" AS "PlanHours_1",
    tp."FebruaryHours" AS "PlanHours_2",
    tp."MarchHours" AS "PlanHours_3",
    tp."AprilHours" AS "PlanHours_4",
    tp."MayHours" AS "PlanHours_5",
    tp."JuneHours" AS "PlanHours_6",
    tp."JulyHours" AS "PlanHours_7",
    tp."AugustHours" AS "PlanHours_8",
    tp."SeptemberHours" AS "PlanHours_9",
    tp."OctoberHours" AS "PlanHours_10",
    tp."NovemberHours" AS "PlanHours_11",
    tp."DecemberHours" AS "PlanHours_12",
    tp."TotalYearHours" AS "PlanTotalHours",
    COALESCE(ah."ActualHours", 0) AS "ActualTotalHours",
    CASE 
        WHEN tp."TotalYearHours" > 0 
        THEN ROUND((COALESCE(ah."ActualHours", 0) / tp."TotalYearHours" * 100)::numeric, 2)
        ELSE 0 
    END AS "ExecutionPercent"
FROM "TransportProgram" tp
JOIN "Departments" d ON tp."DepartmentId" = d."Id"
JOIN "Equipments" e ON tp."EquipmentId" = e."Id"
LEFT JOIN actual_hours ah ON tp."DepartmentId" = ah."DepartmentId" 
                         AND tp."Year" = ah."Year" 
                         AND tp."EquipmentId" = ah."EquipmentId";

-- =============================================
-- ВСТАВКА НАЧАЛЬНЫХ ДАННЫХ
-- =============================================

-- Очищаем существующие данные (для чистой установки)
TRUNCATE "ShiftRequests", "UserFavorites", "TransportProgram", 
         "UserWarehouseAccess", "UserDepartmentAccess", "Users",
         "WarehouseAreas", "Warehouses", "EquipmentDependencies",
         "LicensePlates", "Equipments", "LessorOrganizations",
         "Departments", "Roles", "UserSettings", "AuditLogs" RESTART IDENTITY CASCADE;

-- 1. Вставляем роли
INSERT INTO "Roles" ("Id", "Name", "Code", "Description", 
    "TAB_AuditLogs", "TAB_Departments", "TAB_EquipmentDependencies", "TAB_Equipments", 
    "TAB_LessorOrganizations", "TAB_LicensePlates", "TAB_Roles", "TAB_ShiftRequests", 
    "TAB_TransportProgram", "TAB_UserDepartmentAccess", "TAB_UserFavorites", "TAB_Users", 
    "TAB_UserWarehouseAccess", "TAB_WarehouseAreas", "TAB_Warehouses",
    "SPEC_ExportData", "SPEC_ViewReports", "SPEC_ManageAllDepartments", 
    "SPEC_ManageUsers", "SPEC_SystemAdmin", "SPEC_ConfigureConnection", "IsSystem") 
VALUES 
('RL000001', 'Администратор', 'ADMIN', 'Полный доступ ко всем функциям системы',
 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 
 true, true, true, true, true, true, true),

('RL000002', 'Менеджер', 'MANAGER', 'Управление заявками и справочниками',
 1, 2, 2, 2, 2, 2, 0, 2, 2, 1, 2, 1, 1, 2, 2, 
 true, true, true, false, false, false, true),

('RL000003', 'Инициатор', 'INITIATOR', 'Создание и просмотр заявок для своего отдела',
 0, 1, 1, 1, 1, 1, 0, 2, 1, 0, 2, 0, 0, 1, 1, 
 true, true, false, false, false, false, true),

('RL000004', 'Наблюдатель', 'VIEWER', 'Только просмотр данных',
 0, 1, 1, 1, 1, 1, 0, 1, 1, 0, 1, 0, 0, 1, 1, 
 true, true, false, false, false, false, true),

('RL000005', 'Руководитель отдела', 'DEPARTMENT_HEAD', 'Управление заявками своего отдела',
 0, 1, 1, 1, 1, 1, 0, 2, 2, 0, 1, 0, 0, 1, 1, 
 true, true, false, false, false, false, false);

-- 2. Вставляем отделы
INSERT INTO "Departments" ("Id", "Name", "IsActive") VALUES 
('DE000001', 'Отдел логистики', true),
('DE000002', 'Отдел строительства', true),
('DE000003', 'Складской комплекс', true),
('DE000004', 'Администрация', true);

-- 3. Вставляем организации-арендодатели
INSERT INTO "LessorOrganizations" ("Id", "Name", "INN", "ContactPerson", "Phone", "IsActive") VALUES 
('LO000001', 'ООО "СтройТехСервис"', '7701234567', 'Иванов Петр Сергеевич', '+7 (495) 123-45-67', true),
('LO000002', 'АО "ТрансМаш"', '7707654321', 'Сидорова Мария Ивановна', '+7 (495) 765-43-21', true),
('LO000003', 'ЗАО "Механизация"', '7701112233', 'Петров Алексей Викторович', '+7 (495) 111-22-33', true);

-- 4. Вставляем технику
INSERT INTO "Equipments" ("Id", "Name", "Category", "CanOrderMultiple", "HourlyCost", "RequiresOperator", "IsActive") VALUES 
('EQ000001', 'Автокран 25т', 'Спецтехника', false, 2500.00, true, true),
('EQ000002', 'Автокран 50т', 'Спецтехника', false, 4500.00, true, true),
('EQ000003', 'Автовышка 28м', 'Спецтехника', false, 1800.00, true, true),
('EQ000004', 'Бульдозер', 'Спецтехника', false, 2200.00, true, true),
('EQ000005', 'Экскаватор-погрузчик', 'Спецтехника', false, 2000.00, true, true),
('EQ000006', 'Стропальщик', 'Рабочий', true, 800.00, false, true),
('EQ000007', 'Мастер', 'Рабочий', true, 1200.00, false, true),
('EQ000008', 'Водитель', 'Рабочий', true, 900.00, false, true),
('EQ000009', 'Генератор 100кВт', 'Оборудование', true, 1500.00, false, true),
('EQ000010', 'Компрессор', 'Оборудование', true, 1200.00, false, true);

-- 5. Вставляем зависимости техники
INSERT INTO "EquipmentDependencies" ("MainEquipmentId", "DependentEquipmentId", "RequiredCount", "Description") VALUES 
('EQ000001', 'EQ000006', 3, 'Требуется 3 стропальщика'),
('EQ000001', 'EQ000007', 1, 'Требуется 1 мастер'),
('EQ000002', 'EQ000006', 4, 'Требуется 4 стропальщика'),
('EQ000002', 'EQ000007', 1, 'Требуется 1 мастер');

-- 6. Вставляем госномера
INSERT INTO "LicensePlates" ("Id", "PlateNumber", "EquipmentId", "LessorOrganizationId", "Brand", "Year", "IsActive") VALUES 
('LP000001', 'А123БВ77', 'EQ000001', 'LO000001', 'Liebherr LTM 1050-3.1', 2018, true),
('LP000002', 'В234СТ78', 'EQ000001', 'LO000001', 'XCMG QY25K5', 2020, true),
('LP000003', 'С345ДЕ79', 'EQ000002', 'LO000002', 'Grove GMK 3050', 2019, true),
('LP000004', 'Е456ФГ80', 'EQ000003', 'LO000001', 'Mantall M28JRT', 2021, true),
('LP000005', 'Ж567ХИ81', 'EQ000004', 'LO000003', 'Caterpillar D6', 2020, true);

-- 7. Вставляем склады
INSERT INTO "Warehouses" ("Id", "Name", "DepartmentId", "Address", "IsActive") VALUES 
('WH000001', 'Склад №1 (Логистика)', 'DE000001', 'ул. Складская, д. 1', true),
('WH000002', 'Склад стройматериалов', 'DE000002', 'ул. Строителей, д. 10', true),
('WH000003', 'Логистический центр', 'DE000001', 'ул. Транспортная, д. 5', true),
('WH000004', 'Административный склад', 'DE000004', 'ул. Центральная, д. 1', true);

-- 8. Вставляем территории складов
INSERT INTO "WarehouseAreas" ("Id", "Name", "WarehouseId", "AreaType", "IsActive") VALUES 
('WA000001', 'Зона разгрузки', 'WH000001', 'Разгрузка', true),
('WA000002', 'Сектор А', 'WH000001', 'Хранение', true),
('WA000003', 'Сектор Б', 'WH000001', 'Хранение', true),
('WA000004', 'Открытая площадка', 'WH000002', 'Открытое хранение', true),
('WA000005', 'Крытый ангар', 'WH000002', 'Крытое хранение', true);

-- 9. Вставляем пользователя-администратора
INSERT INTO "Users" ("Id", "WindowsLogin", "FullName", "RoleId", "HasAllDepartments", "IsActive") VALUES 
('US000001', 'AdminUser', 'Иванов Иван Иванович', 'RL000001', true, true);

-- 10. Добавляем доступ администратора ко всем отделам
INSERT INTO "UserDepartmentAccess" ("UserId", "DepartmentId", "HasAllWarehouses") VALUES 
('US000001', 'DE000001', true),
('US000001', 'DE000002', true),
('US000001', 'DE000003', true),
('US000001', 'DE000004', true);

-- 11. Добавляем доступ ко всем складам через UserWarehouseAccess
INSERT INTO "UserWarehouseAccess" ("UserDepartmentAccessKey", "WarehouseId")
SELECT uda."Key", w."Id"
FROM "UserDepartmentAccess" uda
CROSS JOIN "Warehouses" w
WHERE uda."UserId" = 'US000001' AND uda."HasAllWarehouses" = true;

-- 12. Вставляем транспортную программу на текущий год
INSERT INTO "TransportProgram" ("DepartmentId", "Year", "EquipmentId", "HourlyCost", 
    "JanuaryHours", "FebruaryHours", "MarchHours", "AprilHours", "MayHours", "JuneHours",
    "JulyHours", "AugustHours", "SeptemberHours", "OctoberHours", "NovemberHours", "DecemberHours") 
SELECT 
    'DE000001', EXTRACT(YEAR FROM CURRENT_DATE), 'EQ000001', 2500.00, 
    40, 35, 45, 50, 55, 60, 40, 35, 45, 50, 40, 30
WHERE NOT EXISTS (
    SELECT 1 FROM "TransportProgram" 
    WHERE "DepartmentId" = 'DE000001' AND "Year" = EXTRACT(YEAR FROM CURRENT_DATE) AND "EquipmentId" = 'EQ000001'
);

INSERT INTO "TransportProgram" ("DepartmentId", "Year", "EquipmentId", "HourlyCost", 
    "JanuaryHours", "FebruaryHours", "MarchHours", "AprilHours", "MayHours", "JuneHours",
    "JulyHours", "AugustHours", "SeptemberHours", "OctoberHours", "NovemberHours", "DecemberHours") 
SELECT 
    'DE000001', EXTRACT(YEAR FROM CURRENT_DATE), 'EQ000002', 4500.00, 
    20, 18, 25, 30, 35, 40, 25, 20, 30, 35, 25, 15
WHERE NOT EXISTS (
    SELECT 1 FROM "TransportProgram" 
    WHERE "DepartmentId" = 'DE000001' AND "Year" = EXTRACT(YEAR FROM CURRENT_DATE) AND "EquipmentId" = 'EQ000002'
);

INSERT INTO "TransportProgram" ("DepartmentId", "Year", "EquipmentId", "HourlyCost", 
    "JanuaryHours", "FebruaryHours", "MarchHours", "AprilHours", "MayHours", "JuneHours",
    "JulyHours", "AugustHours", "SeptemberHours", "OctoberHours", "NovemberHours", "DecemberHours") 
SELECT 
    'DE000002', EXTRACT(YEAR FROM CURRENT_DATE), 'EQ000003', 1800.00, 
    80, 75, 85, 90, 95, 100, 80, 75, 85, 90, 80, 70
WHERE NOT EXISTS (
    SELECT 1 FROM "TransportProgram" 
    WHERE "DepartmentId" = 'DE000002' AND "Year" = EXTRACT(YEAR FROM CURRENT_DATE) AND "EquipmentId" = 'EQ000003'
);

-- 13. Вставляем избранное для администратора
INSERT INTO "UserFavorites" ("UserId", "EquipmentId", "SortOrder") VALUES 
('US000001', 'EQ000001', 1),
('US000001', 'EQ000002', 2),
('US000001', 'EQ000006', 3);

-- 14. Вставляем тестовую заявку (дневная смена)
INSERT INTO "ShiftRequests" (
    "Date", "Shift", "EquipmentId", "LicensePlateId", "WarehouseId", "AreaId",
    "LessorOrganizationId", "RequestedCount", "WorkedHours", "ActualCost",
    "IsWorked", "IsBlocked", "Comment", "CreatedByUserId", "DepartmentId"
) VALUES (
    CURRENT_DATE, 0, 'EQ000001', 'LP000001', 'WH000001', 'WA000001',
    'LO000001', 1, 8.5, 21250.00,
    true, false, 'Тестовая заявка на кран', 'US000001', 'DE000001'
);

-- 15. Вставляем тестовую заявку с актировкой (погодные условия)
INSERT INTO "ShiftRequests" (
    "Date", "Shift", "EquipmentId", "WarehouseId", "AreaId",
    "LessorOrganizationId", "RequestedCount", "WorkedHours", "ActualCost",
    "IsWorked", "IsNotProvided", "IsWeatherCancellation", "CancellationReason",
    "Comment", "CreatedByUserId", "DepartmentId"
) VALUES (
    CURRENT_DATE - 1, 1, 'EQ000003', 'WH000002', 'WA000004',
    'LO000002', 1, 0, 0,
    false, false, true, 'Неблагоприятные погодные условия (ветер)',
    'Актировка из-за штормового предупреждения', 'US000001', 'DE000002'
);

-- 16. Вставляем настройки пользователя по умолчанию
INSERT INTO "UserSettings" ("UserId", "SettingKey", "SettingValue") VALUES 
('US000001', 'Theme', '"Light"'::jsonb),
('US000001', 'DefaultShift', '0'::jsonb),
('US000001', 'AutoRefresh', 'true'::jsonb),
('US000001', 'RefreshInterval', '60'::jsonb),
('US000001', 'DefaultWarehouseId', '"WH000001"'::jsonb),
('US000001', 'ShowFavoritesOnly', 'false'::jsonb);

-- =============================================
-- ФИНАЛЬНАЯ ПРОВЕРКА
-- =============================================

DO $$
DECLARE
    user_count INTEGER;
    request_count INTEGER;
BEGIN
    SELECT COUNT(*) INTO user_count FROM "Users" WHERE "WindowsLogin" = 'AdminUser';
    SELECT COUNT(*) INTO request_count FROM "ShiftRequests";
    
    RAISE NOTION '=============================================';
    RAISE NOTION 'БАЗА ДАННЫХ УСПЕШНО ИНИЦИАЛИЗИРОВАНА';
    RAISE NOTION '=============================================';
    RAISE NOTION 'Создано таблиц: 16';
    RAISE NOTION 'Создано индексов: 25+';
    RAISE NOTION 'Создано представлений: 2';
    RAISE NOTION 'Создано триггеров: 8';
    RAISE NOTION '---------------------------------------------';
    RAISE NOTION 'Пользователь администратор: %', 
        CASE WHEN user_count > 0 THEN 'СОЗДАН (AdminUser)' ELSE 'НЕ НАЙДЕН' END;
    RAISE NOTION 'Тестовых заявок: %', request_count;
    RAISE NOTION '---------------------------------------------';
    RAISE NOTION 'ДАННЫЕ ДЛЯ ВХОДА В ПРИЛОЖЕНИЕ:';
    RAISE NOTION '  Windows логин: AdminUser (без домена)';
    RAISE NOTION '  Роль: Администратор';
    RAISE NOTION '  Полное имя: Иванов Иван Иванович';
    RAISE NOTION '=============================================';
END $$;

-- Автоматическая очистка старых блокировок (запускаем сразу)
SELECT cleanup_expired_locks();

-- =============================================
-- КОМАНДЫ ДЛЯ ПРОВЕРКИ РАБОТОСПОСОБНОСТИ
-- =============================================
/*
-- Проверить все заявки с расшифровкой
SELECT * FROM "View_ShiftRequests_Full" ORDER BY "Date" DESC, "Shift";

-- Проверить выполнение транспортной программы
SELECT * FROM "View_TransportProgram_Execution" ORDER BY "DepartmentName", "EquipmentName";

-- Проверить блокировки
SELECT "Key", "Date", "LockedByUserId", "LockedAt" 
FROM "ShiftRequests" 
WHERE "LockedByUserId" IS NOT NULL;

-- Проверить настройки пользователя
SELECT u."FullName", us."SettingKey", us."SettingValue"
FROM "UserSettings" us
JOIN "Users" u ON us."UserId" = u."Id";

-- Проверить права доступа пользователя
SELECT u."FullName", r."Name" AS "Role", r."TAB_ShiftRequests" AS "AccessLevel"
FROM "Users" u
JOIN "Roles" r ON u."RoleId" = r."Id"
WHERE u."IsActive" = true;
*/