# Security Architecture

## Introduction

This document outlines the security architecture of the Wendover HOA web application. Security is a fundamental aspect of the application design, ensuring that user data is protected, access is properly controlled, and the application is resilient against common security threats.

## Role-Based Access Control Implementation

The Wendover HOA application implements a comprehensive role-based access control (RBAC) system to ensure that users can only access the features and data they are authorized to use.

### User Roles

The application defines five standard user roles with increasing levels of access:

1. **Guest** - Unauthenticated users with limited access
2. **Resident** - Authenticated homeowners with basic access
3. **Committee Member** - Residents with additional permissions for specific committees
4. **Board Member** - HOA board members with administrative access
5. **Administrator** - Full system access for managing all aspects of the application

### Role Implementation with ASP.NET Core Identity

The application uses ASP.NET Core Identity for role management:

```csharp
public static class ApplicationDbContextSeed
{
    public static async Task SeedDefaultRolesAsync(RoleManager<IdentityRole<int>> roleManager)
    {
        var roles = new List<IdentityRole<int>>
        {
            new IdentityRole<int> { Name = "Administrator" },
            new IdentityRole<int> { Name = "BoardMember" },
            new IdentityRole<int> { Name = "CommitteeMember" },
            new IdentityRole<int> { Name = "Resident" }
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role.Name))
            {
                await roleManager.CreateAsync(role);
            }
        }
    }
}
```

### Role Assignment

Roles are assigned to users through the user management interface or programmatically:

```csharp
public class AssignRoleCommandHandler : IRequestHandler<AssignRoleCommand, Unit>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICurrentUserService _currentUserService;

    public AssignRoleCommandHandler(
        UserManager<ApplicationUser> userManager,
        ICurrentUserService currentUserService)
    {
        _userManager = userManager;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(
        AssignRoleCommand request, 
        CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        
        if (user == null)
        {
            throw new NotFoundException(nameof(ApplicationUser), request.UserId);
        }

        // Only Administrators can assign roles
        if (!_currentUserService.IsInRole("Administrator"))
        {
            throw new ForbiddenAccessException();
        }

        // Validate role
        if (!new[] { "Administrator", "BoardMember", "CommitteeMember", "Resident" }
            .Contains(request.RoleName))
        {
            throw new ValidationException(new[] 
            { 
                new ValidationFailure("RoleName", "Invalid role name") 
            });
        }

        // Remove existing roles
        var existingRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, existingRoles);

        // Assign new role
        await _userManager.AddToRoleAsync(user, request.RoleName);

        return Unit.Value;
    }
}
```

### Authorization Policies

The application defines authorization policies to simplify role-based access control:

```csharp
public static class AuthorizationPolicies
{
    public static void AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireAdministratorRole", policy =>
                policy.RequireRole("Administrator"));
                
            options.AddPolicy("RequireBoardMemberRole", policy =>
                policy.RequireRole("Administrator", "BoardMember"));
                
            options.AddPolicy("RequireCommitteeMemberRole", policy =>
                policy.RequireRole("Administrator", "BoardMember", "CommitteeMember"));
                
            options.AddPolicy("RequireResidentRole", policy =>
                policy.RequireRole("Administrator", "BoardMember", "CommitteeMember", "Resident"));
                
            // Feature-specific policies
            options.AddPolicy("CanManageAnnouncements", policy =>
                policy.RequireRole("Administrator", "BoardMember"));
                
            options.AddPolicy("CanManageDocuments", policy =>
                policy.RequireRole("Administrator", "BoardMember"));
                
            options.AddPolicy("CanManageUsers", policy =>
                policy.RequireRole("Administrator"));
        });
    }
}
```

### Controller Authorization

Controllers and actions are decorated with authorization attributes:

```csharp
[Authorize(Policy = "RequireBoardMemberRole")]
[ApiController]
[Route("api/[controller]")]
public class AnnouncementsController : ControllerBase
{
    // Only Administrators and Board Members can create announcements
    [HttpPost]
    public async Task<ActionResult<int>> CreateAnnouncement(CreateAnnouncementCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetAnnouncement), new { id = result }, result);
    }

    // All authenticated users can view announcements
    [Authorize(Policy = "RequireResidentRole")]
    [HttpGet]
    public async Task<ActionResult<List<AnnouncementDto>>> GetAnnouncements()
    {
        var query = new GetAnnouncementsQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
```

### MediatR Authorization

Commands and queries are authorized using a MediatR behavior:

