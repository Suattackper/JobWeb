using System;
using System.Collections.Generic;

namespace Data_JobWeb.Entity;

public partial class JobSeekerJobPosting
{
    public Guid Id { get; set; }

    public string? JobTitle { get; set; }

    public string? JobDesc { get; set; }

    public string? JobRequirement { get; set; }

    public string? BenefitEnjoyed { get; set; }

    public int? Quantity { get; set; }

    public int? SalaryMin { get; set; }

    public int? SalaryMax { get; set; }

    public string? ExpRequirement { get; set; }

    public int? JobLevelCode { get; set; }

    public string? WorkingType { get; set; }

    public string? GenderRequire { get; set; }

    public string? AcademicLevel { get; set; }

    public string? Address { get; set; }

    public string? Province { get; set; }

    public string? District { get; set; }

    public string? Ward { get; set; }

    public DateTime? TimePost { get; set; }

    public DateTime? ExpiredTime { get; set; }

    public bool? IsUrgent { get; set; }

    public bool? IsHot { get; set; }

    public string? StatusCode { get; set; }

    public Guid? EnterpriseId { get; set; }

    public int? JobCategoryId { get; set; }

    public DateTime? IsCreatedAt { get; set; }

    public DateTime? IsUpdatedAt { get; set; }

    public DateTime? IsDeletedAt { get; set; }

    public virtual JobSeekerEnterprise? Enterprise { get; set; }

    public virtual JobSeekerJobCategory? JobCategory { get; set; }

    public virtual JobSeekerJobLevel? JobLevelCodeNavigation { get; set; }

    public virtual ICollection<JobSeekerJobPostingApply> JobSeekerJobPostingApplies { get; set; } = new List<JobSeekerJobPostingApply>();

    public virtual ICollection<JobSeekerNotification> JobSeekerNotifications { get; set; } = new List<JobSeekerNotification>();

    public virtual ICollection<JobSeekerSavedJobPosting> JobSeekerSavedJobPostings { get; set; } = new List<JobSeekerSavedJobPosting>();

    public virtual JobSeekerStatusCode? StatusCodeNavigation { get; set; }
}
