using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AssignedToAddedToProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignedToId",
                table: "Projects",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_AssignedToId",
                table: "Projects",
                column: "AssignedToId");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_AspNetUsers_AssignedToId",
                table: "Projects",
                column: "AssignedToId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_AspNetUsers_AssignedToId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_AssignedToId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "AssignedToId",
                table: "Projects");
        }
    }
}
