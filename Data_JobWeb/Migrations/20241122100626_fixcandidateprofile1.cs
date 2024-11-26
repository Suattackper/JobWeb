using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data_JobWeb.Migrations
{
    /// <inheritdoc />
    public partial class fixcandidateprofile1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ward",
                table: "job_seeker_candidate_profile",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ward",
                table: "job_seeker_candidate_profile");
        }
    }
}
