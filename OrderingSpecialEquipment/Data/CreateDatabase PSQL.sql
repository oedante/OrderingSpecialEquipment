-- =============================================
-- Скрипт создания базы данных для приложения
-- OrderingSpecialEquipment (Учет специальной техники)
-- =============================================

-- Создание базы данных (выполняется отдельно, вне транзакции)
-- CREATE DATABASE "OrderingSpecialEquipment";
-- \c "OrderingSpecialEquipment";

-- Установка кодировки и параметров
SET client_encoding = 'UTF8';

-- =============================================
-- 1. ТАБЛИЦА ОТДЕЛОВ (Departments) 
-- С СОСТАВНЫМ КЛЮЧОМ
-- =============================================
CREATE TABLE "Departments" (
    "Key"        SERIAL          NOT NULL,
    "Id"         VARCHAR(10)     NOT NULL,
    "Name"       VARCHAR(100)    NOT NULL,
    "IsActive"   BOOLEAN         DEFAULT true NOT NULL,
    "CreatedAt"  TIMESTAMP       DEFAULT CURRENT_TIMESTAMP NOT NULL,
    
    CONSTRAINT "PK_Departments" PRIMARY KEY ("Id"),
    CONSTRAINT "UQ_Departments_Key" UNIQUE ("Key")
);

COMMENT ON TABLE "Departments" IS 'Таблица отделов';
COMMENT ON COLUMN "Departments"."IsActive" IS 'Активен ли отдел';

