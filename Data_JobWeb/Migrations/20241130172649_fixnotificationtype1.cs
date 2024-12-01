using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data_JobWeb.Migrations
{
    /// <inheritdoc />
    public partial class fixnotificationtype1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "id_justforadmin",
                table: "job_seeker_notification_type");

            migrationBuilder.DropColumn(
                name: "is_seen_justforadmin",
                table: "job_seeker_notification_type");

            migrationBuilder.RenameColumn(
                name: "is_create_at_justforadmin",
                table: "job_seeker_notification_type",
                newName: "is_create_at");

            migrationBuilder.AddColumn<Guid>(
                name: "id_user",
                table: "job_seeker_notification_type",
                type: "uniqueidentifier",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "id_user",
                table: "job_seeker_notification_type");

            migrationBuilder.RenameColumn(
                name: "is_create_at",
                table: "job_seeker_notification_type",
                newName: "is_create_at_justforadmin");

            migrationBuilder.AddColumn<string>(
                name: "id_justforadmin",
                table: "job_seeker_notification_type",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_seen_justforadmin",
                table: "job_seeker_notification_type",
                type: "bit",
                nullable: true);
        }
    }
}
