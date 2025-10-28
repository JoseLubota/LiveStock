using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LiveStock.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AzureInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop existing FKs and tables to avoid conflicts on Azure SQL
            migrationBuilder.Sql(@"
                DECLARE @sql NVARCHAR(MAX) = N'';
                SELECT @sql += N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(t.schema_id)) + N'.' + QUOTENAME(t.name) +
                            N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';'
                FROM sys.foreign_keys fk
                JOIN sys.tables t ON fk.parent_object_id = t.object_id;
                IF LEN(@sql) > 0 EXEC sp_executesql @sql;
    
                IF OBJECT_ID(N'[dbo].[FarmTasks]', N'U') IS NOT NULL DROP TABLE [dbo].[FarmTasks];
                IF OBJECT_ID(N'[dbo].[Messages]', N'U') IS NOT NULL DROP TABLE [dbo].[Messages];
                IF OBJECT_ID(N'[dbo].[Cows]', N'U') IS NOT NULL DROP TABLE [dbo].[Cows];
                IF OBJECT_ID(N'[dbo].[Sheep]', N'U') IS NOT NULL DROP TABLE [dbo].[Sheep];
                IF OBJECT_ID(N'[dbo].[CampMovements]', N'U') IS NOT NULL DROP TABLE [dbo].[CampMovements];
                IF OBJECT_ID(N'[dbo].[RainfallRecords]', N'U') IS NOT NULL DROP TABLE [dbo].[RainfallRecords];
                IF OBJECT_ID(N'[dbo].[MedicalRecords]', N'U') IS NOT NULL DROP TABLE [dbo].[MedicalRecords];
                IF OBJECT_ID(N'[dbo].[FinancialRecords]', N'U') IS NOT NULL DROP TABLE [dbo].[FinancialRecords];
                IF OBJECT_ID(N'[dbo].[Assets]', N'U') IS NOT NULL DROP TABLE [dbo].[Assets];
                IF OBJECT_ID(N'[dbo].[Notes]', N'U') IS NOT NULL DROP TABLE [dbo].[Notes];
                IF OBJECT_ID(N'[dbo].[Staff]', N'U') IS NOT NULL DROP TABLE [dbo].[Staff];
                IF OBJECT_ID(N'[dbo].[Camps]', N'U') IS NOT NULL DROP TABLE [dbo].[Camps];
            ");

            migrationBuilder.CreateTable(
                name: "Assets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PurchasePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    PurchaseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Camps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CampNumber = table.Column<int>(type: "int", nullable: false),
                    Hectares = table.Column<double>(type: "float", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Camps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FinancialRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Reference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialRecords", x => x.Id);
                });

            // Guard against existing Notes table from pre-existing scripts
            migrationBuilder.Sql(@"IF OBJECT_ID(N'[dbo].[Notes]', N'U') IS NOT NULL BEGIN DROP TABLE [dbo].[Notes]; END");

            migrationBuilder.CreateTable(
                name: "Notes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Staff",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EmployeeId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Staff", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EarTag = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPregnant = table.Column<bool>(type: "bit", nullable: false),
                    ExpectedCalvingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Breed = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CampId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cows_Camps_CampId",
                        column: x => x.CampId,
                        principalTable: "Camps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RainfallRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CampId = table.Column<int>(type: "int", nullable: false),
                    RainfallDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AmountMl = table.Column<double>(type: "float", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RainfallRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RainfallRecords_Camps_CampId",
                        column: x => x.CampId,
                        principalTable: "Camps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sheep",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SheepID = table.Column<int>(type: "int", nullable: false),
                    PhotoID = table.Column<int>(type: "int", nullable: true),
                    Breed = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CampId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sheep", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sheep_Camps_CampId",
                        column: x => x.CampId,
                        principalTable: "Camps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FarmTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AssignedToId = table.Column<int>(type: "int", nullable: false),
                    Importance = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedById = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FarmTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FarmTasks_Staff_AssignedToId",
                        column: x => x.AssignedToId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FarmTasks_Staff_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SenderId = table.Column<int>(type: "int", nullable: false),
                    RecipientId = table.Column<int>(type: "int", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsBroadcast = table.Column<bool>(type: "bit", nullable: false),
                    AttachmentUrl = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_Staff_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Messages_Staff_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CampMovements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnimalId = table.Column<int>(type: "int", nullable: false),
                    AnimalType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FromCampId = table.Column<int>(type: "int", nullable: false),
                    ToCampId = table.Column<int>(type: "int", nullable: false),
                    MovementDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CampId = table.Column<int>(type: "int", nullable: true),
                    CowId = table.Column<int>(type: "int", nullable: true),
                    SheepId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampMovements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampMovements_Camps_CampId",
                        column: x => x.CampId,
                        principalTable: "Camps",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CampMovements_Camps_FromCampId",
                        column: x => x.FromCampId,
                        principalTable: "Camps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CampMovements_Camps_ToCampId",
                        column: x => x.ToCampId,
                        principalTable: "Camps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CampMovements_Cows_CowId",
                        column: x => x.CowId,
                        principalTable: "Cows",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CampMovements_Sheep_SheepId",
                        column: x => x.SheepId,
                        principalTable: "Sheep",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MedicalRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnimalId = table.Column<int>(type: "int", nullable: false),
                    AnimalType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Treatment = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TreatmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Veterinarian = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Cost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CowId = table.Column<int>(type: "int", nullable: true),
                    SheepId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicalRecords_Cows_CowId",
                        column: x => x.CowId,
                        principalTable: "Cows",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MedicalRecords_Sheep_SheepId",
                        column: x => x.SheepId,
                        principalTable: "Sheep",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "Assets",
                columns: new[] { "Id", "Category", "CreatedAt", "Description", "ExpiryDate", "Location", "Name", "PurchaseDate", "PurchasePrice", "Quantity", "Status", "Type", "Unit", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "Fencing", new DateTime(2025, 8, 26, 18, 45, 57, 756, DateTimeKind.Utc).AddTicks(3010), "High-quality barbed wire for camp fencing", null, "Storage Shed A", "Fencing Wire", new DateTime(2025, 8, 26, 18, 45, 57, 756, DateTimeKind.Utc).AddTicks(1230), 25.00m, 50, "Active", "Barbed Wire", "rolls", null },
                    { 2, "Feed", new DateTime(2025, 9, 25, 18, 45, 57, 756, DateTimeKind.Utc).AddTicks(4240), "Premium grain mix for livestock", new DateTime(2026, 4, 25, 18, 45, 57, 756, DateTimeKind.Utc).AddTicks(3570), "Feed Storage", "Livestock Feed", new DateTime(2025, 9, 25, 18, 45, 57, 756, DateTimeKind.Utc).AddTicks(3560), 0.80m, 1000, "Active", "Grain Mix", "kg", null }
                });

            migrationBuilder.InsertData(
                table: "Camps",
                columns: new[] { "Id", "CampNumber", "Description", "Hectares", "Name" },
                values: new object[,]
                {
                    { 1, 1, "Camp 1 - Livestock grazing area", 153.33000000000001, "Camp 1" },
                    { 2, 2, "Camp 2 - Livestock grazing area", 153.33000000000001, "Camp 2" },
                    { 3, 3, "Camp 3 - Livestock grazing area", 153.33000000000001, "Camp 3" },
                    { 4, 4, "Camp 4 - Livestock grazing area", 153.33000000000001, "Camp 4" },
                    { 5, 5, "Camp 5 - Livestock grazing area", 153.33000000000001, "Camp 5" },
                    { 6, 6, "Camp 6 - Livestock grazing area", 153.33000000000001, "Camp 6" },
                    { 7, 7, "Camp 7 - Livestock grazing area", 153.33000000000001, "Camp 7" },
                    { 8, 8, "Camp 8 - Livestock grazing area", 153.33000000000001, "Camp 8" },
                    { 9, 9, "Camp 9 - Livestock grazing area", 153.33000000000001, "Camp 9" },
                    { 10, 10, "Camp 10 - Livestock grazing area", 153.33000000000001, "Camp 10" },
                    { 11, 11, "Camp 11 - Livestock grazing area", 153.33000000000001, "Camp 11" },
                    { 12, 12, "Camp 12 - Livestock grazing area", 153.33000000000001, "Camp 12" },
                    { 13, 13, "Camp 13 - Livestock grazing area", 153.33000000000001, "Camp 13" },
                    { 14, 14, "Camp 14 - Livestock grazing area", 153.33000000000001, "Camp 14" },
                    { 15, 15, "Camp 15 - Livestock grazing area", 153.33000000000001, "Camp 15" }
                });

            migrationBuilder.InsertData(
                table: "FinancialRecords",
                columns: new[] { "Id", "Amount", "Category", "CreatedAt", "Description", "Notes", "Reference", "TransactionDate", "Type", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 450.00m, "Livestock Sales", new DateTime(2025, 10, 15, 18, 45, 57, 755, DateTimeKind.Utc).AddTicks(4200), "Sheep Sale - Merino Ewe", null, "INV-001", new DateTime(2025, 10, 15, 18, 45, 57, 755, DateTimeKind.Utc).AddTicks(2410), "Income", null },
                    { 2, 150.00m, "Veterinary", new DateTime(2025, 10, 10, 18, 45, 57, 755, DateTimeKind.Utc).AddTicks(4770), "Veterinary Services", null, "REC-001", new DateTime(2025, 10, 10, 18, 45, 57, 755, DateTimeKind.Utc).AddTicks(4770), "Expense", null },
                    { 3, 300.00m, "Feed", new DateTime(2025, 10, 5, 18, 45, 57, 755, DateTimeKind.Utc).AddTicks(4770), "Feed Purchase", null, "INV-002", new DateTime(2025, 10, 5, 18, 45, 57, 755, DateTimeKind.Utc).AddTicks(4770), "Expense", null }
                });

            migrationBuilder.InsertData(
                table: "Staff",
                columns: new[] { "Id", "CreatedAt", "Email", "EmployeeId", "IsActive", "Name", "PhoneNumber", "Role", "UpdatedAt" },
                values: new object[] { 1, new DateTime(2025, 10, 25, 18, 45, 57, 754, DateTimeKind.Utc).AddTicks(8340), "owner@farm.com", "FARM001", true, "Farm Owner", "+1234567890", "Farmer", null });

            migrationBuilder.CreateIndex(
                name: "IX_CampMovements_CampId",
                table: "CampMovements",
                column: "CampId");

            migrationBuilder.CreateIndex(
                name: "IX_CampMovements_CowId",
                table: "CampMovements",
                column: "CowId");

            migrationBuilder.CreateIndex(
                name: "IX_CampMovements_FromCampId",
                table: "CampMovements",
                column: "FromCampId");

            migrationBuilder.CreateIndex(
                name: "IX_CampMovements_SheepId",
                table: "CampMovements",
                column: "SheepId");

            migrationBuilder.CreateIndex(
                name: "IX_CampMovements_ToCampId",
                table: "CampMovements",
                column: "ToCampId");

            migrationBuilder.CreateIndex(
                name: "IX_Cows_CampId",
                table: "Cows",
                column: "CampId");

            migrationBuilder.CreateIndex(
                name: "IX_FarmTasks_AssignedToId",
                table: "FarmTasks",
                column: "AssignedToId");

            migrationBuilder.CreateIndex(
                name: "IX_FarmTasks_CreatedById",
                table: "FarmTasks",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecords_CowId",
                table: "MedicalRecords",
                column: "CowId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecords_SheepId",
                table: "MedicalRecords",
                column: "SheepId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_RecipientId",
                table: "Messages",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SenderId",
                table: "Messages",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_RainfallRecords_CampId",
                table: "RainfallRecords",
                column: "CampId");

            migrationBuilder.CreateIndex(
                name: "IX_Sheep_CampId",
                table: "Sheep",
                column: "CampId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Assets");

            migrationBuilder.DropTable(
                name: "CampMovements");

            migrationBuilder.DropTable(
                name: "FarmTasks");

            migrationBuilder.DropTable(
                name: "FinancialRecords");

            migrationBuilder.DropTable(
                name: "MedicalRecords");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Notes");

            migrationBuilder.DropTable(
                name: "RainfallRecords");

            migrationBuilder.DropTable(
                name: "Cows");

            migrationBuilder.DropTable(
                name: "Sheep");

            migrationBuilder.DropTable(
                name: "Staff");

            migrationBuilder.DropTable(
                name: "Camps");
        }
    }
}
