using System.ComponentModel.DataAnnotations;

namespace HealthcareApp.Models
{
    public class Patient
    {
        [Key]
        public int PatientId { get; set; }

        public int UserId { get; set; }

        [Required]
        public int Age { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        public string Contact { get; set; }

        public string Description { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
