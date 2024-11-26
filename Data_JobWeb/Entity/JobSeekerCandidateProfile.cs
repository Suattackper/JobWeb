using System;
using System.Collections.Generic;

namespace Data_JobWeb.Entity;

public partial class JobSeekerCandidateProfile
{
    public Guid CandidateId { get; set; }

    public DateOnly? Dob { get; set; }

    public string? Fullname { get; set; }
    public string? Gender { get; set; }
    public string? Email { get; set; }

    public string? PhoneNumb { get; set; }

    public string? AvartarUrl { get; set; }

    public string? CvUrl { get; set; }

    public string? Slug { get; set; }

    public string? FacbookLink { get; set; }

    public string? LinkedinLink { get; set; }

    public string? GithubUrl { get; set; }

    public string? TwitterUrl { get; set; }

    public string? PortfolioUrl { get; set; }

    public string? Province { get; set; }

    public string? District { get; set; }
    public string? Ward { get; set; }

    public int? RoleId { get; set; }

    public bool? IsAllowedPublic { get; set; }

    public DateTime? IsCreatedAt { get; set; }

    public DateTime? IsUpdatedAt { get; set; }

    public DateTime? IsDeletedAt { get; set; }

    public string? AddressDetail { get; set; }

    public virtual JobSeekerUserLoginDataExternal? Candidate { get; set; }

    public virtual JobSeekerUserLoginDatum? CandidateNavigation { get; set; }

    public virtual ICollection<JobSeekerApplicantProfileSaved> JobSeekerApplicantProfileSaveds { get; set; } = new List<JobSeekerApplicantProfileSaved>();

    public virtual ICollection<JobSeekerCertificate> JobSeekerCertificates { get; set; } = new List<JobSeekerCertificate>();

    public virtual ICollection<JobSeekerEducationDetail> JobSeekerEducationDetails { get; set; } = new List<JobSeekerEducationDetail>();

    public virtual ICollection<JobSeekerJobPostingApply> JobSeekerJobPostingApplies { get; set; } = new List<JobSeekerJobPostingApply>();

    public virtual ICollection<JobSeekerSavedJobPosting> JobSeekerSavedJobPostings { get; set; } = new List<JobSeekerSavedJobPosting>();

    public virtual ICollection<JobSeekerWorkingExperience> JobSeekerWorkingExperiences { get; set; } = new List<JobSeekerWorkingExperience>();

    public virtual AuthenticationRole? Role { get; set; }
}
