using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.EntityFrameworkCore;
using TCC_Web.Data;

var builder = WebApplication.CreateBuilder(args);

// Adicionar serviços para autenticação (se necessário) e autorização
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
    .AddNegotiate();

builder.Services.AddAuthorization(options =>
{
    // Define a política de fallback
    options.FallbackPolicy = options.DefaultPolicy;
});

// Adiciona suporte para controladores (API)
builder.Services.AddControllers(); // Adiciona suporte para APIs

// Configuração de CORS (se necessário para o front-end se comunicar com o back-end)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Adiciona o ApplicationDbContext com a string de conexão
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ** Registro do EmailService **
builder.Services.AddScoped<EmailService>(); // Adiciona o EmailService ao contêiner de DI

var app = builder.Build();

// Configure o pipeline de requisições HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowAll"); // Ativa o CORS para permitir comunicação entre o front-end e back-end

// app.UseAuthentication(); // Middleware de autenticação, se necessário
app.UseAuthorization();  // Middleware de autorização

app.MapControllers(); // Mapeia os controladores da API

app.Run();
