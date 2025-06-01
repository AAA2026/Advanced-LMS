using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models
{
    public class Fine
    {
        [Key]
        public int? FineID { get; set; }

        [Required]
        public int TransactionID { get; set; }

        [Required]
        public decimal Amount { get; set; }

        public DateTime IssuedDate { get; set; }

        public DateTime? PaymentDate { get; set; }

        [StringLength(20)]
        public string? Status { get; set; }

        public string? Reason { get; set; }

        [ForeignKey("TransactionID")]
        public virtual Transaction? Transaction { get; set; }
    }
} 