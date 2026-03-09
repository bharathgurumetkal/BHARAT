using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Insurance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClaimOfficerAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AssignedOfficerId",
                table: "Claims",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Claims_AssignedOfficerId",
                table: "Claims",
                column: "AssignedOfficerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Claims_ClaimsOfficers_AssignedOfficerId",
                table: "Claims",
                column: "AssignedOfficerId",
                principalTable: "ClaimsOfficers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Claims_ClaimsOfficers_AssignedOfficerId",
                table: "Claims");

            migrationBuilder.DropIndex(
                name: "IX_Claims_AssignedOfficerId",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "AssignedOfficerId",
                table: "Claims");
        }
    }
}