```csharp
public class AuthorizationBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IIdentityService _identityService;

    public AuthorizationBehavior(
        ICurrentUserService currentUserService,
        IIdentityService identityService)
    {
        _currentUserService = currentUserService;
        _identityService = identityService;
    }

    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        var authorizeAttributes = request.GetType().GetCustomAttributes<AuthorizeAttribute>();

        if (authorizeAttributes.Any())
        {
            // Must be authenticated user
            if (_currentUserService.UserId == 0)
            {
                throw new UnauthorizedAccessException();
            }

            // Role-based authorization
            var authorizeAttributesWithRoles = authorizeAttributes
                .Where(a => !string.IsNullOrWhiteSpace(a.Roles));

            if (authorizeAttributesWithRoles.Any())
            {
                foreach (var roles in authorizeAttributesWithRoles.Select(a => a.Roles.Split(',')))
                {
                    var authorized = false;
                    foreach (var role in roles)
                    {
                        var isInRole = await _identityService.IsInRoleAsync(
                            _currentUserService.UserId, role.Trim());
                            
                        if (isInRole)
                        {
                            authorized = true;
                            break;
                        }
                    }

                    // Must be a member of at least one role in roles
                    if (!authorized)
                    {
                        throw new ForbiddenAccessException();
                    }
                }
            }

            // Policy-based authorization
            var authorizeAttributesWithPolicies = authorizeAttributes
                .Where(a => !string.IsNullOrWhiteSpace(a.Policy));

            if (authorizeAttributesWithPolicies.Any())
            {
                foreach (var policy in authorizeAttributesWithPolicies.Select(a => a.Policy))
                {
                    var authorized = await _identityService.AuthorizeAsync(
                        _currentUserService.UserId, policy);

                    if (!authorized)
                    {
                        throw new ForbiddenAccessException();
                    }
                }
            }
        }

        // User is authorized / authorization not required
        return await next();
    }
}
```

## API Security Measures

The Wendover HOA application implements several security measures to protect its API endpoints.

### HTTPS Enforcement

All API endpoints are secured using HTTPS:

```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    // Force HTTPS
    app.UseHttpsRedirection();
    
    // Use HSTS in production
    if (!env.IsDevelopment())
    {
        app.UseHsts();
    }
    
    // Other middleware
}
```

### JWT Authentication

API endpoints are secured using JWT authentication:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Add JWT authentication
    services.AddAuthentication(options =>
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
            ValidIssuer = Configuration["Jwt:Issuer"],
            ValidAudience = Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))
        };
    });
}
```

### JWT Token Generation

JWT tokens are generated with appropriate claims and expiration:

```csharp
public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<ApplicationUser> _userManager;

    public TokenService(
        IConfiguration configuration,
        UserManager<ApplicationUser> userManager)
    {
        _configuration = configuration;
        _userManager = userManager;
    }

    public async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email)
        };
        
        // Add roles as claims
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);
            
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

### CSRF Protection

The application implements CSRF protection for form submissions:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddAntiforgery(options =>
    {
        options.HeaderName = "X-XSRF-TOKEN";
        options.Cookie.Name = "XSRF-TOKEN";
        options.Cookie.HttpOnly = false; // Allow JavaScript to read the cookie
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });
}

public void Configure(IApplicationBuilder app)
{
    // Other middleware
    
    app.Use(async (context, next) =>
    {
        var antiforgery = context.RequestServices.GetRequiredService<IAntiforgery>();
        
        // Generate tokens for views
        if (context.Request.Path.StartsWithSegments("/"))
        {
            var tokens = antiforgery.GetAndStoreTokens(context);
            context.Response.Cookies.Append(
                "XSRF-TOKEN", 
                tokens.RequestToken, 
                new CookieOptions
                {
                    HttpOnly = false, // Allow JavaScript to read the cookie
                    SameSite = SameSiteMode.Strict,
                    Secure = true
                });
        }
        
        await next();
    });
}
```

### Rate Limiting

The application implements rate limiting to prevent abuse:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddRateLimiter(options =>
    {
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        {
            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.User.Identity?.IsAuthenticated == true
                    ? context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                    : context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                factory: partition => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = 100,
                    Window = TimeSpan.FromMinutes(1)
                });
        });

        options.OnRejected = async (context, token) =>
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            
            await context.HttpContext.Response.WriteAsync(
                "Too many requests. Please try again later.", token);
        };
    });
}

public void Configure(IApplicationBuilder app)
{
    // Other middleware
    
    app.UseRateLimiter();
}
```

### Content Security Policy

The application implements a Content Security Policy to prevent XSS attacks:

```csharp
public void Configure(IApplicationBuilder app)
{
    // Other middleware
    
    app.Use(async (context, next) =>
    {
        context.Response.Headers.Add(
            "Content-Security-Policy",
            "default-src 'self'; " +
            "script-src 'self' https://cdn.jsdelivr.net; " +
            "style-src 'self' https://cdn.jsdelivr.net; " +
            "img-src 'self' data:; " +
            "font-src 'self' https://cdn.jsdelivr.net; " +
            "connect-src 'self'; " +
            "frame-ancestors 'none'; " +
            "form-action 'self';");
            
        await next();
    });
}
```

