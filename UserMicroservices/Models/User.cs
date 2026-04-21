using System.ComponentModel.DataAnnotations;

namespace UserMicroservices.Models
{
    public class User
    {
        [Key]
        public string EmailId { get; set; }
        public string UserPassword { get; set; }
        public string RoleName { get; set; }
        public char Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; }
    }
}
