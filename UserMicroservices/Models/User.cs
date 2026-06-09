using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserMicroservices.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        [Column("EmailId")]
        public string EmailId { get; set; } = string.Empty;

        public string? UserPassword { get; set; }

        public string? RoleName { get; set; }

        public int FailedLoginAttempts { get; set; }

        public DateTime? LockoutEnd { get; set; }
       public char  Gender { get; set; }
                  public DateTime?  DateOfBirth {  get; set; }
                  public string Address { get; set; }
        
    }
}
