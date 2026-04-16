namespace HealthcareApp.Models
{
    public class AppointmentDetailsViewModel
    {

        // Patient Info
        public string PatientName { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
        public string Contact { get; set; }
        public string Description { get; set; }

        // Doctor + Appointment Info
        public string DoctorName { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan Time { get; set; }
        public int Status { get; set; }

        public int AppointmentId { get; set; }
    }
}