## Data Protection Strategies

The Wendover HOA application implements several strategies to protect sensitive data.

### Sensitive Data Encryption

Sensitive data is encrypted before being stored in the database:

```csharp
public class EncryptionService : IEncryptionService
{
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IDataProtector _protector;

    public EncryptionService(IDataProtectionProvider dataProtectionProvider)
    {
        _dataProtectionProvider = dataProtectionProvider;
        _protector = _dataProtectionProvider.CreateProtector("WendoverHOA.SensitiveData");
    }

    public string Encrypt(string plainText)
    {
        return _protector.Protect(plainText);
    }

    public string Decrypt(string cipherText)
    {
        return _protector.Unprotect(cipherText);
    }
}
```

### Entity Framework Value Converters

Entity Framework value converters are used to automatically encrypt and decrypt sensitive data:

```csharp
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
{
    private readonly IEncryptionService _encryptionService;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IEncryptionService encryptionService)
        : base(options)
    {
        _encryptionService = encryptionService;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure value converters for sensitive data
        builder.Entity<Resident>()
            .Property(r => r.PhoneNumber)
            .HasConversion(
                v => _encryptionService.Encrypt(v),
                v => _encryptionService.Decrypt(v));
                
        builder.Entity<Resident>()
            .Property(r => r.Email)
            .HasConversion(
                v => _encryptionService.Encrypt(v),
                v => _encryptionService.Decrypt(v));
    }
}
```

### Password Hashing

User passwords are securely hashed using ASP.NET Core Identity:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
    {
        // Password settings
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 12;
        
        // Lockout settings
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.MaxFailedAccessAttempts = 5;
        
        // User settings
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
}
```

### Secure File Storage

Files are stored securely with appropriate access controls:

```csharp
public class FileStorageService : IFileStorageService
{
    private readonly string _basePath;
    private readonly ICurrentUserService _currentUserService;

    public FileStorageService(
        IConfiguration configuration,
        ICurrentUserService currentUserService)
    {
        _basePath = configuration["FileStorage:BasePath"];
        _currentUserService = currentUserService;
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string contentType)
    {
        // Generate a unique file name
        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        
        // Determine the file path
        var filePath = Path.Combine(_basePath, uniqueFileName);
        
        // Create directory if it doesn't exist
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        
        // Save the file
        using (var fileStream2 = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(fileStream2);
        }
        
        // Log the file upload
        // _logger.LogInformation("File {FileName} uploaded by user {UserId}", uniqueFileName, _currentUserService.UserId);
        
        return uniqueFileName;
    }

    public async Task<(Stream FileStream, string ContentType)> GetFileAsync(string fileName)
    {
        // Determine the file path
        var filePath = Path.Combine(_basePath, fileName);
        
        // Check if the file exists
        if (!File.Exists(filePath))
        {
            throw new NotFoundException(nameof(File), fileName);
        }
        
        // Determine the content type
        var contentType = GetContentType(fileName);
        
        // Open the file
        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        
        return (fileStream, contentType);
    }

    private string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            _ => "application/octet-stream"
        };
    }
}
```

### Audit Logging

All sensitive operations are logged for audit purposes:

```csharp
public class AuditLoggingBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogService _auditLogService;

    public AuditLoggingBehavior(
        ICurrentUserService currentUserService,
        IAuditLogService auditLogService)
    {
        _currentUserService = currentUserService;
        _auditLogService = auditLogService;
    }

    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        // Check if the request is auditable
        var auditableAttribute = request.GetType().GetCustomAttribute<AuditableAttribute>();
        
        if (auditableAttribute != null)
        {
            // Log the request
            await _auditLogService.LogAsync(
                auditableAttribute.EntityType,
                auditableAttribute.Action,
                JsonSerializer.Serialize(request),
                _currentUserService.UserId);
        }
        
        return await next();
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class AuditableAttribute : Attribute
{
    public string EntityType { get; }
    public string Action { get; }

    public AuditableAttribute(string entityType, string action)
    {
        EntityType = entityType;
        Action = action;
    }
}

public interface IAuditLogService
{
    Task LogAsync(string entityType, string action, string data, int userId);
}

public class AuditLogService : IAuditLogService
{
    private readonly ApplicationDbContext _context;

    public AuditLogService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(string entityType, string action, string data, int userId)
    {
        var auditLog = new AuditLog
        {
            EntityType = entityType,
            Action = action,
            Data = data,
            UserId = userId,
            Timestamp = DateTime.UtcNow
        };
        
        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }
}
```

## Conclusion

The security architecture of the Wendover HOA application provides a comprehensive approach to protecting user data, controlling access, and preventing common security threats. By implementing role-based access control, API security measures, and data protection strategies, the application achieves a high level of security while maintaining usability and performance.
