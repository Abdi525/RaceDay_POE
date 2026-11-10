namespace POE_P3_WEB.Models
{
    public class ViewsModels
    {
        public class LoginViewModel
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        public class RegisterViewModel
        {
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string? PhoneNumber { get; set; }
            public string? Address { get; set; }
            public string UserRole { get; set; } = "Participant";
        }

        public class EventViewModel
        {
            public int EventId { get; set; }
            public string EventName { get; set; } = string.Empty;
            public string EventLocation { get; set; } = string.Empty;
            public DateTime EventDate { get; set; }
            public string? EventDescription { get; set; }
            public IFormFile? BannerImage { get; set; }
            public string? ImagePath { get; set; }
            public List<EventTypeViewModel> EventTypes { get; set; } = new();
        }

        public class EventTypeViewModel
        {
            public int EventTypeCode { get; set; }
            public int EventId { get; set; }
            public string EventTypeName { get; set; } = string.Empty;
            public decimal DistanceKM { get; set; }
            public decimal EntranceFee { get; set; }
            public int MaxParticipants { get; set; }
        }

        public class EnrollmentViewModel
        {
            public int EventTypeCode { get; set; }
            public string? MedicalNotes { get; set; }
            public string EmergencyContactNum { get; set; } = string.Empty;
        }


        public class MyEventViewModel
        {
            public int EntranceId { get; set; }
            public int EventTypeCode { get; set; }
            public string? ChipNumber { get; set; }
            public string? PaymentStatus { get; set; }
            public string? CategoryName { get; set; }
            public string? EventName { get; set; }
            public DateTime EventDate { get; set; }
            public string? EventLocation { get; set; }
        }

        public class EnrollmentAdminViewModel
        {
            public int EntranceId { get; set; }
            public int UserId { get; set; }
            public int EventTypeCode { get; set; }
            public string? ChipNumber { get; set; }
            public string? PaymentStatus { get; set; }
            public string? MedicalNotes { get; set; }
            public string? EmergencyContactNum { get; set; }
            public ParticipantSummaryViewModel? Participant { get; set; }
            public string? Category { get; set; }
        }

        public class ParticipantSummaryViewModel
        {
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public string? Email { get; set; }
        }

        public class ResultCaptureViewModel
        {
            public int EntranceId { get; set; }
            public string FinishStatus { get; set; } = "Finished";
            public int? OverallRank { get; set; }
            public int? CategoryRank { get; set; }
            public int? AgeGroupRank { get; set; }
            public int EventId { get; set; }
        }

        public class ProfileViewModel
        {
            public int UserId { get; set; }
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public string? Email { get; set; }
            public string? PhoneNumber { get; set; }
            public string? Address { get; set; }
            public string? UserRole { get; set; }
            public string? ProfileImagePath { get; set; }
            public IFormFile? ProfileImage { get; set; }
        }

        public class ResultViewModel
        {
            public int EntranceId { get; set; }
            public string FinishStatus { get; set; } = "Finished";
            public int? OverallRank { get; set; }
            public int? CategoryRank { get; set; }
            public int? AgeGroupRank { get; set; }
            public string? EventName { get; set; }
            public string? CategoryName { get; set; }
        }
    }
}

