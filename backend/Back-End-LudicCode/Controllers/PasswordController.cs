using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using TCC_Web.Data;
using TCC_Web.Models;
using static TCC_Web.Data.ApplicationDbContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Identity.Data;
using System.Text;

namespace TCC_Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PasswordController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;
        object test = "";
        public PasswordController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
            
        }
        private async Task<bool> UpdateUserPasswordHash(int userId, string newPasswordHash)
        {
            string connectionString = "Server=DESKTOP-QLE7REI\\SQLEXPRESS;Database=DataBaseCode;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string updateCommandText = "UPDATE Users SET Password_Hash = @PasswordHash WHERE Id = @UserId";
                    using (var command = new SqlCommand(updateCommandText, connection))
                    {
                        command.Parameters.AddWithValue("@PasswordHash", newPasswordHash);
                        command.Parameters.AddWithValue("@UserId", userId);

                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao atualizar a senha: " + ex.Message);
                return false;
            }
        }

        // Rota para redefinir a senha
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (string.IsNullOrEmpty(request.Token) || string.IsNullOrEmpty(request.NewPassword))
                return BadRequest("Token e nova senha são obrigatórios.");

            var tokenRecord = await _context.PasswordResetTokens
            .FirstOrDefaultAsync(t => t.Token == request.Token);

            Console.WriteLine("Token Record UserId: " + tokenRecord.UserId);

            var user = await _context.Users
            .Where(u => u.Id == tokenRecord.UserId)
            .Select(u => new User { Id = u.Id, Usuario = u.Usuario, Email = u.Email })
            .FirstOrDefaultAsync();

            // 3. Criptografar a nova senha
            var salt = Encoding.UTF8.GetBytes(user.Email);
            var newPasswordHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: request.NewPassword,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            // 4. Atualizar a senha do usuário diretamente no banco

            
            bool updateSuccess = await UpdateUserPasswordHash(tokenRecord.UserId, newPasswordHash);
            
            if (!updateSuccess)
            {
                return StatusCode(500, "Erro ao atualizar a senha.");
            }
            
            //user.Password_Hash = newPasswordHash;

            // 5. Invalidação do token
            _context.PasswordResetTokens.Remove(tokenRecord);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Senha redefinida com sucesso." });

        }

        // Método para enviar o e-mail de redefinição
        [HttpPost("send-reset-email")]
        [AllowAnonymous]
        public async Task<IActionResult> SendPasswordResetEmail([FromBody] SendPasswordResetEmailRequest request)
        {
            // Verifica se o e-mail foi enviado
            Console.WriteLine("Requisição recebida para redefinir a senha.");
            if (string.IsNullOrEmpty(request.Email))
            {
                return BadRequest("O e-mail é obrigatório.");
            }
            Console.WriteLine($"{request.Email}");
            var user = await GetUserByEmailADOAsync(request.Email);
            if (user == null)
            {
                return BadRequest("Email não encontrado.");
            }

            // Gerar o token de redefinição
            var token = Guid.NewGuid().ToString();
            var expiryDate = DateTime.UtcNow.AddHours(1); // O token expira em 1 hora

            var resetToken = new PasswordResetToken
            {
                UserId = user.Id,
                Token = token,
                ExpiryDate = expiryDate
            };

            _context.PasswordResetTokens.Add(resetToken);
            await _context.SaveChangesAsync();

            // Gerar o link para envio por e-mail
            var resetUrl = $"http://127.0.0.1:5500/ludic_code_csshtml/frontend/src/modify_password.html?token={token}";

            // Enviar e-mail usando o EmailService
            string subject = "Redefinição de Senha";
            string body = $"Clique no link para redefinir sua senha: <a href='{resetUrl}'>Redefinir Senha</a>";
            await _emailService.EnviarEmailAsync(user.Email, subject, body);

            return Ok(new { message = "E-mail de redefinição de senha enviado." });
        }
        public async Task<User> GetUserByEmailADOAsync(string email)
        {
            User user = null;

            // Aqui você coloca a string de conexão com o seu banco de dados.
            using (var connection = new SqlConnection("Server=DESKTOP-QLE7REI\\SQLEXPRESS;Database=DataBaseCode;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"))
            {
                await connection.OpenAsync();

                // Alteramos a query para buscar pelo e-mail ao invés do nome de usuário
                var command = new SqlCommand("SELECT * FROM Users WHERE Email = @Email", connection);
                command.Parameters.AddWithValue("@Email", email);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        user = new User
                        {
                            // Mapeie as colunas da tabela 'Users' para os campos do modelo 'User'
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Usuario = reader.GetString(reader.GetOrdinal("Usuario")), // Se você também precisa do campo 'Usuario', mapeia aqui.
                            Email = reader.GetString(reader.GetOrdinal("Email")), // Mapeie o campo de email também
                            Password_Hash = reader.GetString(reader.GetOrdinal("Password_Hash")),
                            // Adicione outros campos conforme necessário
                        };
                    }
                }
            }

            return user;
        }

    }



}

// Modelo para o reset de senha
    public class ResetPasswordRequest
    {
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }

    public class SendPasswordResetEmailRequest
    {
        public string Email { get; set; }
    }
