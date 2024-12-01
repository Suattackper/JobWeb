using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data_JobWeb.Migrations
{
    /// <inheritdoc />
    public partial class fixdeledjobfield : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "job_field_id_fk",
                table: "job_seeker_enterprise");

            migrationBuilder.AddForeignKey(
                name: "job_field_id_fk",
                table: "job_seeker_enterprise",
                column: "job_field_id",
                principalTable: "job_seeker_job_field",
                principalColumn: "job_field_id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "job_field_id_fk",
                table: "job_seeker_enterprise");

            migrationBuilder.AddForeignKey(
                name: "job_field_id_fk",
                table: "job_seeker_enterprise",
                column: "job_field_id",
                principalTable: "job_seeker_job_field",
                principalColumn: "job_field_id");
        }
    }
}
