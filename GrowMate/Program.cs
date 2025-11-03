using GrowMate.Repositories;
using GrowMate.Repositories.Data;
using GrowMate.Repositories.Interfaces;
using GrowMate.Services.Authentication;
using GrowMate.Services.Carts;
using GrowMate.Services.Customers;
using GrowMate.Services.EmailRegister;
using GrowMate.Services.Farmers;
using GrowMate.Services.Media;
using GrowMate.Services.Orders;
using GrowMate.Services.Posts;
using GrowMate.Services.Products;
using GrowMate.Services.TreeListings;
using GrowMate.Services.Trees;
using GrowMate.Services.Adoptions;
using GrowMate.Services.Payments;
using GrowMate.Services.Users;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// In development, load sensitive configuration from a separate secrets file.
// This allows you to keep secrets out of source control.
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile("Secret/appsettings.Secret.json", optional: true, reloadOnChange: true);
}

// XML comments will be configured in the unified Swagger setup below

// Add Repositories to the DI container
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IEmailVerificationRepository, EmailVerificationRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IFarmerRepository, FarmerRepository>();
builder.Services.AddScoped<IMediaRepository, MediaRepository>();
builder.Services.AddScoped<ITreeListingRepository, TreeListingRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IAdoptionRepository, AdoptionRepository>();
builder.Services.AddScoped<ITreeRepository, TreeRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<ICartItemRepository, CartItemRepository>();

// DbContext for EF Core tools and runtime
builder.Services.AddDbContext<EXE201_GrowMateContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Services
builder.Services.AddControllers();
builder.Services.AddScoped<IRegisterService, RegisterService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IFarmerService, FarmerService>();
builder.Services.AddScoped<IMediaService, MediaService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ITreeListingService, TreeListingService>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IAdoptionService, AdoptionService>();
builder.Services.AddScoped<ITreeService, TreeService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IOrderService, OrderService>();


// AuthN/Z
var authBuilder = builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    var key = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured.");
    var issuer = builder.Configuration["Jwt:Issuer"];
    var audience = builder.Configuration["Jwt:Audience"];

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        ValidateIssuer = !string.IsNullOrWhiteSpace(issuer),
        ValidIssuer = issuer,
        ValidateAudience = !string.IsNullOrWhiteSpace(audience),
        ValidAudience = audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(30)
    };

    options.IncludeErrorDetails = true;
});

// Register Google only if configured
var googleClientId = builder.Configuration["Google:ClientId"];
var googleClientSecret = builder.Configuration["Google:ClientSecret"];
if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
{
    authBuilder.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
    {
        options.ClientId = googleClientId;
        options.ClientSecret = googleClientSecret;
        options.CallbackPath = "/api/auth/google-callback"; // align with your controller route if enabled
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    });
}

builder.Services.AddAuthorization();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "GrowMate API", Version = "v1" });
    // Avoid schema ID collisions for types with the same class name in different namespaces
    c.CustomSchemaIds(type => type.FullName?.Replace('+', '.') ?? type.Name);
    // Include XML comments
    var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization using the Bearer scheme. Paste ONLY the token (no 'Bearer ' prefix).",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(
            "http://localhost:5173",
            "https://growmate.site",
            "https://www.growmate.site",
            "https://growmate-xdig.vercel.app"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

var app = builder.Build();



app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();
app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
