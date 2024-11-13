using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectEstágio.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedEquipmentRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Module",
                table: "Veicules",
                newName: "Name");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "EquipmentRequests",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "EquipmentRequests");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Veicules",
                newName: "Module");
        }
    }
}
