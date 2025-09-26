using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Agitprop.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class Addinfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "entities",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "articles",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "entities");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "articles");
        }
    }
}
