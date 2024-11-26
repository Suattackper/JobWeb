using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data_JobWeb.Migrations
{
    /// <inheritdoc />
    public partial class fixrecruiterprofile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Fullname",
                table: "job_seeker_candidate_profile",
                newName: "fullname");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "job_seeker_candidate_profile",
                newName: "email");

            migrationBuilder.AddColumn<string>(
                name: "email",
                table: "job_seeker_recruiter_profile",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "fullname",
                table: "job_seeker_recruiter_profile",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "fullname",
                table: "job_seeker_candidate_profile",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "job_seeker_candidate_profile",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "email",
                table: "job_seeker_recruiter_profile");

            migrationBuilder.DropColumn(
                name: "fullname",
                table: "job_seeker_recruiter_profile");

            migrationBuilder.RenameColumn(
                name: "fullname",
                table: "job_seeker_candidate_profile",
                newName: "Fullname");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "job_seeker_candidate_profile",
                newName: "Email");

            migrationBuilder.AlterColumn<string>(
                name: "Fullname",
                table: "job_seeker_candidate_profile",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "job_seeker_candidate_profile",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);
        }
    }
}
