using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

public class EmailService
{
    private readonly SmtpClient _smtpClient;

    public EmailService(IConfiguration configuration)
    {
        var smtpSettings = configuration.GetSection("SmtpSettings");

        Console.WriteLine("Host: " + smtpSettings["Host"]);
        Console.WriteLine("Port: " + smtpSettings["Port"]);
        Console.WriteLine("EnableSsl: " + smtpSettings["EnableSsl"]);
        Console.WriteLine("Username: " + smtpSettings["Username"]);
        // Remova o próximo depois de confirmar que está correto
        Console.WriteLine("Password: " + smtpSettings["Password"]);

        _smtpClient = new SmtpClient(smtpSettings["Host"])
        {
            Port = int.Parse(smtpSettings["Port"]),
            Credentials = new NetworkCredential(smtpSettings["Username"], smtpSettings["Password"]),
            EnableSsl = bool.Parse(smtpSettings["EnableSsl"])
        };

    }

    public async Task EnviarEmailAsync(string destinatario, string assunto, string corpo)
    {
        MailMessage mailMessage = new MailMessage
        {
            From = new MailAddress(destinatario),
            Subject = assunto,
            Body = corpo,
            IsBodyHtml = true
        };
        mailMessage.To.Add(destinatario);

        try
        {
            await _smtpClient.SendMailAsync(mailMessage);
            Console.WriteLine("E-mail enviado com sucesso!");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao enviar e-mail: " + ex.Message);
        }
    }
}
