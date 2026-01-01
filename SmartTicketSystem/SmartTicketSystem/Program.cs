using System.Security.Claims;
using System.Text;

using FluentValidation;
using FluentValidation.AspNetCore;

using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using SmartTicketSystem.API.Hubs;
using SmartTicketSystem.Application.Interfaces.Repositories;
using SmartTicketSystem.Application.Mapping;
using SmartTicketSystem.Application.Services.Interfaces;
using SmartTicketSystem.Application.Validators;
using SmartTicketSystem.Infrastructure.Events;
using SmartTicketSystem.Infrastructure.Persistence;
using SmartTicketSystem.Infrastructure.Repositories;
using SmartTicketSystem.Infrastructure.Services;
using SmartTicketSystem.Infrastructure.Services.Implementations;
using SmartTicketSystem.Infrastructure.Workers;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddSignalR();

builder.Services.AddSingleton<IEventQueue, EventQueue>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<ITicketCommentRepository, TicketCommentRepository>();
builder.Services.AddScoped<ITicketHistoryRepository, TicketHistoryRepository>();
builder.Services.AddScoped<IAgentRepository, AgentRepository>();

builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<ITicketCommentService, TicketCommentService>();
builder.Services.AddScoped<ITicketHistoryService, TicketHistoryService>();
builder.Services.AddScoped<LuceneIndexService>();
builder.Services.AddScoped<LuceneCategoryMatcher>();

builder.Services.AddHttpContextAccessor(); 
builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddHostedService<TicketEventWorker>();
builder.Services.AddHostedService<AutoAssignmentWorker>();
builder.Services.AddHostedService<TicketEventWorker>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
            ),
            RoleClaimType = ClaimTypes.Role
        };
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    scope.ServiceProvider.GetRequiredService<LuceneIndexService>().BuildCategoryIndex();
    context.Database.Migrate();
}

app.MapHub<TicketHub>("/ticketHub");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();