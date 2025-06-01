using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models
{
    public class MemberPhone
    {
        [Key]
        [Column(Order = 0)]
        public int MemberID { get; set; }

        [Key]
        [Column(Order = 1)]
        public string Phone { get; set; } = string.Empty;

        [ForeignKey("MemberID")]
        public virtual Member? Member { get; set; }
    }
} 