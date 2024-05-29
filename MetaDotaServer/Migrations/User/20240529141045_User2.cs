using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetaDotaServer.Migrations.User
{
    /// <inheritdoc />
    public partial class User2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Jwt",
                table: "User",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Jwt",
                table: "User");
        }
    }
}
