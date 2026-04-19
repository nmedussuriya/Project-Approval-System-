using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PAS_Full_System.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GroupMemberIds",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GroupMemberNames",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProposalFileName",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProposalFilePath",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GroupMemberIds",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "GroupMemberNames",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ProposalFileName",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ProposalFilePath",
                table: "Projects");
        }
    }
}
