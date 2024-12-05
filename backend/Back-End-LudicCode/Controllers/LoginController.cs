using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using TCC_Web.Data;
using TCC_Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Text;

namespace TCC_Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly EmailService _emailService;

        public LoginController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
            _passwordHasher = new PasswordHasher<User>();
        }

        // POST: api/Login/Register
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterUser newUser)
        {
            // Verifica se o objeto é válido
            if (newUser == null || string.IsNullOrWhiteSpace(newUser.Usuario) || string.IsNullOrWhiteSpace(newUser.Password))
            {
                return BadRequest("Nome de usuário e senha são obrigatórios.");
            }

            // Verifica se o email é válido
            if (string.IsNullOrWhiteSpace(newUser.Email) || !IsValidEmail(newUser.Email))
            {
                return BadRequest("Um endereço de e-mail válido é obrigatório.");
            }

            try
            {
                var salt = Encoding.UTF8.GetBytes(newUser.Email);
                var hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: newUser.Password,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA512,
                    iterationCount: 10000,
                    numBytesRequested: 256 / 8));

                // Cria um novo usuário com o hash da senha
                var user = new User
                {
                    Usuario = newUser.Usuario,
                    Password_Hash = hashedPassword,
                    Data_Ativacao = DateTime.Now,
                    Data_Desativacao = null,
                    Email = newUser.Email
                };

                // Adiciona o usuário ao contexto e salva
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                var htmlContent = @"
                <!DOCTYPE html>
                <html lang='en'>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>Bem-vindo!</title>
                </head>
                <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px;'>
                    <div style='max-width: 600px; margin: auto; background-color: rgb(39, 34, 34); padding: 20px; border-radius: 8px; box-shadow: 0 2px 5px rgba(0,0,0,0.1);'>
                        <h2 style='color: rgb(220, 114, 114); text-align: center;'>Bem-vindo à nossa plataforma!</h2>
                        <p style='font-size: 16px; color: rgba(255, 255, 255, .75);'>Olá,</p>
                        <p style='font-size: 16px; color: rgba(255, 255, 255, .75);'>Obrigado por se cadastrar em nossa plataforma. Estamos animados para tê-lo(a) conosco!</p>
                        <p style='font-size: 16px; color: rgba(255, 255, 255, .75);'>Caso tenha dúvidas ou precise de ajuda, não hesite em nos contatar.</p>
                        <div style='text-align: center; margin-top: 20px;'>
                            <a href='http://127.0.0.1:5500/ludic_code_csshtml/frontend/src/login.html' style='display: inline-block; padding: 10px 20px; font-size: 16px; color: #ffffff; background: rgb(70, 68, 167); text-decoration: none; border-radius: 5px;'>Visite Nosso Site</a>
                        </div>
                        <p style='font-size: 12px; color: #777; margin-top: 20px; text-align: center;'>Este é um e-mail automático, por favor, não responda.</p>
                    </div>
                </body>
                </html>
                ";


                // Envia o e-mail de confirmação em um bloco separado
                try
                {
                    await _emailService.EnviarEmailAsync(
                        user.Email,
                        "Bem-vindo à nossa plataforma!",
                        htmlContent
                    );
                }
                catch (Exception emailEx)
                {
                    // Log para o erro de envio de e-mail (exemplo)
                    Console.WriteLine("Erro ao enviar e-mail: " + emailEx.Message);
                    // Aqui você pode decidir se quer notificar o usuário que o e-mail falhou
                }

                return Ok("Usuário criado com sucesso. Um e-mail de confirmação foi enviado.");
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, $"Erro ao registrar usuário no banco de dados: {dbEx.Message} - Inner Exception: {dbEx.InnerException?.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao processar a requisição: {ex.Message} - Inner Exception: {ex.InnerException?.Message}");
            }
        }

        // Método auxiliar para validar o formato do e-mail
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        // POST: api/Login/Login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] Login loginRequest)
        {
            if (string.IsNullOrWhiteSpace(loginRequest.Usuario) || string.IsNullOrWhiteSpace(loginRequest.Password))
            {
                return BadRequest("Nome de usuário e senha são obrigatórios.");
            }

            // Busca o usuário pelo nome
            var user = await GetUserByUsernameADOAsync(loginRequest.Usuario);

            if (user == null)
            {
                return NotFound("Usuário não encontrado.");
            }

            // Verifica se o Password_Hash está nulo antes de verificar a senha
            if (string.IsNullOrWhiteSpace(user.Password_Hash))
            {
                return StatusCode(500, "Ocorreu um erro interno: a senha hash não pode ser nula.");
            }

            var salt = Encoding.UTF8.GetBytes(user.Email);
            var providedPasswordHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: loginRequest.Password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            if (user.Password_Hash != providedPasswordHash)
                return Unauthorized("Senha incorreta.");

            // Se tudo estiver certo, retorna uma resposta de sucesso
            return Ok(new { message = "Login bem-sucedido." });
        }


        public async Task<User> GetUserByUsernameADOAsync(string username)
        {
            User user = null;

            using (var connection = new SqlConnection("Server=DESKTOP-QLE7REI\\SQLEXPRESS;Database=DataBaseCode;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"))
            {
                await connection.OpenAsync();

                var command = new SqlCommand("SELECT * FROM Users WHERE Usuario = @usuario", connection);
                command.Parameters.AddWithValue("@usuario", username);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        user = new User
                        {
                            // Mapeie as colunas do seu banco de dados para os atributos do seu modelo User
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Usuario = reader.GetString(reader.GetOrdinal("Usuario")),
                            Email = reader.GetString(reader.GetOrdinal("Email")),
                            Password_Hash = reader.GetString(reader.GetOrdinal("Password_Hash")),
                            // Adicione outros campos conforme necessário
                        };
                    }
                }
            }

            return user;
        }

        // PUT: api/Login/Update
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdatePassword(int id, [FromBody] string newPassword)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null || user.Data_Desativacao != null)
            {
                return NotFound("Usuário não encontrado ou excluído.");
            }

            // Atualiza o hash da nova senha
            user.Password_Hash = _passwordHasher.HashPassword(user, newPassword);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Senha atualizada com sucesso." });
        }

        //// DELETE: api/Login/Delete
        //[HttpDelete("delete/{id}")]
        //public async Task<IActionResult> Delete(int id)
        //{
        //    var user = await _context.Users.FindAsync(id);
        //    if (user == null || user.Data_Desativacao != null)
        //    {
        //        return NotFound("Usuário não encontrado ou já excluído.");
        //    }

        //    // Marca o usuário como excluído
        //    user.Data_Desativacao = DateTime.UtcNow;
        //    await _context.SaveChangesAsync();

        //    return Ok(new { message = "Usuário excluído com sucesso." });
        //}

        //// GET: api/Login/CheckUsername
        //[HttpGet("check-username/{username}")]
        //public async Task<IActionResult> CheckUsername(string username)
        //{
        //    var userExists = await _context.Users.AnyAsync(u => u.Usuario == username);
        //    if (userExists)
        //    {
        //        return Conflict("Nome de usuário já está em uso.");
        //    }

        //    return Ok(new { message = "Nome de usuário disponível." });
        //}
    }
}
