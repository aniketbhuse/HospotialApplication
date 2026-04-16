using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace HealthcareApp.Models
{
    public class BookAppointmentViewModel
    {
        [Required(ErrorMessage = "Please select doctor")]
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "Please select date")]
        public DateTime AppointmentDate { get; set; }

        public List<Doctor> Doctors { get; set; }
    }
}
