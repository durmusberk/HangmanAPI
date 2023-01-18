using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hangman.Migrations
{
    /// <inheritdoc />
    public partial class SessionTableWithChangedAFieldName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Guessed",
                table: "Sessions",
                newName: "IsGuessed");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsGuessed",
                table: "Sessions",
                newName: "Guessed");
        }
    }
}
