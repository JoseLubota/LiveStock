using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LiveStock.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnsureAnimalStatusColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ensure Sheep.Status exists
            migrationBuilder.Sql(@"
                IF COL_LENGTH('dbo.Sheep', 'Status') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[Sheep]
                    ADD [Status] nvarchar(max) NOT NULL CONSTRAINT DF_Sheep_Status DEFAULT ('Active');
                END
            ");

            // Ensure Cows.Status exists
            migrationBuilder.Sql(@"
                IF COL_LENGTH('dbo.Cows', 'Status') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[Cows]
                    ADD [Status] nvarchar(max) NOT NULL CONSTRAINT DF_Cows_Status DEFAULT ('Active');
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Down intentionally left empty; dropping columns with default constraints
            // requires dynamic discovery of constraint names in SQL Server.
        }
    }
}
