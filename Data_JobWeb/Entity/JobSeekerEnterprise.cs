using System;
using System.Collections.Generic;

namespace Data_JobWeb.Entity;

public partial class JobSeekerEnterprise
{
    public Guid EnterpriseId { get; set; }

    public string? FullCompanyName { get; set; }

    public string? LogoUrl { get; set; }

    public string? CompanyEmail { get; set; }

    public string? CompanyPhoneContact { get; set; }

    public string? CoverImgUrl { get; set; }

    public string? SlugImg { get; set; }

    public string? FacebookUrl { get; set; }

    public string? LinkedinUrl { get; set; }

    public string? WebsiteUrl { get; set; }

    public string? TaxCode { get; set; }

    public DateOnly? FoundedDate { get; set; }

    public string? EnterpriseSize { get; set; }

    public int? JobFieldId { get; set; }

    public string? Address { get; set; }

    public string? Province { get; set; }
    public string? City { get; set; }

    public string? District { get; set; }

    public string? Ward { get; set; }

    public string? Descriptions { get; set; }

    public DateTime? IsCreatedAt { get; set; }

    public DateTime? IsUpdatedAt { get; set; }
    public bool? IsCensorship { get; set; }

    public virtual JobSeekerJobField? JobField { get; set; }

    public virtual ICollection<JobSeekerApplicantProfileSaved> JobSeekerApplicantProfileSaveds { get; set; } = new List<JobSeekerApplicantProfileSaved>();
    public virtual ICollection<JobSeekerEnterpriseFollowed> JobSeekerEnterpriseFolloweds { get; set; } = new List<JobSeekerEnterpriseFollowed>();

    public virtual ICollection<JobSeekerJobPosting> JobSeekerJobPostings { get; set; } = new List<JobSeekerJobPosting>();

    public virtual ICollection<JobSeekerRecruiterProfile> JobSeekerRecruiterProfiles { get; set; } = new List<JobSeekerRecruiterProfile>();
}
