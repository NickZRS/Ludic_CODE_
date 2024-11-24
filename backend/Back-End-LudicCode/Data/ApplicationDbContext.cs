using Microsoft.EntityFrameworkCore;
using TCC_Web.Models;

namespace TCC_Web.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

        public class PasswordResetToken
        {
            public int Id { get; set; }
            public string Token { get; set; }
            public DateTime ExpiryDate { get; set; }

            // Foreign Key
            public int UserId { get; set; }

            // Navigation property
            public User User { get; set; }
        }

        // Método para buscar usuários via SQL
        public async Task<User> GetUserByUsernameAsync(string username)
        {
            var sql = "SELECT * FROM Users WHERE Usuario = @p0";
            return await Users.FromSqlRaw(sql, username).FirstOrDefaultAsync(); // Use Users que representa a tabela
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PasswordResetToken>()
                .HasOne(prt => prt.User)  // PasswordResetToken tem um User
                .WithMany(u => u.PasswordResetTokens)  // User pode ter muitos PasswordResetTokens
                .HasForeignKey(prt => prt.UserId);  // FK é UserId

            base.OnModelCreating(modelBuilder);
        }

    }
}
