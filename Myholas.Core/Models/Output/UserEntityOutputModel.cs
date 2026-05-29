using static Myholas.Core.Enums;

namespace Myholas.Core.Models.Output
{
    public class UserEntityOutputModel
    {
        public int Id { get; set; }

        public required string Username { get; set; }

        public UserRole Role { get; set; }


        public bool IsActive { get; set; }


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public DateTime? LastLogin { get; set; }

    }
}

