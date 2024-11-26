using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data_JobWeb.Migrations
{
    /// <inheritdoc />
    public partial class createdb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "authentication_permission",
                columns: table => new
                {
                    permission_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    permission_descri = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__authenti__E5331AFA559E7FF8", x => x.permission_id);
                });

            migrationBuilder.CreateTable(
                name: "authentication_role",
                columns: table => new
                {
                    role_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    role_name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__authenti__760965CC6BFE40AD", x => x.role_id);
                });

            migrationBuilder.CreateTable(
                name: "job_seeker_job_category",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    job_category_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    app_icon_name = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    is_created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    is_updated_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__job_seek__3213E83F22755607", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "job_seeker_job_field",
                columns: table => new
                {
                    job_field_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    job_field_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__job_seek__326B06EF6FD2D7D7", x => x.job_field_id);
                });

            migrationBuilder.CreateTable(
                name: "job_seeker_job_level",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    job_level_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__job_seek__3213E83FFBA46985", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "job_seeker_notification_type",
                columns: table => new
                {
                    id = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    type_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__job_seek__3213E83FFFCA9F52", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "job_seeker_province",
                columns: table => new
                {
                    code = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    province_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__job_seek__357D4CF8DE0F9512", x => x.code);
                });

            migrationBuilder.CreateTable(
                name: "job_seeker_status_code",
                columns: table => new
                {
                    id = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: false),
                    code_value = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__job_seek__3213E83F32974630", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "job_seeker_user_login_data_external",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newsequentialid())"),
                    provider_name = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    external_provider_token = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    ws_endpoint = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    extra_data = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    is_created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    is_updated_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__job_seek__3213E83FBF2934B2", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "authentication_granted_permissions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    role_id = table.Column<int>(type: "int", nullable: true),
                    permission_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__authenti__3213E83F4C943339", x => x.id);
                    table.ForeignKey(
                        name: "permission_id_fk",
                        column: x => x.permission_id,
                        principalTable: "authentication_permission",
                        principalColumn: "permission_id");
                    table.ForeignKey(
                        name: "role_id_fk",
                        column: x => x.role_id,
                        principalTable: "authentication_role",
                        principalColumn: "role_id");
                });

            migrationBuilder.CreateTable(
                name: "job_seeker_enterprise",
                columns: table => new
                {
                    enterprise_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newsequentialid())"),
                    full_company_name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    logo_url = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    company_email = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    company_phone_contact = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    cover_img_url = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    slug_img = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    facebook_url = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    linkedin_url = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    website_url = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    tax_code = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    founded_date = table.Column<DateOnly>(type: "date", nullable: true),
                    enterprise_size = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    job_field_id = table.Column<int>(type: "int", nullable: true),
                    address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    province = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    district = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ward = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    descriptions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    is_created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    is_updated_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__job_seek__A541BC651EBE1726", x => x.enterprise_id);
                    table.ForeignKey(
                        name: "job_field_id_fk",
                        column: x => x.job_field_id,
                        principalTable: "job_seeker_job_field",
                        principalColumn: "job_field_id");
                });

            migrationBuilder.CreateTable(
                name: "job_seeker_district",
                columns: table => new
                {
                    code = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    district_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    province_code = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__job_seek__357D4CF830DFBA8A", x => x.code);
                    table.ForeignKey(
                        name: "province_code_fk",
                        column: x => x.province_code,
                        principalTable: "job_seeker_province",
                        principalColumn: "code");
                });

            migrationBuilder.CreateTable(
                name: "job_seeker_user_login_data",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newsequentialid())"),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    full_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    email = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    password = table.Column<string>(type: "varchar(512)", unicode: false, maxLength: 512, nullable: true),
                    role_id = table.Column<int>(type: "int", nullable: true),
                    email_verified = table.Column<bool>(type: "bit", nullable: true),
                    status_code = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: true),
                    last_active_time = table.Column<DateTime>(type: "datetime", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    is_created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    is_updated_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__job_seek__3213E83F122A0F1D", x => x.id);
                    table.ForeignKey(
                        name: "user_login_data_status_code_fk",
                        column: x => x.status_code,
                        principalTable: "job_seeker_status_code",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "job_seeker_job_posting",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newsequentialid())"),
                    job_title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    job_desc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    job_requirement = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    benefit_enjoyed = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    quantity = table.Column<int>(type: "int", nullable: true),
                    salary_min = table.Column<int>(type: "int", nullable: true),
                    salary_max = table.Column<int>(type: "int", nullable: true),
                    exp_requirement = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    job_level_code = table.Column<int>(type: "int", nullable: true),
                    working_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    gender_require = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    academic_level = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    address = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    province = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    district = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ward = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    time_post = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    expired_time = table.Column<DateTime>(type: "datetime", nullable: true),
                    is_urgent = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    is_hot = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    status_code = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: true),
                    enterprise_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    job_category_id = table.Column<int>(type: "int", nullable: true),
                    is_created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    is_updated_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    is_deleted_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__job_seek__3213E83FB1615EAF", x => x.id);
                    table.ForeignKey(
                        name: "job_posting_enterprise_id_fk",
                        column: x => x.enterprise_id,
                        principalTable: "job_seeker_enterprise",
                        principalColumn: "enterprise_id");
                    table.ForeignKey(
                        name: "job_posting_job_category_id_fk",
                        column: x => x.job_category_id,
                        principalTable: "job_seeker_job_category",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "job_posting_job_level_code_fk",
                        column: x => x.job_level_code,
                        principalTable: "job_seeker_job_level",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "job_posting_status_code_fk",
                        column: x => x.status_code,
                        principalTable: "job_seeker_status_code",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "job_seeker_ward",
                columns: table => new
                {
                    code = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    ward_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    district_code = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__job_seek__357D4CF802F04ADC", x => x.code);
                    table.ForeignKey(
                        name: "district_code_fk",
                        column: x => x.district_code,
                        principalTable: "job_seeker_district",
                        principalColumn: "code");
                });

            migrationBuilder.CreateTable(
                name: "job_seeker_candidate_profile",
                columns: table => new
                {
                    candidate_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newsequentialid())"),
                    dob = table.Column<DateOnly>(type: "date", nullable: true),
                    gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    phone_numb = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    avartar_url = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    cv_url = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    slug = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    facbook_link = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    linkedin_link = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    github_url = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    twitter_url = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    portfolio_url = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    province = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    district = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    role_id = table.Column<int>(type: "int", nullable: true),
                    is_allowed_public = table.Column<bool>(type: "bit", nullable: true),
                    is_created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    is_updated_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    is_deleted_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    address_detail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__job_seek__39BD4C188AE6E8E9", x => x.candidate_id);
                    table.ForeignKey(
                        name: "candidate_id_external_fk",
                        column: x => x.candidate_id,
                        principalTable: "job_seeker_user_login_data_external",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "candidate_id_fk",
                        column: x => x.candidate_id,
                        principalTable: "job_seeker_user_login_data",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "role_id_two_fk",
                        column: x => x.role_id,
                        principalTable: "authentication_role",
                        principalColumn: "role_id");
                });

            migrationBuilder.CreateTable(
                name: "job_seeker_recruiter_profile",
                columns: table => new
                {
                    recruiter_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newsequentialid())"),
                    gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    phone_numb = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    avatar_link = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    linkedin_url = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    enterprise_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    role_id = table.Column<int>(type: "int", nullable: true),
                    is_created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    is_updated_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__job_seek__42ABA257E995083D", x => x.recruiter_id);
                    table.ForeignKey(
                        name: "enterprise_id_fk",
                        column: x => x.enterprise_id,
                        principalTable: "job_seeker_enterprise",
                        principalColumn: "enterprise_id");
                    table.ForeignKey(
                        name: "recruiter_id_fk",
                        column: x => x.recruiter_id,
                        principalTable: "job_seeker_user_login_data",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "role_id_three_fk",
                        column: x => x.role_id,
                        principalTable: "authentication_role",
                        principalColumn: "role_id");
                });

            migrationBuilder.CreateTable(
                name: "job_seeker_notification",
                columns: table => new
                {
                    id = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    notify_type_id = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    user_login_data_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    job_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    is_created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    is_sent = table.Column<bool>(type: "bit", nullable: true),
                    is_seen = table.Column<bool>(type: "bit", nullable: true),
                    is_deleted = table.Column<bool>(type: "bit", nullable: true, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__job_seek__3213E83F63136065", x => x.id);
                    table.ForeignKey(
                        name: "job_posting_notify_id_fk",
                        column: x => x.job_id,
                        principalTable: "job_seeker_job_posting",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "notify_type_id_fk",
                        column: x => x.notify_type_id,
                        principalTable: "job_seeker_notification_type",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "user_login_data_id_fk",
                        column: x => x.user_login_data_id,
                        principalTable: "job_seeker_user_login_data",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "job_seeker_applicant_profile_saved",
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
                    table.PrimaryKey("PK__job_seek__3213E83F1E4AB249", x => x.id);
                    table.ForeignKey(
                        name: "profile_saved_candidate_id_fk",
                        column: x => x.candidate_id,
                        principalTable: "job_seeker_candidate_profile",
                        principalColumn: "candidate_id");
                    table.ForeignKey(
                        name: "profile_saved_enterprise_id_fk",
                        column: x => x.enterprise_id,
                        principalTable: "job_seeker_enterprise",
                        principalColumn: "enterprise_id");
                });

            migrationBuilder.CreateTable(
                name: "job_seeker_certificate",
                columns: table => new
                {
                    certificate_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newsequentialid())"),
                    certificate_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    organization = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    certificate_link = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    candidate_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    is_created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    is_updated_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    is_deleted_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__job_seek__E2256D31F6A5AB8B", x => x.certificate_id);
                    table.ForeignKey(
                        name: "certificate_candidate_id_fk",
                        column: x => x.candidate_id,
                        principalTable: "job_seeker_candidate_profile",
                        principalColumn: "candidate_id");
                });

            migrationBuilder.CreateTable(
                name: "job_seeker_education_detail",
                columns: table => new
                {
                    education_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newsequentialid())"),
                    school_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    major = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    degree = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    candidate_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    is_created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    is_updated_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    is_deleted_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__job_seek__45C0CFE73E934531", x => x.education_id);
                    table.ForeignKey(
                        name: "education_detail_candidate_id_fk",
                        column: x => x.candidate_id,
                        principalTable: "job_seeker_candidate_profile",
                        principalColumn: "candidate_id");
                });

            migrationBuilder.CreateTable(
                name: "job_seeker_job_posting_apply",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    job_posting_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    candidate_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    apply_time = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    status_code = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__job_seek__3213E83F2C653C72", x => x.id);
                    table.ForeignKey(
                        name: "job_apply_candidate_id_fk",
                        column: x => x.candidate_id,
                        principalTable: "job_seeker_candidate_profile",
                        principalColumn: "candidate_id");
                    table.ForeignKey(
                        name: "job_apply_status_code_fk",
                        column: x => x.status_code,
                        principalTable: "job_seeker_status_code",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "job_posting_id_fk",
                        column: x => x.job_posting_id,
                        principalTable: "job_seeker_job_posting",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "job_seeker_saved_job_posting",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    candidate_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    job_posting_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    saved_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    deleted_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__job_seek__3213E83FEFE1919E", x => x.id);
                    table.ForeignKey(
                        name: "saved_job_candidate_id_fk",
                        column: x => x.candidate_id,
                        principalTable: "job_seeker_candidate_profile",
                        principalColumn: "candidate_id");
                    table.ForeignKey(
                        name: "saved_job_posting_id_fk",
                        column: x => x.job_posting_id,
                        principalTable: "job_seeker_job_posting",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "job_seeker_working_experience",
                columns: table => new
                {
                    working_exp_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newsequentialid())"),
                    job_title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    company_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    candidate_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    is_created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    is_updated_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    is_deleted_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__job_seek__1BBADB9CB7BAB0B3", x => x.working_exp_id);
                    table.ForeignKey(
                        name: "working_exp_candidate_id_fk",
                        column: x => x.candidate_id,
                        principalTable: "job_seeker_candidate_profile",
                        principalColumn: "candidate_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_authentication_granted_permissions_permission_id",
                table: "authentication_granted_permissions",
                column: "permission_id");

            migrationBuilder.CreateIndex(
                name: "IX_authentication_granted_permissions_role_id",
                table: "authentication_granted_permissions",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_job_seeker_applicant_profile_saved_candidate_id",
                table: "job_seeker_applicant_profile_saved",
                column: "candidate_id");

            migrationBuilder.CreateIndex(
                name: "IX_job_seeker_applicant_profile_saved_enterprise_id",
                table: "job_seeker_applicant_profile_saved",
                column: "enterprise_id");

            migrationBuilder.CreateIndex(
                name: "IX_job_seeker_candidate_profile_role_id",
                table: "job_seeker_candidate_profile",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_job_seeker_certificate_candidate_id",
                table: "job_seeker_certificate",
                column: "candidate_id");

            migrationBuilder.CreateIndex(
                name: "IX_job_seeker_district_province_code",
                table: "job_seeker_district",
                column: "province_code");

            migrationBuilder.CreateIndex(
                name: "IX_job_seeker_education_detail_candidate_id",
                table: "job_seeker_education_detail",
                column: "candidate_id");

            migrationBuilder.CreateIndex(
                name: "IX_job_seeker_enterprise_job_field_id",
                table: "job_seeker_enterprise",
                column: "job_field_id");

            migrationBuilder.CreateIndex(
                name: "IX_job_seeker_job_posting_enterprise_id",
                table: "job_seeker_job_posting",
                column: "enterprise_id");

            migrationBuilder.CreateIndex(
                name: "IX_job_seeker_job_posting_job_category_id",
                table: "job_seeker_job_posting",
                column: "job_category_id");

            migrationBuilder.CreateIndex(
                name: "IX_job_seeker_job_posting_job_level_code",
                table: "job_seeker_job_posting",
                column: "job_level_code");

            migrationBuilder.CreateIndex(
                name: "IX_job_seeker_job_posting_status_code",
                table: "job_seeker_job_posting",
                column: "status_code");

            migrationBuilder.CreateIndex(
                name: "IX_job_seeker_job_posting_apply_candidate_id",
                table: "job_seeker_job_posting_apply",
                column: "candidate_id");

            migrationBuilder.CreateIndex(
                name: "IX_job_seeker_job_posting_apply_job_posting_id",
                table: "job_seeker_job_posting_apply",
                column: "job_posting_id");

            migrationBuilder.CreateIndex(
                name: "IX_job_seeker_job_posting_apply_status_code",
                table: "job_seeker_job_posting_apply",
                column: "status_code");

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

            migrationBuilder.CreateIndex(
                name: "IX_job_seeker_recruiter_profile_enterprise_id",
                table: "job_seeker_recruiter_profile",
                column: "enterprise_id");

            migrationBuilder.CreateIndex(
                name: "IX_job_seeker_recruiter_profile_role_id",
                table: "job_seeker_recruiter_profile",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_job_seeker_saved_job_posting_candidate_id",
                table: "job_seeker_saved_job_posting",
                column: "candidate_id");

            migrationBuilder.CreateIndex(
                name: "IX_job_seeker_saved_job_posting_job_posting_id",
                table: "job_seeker_saved_job_posting",
                column: "job_posting_id");

            migrationBuilder.CreateIndex(
                name: "IX_job_seeker_user_login_data_status_code",
                table: "job_seeker_user_login_data",
                column: "status_code");

            migrationBuilder.CreateIndex(
                name: "UQ__job_seek__AB6E6164D4EE156A",
                table: "job_seeker_user_login_data",
                column: "email",
                unique: true,
                filter: "[email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_job_seeker_ward_district_code",
                table: "job_seeker_ward",
                column: "district_code");

            migrationBuilder.CreateIndex(
                name: "IX_job_seeker_working_experience_candidate_id",
                table: "job_seeker_working_experience",
                column: "candidate_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "authentication_granted_permissions");

            migrationBuilder.DropTable(
                name: "job_seeker_applicant_profile_saved");

            migrationBuilder.DropTable(
                name: "job_seeker_certificate");

            migrationBuilder.DropTable(
                name: "job_seeker_education_detail");

            migrationBuilder.DropTable(
                name: "job_seeker_job_posting_apply");

            migrationBuilder.DropTable(
                name: "job_seeker_notification");

            migrationBuilder.DropTable(
                name: "job_seeker_recruiter_profile");

            migrationBuilder.DropTable(
                name: "job_seeker_saved_job_posting");

            migrationBuilder.DropTable(
                name: "job_seeker_ward");

            migrationBuilder.DropTable(
                name: "job_seeker_working_experience");

            migrationBuilder.DropTable(
                name: "authentication_permission");

            migrationBuilder.DropTable(
                name: "job_seeker_notification_type");

            migrationBuilder.DropTable(
                name: "job_seeker_job_posting");

            migrationBuilder.DropTable(
                name: "job_seeker_district");

            migrationBuilder.DropTable(
                name: "job_seeker_candidate_profile");

            migrationBuilder.DropTable(
                name: "job_seeker_enterprise");

            migrationBuilder.DropTable(
                name: "job_seeker_job_category");

            migrationBuilder.DropTable(
                name: "job_seeker_job_level");

            migrationBuilder.DropTable(
                name: "job_seeker_province");

            migrationBuilder.DropTable(
                name: "job_seeker_user_login_data_external");

            migrationBuilder.DropTable(
                name: "job_seeker_user_login_data");

            migrationBuilder.DropTable(
                name: "authentication_role");

            migrationBuilder.DropTable(
                name: "job_seeker_job_field");

            migrationBuilder.DropTable(
                name: "job_seeker_status_code");
        }
    }
}
