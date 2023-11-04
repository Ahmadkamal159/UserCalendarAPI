using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using UserCalendarAPI.Entity;
using UserCalendarAPI.IServices;
using UserCalendarAPI.Models;
using UserCalendarAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IGoogleCalendarService, GoogleCalendarService>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CalenderAPI", Version = "v1" });
});
builder.Services.AddSwaggerGen(swagger =>
{
    //This is to generate the Default UI of Swagger Documentation    
    swagger.SwaggerDoc("v2", new OpenApiInfo {
        Version = "v1",
        Title = "CalenderAPI Demo",
        Description = "a demonstration of Calender event creator api using signed in Users"
    });

    // To Enable authorization using Swagger (JWT)    
    swagger.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme() {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9\"",
    });
    swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
                    {
                    new OpenApiSecurityScheme
                    {
                    Reference = new OpenApiReference
                    {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                    }
                    },
                    new string[] {}
                    }
                });
});
//injecting EFCore Services for db and merging it with identity
builder.Services.AddDbContext<APIEntity>(DBoptions=>DBoptions.UseSqlServer(builder.Configuration.GetConnectionString("ConnSTR")));
builder.Services.AddIdentity<ApplicationUser,IdentityRole>().AddEntityFrameworkStores<APIEntity>();
//adding CORS Service
builder.Services.AddCors(options => {
    options.AddPolicy("PublicPolicy", Policyoptions => {
        Policyoptions.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });

});
//inject services of jwt schema
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options => {
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters() {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JWT:ValidAudiance"],
        IssuerSigningKey =
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]))
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
//added the CORS' MiddleWare so as frontEnd can consume my apies
app.UseCors("PublicPolicy");
app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
