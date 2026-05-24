using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Myholas.Core.Dtos
{
    [Table("Users")]
    public class UserEntityDto
    {
        [Key]
        public int Id { get; set; }


        [MaxLength(100)]
        public string Username { get; set; } = "";


        [MaxLength(255)]
        public string Email { get; set; } = "";


        [MaxLength(255)]
        public string PasswordHash { get; set; } = "";   // Хеш пароля (BCrypt)


        [MaxLength(50)]
        public string? Role { get; set; } = "user";      // "admin", "user", "viewer"


        public bool IsActive { get; set; } = true;


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public DateTime? LastLogin { get; set; }


        // Навигационное свойство: токены обновления (для JWT)
        public virtual ICollection<RefreshTokenEntity> RefreshTokens { get; set; } = new List<RefreshTokenEntity>();
    }

    [Table("RefreshTokens")]
    public class RefreshTokenEntity
    {
        [Key]
        public int Id { get; set; }


        [MaxLength(500)]
        public string Token { get; set; } = "";


        public DateTime ExpiresAt { get; set; }


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public int UserId { get; set; }


        [ForeignKey(nameof(UserId))]
        public virtual UserEntityDto User { get; set; } = null!;
    }
}
