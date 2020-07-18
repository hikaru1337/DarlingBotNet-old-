using Microsoft.EntityFrameworkCore.Migrations;

namespace DarlingBotNet.Migrations
{
    public partial class marryedadd : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ulong>(
                name: "marryedid",
                table: "Users",
                nullable: false,
                defaultValue: 0ul);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "marryedid",
                table: "Users");
        }
    }
}
