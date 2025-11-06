using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace proiect.Migrations
{
    public partial class RemoveIsWarningFromMessage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsWarning",
                table: "Messages");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsWarning",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}