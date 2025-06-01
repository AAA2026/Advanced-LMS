using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models
{
    public class Reservation
    {
        [Key]
        public int ReservationID { get; set; }

        [Required]
        [StringLength(13)]
        public string ISBN { get; set; } = string.Empty;

        [Required]
        public int MemberID { get; set; }

        public DateTime ReservationDate { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = string.Empty;

        [ForeignKey("ISBN")]
        public virtual Book? Book { get; set; }

        [ForeignKey("MemberID")]
        public virtual Member? Member { get; set; }
    }
} 