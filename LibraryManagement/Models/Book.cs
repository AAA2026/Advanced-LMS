#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models
{
    public class Book
    {
        [Key]
        [StringLength(13)]
        public string ISBN { get; set; } = string.Empty;

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public int PublicationYear { get; set; }

        [Required]
        public string Publisher { get; set; } = string.Empty;

        [Required]
        public string Language { get; set; } = string.Empty;

        public int PageCount { get; set; }

        public string? Description { get; set; }

        public int Availability { get; set; }

        // Navigation properties
        public virtual ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
        public virtual ICollection<BookGenre> BookGenres { get; set; } = new List<BookGenre>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
} 