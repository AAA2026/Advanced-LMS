using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models
{
    public class Review
    {
        [Key]
        public int ReviewID { get; set; }
        [StringLength(20)]
        public string ISBN { get; set; }
        public int MemberID { get; set; }
        public string ReviewText { get; set; }
        public int Rating { get; set; }
        // Navigation properties
        public virtual Book Book { get; set; }
        public virtual Member Member { get; set; }
    }
} 