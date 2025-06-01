#nullable enable

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models
{
    public class BookAuthor
    {
        [Key]
        [Column(Order = 0)]
        public string ISBN { get; set; } = string.Empty;
        [Key]
        [Column(Order = 1)]
        public int AuthorID { get; set; }
        // Navigation properties
        [ForeignKey("ISBN")]
        public virtual Book? Book { get; set; }
        [ForeignKey("AuthorID")]
        public virtual Author? Author { get; set; }
    }
} 