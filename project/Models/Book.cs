using System.ComponentModel.DataAnnotations;

namespace project.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Author { get; set; }

        public string Genre { get; set; }

        [DataType(DataType.Date)]
        public DateTime PublishedDate { get; set; }

        public string PhotoUrl { get; set; }
    }
}
