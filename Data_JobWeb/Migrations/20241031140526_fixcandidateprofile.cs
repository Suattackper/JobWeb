using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data_JobWeb.Migrations
{
    /// <inheritdoc />
    public partial class fixcandidateprofile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "user_id",
                table: "job_seeker_user_login_data");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "job_seeker_candidate_profile",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Fullname",
                table: "job_seeker_candidate_profile",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "job_seeker_candidate_profile");

            migrationBuilder.DropColumn(
                name: "Fullname",
                table: "job_seeker_candidate_profile");

            migrationBuilder.AddColumn<Guid>(
                name: "user_id",
                table: "job_seeker_user_login_data",
                type: "uniqueidentifier",
                nullable: true);
        }
    }
}
