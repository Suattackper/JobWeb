using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data_JobWeb.Migrations
{
    /// <inheritdoc />
    public partial class addtableenterprisedfollowed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "job_seeker_enterprise_followed",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    candidate_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    enterprise_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    is_created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    is_updated_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    is_deleted_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__job_seek__3213E83F1E4AB212", x => x.id);
                    table.ForeignKey(
                        name: "enterprise_followed_candidate_id_fk",
                        column: x => x.candidate_id,
                        principalTable: "job_seeker_candidate_profile",
                        principalColumn: "candidate_id");
                    table.ForeignKey(
                        name: "enterprise_followed_enterprise_id_fk",
                        column: x => x.enterprise_id,
                        principalTable: "job_seeker_enterprise",
                        principalColumn: "enterprise_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_job_seeker_enterprise_followed_candidate_id",
                table: "job_seeker_enterprise_followed",
                column: "candidate_id");

            migrationBuilder.CreateIndex(
                name: "IX_job_seeker_enterprise_followed_enterprise_id",
                table: "job_seeker_enterprise_followed",
                column: "enterprise_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "job_seeker_enterprise_followed");
        }
    }
}
