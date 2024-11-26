using Data_JobWeb.Entity;

namespace Data_JobWeb.Dtos
{
    public class RegisterRecruiter
    {
        public JobSeekerUserLoginDatum UserLoginData { get; set; }
        public JobSeekerEnterprise EnterPrise { get; set; }
        public string Gender { get; set; }
        public string Phone { get; set; }
        public RegisterRecruiter() { }
    }
}
