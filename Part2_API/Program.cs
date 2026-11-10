using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using poe11.RaceDayDatabase;

namespace poe11
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter your JWT token."
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            });
            builder.Services.AddDbContext<RaceDayDatabaseContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            var jwtKey = builder.Configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("Jwt:Key is missing from configuration.");
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                        ValidateIssuer = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidateAudience = true,
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromSeconds(30)
                    };
                });
            builder.Services.AddAuthorization();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var database = scope.ServiceProvider.GetRequiredService<RaceDayDatabaseContext>();
                database.Database.EnsureCreated();
                database.Database.ExecuteSqlRaw(@"
                    IF OBJECT_ID(N'[dbo].[Events]', N'U') IS NOT NULL
                       AND COL_LENGTH(N'dbo.Events', N'ImagePath') IS NULL
                        ALTER TABLE [dbo].[Events] ADD [ImagePath] nvarchar(max) NULL;

                    IF OBJECT_ID(N'[dbo].[Users]', N'U') IS NOT NULL
                       AND COL_LENGTH(N'dbo.Users', N'ProfileImagePath') IS NULL
                        ALTER TABLE [dbo].[Users] ADD [ProfileImagePath] nvarchar(max) NULL;

                    -- Repair an older EventTypes table that exists without a primary key.
                    IF OBJECT_ID(N'[dbo].[EventTypes]', N'U') IS NOT NULL
                       AND NOT EXISTS (
                           SELECT 1
                           FROM sys.key_constraints
                           WHERE [type] = 'PK'
                             AND [parent_object_id] = OBJECT_ID(N'[dbo].[EventTypes]')
                       )
                        ALTER TABLE [dbo].[EventTypes]
                        ADD CONSTRAINT [PK_EventTypes]
                        PRIMARY KEY CLUSTERED ([EventTypeCode]);
                ");
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
