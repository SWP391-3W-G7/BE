using BLL.IServices;
using BLL.Services;
using DAL.IRepositories;
using DAL.Models;
using DAL.Repositories;
using LostFoundApi.Hubs;
using LostFoundApi.HostedServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();

// Add Memory Cache for storing temporary data (campus selection during OAuth)
builder.Services.AddMemoryCache();

builder.Services.AddSignalR();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "LostFoundApi", Version = "v1" });

    // Define the Bearer token security scheme
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    // Add a security requirement to use the Bearer token
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });

    c.OrderActionsBy((apiDesc) =>
    {
        var controller = apiDesc.ActionDescriptor.RouteValues["controller"];
        return controller switch
        {
            "Users" => "0",
            "Auth" => "1",
            _ => controller
        };
    });
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<LostFoundTrackingSystemContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserActivityService, UserActivityService>();

builder.Services.AddScoped<IClaimRequestRepository, ClaimRequestRepository>();
builder.Services.AddScoped<IClaimRequestService, ClaimRequestService>();

builder.Services.AddScoped<ILostItemRepository, LostItemRepository>();
builder.Services.AddScoped<ILostItemService, LostItemService>();

builder.Services.AddScoped<IImageRepository, ImageRepository>();
builder.Services.AddHttpClient<IImageService, ImageService>();

builder.Services.AddScoped<ICampusRepository, CampusRepository>();
builder.Services.AddScoped<ICampusService, CampusService>();

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

builder.Services.AddScoped<IFoundItemRepository, FoundItemRepository>();
builder.Services.AddScoped<IFoundItemService, FoundItemService>();

builder.Services.AddScoped<IMatchingRepository, MatchingRepository>();
builder.Services.AddScoped<IMatchingService, MatchingService>();

builder.Services.AddScoped<IMatchHistoryRepository, MatchHistoryRepository>();
builder.Services.AddHostedService<LostFoundApi.HostedServices.MatchingHostedService>();
builder.Services.AddHostedService<ConflictingClaimsScannerService>();

builder.Services.AddScoped<IReturnRecordRepository, ReturnRecordRepository>();
builder.Services.AddScoped<IReturnRecordService, ReturnRecordService>();

builder.Services.AddScoped<IItemActionLogRepository, ItemActionLogRepository>();
builder.Services.AddScoped<IItemActionLogService, ItemActionLogService>();

builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IReportService, ReportService>();

builder.Services.AddScoped<IEvidenceRepository, EvidenceRepository>();

builder.Services.AddScoped<IAdminService, AdminService>();

builder.Services.AddScoped<IStaffService, StaffService>();

builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, SignalRNotification>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
        //.AllowCredentials() // Uncomment for SignalR if needed
    });
});

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("JWT Key is not configured.");
}

// Configure Authentication
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notificationHub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    // Set the callback path - this should match your controller route
    options.CallbackPath = "/signin-google";
    // Sign in with the cookie scheme after Google auth
    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    var port = Environment.GetEnvironmentVariable("WEBSITES_PORT") ?? Environment.GetEnvironmentVariable("PORT") ?? "8000";
    serverOptions.ListenAnyIP(int.Parse(port));
});

var app = builder.Build();

app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "LostFound API V1");
    c.RoutePrefix = "swagger";
});

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowAll");
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapHub<NotificationHub>("/notificationHub");
app.MapControllers();

app.Run();