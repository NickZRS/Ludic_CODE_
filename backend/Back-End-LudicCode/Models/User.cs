using System;
using static TCC_Web.Data.ApplicationDbContext;

namespace TCC_Web.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Usuario { get; set; }
        public string Password { get; set; }
        public string Password_Hash { get; set; }
        public DateTime Data_Ativacao { get; set; }
        public DateTime? Data_Desativacao { get; set; }

        public ICollection<PasswordResetToken> PasswordResetTokens { get; set; }
    }
}
