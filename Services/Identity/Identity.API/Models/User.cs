using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Identity.API.Models
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("id")]
        [MaxLength(128)]
        public string Id { get; set; } = string.Empty;

        [Required]
        [Column("email")]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Column("password_hash")]
        [MaxLength(256)]
        public string? PasswordHash { get; set; }

        [Required]
        [Column("full_name")]
        [MaxLength(256)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [Column("role")]
        [MaxLength(50)]
        public string Role { get; set; } = "Student";

        [Column("phone_number")]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("is_blocked")]
        public bool IsBlocked { get; set; } = false;

        [Column("profile_image_url")]
        [MaxLength(512)]
        public string? ProfileImageUrl { get; set; }
    }
}
