
using Azure.Storage.Blobs;
using InvoiceApp.Data.DAO;
using InvoiceApp.Data.Models;
using InvoiceApp.Data.Models.IRepository;
using InvoiceApp.Data.Models.Repository;
using InvoiceApp.Middlewares;
using InvoiceApp.Services.Helper;
using InvoiceApp.Services.IServices;
using InvoiceApp.Services.Services;
using InvoiceAppApi.Mapping;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Middlewares;
using System.Text;
using System.Text.Json.Serialization;

namespace InvoiceAppWebApi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<InvoiceAppDbContext>(options =>
                options.UseSqlServer(connectionString, b => b.MigrationsAssembly("InvoiceApp.Api")));

            builder.Services.AddControllers().AddJsonOptions(opt =>
            {
                opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });

            builder.Services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
            });

            builder.Services
                .AddIdentity<ApplicationUser, IdentityRole>(options =>
                {
                    options.SignIn.RequireConfirmedAccount = false;
                    options.User.RequireUniqueEmail = true;
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireUppercase = false;
                    options.Password.RequiredLength = 6;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Lockout.AllowedForNewUsers = false;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(2);
                    options.Lockout.MaxFailedAccessAttempts = 3;
                })
                .AddDefaultTokenProviders()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<InvoiceAppDbContext>();

            var jwtTokenSettings = builder.Configuration.GetSection("JwtTokenSettings").Get<JwtTokenSettings>();

            builder.Services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.IncludeErrorDetails = true;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ClockSkew = TimeSpan.Zero,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtTokenSettings.ValidIssuer,
                    ValidAudience = jwtTokenSettings.ValidAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtTokenSettings.SymmetricSecurityKey)
                    ),
                };
            });

            //CORS
            //builder.Services.AddCors(options =>
            //{
            //    options.AddPolicy("all", builder => builder.AllowAnyOrigin()
            //        .AllowAnyHeader()
            //        .AllowAnyMethod());
            //});

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(option =>
            {
                option.SwaggerDoc("v1", new OpenApiInfo { Title = "Invoice App API", Version = "v1" });
                option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Enter the Bearer Authorization: `Bearer Generated-JWT-Token`",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                option.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type=ReferenceType.SecurityScheme,
                                Id=JwtBearerDefaults.AuthenticationScheme
                            }
                        },
                        new string[]{}
                    }
                });
                option.EnableAnnotations();
            });

            var storageConnection = builder.Configuration["BlobStorageSettings:ConnectionString"];

            // Custom Services 
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddScoped<ITokenService, TokenService>(); 
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IInvoiceService, InvoiceService>();
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IProfilePictureService, ProfilePictureService>();
            builder.Services.AddScoped<ISwaggerCredentialsService, SwaggerCredentialsService>();
            builder.Services.AddScoped<IPokemonService, PokemonService>();
            builder.Services.AddScoped<IInvoiceIdService, InvoiceIdService>();
            builder.Services.AddScoped<IFeedbackService, FeedbackService>();

            builder.Services.AddSingleton(jwtTokenSettings);
            builder.Services.AddSingleton<IBlobRepository, BlobRepository>();

            builder.Services.AddHostedService<CacheRefreshBackgroundService>();
            builder.Services.AddHostedService<ClearExpiredCredentialsService>();

            builder.Services.AddHttpClient<PokemonService>();


            builder.Services.AddAzureClients(azureBuilder =>
            {
                azureBuilder.AddBlobServiceClient(storageConnection);
            });
            builder.Services.Configure<BlobStorageSettings>(builder.Configuration.GetSection("BlobStorageSettings"));
            builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

            var app = builder.Build();

            app.Use(async (context, next) =>
            {
                if (context.Request.Path.Value == "/")
                {
                    context.Response.Redirect("/swagger");
                }
                else
                {
                    await next();
                }
            });

            // Custom Middleware             
            app.UseMiddleware<SwaggerBasicAuthMiddleware>();
           

            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI();
            //app.UseCors("all");


            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseMiddleware<UserDetailsMiddleware>();
            app.UseAuthorization();
            app.MapControllers();

            using (var scope = app.Services.CreateScope())
            {
                var invoiceIdService = scope.ServiceProvider.GetRequiredService<IInvoiceIdService>();
                await invoiceIdService.RefreshCacheAsync();
            }
            app.Run();
        }
    }
}