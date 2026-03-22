using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Insurance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIncidentDataToClaim : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DamageSeverity",
                table: "Claims",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DamageType",
                table: "Claims",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "IncidentDate",
                table: "Claims",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IncidentLocation",
                table: "Claims",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DamageSeverity",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "DamageType",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "IncidentDate",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "IncidentLocation",
                table: "Claims");
        }
    }
}
