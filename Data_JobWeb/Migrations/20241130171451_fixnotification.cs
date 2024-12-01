using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data_JobWeb.Migrations
{
    /// <inheritdoc />
    public partial class fixnotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "job_posting_notify_id_fk",
                table: "job_seeker_notification");

            migrationBuilder.DropForeignKey(
                name: "notify_type_id_fk",
                table: "job_seeker_notification");

            migrationBuilder.DropForeignKey(
                name: "user_login_data_id_fk",
                table: "job_seeker_notification");

            migrationBuilder.DropIndex(
                name: "IX_job_seeker_notification_job_id",
                table: "job_seeker_notification");

            migrationBuilder.DropIndex(
                name: "IX_job_seeker_notification_notify_type_id",
                table: "job_seeker_notification");

            migrationBuilder.DropIndex(
                name: "IX_job_seeker_notification_user_login_data_id",
                table: "job_seeker_notification");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "job_seeker_notification");

            migrationBuilder.DropColumn(
                name: "notify_type_id",
                table: "job_seeker_notification");

            migrationBuilder.RenameColumn(
                name: "user_login_data_id",
                table: "job_seeker_notification",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "job_id",
                table: "job_seeker_notification",
                newName: "concern_id");

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "job_seeker_notification",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "title",
                table: "job_seeker_notification",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "type_name",
                table: "job_seeker_notification",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "description",
                table: "job_seeker_notification");

            migrationBuilder.DropColumn(
                name: "title",
                table: "job_seeker_notification");

            migrationBuilder.DropColumn(
                name: "type_name",
                table: "job_seeker_notification");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "job_seeker_notification",
                newName: "user_login_data_id");

            migrationBuilder.RenameColumn(
                name: "concern_id",
                table: "job_seeker_notification",
                newName: "job_id");

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "job_seeker_notification",
                type: "bit",
                nullable: true,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "notify_type_id",
                table: "job_seeker_notification",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_job_seeker_notification_job_id",
                table: "job_seeker_notification",
                column: "job_id");

            migrationBuilder.CreateIndex(
                name: "IX_job_seeker_notification_notify_type_id",
                table: "job_seeker_notification",
                column: "notify_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_job_seeker_notification_user_login_data_id",
                table: "job_seeker_notification",
                column: "user_login_data_id");

            migrationBuilder.AddForeignKey(
                name: "job_posting_notify_id_fk",
                table: "job_seeker_notification",
                column: "job_id",
                principalTable: "job_seeker_job_posting",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "notify_type_id_fk",
                table: "job_seeker_notification",
                column: "notify_type_id",
                principalTable: "job_seeker_notification_type",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "user_login_data_id_fk",
                table: "job_seeker_notification",
                column: "user_login_data_id",
                principalTable: "job_seeker_user_login_data",
                principalColumn: "id");
        }
    }
}
