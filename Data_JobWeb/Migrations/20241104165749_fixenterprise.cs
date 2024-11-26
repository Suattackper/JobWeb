using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data_JobWeb.Migrations
{
    /// <inheritdoc />
    public partial class fixenterprise : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "city",
                table: "job_seeker_enterprise",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "city",
                table: "job_seeker_enterprise");
        }
    }
}
