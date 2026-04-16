using System.ComponentModel.DataAnnotations;

namespace HealthcareApp.Models
{
    public class Doctor
    {
        [Key]
        public int DoctorId { get; set; }   // Identity column

        public int UserId { get; set; }

        public string FullName { get; set; }

        public string Specialization { get; set; }

        public TimeSpan AvailableFrom { get; set; }

        public TimeSpan AvailableTo { get; set; }

        public DateTime UpdatedDate { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
