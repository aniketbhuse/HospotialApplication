namespace HealthcareApp.Models
{
    public class PatientProfileViewModel
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string? ProfileImageUrl { get; set; }

        public List<Doctor> Doctors { get; set; }

        // ✅ Only ONE appointment list (for UI)
        public List<AppointmentDetailsViewModel> Appointments { get; set; }
    }
}