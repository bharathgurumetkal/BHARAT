using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Insurance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAiProspectingCache : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "AiChurnProbability",
                table: "Customers",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AiExplanation",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AiLastAnalyzedAt",
                table: "Customers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AiLikelihood",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AiRecommendedAction",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AiRenewalScore",
                table: "Customers",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AiChurnProbability",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "AiExplanation",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "AiLastAnalyzedAt",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "AiLikelihood",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "AiRecommendedAction",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "AiRenewalScore",
                table: "Customers");
        }
    }
}
