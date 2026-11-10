namespace poe11.Models
{
    public class Users
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty; 
        public string LastName { get; set; } = string.Empty; 
        public string Email { get; set; } = string.Empty; 
        public string PasswordHash { get; set; } = string.Empty; 
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string UserRole { get; set; } = "Participant";
        public string? ProfileImagePath { get; set; }
    }
}
