using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models
{
    public class Member
    {
        [Key]
        public int MemberID { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        public string? Address { get; set; }

        public DateTime RegistrationDate { get; set; }

        public bool IsActive { get; set; }

        // Navigation property
        public virtual ICollection<MemberPhone> MemberPhones { get; set; } = new List<MemberPhone>();
    }
} 