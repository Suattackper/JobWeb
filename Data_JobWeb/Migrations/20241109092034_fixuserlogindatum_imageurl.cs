using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data_JobWeb.Migrations
{
    /// <inheritdoc />
    public partial class fixuserlogindatum_imageurl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "avartar_url",
                table: "job_seeker_user_login_data",
                type: "varchar(500)",
                unicode: false,
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "avartar_url",
                table: "job_seeker_user_login_data");
        }
    }
}
