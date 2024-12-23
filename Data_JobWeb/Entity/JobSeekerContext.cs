using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Data_JobWeb.Entity;

public partial class JobSeekerContext : DbContext
{
    public JobSeekerContext()
    {
    }

    public JobSeekerContext(DbContextOptions<JobSeekerContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AuthenticationGrantedPermission> AuthenticationGrantedPermissions { get; set; }

    public virtual DbSet<AuthenticationPermission> AuthenticationPermissions { get; set; }

    public virtual DbSet<AuthenticationRole> AuthenticationRoles { get; set; }

    public virtual DbSet<JobSeekerApplicantProfileSaved> JobSeekerApplicantProfileSaveds { get; set; }
    public virtual DbSet<JobSeekerEnterpriseFollowed> JobSeekerEnterpriseFolloweds { get; set; }

    public virtual DbSet<JobSeekerCandidateProfile> JobSeekerCandidateProfiles { get; set; }

    public virtual DbSet<JobSeekerCertificate> JobSeekerCertificates { get; set; }

    public virtual DbSet<JobSeekerDistrict> JobSeekerDistricts { get; set; }

    public virtual DbSet<JobSeekerEducationDetail> JobSeekerEducationDetails { get; set; }

    public virtual DbSet<JobSeekerEnterprise> JobSeekerEnterprises { get; set; }

    public virtual DbSet<JobSeekerJobCategory> JobSeekerJobCategories { get; set; }

    public virtual DbSet<JobSeekerJobField> JobSeekerJobFields { get; set; }

    public virtual DbSet<JobSeekerJobLevel> JobSeekerJobLevels { get; set; }

    public virtual DbSet<JobSeekerJobPosting> JobSeekerJobPostings { get; set; }

    public virtual DbSet<JobSeekerJobPostingApply> JobSeekerJobPostingApplies { get; set; }

    public virtual DbSet<JobSeekerNotification> JobSeekerNotifications { get; set; }

    public virtual DbSet<JobSeekerNotificationType> JobSeekerNotificationTypes { get; set; }

    public virtual DbSet<JobSeekerProvince> JobSeekerProvinces { get; set; }

    public virtual DbSet<JobSeekerRecruiterProfile> JobSeekerRecruiterProfiles { get; set; }

    public virtual DbSet<JobSeekerSavedJobPosting> JobSeekerSavedJobPostings { get; set; }

    public virtual DbSet<JobSeekerStatusCode> JobSeekerStatusCodes { get; set; }

    public virtual DbSet<JobSeekerUserLoginDataExternal> JobSeekerUserLoginDataExternals { get; set; }

    public virtual DbSet<JobSeekerUserLoginDatum> JobSeekerUserLoginData { get; set; }

    public virtual DbSet<JobSeekerWard> JobSeekerWards { get; set; }

    public virtual DbSet<JobSeekerWorkingExperience> JobSeekerWorkingExperiences { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=VVV\\ANHTAN;Initial Catalog=job_seeker;Integrated Security=True;Encrypt=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuthenticationGrantedPermission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__authenti__3213E83F4C943339");

            entity.ToTable("authentication_granted_permissions");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.PermissionId).HasColumnName("permission_id");
            entity.Property(e => e.RoleId).HasColumnName("role_id");

            entity.HasOne(d => d.Permission).WithMany(p => p.AuthenticationGrantedPermissions)
                .HasForeignKey(d => d.PermissionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("permission_id_fk");

            entity.HasOne(d => d.Role).WithMany(p => p.AuthenticationGrantedPermissions)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("role_id_fk");
        });

        modelBuilder.Entity<AuthenticationPermission>(entity =>
        {
            entity.HasKey(e => e.PermissionId).HasName("PK__authenti__E5331AFA559E7FF8");

            entity.ToTable("authentication_permission");

            entity.Property(e => e.PermissionId).HasColumnName("permission_id");
            entity.Property(e => e.PermissionDescri)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("permission_descri");
        });

        modelBuilder.Entity<AuthenticationRole>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__authenti__760965CC6BFE40AD");

