using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Insurance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAiFieldsToClaim : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AiExplanation",
                table: "Claims",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "AiFraudProbability",
                table: "Claims",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AiRecommendation",
                table: "Claims",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AiRiskLevel",
                table: "Claims",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AiRiskScore",
                table: "Claims",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AiSource",
                table: "Claims",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AiExplanation",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "AiFraudProbability",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "AiRecommendation",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "AiRiskLevel",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "AiRiskScore",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "AiSource",
                table: "Claims");
        }
    }
}
