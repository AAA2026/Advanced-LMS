#nullable enable

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models
{
    public class BookGenre
    {
        [Key]
        [Column(Order = 0)]
        public string ISBN { get; set; } = string.Empty;
        [Key]
        [Column(Order = 1)]
        public int GenreID { get; set; }
        // Navigation properties
        [ForeignKey("ISBN")]
        public virtual Book? Book { get; set; }
        [ForeignKey("GenreID")]
        public virtual Genre? Genre { get; set; }
    }
} 