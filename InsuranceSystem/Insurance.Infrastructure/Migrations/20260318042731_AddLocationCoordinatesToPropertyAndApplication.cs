using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Insurance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationCoordinatesToPropertyAndApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Properties",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Properties",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "PolicyApplications",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "PolicyApplications",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "PolicyApplications");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "PolicyApplications");
        }
    }
}
