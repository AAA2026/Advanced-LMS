using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionID { get; set; }

        [Required]
        [StringLength(20)]
        public string ISBN { get; set; } = string.Empty;

        [Required]
        public int MemberID { get; set; }

        public DateTime TransactionDate { get; set; }

        [StringLength(20)]
        public string TransactionType { get; set; } = string.Empty;

        public DateTime? DueDate { get; set; }

        public string? Status { get; set; }

        public DateTime? ReturnDate { get; set; }

        [ForeignKey("ISBN")]
        public virtual Book? Book { get; set; }

        [ForeignKey("MemberID")]
        public virtual Member? Member { get; set; }
    }
} 