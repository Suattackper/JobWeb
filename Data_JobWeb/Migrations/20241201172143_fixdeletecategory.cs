using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data_JobWeb.Migrations
{
    /// <inheritdoc />
    public partial class fixdeletecategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "job_posting_job_category_id_fk",
                table: "job_seeker_job_posting");

            migrationBuilder.DropForeignKey(
                name: "job_posting_id_fk",
                table: "job_seeker_job_posting_apply");

            migrationBuilder.DropForeignKey(
                name: "saved_job_posting_id_fk",
                table: "job_seeker_saved_job_posting");

            migrationBuilder.AddForeignKey(
                name: "job_posting_job_category_id_fk",
                table: "job_seeker_job_posting",
                column: "job_category_id",
                principalTable: "job_seeker_job_category",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "job_posting_id_fk",
                table: "job_seeker_job_posting_apply",
                column: "job_posting_id",
                principalTable: "job_seeker_job_posting",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "saved_job_posting_id_fk",
                table: "job_seeker_saved_job_posting",
                column: "job_posting_id",
                principalTable: "job_seeker_job_posting",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "job_posting_job_category_id_fk",
                table: "job_seeker_job_posting");

            migrationBuilder.DropForeignKey(
                name: "job_posting_id_fk",
                table: "job_seeker_job_posting_apply");

            migrationBuilder.DropForeignKey(
                name: "saved_job_posting_id_fk",
                table: "job_seeker_saved_job_posting");

            migrationBuilder.AddForeignKey(
                name: "job_posting_job_category_id_fk",
                table: "job_seeker_job_posting",
                column: "job_category_id",
                principalTable: "job_seeker_job_category",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "job_posting_id_fk",
                table: "job_seeker_job_posting_apply",
                column: "job_posting_id",
                principalTable: "job_seeker_job_posting",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "saved_job_posting_id_fk",
                table: "job_seeker_saved_job_posting",
                column: "job_posting_id",
                principalTable: "job_seeker_job_posting",
                principalColumn: "id");
        }
    }
}
