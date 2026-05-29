using EcommerceApp.Data;
using EcommerceApp.Constants;
using EcommerceApp.CronJobs;
using EcommerceApp.Filters;
using EcommerceApp.Models.Configurations;
using EcommerceApp.Repositories;
using EcommerceApp.Repositories.Interfaces;
using EcommerceApp.Services;
using EcommerceApp.Services.Interfaces;
using EcommerceApp.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using Stripe;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configuration
ConfigurationManager configuration = builder.Configuration;
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));


// Add services to the container.
builder.Services.AddSwaggerGen(c =>
{
    c.OperationFilter<HideJwtUserIdParameterFilter>();

    // Surface the XML doc comments (enabled via GenerateDocumentationFile) in Swagger UI.
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (System.IO.File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = configuration["JwtSettings:Issuer"],
        ValidAudience = configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:Key"])),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Identity.AdminUserPolicy, p => 
        p.RequireClaim(Identity.AdminUserClaim, "true"));
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>(); 
builder.Services.AddScoped<IProductsService, ProductsService>();
builder.Services.AddScoped<IProductsRepository, ProductsRepository>();
builder.Services.AddScoped<IOrdersService, OrdersService>();
builder.Services.AddScoped<IOrdersRepository, OrdersRepository>();
builder.Services.AddScoped<IPaymentService, PaymentService>(); 
builder.Services.AddScoped<IInvoiceGeneratorService, InvoiceGeneratorService>();
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddScoped<IHttpClientService, HttpClientService>();
builder.Services.AddHttpClient();

// Database Conection
builder.Services.AddDbContext<EcommerceContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("EcommerceDB")));

StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

// Add Quartz services to the container
builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("SyncProductsJob");
    q.AddJob<SyncProductsJob>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithCronSchedule("0 0 12 * * ?"));
});

// Add Quartz hosted service
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});
//===========================================

// Build Application
var app = builder.Build();

//===========================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
