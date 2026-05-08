using ClienteService.Context;
using ClienteService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Steeltoe.Discovery.Eureka;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connection = builder.Configuration.GetConnectionString("Mysql");
builder.Services.AddDbContext<Db>(options =>
options.UseMySql(connection, ServerVersion.AutoDetect(connection)));

builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<ClienteService.Services.ClienteService>();
builder.Services.AddScoped<EnderecoService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<HistoricoPedidoService>();
builder.Services.AddSingleton<UserProducer>();
builder.Services.AddHostedService<PedidoStatusConsumer>();

builder.Services.AddEurekaDiscoveryClient();

var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
