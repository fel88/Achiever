using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Achiever.Migrations
{
    /// <inheritdoc />
    public partial class Add_User_XmlConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "XmlConfig",
                table: "Users",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "XmlConfig",
                table: "Users");
        }
    }
}
