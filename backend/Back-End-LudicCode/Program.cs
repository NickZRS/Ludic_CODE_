using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.EntityFrameworkCore;
using TCC_Web.Data;

var builder = WebApplication.CreateBuilder(args);

// Adicionar servi�os para autentica��o (se necess�rio) e autoriza��o
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
    .AddNegotiate();

builder.Services.AddAuthorization(options =>
{
    // Define a pol�tica de fallback
    options.FallbackPolicy = options.DefaultPolicy;
});

// Adiciona suporte para controladores (API)
builder.Services.AddControllers(); // Adiciona suporte para APIs

// Configura��o de CORS (se necess�rio para o front-end se comunicar com o back-end)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Adiciona o ApplicationDbContext com a string de conex�o
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ** Registro do EmailService **
builder.Services.AddScoped<EmailService>(); // Adiciona o EmailService ao cont�iner de DI

var app = builder.Build();

// Configure o pipeline de requisi��es HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowAll"); // Ativa o CORS para permitir comunica��o entre o front-end e back-end

// app.UseAuthentication(); // Middleware de autentica��o, se necess�rio
app.UseAuthorization();  // Middleware de autoriza��o

app.MapControllers(); // Mapeia os controladores da API

app.Run();