-- =============================================
-- 2. ТАБЛИЦА ОРГАНИЗАЦИЙ-АРЕНДОДАТЕЛЕЙ (LessorOrganizations)
-- С СОСТАВНЫМ КЛЮЧОМ
-- =============================================
CREATE TABLE "LessorOrganizations" (
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

-- =============================================
-- 3. ТАБЛИЦА ТЕХНИКИ (Equipments) 
-- С СОСТАВНЫМ КЛЮЧОМ
-- =============================================
CREATE TABLE "Equipments" (
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

-- =============================================
-- 4. ТАБЛИЦА ГОСНОМЕРОВ (LicensePlates) 
-- С СОСТАВНЫМ КЛЮЧОМ
-- =============================================
CREATE TABLE "LicensePlates" (
    "Key"                 SERIAL          NOT NULL,
    "Id"                  VARCHAR(10)     NOT NULL,
    "PlateNumber"         VARCHAR(20)     NOT NULL,
    "EquipmentId"         VARCHAR(10)     NOT NULL,
    "LessorOrganizationId" VARCHAR(10)    NOT NULL,
    "Brand"               VARCHAR(100)    NULL,
    "Year"                INTEGER         NULL,
    "Capacity"            VARCHAR(50)     NULL,
    "VIN"                 VARCHAR(50)     NULL,
    "IsActive"            BOOLEAN         DEFAULT true NOT NULL,
    "CreatedAt"           TIMESTAMP       DEFAULT CURRENT_TIMESTAMP NOT NULL,
    
    CONSTRAINT "PK_LicensePlates" PRIMARY KEY ("Id"),
    CONSTRAINT "UQ_LicensePlates_Key" UNIQUE ("Key"),
    CONSTRAINT "UQ_LicensePlates_PlateNumber" UNIQUE ("PlateNumber"),
    CONSTRAINT "FK_LicensePlates_Equipments" FOREIGN KEY ("EquipmentId") 
        REFERENCES "Equipments" ("Id"),
    CONSTRAINT "FK_LicensePlates_LessorOrganizations" FOREIGN KEY ("LessorOrganizationId") 
        REFERENCES "LessorOrganizations" ("Id")
);

-- =============================================
-- 5. ТАБЛИЦА СВЯЗИ ТЕХНИКИ С ДОПОЛНИТЕЛЬНОЙ ТЕХНИКОЙ (EquipmentDependencies)
-- =============================================
CREATE TABLE "EquipmentDependencies" (
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

-- =============================================
-- 6. ТАБЛИЦА ТРАНСПОРТНОЙ ПРОГРАММЫ (TransportProgram)
-- =============================================
CREATE TABLE "TransportProgram" (
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

-- =============================================
-- 7. ТАБЛИЦА РОЛЕЙ (Roles) С ПРАВАМИ ДОСТУПА
-- =============================================
CREATE TABLE "Roles" (
    "Key"                        SERIAL          NOT NULL,
    "Id"                         VARCHAR(10)     NOT NULL,
    "Name"                       VARCHAR(50)     NOT NULL,
    "Code"                       VARCHAR(20)     NOT NULL,
    "Description"                VARCHAR(200)    NULL,
    
    -- Права доступа к таблицам (0-Запрещено, 1-Чтение, 2-Запись)
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
    
    -- Специальные права (битовые флаги)
    "SPEC_ExportData"            BOOLEAN         DEFAULT false NOT NULL,
    "SPEC_ViewReports"           BOOLEAN         DEFAULT false NOT NULL,
    "SPEC_ManageAllDepartments"  BOOLEAN         DEFAULT false NOT NULL,
    "SPEC_ManageUsers"           BOOLEAN         DEFAULT false NOT NULL,
    "SPEC_SystemAdmin"           BOOLEAN         DEFAULT false NOT NULL,
    
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
CREATE TABLE "Users" (
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

-- =============================================
-- 9. ТАБЛИЦА ДОСТУПА ПОЛЬЗОВАТЕЛЕЙ К ОТДЕЛАМ (UserDepartmentAccess)
-- =============================================
CREATE TABLE "UserDepartmentAccess" (
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
CREATE TABLE "Warehouses" (
    "Key"         SERIAL          NOT NULL,
    "Id"          VARCHAR(10)     NOT NULL,
    "Name"        VARCHAR(100)    NOT NULL,
    "DepartmentId" VARCHAR(10)    NOT NULL,
    "Address"     VARCHAR(500)    NULL,
    "IsActive"    BOOLEAN         DEFAULT true NOT NULL,
    "CreatedAt"   TIMESTAMP       DEFAULT CURRENT_TIMESTAMP NOT NULL,
    
    CONSTRAINT "PK_Warehouses" PRIMARY KEY ("Id"),
    CONSTRAINT "UQ_Warehouses_Key" UNIQUE ("Key"),
    CONSTRAINT "FK_Warehouses_Departments" FOREIGN KEY ("DepartmentId") 
        REFERENCES "Departments" ("Id")
);

-- =============================================
-- 11. ТАБЛИЦА ДОСТУПА ПОЛЬЗОВАТЕЛЕЙ К СКЛАДАМ (UserWarehouseAccess)
-- =============================================
CREATE TABLE "UserWarehouseAccess" (
    "Key"                      SERIAL          NOT NULL,
    "UserDepartmentAccessKey"  INTEGER         NOT NULL,
    "WarehouseId"              VARCHAR(10)     NOT NULL,
    "CreatedAt"                TIMESTAMP       DEFAULT CURRENT_TIMESTAMP NOT NULL,
    
    CONSTRAINT "PK_UserWarehouseAccess" PRIMARY KEY ("Key"),
    CONSTRAINT "UQ_UserWarehouseAccess_DeptAccess_Warehouse" UNIQUE ("UserDepartmentAccessKey", "WarehouseId"),
    CONSTRAINT "FK_UserWarehouseAccess_UserDepartmentAccess" FOREIGN KEY ("UserDepartmentAccessKey") 
        REFERENCES "UserDepartmentAccess" ("Key"),
    CONSTRAINT "FK_UserWarehouseAccess_Warehouses" FOREIGN KEY ("WarehouseId") 
        REFERENCES "Warehouses" ("Id")
);

-- =============================================
-- 12. ТАБЛИЦА ТЕРРИТОРИЙ СКЛАДОВ (WarehouseAreas) 
-- =============================================
CREATE TABLE "WarehouseAreas" (
    "Key"        SERIAL          NOT NULL,
    "Id"         VARCHAR(10)     NOT NULL,
    "Name"       VARCHAR(100)    NOT NULL,
    "WarehouseId" VARCHAR(10)    NOT NULL,
    "AreaType"   VARCHAR(50)     NULL,
    "MaxCapacity" INTEGER        NULL,
    "IsActive"   BOOLEAN         DEFAULT true NOT NULL,
    "CreatedAt"  TIMESTAMP       DEFAULT CURRENT_TIMESTAMP NOT NULL,
    
    CONSTRAINT "PK_WarehouseAreas" PRIMARY KEY ("Id"),
    CONSTRAINT "UQ_WarehouseAreas_Key" UNIQUE ("Key"),
    CONSTRAINT "FK_WarehouseAreas_Warehouses" FOREIGN KEY ("WarehouseId") 
        REFERENCES "Warehouses" ("Id") ON DELETE CASCADE
);

-- =============================================
-- 13. ТАБЛИЦА ЗАЯВОК (ShiftRequests) 
-- =============================================
CREATE TABLE "ShiftRequests" (
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
    "Comment"                TEXT             NULL,
    "CreatedByUserId"        VARCHAR(10)      NOT NULL,
    "CreatedAt"              TIMESTAMP        DEFAULT CURRENT_TIMESTAMP NOT NULL,
    "DepartmentId"           VARCHAR(10)      NULL,
    "ProgramYear"            INTEGER          NULL,
    "ProgramMonth"           INTEGER          NULL,
    
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
    CONSTRAINT "CHK_ShiftRequests_ProgramMonth" CHECK ("ProgramMonth" IS NULL OR ("ProgramMonth" >= 1 AND "ProgramMonth" <= 12))
);

-- =============================================
-- 14. ТАБЛИЦА ИЗБРАННОГО ПОЛЬЗОВАТЕЛЯ (UserFavorites) 
-- =============================================
CREATE TABLE "UserFavorites" (
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
-- 15. ТАБЛИЦА ЛОГОВ ИЗМЕНЕНИЙ (AuditLogs) 
-- =============================================
CREATE TABLE "AuditLogs" (
    "Key"             SERIAL          NOT NULL,
    "TableName"       VARCHAR(50)     NOT NULL,
    "RecordId"        VARCHAR(50)     NOT NULL,
    "Action"          VARCHAR(20)     NOT NULL,
    "OldValues"       TEXT            NULL,
    "NewValues"       TEXT            NULL,
    "ChangedByUserId" VARCHAR(10)     NOT NULL,
    "ChangedAt"       TIMESTAMP       DEFAULT CURRENT_TIMESTAMP NOT NULL,
    "IPAddress"       VARCHAR(50)     NULL,
    "UserAgent"       VARCHAR(500)    NULL,
    
    CONSTRAINT "PK_AuditLogs" PRIMARY KEY ("Key"),
    CONSTRAINT "FK_AuditLogs_Users" FOREIGN KEY ("ChangedByUserId") 
        REFERENCES "Users" ("Id")
);

-- =============================================
-- ФУНКЦИИ ДЛЯ ГЕНЕРИРОВАНИЯ ID (вместо триггеров)
-- =============================================

-- Функция для генерации следующего ID для таблицы
CREATE OR REPLACE FUNCTION generate_next_id(table_name VARCHAR, prefix VARCHAR)
RETURNS VARCHAR AS $$
DECLARE
    max_id INT;
    sql_query TEXT;
BEGIN
    sql_query := format('SELECT COALESCE(MAX(CAST(substring("Id" from 3) AS INTEGER)), 0) 
                        FROM %I WHERE "Id" LIKE $1', table_name);
    EXECUTE sql_query INTO max_id USING prefix || '%';
    
    RETURN prefix || LPAD((max_id + 1)::VARCHAR, 6, '0');
END;
$$ LANGUAGE plpgsql;

-- Функция для вставки отдела с автоматической генерацией ID
CREATE OR REPLACE FUNCTION insert_department(
    p_name VARCHAR(100),
    p_is_active BOOLEAN DEFAULT true
)
RETURNS VARCHAR AS $$
DECLARE
    new_id VARCHAR(10);
BEGIN
    new_id := generate_next_id('Departments', 'DE');
    
    INSERT INTO "Departments" ("Id", "Name", "IsActive")
    VALUES (new_id, p_name, p_is_active);
    
    RETURN new_id;
END;
$$ LANGUAGE plpgsql;

-- Аналогичные функции для других таблиц можно создать по тому же принципу

-- =============================================
-- ИНДЕКСЫ ДЛЯ УСКОРЕНИЯ ПОИСКА
-- =============================================

-- ShiftRequests
CREATE INDEX "IX_ShiftRequests_Date_Shift" ON "ShiftRequests" ("Date", "Shift");
CREATE INDEX "IX_ShiftRequests_EquipmentId" ON "ShiftRequests" ("EquipmentId");
CREATE INDEX "IX_ShiftRequests_WarehouseId" ON "ShiftRequests" ("WarehouseId");
CREATE INDEX "IX_ShiftRequests_DepartmentId" ON "ShiftRequests" ("DepartmentId");
CREATE INDEX "IX_ShiftRequests_CreatedByUserId" ON "ShiftRequests" ("CreatedByUserId");
CREATE INDEX "IX_ShiftRequests_LicensePlateId" ON "ShiftRequests" ("LicensePlateId");
CREATE INDEX "IX_ShiftRequests_ProgramYear_Month" ON "ShiftRequests" ("ProgramYear", "ProgramMonth");

-- TransportProgram
CREATE INDEX "IX_TransportProgram_Dept_Year" ON "TransportProgram" ("DepartmentId", "Year");
CREATE INDEX "IX_TransportProgram_EquipmentId" ON "TransportProgram" ("EquipmentId");

-- LicensePlates
CREATE INDEX "IX_LicensePlates_EquipmentId" ON "LicensePlates" ("EquipmentId");
CREATE INDEX "IX_LicensePlates_LessorOrgId" ON "LicensePlates" ("LessorOrganizationId");
CREATE INDEX "IX_LicensePlates_PlateNumber" ON "LicensePlates" ("PlateNumber");

-- Users
CREATE INDEX "IX_Users_WindowsLogin" ON "Users" ("WindowsLogin");
CREATE INDEX "IX_Users_RoleId" ON "Users" ("RoleId");

-- UserDepartmentAccess
CREATE INDEX "IX_UserDepartmentAccess_UserId" ON "UserDepartmentAccess" ("UserId");
CREATE INDEX "IX_UserDepartmentAccess_DepartmentId" ON "UserDepartmentAccess" ("DepartmentId");

-- UserWarehouseAccess
CREATE INDEX "IX_UserWarehouseAccess_UserDeptAccessKey" ON "UserWarehouseAccess" ("UserDepartmentAccessKey");
CREATE INDEX "IX_UserWarehouseAccess_WarehouseId" ON "UserWarehouseAccess" ("WarehouseId");

-- EquipmentDependencies
CREATE INDEX "IX_EquipmentDependencies_MainEquipmentId" ON "EquipmentDependencies" ("MainEquipmentId");
CREATE INDEX "IX_EquipmentDependencies_DependentEquipmentId" ON "EquipmentDependencies" ("DependentEquipmentId");

-- AuditLogs
CREATE INDEX "IX_AuditLogs_TableName_RecordId" ON "AuditLogs" ("TableName", "RecordId");
CREATE INDEX "IX_AuditLogs_ChangedByUserId" ON "AuditLogs" ("ChangedByUserId");
CREATE INDEX "IX_AuditLogs_ChangedAt" ON "AuditLogs" ("ChangedAt");

-- =============================================
-- ВСТАВКА ТЕСТОВЫХ ДАННЫХ
-- =============================================

-- Вставляем системные роли
INSERT INTO "Roles" ("Id", "Name", "Code", "Description", "TAB_AuditLogs", "TAB_Departments", "TAB_EquipmentDependencies", "TAB_Equipments", "TAB_LessorOrganizations", "TAB_LicensePlates", "TAB_Roles", "TAB_ShiftRequests", "TAB_TransportProgram", "TAB_UserDepartmentAccess", "TAB_UserFavorites", "TAB_Users", "TAB_UserWarehouseAccess", "TAB_WarehouseAreas", "TAB_Warehouses", "SPEC_ExportData", "SPEC_ViewReports", "SPEC_ManageAllDepartments", "SPEC_ManageUsers", "SPEC_SystemAdmin", "IsSystem") VALUES 
('RL000001', 'Администратор', 'ADMIN', 'Полный доступ ко всем функциям системы', 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, true, true, true, true, true, true),
('RL000002', 'Менеджер', 'MANAGER', 'Управление заявками и справочниками', 1, 2, 2, 2, 2, 2, 0, 2, 2, 1, 2, 1, 1, 2, 2, true, true, true, false, false, true),
('RL000003', 'Инициатор', 'INITIATOR', 'Создание и просмотр заявок для своего отдела', 0, 1, 1, 1, 1, 1, 0, 2, 1, 0, 2, 0, 0, 1, 1, true, true, false, false, false, true),
('RL000004', 'Наблюдатель', 'VIEWER', 'Только просмотр данных', 0, 1, 1, 1, 1, 1, 0, 1, 1, 0, 1, 0, 0, 1, 1, true, true, false, false, false, true),
('RL000005', 'Руководитель отдела', 'DEPARTMENT_HEAD', 'Управление заявками своего отдела', 0, 1, 1, 1, 1, 1, 0, 2, 2, 0, 1, 0, 0, 1, 1, true, true, false, false, false, false);

-- Вставляем тестовые отделы
INSERT INTO "Departments" ("Id", "Name") VALUES 
('DE000001', 'Отдел логистики'),
('DE000002', 'Отдел строительства'),
('DE000003', 'Складской комплекс'),
('DE000004', 'Администрация');

-- Вставляем тестовые организации-арендодатели
INSERT INTO "LessorOrganizations" ("Id", "Name", "INN", "ContactPerson", "Phone") VALUES 
('LO000001', 'ООО "СтройТехСервис"', '7701234567', 'Иванов Петр Сергеевич', '+7 (495) 123-45-67'),
('LO000002', 'АО "ТрансМаш"', '7707654321', 'Сидорова Мария Ивановна', '+7 (495) 765-43-21'),
('LO000003', 'ЗАО "Механизация"', '7701112233', 'Петров Алексей Викторович', '+7 (495) 111-22-33');

-- Вставляем тестовую технику
INSERT INTO "Equipments" ("Id", "Name", "Category", "CanOrderMultiple", "HourlyCost", "RequiresOperator") VALUES 
('EQ000001', 'Автокран 25т', 'Спецтехника', false, 2500.00, true),
('EQ000002', 'Автокран 50т', 'Спецтехника', false, 4500.00, true),
('EQ000003', 'Автовышка 28м', 'Спецтехника', false, 1800.00, true),
('EQ000004', 'Бульдозер', 'Спецтехника', false, 2200.00, true),
('EQ000005', 'Экскаватор-погрузчик', 'Спецтехника', false, 2000.00, true),
('EQ000006', 'Стропальщик', 'Рабочий', true, 800.00, false),
('EQ000007', 'Мастер', 'Рабочий', true, 1200.00, false),
('EQ000008', 'Водитель', 'Рабочий', true, 900.00, false),
('EQ000009', 'Генератор 100кВт', 'Оборудование', true, 1500.00, false),
('EQ000010', 'Компрессор', 'Оборудование', true, 1200.00, false);

-- Вставляем зависимости (кран требует стропальщиков и мастера)
INSERT INTO "EquipmentDependencies" ("MainEquipmentId", "DependentEquipmentId", "RequiredCount", "Description") VALUES 
('EQ000001', 'EQ000006', 3, 'Требуется 3 стропальщика'),
('EQ000001', 'EQ000007', 1, 'Требуется 1 мастер'),
('EQ000002', 'EQ000006', 4, 'Требуется 4 стропальщика'),
('EQ000002', 'EQ000007', 1, 'Требуется 1 мастер');

-- Вставляем тестовые госномера
INSERT INTO "LicensePlates" ("Id", "PlateNumber", "EquipmentId", "LessorOrganizationId", "Brand", "Year") VALUES 
('LP000001', 'А123БВ77', 'EQ000001', 'LO000001', 'Liebherr LTM 1050-3.1', 2018),
('LP000002', 'В234СТ78', 'EQ000001', 'LO000001', 'XCMG QY25K5', 2020),
('LP000003', 'С345ДЕ79', 'EQ000002', 'LO000002', 'Grove GMK 3050', 2019),
('LP000004', 'Е456ФГ80', 'EQ000003', 'LO000001', 'Mantall M28JRT', 2021),
('LP000005', 'Ж567ХИ81', 'EQ000004', 'LO000003', 'Caterpillar D6', 2020);

-- Вставляем тестовые склады
INSERT INTO "Warehouses" ("Id", "Name", "DepartmentId", "Address") VALUES 
('WH000001', 'Склад №1 (Логистика)', 'DE000001', 'ул. Складская, д. 1'),
('WH000002', 'Склад стройматериалов', 'DE000002', 'ул. Строителей, д. 10'),
('WH000003', 'Логистический центр', 'DE000001', 'ул. Транспортная, д. 5'),
('WH000004', 'Административный склад', 'DE000004', 'ул. Центральная, д. 1');

-- Вставляем территории складов
INSERT INTO "WarehouseAreas" ("Id", "Name", "WarehouseId", "AreaType") VALUES 
('WA000001', 'Зона разгрузки', 'WH000001', 'Разгрузка'),
('WA000002', 'Сектор А', 'WH000001', 'Хранение'),
('WA000003', 'Сектор Б', 'WH000001', 'Хранение'),
('WA000004', 'Открытая площадка', 'WH000002', 'Открытое хранение'),
('WA000005', 'Крытый ангар', 'WH000002', 'Крытое хранение');

-- Вставляем тестового администратора
INSERT INTO "Users" ("Id", "WindowsLogin", "FullName", "RoleId", "HasAllDepartments") VALUES 
('US000001', 'DOMAIN\AdminUser', 'Иванов Иван Иванович', 'RL000001', true);

-- Добавляем доступ администратора ко всем отделам
INSERT INTO "UserDepartmentAccess" ("UserId", "DepartmentId", "HasAllWarehouses") VALUES 
('US000001', 'DE000001', true),
('US000001', 'DE000002', true),
('US000001', 'DE000003', true),
('US000001', 'DE000004', true);

-- Вставляем тестовую транспортную программу на текущий год
INSERT INTO "TransportProgram" ("DepartmentId", "Year", "EquipmentId", "HourlyCost", 
    "JanuaryHours", "FebruaryHours", "MarchHours", "AprilHours", "MayHours", "JuneHours",
    "JulyHours", "AugustHours", "SeptemberHours", "OctoberHours", "NovemberHours", "DecemberHours") 
VALUES 
('DE000001', EXTRACT(YEAR FROM CURRENT_DATE), 'EQ000001', 2500.00, 40, 35, 45, 50, 55, 60, 40, 35, 45, 50, 40, 30),
('DE000001', EXTRACT(YEAR FROM CURRENT_DATE), 'EQ000002', 4500.00, 20, 18, 25, 30, 35, 40, 25, 20, 30, 35, 25, 15),
('DE000002', EXTRACT(YEAR FROM CURRENT_DATE), 'EQ000003', 1800.00, 80, 75, 85, 90, 95, 100, 80, 75, 85, 90, 80, 70);

-- Вставляем тестовые избранные для пользователя
INSERT INTO "UserFavorites" ("UserId", "EquipmentId", "SortOrder") VALUES 
('US000001', 'EQ000001', 1),
('US000001', 'EQ000002', 2),
('US000001', 'EQ000006', 3);

-- Вставляем тестовую заявку
INSERT INTO "ShiftRequests" (
    "Date", "Shift", "EquipmentId", "LicensePlateId", "WarehouseId", "AreaId",
    "LessorOrganizationId", "RequestedCount", "WorkedHours", "ActualCost",
    "IsWorked", "IsBlocked", "Comment", "CreatedByUserId", "DepartmentId",
    "ProgramYear", "ProgramMonth"
) VALUES (
    CURRENT_DATE, 0, 'EQ000001', 'LP000001', 'WH000001', 'WA000001',
    'LO000001', 1, 8.5, 21250.00,
    true, false, 'Тестовая заявка на кран', 'US000001', 'DE000001',
    EXTRACT(YEAR FROM CURRENT_DATE), EXTRACT(MONTH FROM CURRENT_DATE)
);

-- =============================================
-- ПРОВЕРКА ДАННЫХ
-- =============================================
DO $$
BEGIN
    RAISE NOTICE '=============================================';
    RAISE NOTICE 'ПРОВЕРКА СГЕНЕРИРОВАННЫХ ДАННЫХ:';
    RAISE NOTICE '=============================================';
END $$;

SELECT 'Departments' AS "TableName", COUNT(*) AS "RecordCount" FROM "Departments"
UNION ALL
SELECT 'LessorOrganizations', COUNT(*) FROM "LessorOrganizations"
UNION ALL
SELECT 'Equipments', COUNT(*) FROM "Equipments"
UNION ALL
SELECT 'LicensePlates', COUNT(*) FROM "LicensePlates"
UNION ALL
SELECT 'EquipmentDependencies', COUNT(*) FROM "EquipmentDependencies"
UNION ALL
SELECT 'TransportProgram', COUNT(*) FROM "TransportProgram"
UNION ALL
SELECT 'Roles', COUNT(*) FROM "Roles"
UNION ALL
SELECT 'Users', COUNT(*) FROM "Users"
UNION ALL
SELECT 'Warehouses', COUNT(*) FROM "Warehouses"
UNION ALL
SELECT 'WarehouseAreas', COUNT(*) FROM "WarehouseAreas"
UNION ALL
SELECT 'ShiftRequests', COUNT(*) FROM "ShiftRequests"
UNION ALL
SELECT 'UserFavorites', COUNT(*) FROM "UserFavorites"
ORDER BY "TableName";

DO $$
BEGIN
    RAISE NOTICE '=============================================';
    RAISE NOTICE 'СИСТЕМА УСПЕШНО СОЗДАНА!';
    RAISE NOTICE '=============================================';
    RAISE NOTICE 'Данные для входа:';
    RAISE NOTICE '  Windows логин: DOMAIN\AdminUser';
    RAISE NOTICE '  Роль: Администратор (полный доступ)';
    RAISE NOTICE '  ID пользователя: US000001';
    RAISE NOTICE '=============================================';
END $$;