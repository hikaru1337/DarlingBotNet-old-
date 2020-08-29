using Microsoft.EntityFrameworkCore.Migrations;

namespace DarlingBotNet.Migrations
{
    public partial class mss : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "UseRPcommand",
                table: "Channels",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SendUrlImage",
                table: "Channels",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "csUrlWhiteListString",
                table: "Channels",
                nullable: true,
                defaultValue: null);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
