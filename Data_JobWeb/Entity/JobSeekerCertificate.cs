using System;
using System.Collections.Generic;

namespace Data_JobWeb.Entity;

public partial class JobSeekerCertificate
{
    public Guid CertificateId { get; set; }

    public string? CertificateName { get; set; }

    public string? Organization { get; set; }

    public string? CertificateLink { get; set; }

    public string? Description { get; set; }

    public Guid? CandidateId { get; set; }

    public DateTime? IsCreatedAt { get; set; }

    public DateTime? IsUpdatedAt { get; set; }

    public DateTime? IsDeletedAt { get; set; }

    public virtual JobSeekerCandidateProfile? Candidate { get; set; }
}
