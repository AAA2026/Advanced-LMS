using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models
{
    public class Genre
    {
        [Key]
        public int? GenreID { get; set; }
        [Required]
        public string? ISBN { get; set; } // Make nullable if can be null
        [Required]
        public string Name { get; set; } = string.Empty; // Initialize non-nullable string
        public string? Description { get; set; } // Make nullable if can be null
    }
} 