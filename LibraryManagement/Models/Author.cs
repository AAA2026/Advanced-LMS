#nullable enable

using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models
{
    public class Author
    {
        [Key]
        public int? AuthorID { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty; // Initialize non-nullable string
        public string? Biography { get; set; } // Make nullable if can be null
    }
} 