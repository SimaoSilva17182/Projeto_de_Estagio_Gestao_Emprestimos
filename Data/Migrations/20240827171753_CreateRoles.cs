using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectEstágio.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "RoomRequests",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_RoomRequests_UserId",
                table: "RoomRequests",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_RoomRequests_AspNetUsers_UserId",
                table: "RoomRequests",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoomRequests_AspNetUsers_UserId",
                table: "RoomRequests");

            migrationBuilder.DropIndex(
                name: "IX_RoomRequests_UserId",
                table: "RoomRequests");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "RoomRequests");
        }
    }
}
