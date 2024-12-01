namespace Data_JobWeb.Entity
{
    public class JobSeekerEnterpriseFollowed
    {
        public int Id { get; set; }

        public Guid? CandidateId { get; set; }

        public Guid? EnterpriseId { get; set; }

        public DateTime? IsCreatedAt { get; set; }

        public DateTime? IsUpdatedAt { get; set; }

        public DateTime? IsDeletedAt { get; set; }

        public virtual JobSeekerCandidateProfile? Candidate { get; set; }

        public virtual JobSeekerEnterprise? Enterprise { get; set; }
    }
}