            entity.ToTable("authentication_role");

            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<JobSeekerApplicantProfileSaved>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__job_seek__3213E83F1E4AB249");

            entity.ToTable("job_seeker_applicant_profile_saved");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CandidateId).HasColumnName("candidate_id");
            entity.Property(e => e.EnterpriseId).HasColumnName("enterprise_id");
            entity.Property(e => e.IsCreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("is_created_at");
            entity.Property(e => e.IsDeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("is_deleted_at");
            entity.Property(e => e.IsUpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("is_updated_at");

            entity.HasOne(d => d.Candidate).WithMany(p => p.JobSeekerApplicantProfileSaveds)
                .HasForeignKey(d => d.CandidateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("profile_saved_candidate_id_fk");

            entity.HasOne(d => d.Enterprise).WithMany(p => p.JobSeekerApplicantProfileSaveds)
                .HasForeignKey(d => d.EnterpriseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("profile_saved_enterprise_id_fk");
        });
        modelBuilder.Entity<JobSeekerEnterpriseFollowed>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__job_seek__3213E83F1E4AB212");

            entity.ToTable("job_seeker_enterprise_followed");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CandidateId).HasColumnName("candidate_id");
            entity.Property(e => e.EnterpriseId).HasColumnName("enterprise_id");
            entity.Property(e => e.IsCreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("is_created_at");
            entity.Property(e => e.IsDeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("is_deleted_at");
            entity.Property(e => e.IsUpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("is_updated_at");

            entity.HasOne(d => d.Candidate).WithMany(p => p.JobSeekerEnterpriseFolloweds)
                .HasForeignKey(d => d.CandidateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("enterprise_followed_candidate_id_fk");

            entity.HasOne(d => d.Enterprise).WithMany(p => p.JobSeekerEnterpriseFolloweds)
                .HasForeignKey(d => d.EnterpriseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("enterprise_followed_enterprise_id_fk");
        });

        modelBuilder.Entity<JobSeekerCandidateProfile>(entity =>
        {
            entity.HasKey(e => e.CandidateId).HasName("PK__job_seek__39BD4C188AE6E8E9");

            entity.ToTable("job_seeker_candidate_profile");

            entity.Property(e => e.CandidateId)
                .HasDefaultValueSql("(newsequentialid())")
                .HasColumnName("candidate_id");
            entity.Property(e => e.AddressDetail)
                .HasMaxLength(200)
                .HasColumnName("address_detail");
            entity.Property(e => e.AvartarUrl)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("avartar_url");
            entity.Property(e => e.CvUrl)
                .IsUnicode(false)
                .HasColumnName("cv_url");
            entity.Property(e => e.District)
                .HasMaxLength(200)
                .HasColumnName("district");
            entity.Property(e => e.Ward)
                .HasMaxLength(200)
                .HasColumnName("ward");
            entity.Property(e => e.Email)
                .HasMaxLength(200)
                .HasColumnName("email");
            entity.Property(e => e.Fullname)
                .HasMaxLength(200)
                .HasColumnName("fullname");
            entity.Property(e => e.Dob).HasColumnName("dob");
            entity.Property(e => e.FacbookLink)
                .IsUnicode(false)
                .HasColumnName("facbook_link");
            entity.Property(e => e.Gender)
                .HasMaxLength(10)
                .HasColumnName("gender");
            entity.Property(e => e.GithubUrl)
                .IsUnicode(false)
                .HasColumnName("github_url");
            entity.Property(e => e.IsAllowedPublic).HasColumnName("is_allowed_public");
            entity.Property(e => e.IsCreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("is_created_at");
            entity.Property(e => e.IsDeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("is_deleted_at");
            entity.Property(e => e.IsUpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("is_updated_at");
            entity.Property(e => e.LinkedinLink)
                .IsUnicode(false)
                .HasColumnName("linkedin_link");
            entity.Property(e => e.PhoneNumb)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("phone_numb");
            entity.Property(e => e.PortfolioUrl)
                .IsUnicode(false)
                .HasColumnName("portfolio_url");
            entity.Property(e => e.Province)
                .HasMaxLength(200)
                .HasColumnName("province");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.Slug)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("slug");
            entity.Property(e => e.TwitterUrl)
                .IsUnicode(false)
                .HasColumnName("twitter_url");

            entity.HasOne(d => d.Candidate).WithOne(p => p.JobSeekerCandidateProfile)
                .HasForeignKey<JobSeekerCandidateProfile>(d => d.CandidateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("candidate_id_external_fk");

            entity.HasOne(d => d.CandidateNavigation).WithOne(p => p.JobSeekerCandidateProfile)
                .HasForeignKey<JobSeekerCandidateProfile>(d => d.CandidateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("candidate_id_fk");

            entity.HasOne(d => d.Role).WithMany(p => p.JobSeekerCandidateProfiles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("role_id_two_fk");
        });

        modelBuilder.Entity<JobSeekerCertificate>(entity =>
        {
            entity.HasKey(e => e.CertificateId).HasName("PK__job_seek__E2256D31F6A5AB8B");

            entity.ToTable("job_seeker_certificate");

            entity.Property(e => e.CertificateId)
                .HasDefaultValueSql("(newsequentialid())")
                .HasColumnName("certificate_id");
            entity.Property(e => e.CandidateId).HasColumnName("candidate_id");
            entity.Property(e => e.CertificateLink)
                .IsUnicode(false)
                .HasColumnName("certificate_link");
            entity.Property(e => e.CertificateName)
                .HasMaxLength(200)
                .HasColumnName("certificate_name");
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .HasColumnName("description");
            entity.Property(e => e.IsCreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("is_created_at");
            entity.Property(e => e.IsDeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("is_deleted_at");
            entity.Property(e => e.IsUpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("is_updated_at");
            entity.Property(e => e.Organization)
                .HasMaxLength(200)
                .HasColumnName("organization");

            entity.HasOne(d => d.Candidate).WithMany(p => p.JobSeekerCertificates)
                .HasForeignKey(d => d.CandidateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("certificate_candidate_id_fk");
        });

        modelBuilder.Entity<JobSeekerDistrict>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("PK__job_seek__357D4CF830DFBA8A");

            entity.ToTable("job_seeker_district");

            entity.Property(e => e.Code)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("code");
            entity.Property(e => e.DistrictName)
                .HasMaxLength(200)
                .HasColumnName("district_name");
            entity.Property(e => e.ProvinceCode)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("province_code");

            entity.HasOne(d => d.ProvinceCodeNavigation).WithMany(p => p.JobSeekerDistricts)
                .HasForeignKey(d => d.ProvinceCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("province_code_fk");
        });

        modelBuilder.Entity<JobSeekerEducationDetail>(entity =>
        {
            entity.HasKey(e => e.EducationId).HasName("PK__job_seek__45C0CFE73E934531");

            entity.ToTable("job_seeker_education_detail");

            entity.Property(e => e.EducationId)
                .HasDefaultValueSql("(newsequentialid())")
                .HasColumnName("education_id");
            entity.Property(e => e.CandidateId).HasColumnName("candidate_id");
            entity.Property(e => e.Degree)
                .HasMaxLength(100)
                .HasColumnName("degree");
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .HasColumnName("description");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.IsCreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("is_created_at");
            entity.Property(e => e.IsDeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("is_deleted_at");
            entity.Property(e => e.IsUpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("is_updated_at");
            entity.Property(e => e.Major)
                .HasMaxLength(100)
                .HasColumnName("major");
            entity.Property(e => e.SchoolName)
                .HasMaxLength(200)
                .HasColumnName("school_name");
            entity.Property(e => e.StartDate).HasColumnName("start_date");

            entity.HasOne(d => d.Candidate).WithMany(p => p.JobSeekerEducationDetails)
                .HasForeignKey(d => d.CandidateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("education_detail_candidate_id_fk");
        });

        modelBuilder.Entity<JobSeekerEnterprise>(entity =>
        {
            entity.HasKey(e => e.EnterpriseId).HasName("PK__job_seek__A541BC651EBE1726");

            entity.ToTable("job_seeker_enterprise");

            entity.Property(e => e.EnterpriseId)
                .HasDefaultValueSql("(newsequentialid())")
                .HasColumnName("enterprise_id");
            entity.Property(e => e.Address)
                .HasMaxLength(200)
                .HasColumnName("address");
            entity.Property(e => e.CompanyEmail)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("company_email");
            entity.Property(e => e.CompanyPhoneContact)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("company_phone_contact");
            entity.Property(e => e.CoverImgUrl)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("cover_img_url");
            entity.Property(e => e.Descriptions).HasColumnName("descriptions");
            entity.Property(e => e.District)
                .HasMaxLength(100)
                .HasColumnName("district");
            entity.Property(e => e.City)
                .HasMaxLength(100)
                .HasColumnName("city");
            entity.Property(e => e.EnterpriseSize)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("enterprise_size");
            entity.Property(e => e.FacebookUrl)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("facebook_url");
            entity.Property(e => e.FoundedDate).HasColumnName("founded_date");
            entity.Property(e => e.FullCompanyName)
                .HasMaxLength(500)
                .HasColumnName("full_company_name");
            entity.Property(e => e.IsCreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("is_created_at");
            entity.Property(e => e.IsUpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("is_updated_at");
            entity.Property(e => e.IsCensorship)
                .HasDefaultValue(false)
                .HasColumnName("is_censorship");
            entity.Property(e => e.ViewCount)
                .HasDefaultValue(0)
                .HasColumnName("viewcount");
            entity.Property(e => e.JobFieldId).HasColumnName("job_field_id");
            entity.Property(e => e.LinkedinUrl)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("linkedin_url");
            entity.Property(e => e.LogoUrl)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("logo_url");
            entity.Property(e => e.Province)
                .HasMaxLength(100)
                .HasColumnName("province");
            entity.Property(e => e.SlugImg)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("slug_img");
            entity.Property(e => e.TaxCode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("tax_code");
            entity.Property(e => e.Ward)
                .HasMaxLength(100)
                .HasColumnName("ward");
            entity.Property(e => e.WebsiteUrl)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("website_url");

            entity.HasOne(d => d.JobField).WithMany(p => p.JobSeekerEnterprises)
                .HasForeignKey(d => d.JobFieldId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("job_field_id_fk");
        });

        modelBuilder.Entity<JobSeekerJobCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__job_seek__3213E83F22755607");

            entity.ToTable("job_seeker_job_category");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AppIconName)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("app_icon_name");
            entity.Property(e => e.IsCreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("is_created_at");
            entity.Property(e => e.IsUpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("is_updated_at");
            entity.Property(e => e.JobCategoryName)
                .HasMaxLength(200)
                .HasColumnName("job_category_name");
        });

        modelBuilder.Entity<JobSeekerJobField>(entity =>
        {
            entity.HasKey(e => e.JobFieldId).HasName("PK__job_seek__326B06EF6FD2D7D7");

            entity.ToTable("job_seeker_job_field");

            entity.Property(e => e.JobFieldId).HasColumnName("job_field_id");
            entity.Property(e => e.JobFieldName)
                .HasMaxLength(200)
                .HasColumnName("job_field_name");
        });

        modelBuilder.Entity<JobSeekerJobLevel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__job_seek__3213E83FFBA46985");

            entity.ToTable("job_seeker_job_level");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.JobLevelName)
                .HasMaxLength(100)
                .HasColumnName("job_level_name");
        });

        modelBuilder.Entity<JobSeekerJobPosting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__job_seek__3213E83FB1615EAF");

            entity.ToTable("job_seeker_job_posting");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newsequentialid())")
                .HasColumnName("id");
            entity.Property(e => e.AcademicLevel)
                .HasMaxLength(100)
                .HasColumnName("academic_level");
            entity.Property(e => e.Address)
                .HasMaxLength(100)
                .HasColumnName("address");
            entity.Property(e => e.ViewCount)
                .HasDefaultValue(0)
                .HasColumnName("viewcount");
            entity.Property(e => e.BenefitEnjoyed).HasColumnName("benefit_enjoyed");
            entity.Property(e => e.District)
                .HasMaxLength(100)
                .HasColumnName("district");
            entity.Property(e => e.EnterpriseId).HasColumnName("enterprise_id");
            entity.Property(e => e.ExpRequirement)
                .HasMaxLength(100)
                .HasColumnName("exp_requirement");
            entity.Property(e => e.KeyWord)
                .HasMaxLength(500)
                .HasColumnName("keyword");
            entity.Property(e => e.ExpiredTime)
                .HasColumnType("datetime")
                .HasColumnName("expired_time");
            entity.Property(e => e.GenderRequire)
                .HasMaxLength(50)
                .HasColumnName("gender_require");
            entity.Property(e => e.IsCreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("is_created_at");
            entity.Property(e => e.IsDeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("is_deleted_at");
            entity.Property(e => e.IsHot)
                .HasDefaultValue(false)
                .HasColumnName("is_hot");
            entity.Property(e => e.IsUpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("is_updated_at");
            entity.Property(e => e.IsUrgent)
                .HasDefaultValue(false)
                .HasColumnName("is_urgent");
            entity.Property(e => e.JobCategoryId).HasColumnName("job_category_id");
            entity.Property(e => e.JobDesc).HasColumnName("job_desc");
            entity.Property(e => e.JobLevelCode).HasColumnName("job_level_code");
            entity.Property(e => e.JobRequirement).HasColumnName("job_requirement");
            entity.Property(e => e.JobTitle)
                .HasMaxLength(200)
                .HasColumnName("job_title");
            entity.Property(e => e.Province)
                .HasMaxLength(100)
                .HasColumnName("province");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.SalaryMax).HasColumnName("salary_max");
            entity.Property(e => e.SalaryMin).HasColumnName("salary_min");
            entity.Property(e => e.StatusCode)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("status_code");
            entity.Property(e => e.TimePost)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("time_post");
            entity.Property(e => e.Ward)
                .HasMaxLength(100)
                .HasColumnName("ward");
            entity.Property(e => e.WorkingType)
                .HasMaxLength(50)
                .HasColumnName("working_type");

            entity.HasOne(d => d.Enterprise).WithMany(p => p.JobSeekerJobPostings)
                .HasForeignKey(d => d.EnterpriseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("job_posting_enterprise_id_fk");

            entity.HasOne(d => d.JobCategory).WithMany(p => p.JobSeekerJobPostings)
                .HasForeignKey(d => d.JobCategoryId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("job_posting_job_category_id_fk");

            entity.HasOne(d => d.JobLevelCodeNavigation).WithMany(p => p.JobSeekerJobPostings)
                .HasForeignKey(d => d.JobLevelCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("job_posting_job_level_code_fk");

            entity.HasOne(d => d.StatusCodeNavigation).WithMany(p => p.JobSeekerJobPostings)
                .HasForeignKey(d => d.StatusCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("job_posting_status_code_fk");
        });

        modelBuilder.Entity<JobSeekerJobPostingApply>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__job_seek__3213E83F2C653C72");

            entity.ToTable("job_seeker_job_posting_apply");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ApplyTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("apply_time");
            entity.Property(e => e.CandidateId).HasColumnName("candidate_id");
            entity.Property(e => e.JobPostingId).HasColumnName("job_posting_id");
            entity.Property(e => e.StatusCode)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("status_code");
            entity.Property(e => e.CoverLetter)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("covverletter");

            entity.HasOne(d => d.Candidate).WithMany(p => p.JobSeekerJobPostingApplies)
                .HasForeignKey(d => d.CandidateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("job_apply_candidate_id_fk");

            entity.HasOne(d => d.JobPosting).WithMany(p => p.JobSeekerJobPostingApplies)
                .HasForeignKey(d => d.JobPostingId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("job_posting_id_fk");

            entity.HasOne(d => d.StatusCodeNavigation).WithMany(p => p.JobSeekerJobPostingApplies)
                .HasForeignKey(d => d.StatusCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("job_apply_status_code_fk");
        });

        modelBuilder.Entity<JobSeekerNotification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__job_seek__3213E83F63136065");

            entity.ToTable("job_seeker_notification");

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("id");
            entity.Property(e => e.IsCreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("is_created_at");
            entity.Property(e => e.IsSeen).HasColumnName("is_seen");
            entity.Property(e => e.IsSent).HasColumnName("is_sent");
            entity.Property(e => e.IdConcern).HasColumnName("concern_id");
            entity.Property(e => e.IdUserReceive).HasColumnName("user_id");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.Type)
                .HasMaxLength(200)
                .HasColumnName("type_name");
            entity.Property(e => e.Title)
                .HasMaxLength(500)
                .HasColumnName("title");
        });

        modelBuilder.Entity<JobSeekerNotificationType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__job_seek__3213E83FFFCA9F52");

            entity.ToTable("job_seeker_notification_type");

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .HasColumnName("description");
            entity.Property(e => e.TypeName)
                .HasMaxLength(100)
                .HasColumnName("type_name");
            entity.Property(e => e.IdUser)
                .HasMaxLength(100)
                .HasColumnName("id_user");
            entity.Property(e => e.IsCreateAt)
                .HasColumnType("datetime")
                .HasColumnName("is_create_at");
        });

        modelBuilder.Entity<JobSeekerProvince>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("PK__job_seek__357D4CF8DE0F9512");

            entity.ToTable("job_seeker_province");

            entity.Property(e => e.Code)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("code");
            entity.Property(e => e.ProvinceName)
                .HasMaxLength(200)
                .HasColumnName("province_name");
        });

        modelBuilder.Entity<JobSeekerRecruiterProfile>(entity =>
        {
            entity.HasKey(e => e.RecruiterId).HasName("PK__job_seek__42ABA257E995083D");

            entity.ToTable("job_seeker_recruiter_profile");

            entity.Property(e => e.RecruiterId)
                .HasDefaultValueSql("(newsequentialid())")
                .HasColumnName("recruiter_id");
            entity.Property(e => e.AvatarLink)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("avatar_link");
            entity.Property(e => e.EnterpriseId).HasColumnName("enterprise_id");
            entity.Property(e => e.Gender)
                .HasMaxLength(10)
                .HasColumnName("gender");
            entity.Property(e => e.Email)
                .HasMaxLength(200)
                .HasColumnName("email");
            entity.Property(e => e.Fullname)
                .HasMaxLength(200)
                .HasColumnName("fullname");
            entity.Property(e => e.IsCreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("is_created_at");
            entity.Property(e => e.IsUpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("is_updated_at");
            entity.Property(e => e.LinkedinUrl)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("linkedin_url");
            entity.Property(e => e.PhoneNumb)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("phone_numb");
            entity.Property(e => e.RoleId).HasColumnName("role_id");

            entity.HasOne(d => d.Enterprise).WithMany(p => p.JobSeekerRecruiterProfiles)
                .HasForeignKey(d => d.EnterpriseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("enterprise_id_fk");

            entity.HasOne(d => d.Recruiter).WithOne(p => p.JobSeekerRecruiterProfile)
                .HasForeignKey<JobSeekerRecruiterProfile>(d => d.RecruiterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("recruiter_id_fk");

            entity.HasOne(d => d.Role).WithMany(p => p.JobSeekerRecruiterProfiles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("role_id_three_fk");
        });

        modelBuilder.Entity<JobSeekerSavedJobPosting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__job_seek__3213E83FEFE1919E");

            entity.ToTable("job_seeker_saved_job_posting");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CandidateId).HasColumnName("candidate_id");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.JobPostingId).HasColumnName("job_posting_id");
            entity.Property(e => e.SavedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("saved_at");

            entity.HasOne(d => d.Candidate).WithMany(p => p.JobSeekerSavedJobPostings)
                .HasForeignKey(d => d.CandidateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("saved_job_candidate_id_fk");

            entity.HasOne(d => d.JobPosting).WithMany(p => p.JobSeekerSavedJobPostings)
                .HasForeignKey(d => d.JobPostingId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("saved_job_posting_id_fk");
        });

        modelBuilder.Entity<JobSeekerStatusCode>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__job_seek__3213E83F32974630");

            entity.ToTable("job_seeker_status_code");

            entity.Property(e => e.Id)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("id");
            entity.Property(e => e.CodeValue)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("code_value");
        });

        modelBuilder.Entity<JobSeekerUserLoginDataExternal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__job_seek__3213E83FBF2934B2");

            entity.ToTable("job_seeker_user_login_data_external");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newsequentialid())")
                .HasColumnName("id");
            entity.Property(e => e.ExternalProviderToken)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("external_provider_token");
            entity.Property(e => e.ExtraData)
                .IsUnicode(false)
                .HasColumnName("extra_data");
            entity.Property(e => e.IsCreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("is_created_at");
            entity.Property(e => e.IsUpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("is_updated_at");
            entity.Property(e => e.ProviderName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("provider_name");
            entity.Property(e => e.WsEndpoint)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("ws_endpoint");
        });

        modelBuilder.Entity<JobSeekerUserLoginDatum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__job_seek__3213E83F122A0F1D");

            entity.ToTable("job_seeker_user_login_data");

            entity.HasIndex(e => e.Email, "UQ__job_seek__AB6E6164D4EE156A").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newsequentialid())")
                .HasColumnName("id");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.EmailVerified).HasColumnName("email_verified");
            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .HasColumnName("full_name");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(false)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDisable)
                .HasDefaultValue(false)
                .HasColumnName("is_disable");
            entity.Property(e => e.IsCreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("is_created_at");
            entity.Property(e => e.IsUpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("is_updated_at");
            entity.Property(e => e.LastActiveTime)
                .HasColumnType("datetime")
                .HasColumnName("last_active_time");
            entity.Property(e => e.Password)
                .HasMaxLength(512)
                .IsUnicode(false)
                .HasColumnName("password");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.StatusCode)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("status_code");
            entity.Property(e => e.AvartarUrl)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("avartar_url");

            entity.HasOne(d => d.StatusCodeNavigation).WithMany(p => p.JobSeekerUserLoginData)
                .HasForeignKey(d => d.StatusCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user_login_data_status_code_fk");
        });

        modelBuilder.Entity<JobSeekerWard>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("PK__job_seek__357D4CF802F04ADC");

            entity.ToTable("job_seeker_ward");

            entity.Property(e => e.Code)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("code");
            entity.Property(e => e.DistrictCode)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("district_code");
            entity.Property(e => e.WardName)
                .HasMaxLength(200)
                .HasColumnName("ward_name");

            entity.HasOne(d => d.DistrictCodeNavigation).WithMany(p => p.JobSeekerWards)
                .HasForeignKey(d => d.DistrictCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("district_code_fk");
        });

        modelBuilder.Entity<JobSeekerWorkingExperience>(entity =>
        {
            entity.HasKey(e => e.WorkingExpId).HasName("PK__job_seek__1BBADB9CB7BAB0B3");

            entity.ToTable("job_seeker_working_experience");

            entity.Property(e => e.WorkingExpId)
                .HasDefaultValueSql("(newsequentialid())")
                .HasColumnName("working_exp_id");
            entity.Property(e => e.CandidateId).HasColumnName("candidate_id");
            entity.Property(e => e.CompanyName)
                .HasMaxLength(200)
                .HasColumnName("company_name");
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .HasColumnName("description");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.IsCreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("is_created_at");
            entity.Property(e => e.IsDeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("is_deleted_at");
            entity.Property(e => e.IsUpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("is_updated_at");
            entity.Property(e => e.JobTitle)
                .HasMaxLength(200)
                .HasColumnName("job_title");
            entity.Property(e => e.StartDate).HasColumnName("start_date");

            entity.HasOne(d => d.Candidate).WithMany(p => p.JobSeekerWorkingExperiences)
                .HasForeignKey(d => d.CandidateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("working_exp_candidate_id_fk");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
