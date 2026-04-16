using System.ComponentModel.DataAnnotations;

namespace HealthcareApp.Models
{
    public class Appointments
    {
        [Key]
        public int AppointmentId { get; set; }

        public int DoctorId { get; set; }
        public int PatientId { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        public int  Status { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        public TimeSpan Time { get; set; }
    }
}
