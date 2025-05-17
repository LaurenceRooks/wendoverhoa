# Authentication Flow Documentation

This document outlines the authentication and authorization mechanisms used in the Wendover HOA application, following Clean Architecture principles, CQRS pattern, and industry best practices for security.

## Authentication Architecture

The Wendover HOA application implements a comprehensive token-based authentication system using OAuth 2.0 and JWT (JSON Web Tokens), built on ASP.NET Core Identity.

### High-Level Authentication Flow

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│             │     │             │     │             │     │             │
│    Client   │────▶│  Auth API   │────▶│  Token Gen  │────▶│  Protected  │
│             │     │  Endpoints  │     │  Service    │     │  Resources  │
│             │     │             │     │             │     │             │
└─────────────┘     └─────────────┘     └─────────────┘     └─────────────┘
       │                                       │                   │
       │                                       │                   │
       │                                       ▼                   │
       │                               ┌─────────────┐            │
       │                               │             │            │
       └──────────────────────────────│  Identity    │◀───────────┘
                                      │  Store       │
                                      │             │
                                      └─────────────┘
```

## Authentication Mechanisms

### 1. JWT-Based Authentication

The primary authentication mechanism is JWT (JSON Web Token) based, providing a stateless authentication solution.

#### JWT Structure

```
┌─────────────┐.┌─────────────┐.┌─────────────┐
│             │ │             │ │             │
│   Header    │.│   Payload   │.│  Signature  │
│             │ │             │ │             │
└─────────────┘ └─────────────┘ └─────────────┘
```

**Header Example:**
```json
{
  "alg": "RS256",
  "typ": "JWT",
  "kid": "wendover-key-2025-05"
}
```

**Payload Example:**
```json
{
  "sub": "123",
  "name": "John Doe",
  "email": "john.doe@example.com",
  "roles": ["Resident", "BoardMember"],
  "permissions": ["property.view", "dues.pay"],
  "iat": 1589547426,
  "exp": 1589551026,
  "iss": "https://api.wendoverhoa.org",
  "aud": "wendoverhoa-web"
}
```

#### JWT Lifetime and Security

- **Access Token Lifetime**: 15 minutes
- **Refresh Token Lifetime**: 14 days (sliding expiration)
- **Token Signing**: RS256 (asymmetric)
- **Key Rotation**: Every 90 days
- **Token Storage**: HttpOnly, Secure cookies for web clients; Secure storage for mobile clients

### 2. Refresh Token Mechanism

To maintain user sessions without requiring frequent re-authentication, a refresh token mechanism is implemented.

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│             │     │             │     │             │
│    Client   │────▶│  /token/    │────▶│   Validate  │
│ (Expired JWT)     │  refresh    │     │   Refresh   │
│             │     │             │     │    Token    │
└─────────────┘     └─────────────┘     └─────────────┘
                                               │
                                               ▼
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│             │     │             │     │             │
│    Client   │◀────│   Return    │◀────│  Generate   │
│ (New Tokens)│     │  New Tokens │     │ New Tokens  │
│             │     │             │     │             │
└─────────────┘     └─────────────┘     └─────────────┘
```

#### Refresh Token Security

- Refresh tokens are stored in the database with a cryptographic hash
- Each refresh token is associated with a specific device/client
- Refresh tokens can be revoked individually or for all user sessions
- Suspicious activity triggers automatic token revocation
- Rotation of refresh tokens on each use (one-time use)

### 3. External Authentication Providers

The application supports authentication via external providers:

- Microsoft Account
- Google
- Apple ID

#### External Authentication Flow

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│             │     │             │     │             │
│    Client   │────▶│  External   │────▶│  External   │
│             │     │  Auth URL   │     │  Provider   │
│             │     │             │     │             │
└─────────────┘     └─────────────┘     └─────────────┘
                                               │
                                               ▼
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│             │     │             │     │             │
│    Client   │◀────│  Wendover   │◀────│  Callback   │
│ (With Tokens)     │  Auth API   │     │  with Code  │
│             │     │             │     │             │
└─────────────┘     └─────────────┘     └─────────────┘
```

## Authorization

### Role-Based Access Control (RBAC)

The application implements RBAC with the following predefined roles:

| Role | Description |
|------|-------------|
| Administrator | Full system access |
| BoardMember | Access to board functions and reporting |
| CommitteeMember | Access to committee-specific functions |
| Resident | Basic resident access |
| Guest | Limited public access |

### Permission-Based Access Control

In addition to roles, fine-grained permissions are used:

| Permission | Description |
|------------|-------------|
| property.view | View property details |
| property.edit | Edit property information |
| dues.view | View dues information |
| dues.pay | Make dues payments |
| expense.approve | Approve expenses |
| meeting.create | Create meeting minutes |
| report.view | View financial reports |
| user.manage | Manage user accounts |

Permissions are assigned to roles and can also be assigned directly to users.

### Policy-Based Authorization

Complex authorization rules are implemented as policies:

```csharp
services.AddAuthorization(options =>
{
    options.AddPolicy("CanManageProperties", policy =>
        policy.RequireAssertion(context =>
            context.User.IsInRole("Administrator") ||
            context.User.IsInRole("BoardMember") ||
            context.User.HasClaim(c => c.Type == "Permission" && c.Value == "property.edit")));
    
    options.AddPolicy("CanApproveExpenses", policy =>
        policy.RequireAssertion(context =>
            context.User.IsInRole("Administrator") ||
            (context.User.IsInRole("BoardMember") && 
             context.User.HasClaim(c => c.Type == "Position" && 
                                   (c.Value == "President" || c.Value == "Treasurer")))));
});
```

## API Authentication Endpoints

### 1. User Registration

```
POST /api/v1/auth/register
```

Request:
```json
{
  "email": "john.doe@example.com",
  "password": "SecureP@ssw0rd",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+12345678901"
}
```

Response:
```json
{
  "success": true,
  "data": {
    "userId": 123,
    "email": "john.doe@example.com",
    "requiresEmailConfirmation": true
  }
}
```

### 2. Email Confirmation

```
GET /api/v1/auth/confirm-email?userId=123&token=ABC123
```

Response:
```json
{
  "success": true,
  "data": {
    "emailConfirmed": true,
    "loginUrl": "/login"
  }
}
```

### 3. User Login

```
POST /api/v1/auth/login
```

Request:
```json
{
  "email": "john.doe@example.com",
  "password": "SecureP@ssw0rd",
  "rememberMe": true,
  "deviceInfo": {
    "deviceId": "browser-chrome-windows",
    "deviceName": "Chrome on Windows",
    "ipAddress": "192.168.1.1"
  }
}
```

Response:
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "def5020089abc...",
    "expiresIn": 900,
    "tokenType": "Bearer",
    "user": {
      "id": 123,
      "email": "john.doe@example.com",
      "firstName": "John",
      "lastName": "Doe",
      "roles": ["Resident"]
    }
  }
}
```

### 4. External Provider Authentication

```
GET /api/v1/auth/external-login?provider=Google
```

Response: Redirect to Google authentication page

Callback:
```
GET /api/v1/auth/external-login-callback?code=ABC123
```

Response:
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "def5020089abc...",
    "expiresIn": 900,
    "tokenType": "Bearer",
    "user": {
      "id": 123,
      "email": "john.doe@example.com",
      "firstName": "John",
      "lastName": "Doe",
      "roles": ["Resident"]
    }
  }
}
```

### 5. Refresh Token

```
POST /api/v1/auth/refresh-token
```

Request:
```json
{
  "refreshToken": "def5020089abc..."
}
```

Response:
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "ghi6030089def...",
    "expiresIn": 900,
    "tokenType": "Bearer"
  }
}
```

### 6. Logout

```
POST /api/v1/auth/logout
```

Request:
```json
{
  "refreshToken": "def5020089abc..."
}
```

Response:
```json
{
  "success": true,
  "data": {
    "message": "Successfully logged out"
  }
}
```

### 7. Password Reset Request

```
POST /api/v1/auth/forgot-password
```

Request:
```json
{
  "email": "john.doe@example.com"
}
```

Response:
```json
{
  "success": true,
  "data": {
    "message": "If your email is registered, you will receive password reset instructions"
  }
}
```

### 8. Password Reset

```
POST /api/v1/auth/reset-password
```

Request:
```json
{
  "email": "john.doe@example.com",
  "token": "ABC123",
  "newPassword": "NewSecureP@ssw0rd"
}
```

Response:
```json
{
  "success": true,
  "data": {
    "message": "Password has been reset successfully"
  }
}
```

### 9. Change Password

```
POST /api/v1/auth/change-password
```

Request:
```json
{
  "currentPassword": "SecureP@ssw0rd",
  "newPassword": "NewSecureP@ssw0rd"
}
```

Response:
```json
{
  "success": true,
  "data": {
    "message": "Password has been changed successfully"
  }
}
```

### 10. Get Current User

```
GET /api/v1/auth/me
```

Response:
```json
{
  "success": true,
  "data": {
    "id": 123,
    "email": "john.doe@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "phoneNumber": "+12345678901",
    "roles": ["Resident", "BoardMember"],
    "permissions": ["property.view", "dues.pay", "meeting.create"],
    "emailConfirmed": true,
    "twoFactorEnabled": false,
    "lastLoginDate": "2025-05-15T10:30:00Z"
  }
}
```

## Client Implementation

### 1. Authentication Header

All authenticated requests must include the JWT in the Authorization header:

```
Authorization: Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...
```

### 2. Token Management

Clients should implement the following token management logic:

1. Store access and refresh tokens securely
2. Include access token in all API requests
3. Monitor token expiration
4. Automatically refresh tokens when they expire
5. Handle authentication errors (401, 403)
6. Clear tokens on logout

### 3. Web Client Example (JavaScript)

```javascript
// Axios interceptor for token management
axios.interceptors.request.use(
  config => {
    const token = localStorage.getItem('accessToken');
    if (token) {
      config.headers['Authorization'] = `Bearer ${token}`;
    }
    return config;
  },
  error => Promise.reject(error)
);

// Handle 401 responses
axios.interceptors.response.use(
  response => response,
  async error => {
    const originalRequest = error.config;
    
    // If error is 401 and we haven't tried refreshing the token yet
    if (error.response.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;
      
      try {
        // Attempt to refresh the token
        const refreshToken = localStorage.getItem('refreshToken');
        const response = await axios.post('/api/v1/auth/refresh-token', {
          refreshToken
        });
        
        // Store the new tokens
        const { accessToken, refreshToken: newRefreshToken } = response.data.data;
        localStorage.setItem('accessToken', accessToken);
        localStorage.setItem('refreshToken', newRefreshToken);
        
        // Retry the original request with the new token
        originalRequest.headers['Authorization'] = `Bearer ${accessToken}`;
        return axios(originalRequest);
      } catch (error) {
        // If refresh fails, redirect to login
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
        window.location.href = '/login';
        return Promise.reject(error);
      }
    }
    
    return Promise.reject(error);
  }
);
```

## Two-Factor Authentication (2FA)

The application supports two-factor authentication for enhanced security.

### 2FA Flow

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│             │     │             │     │             │
│    Client   │────▶│  /auth/     │────▶│  Generate   │
│             │     │  login      │     │  2FA Code   │
│             │     │             │     │             │
└─────────────┘     └─────────────┘     └─────────────┘
                                               │
                                               ▼
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│             │     │             │     │             │
│    Client   │────▶│  /auth/     │────▶│   Verify    │
│ (Enter Code)│     │  verify-2fa │     │   2FA Code  │
│             │     │             │     │             │
└─────────────┘     └─────────────┘     └─────────────┘
                                               │
                                               ▼
┌─────────────┐     ┌─────────────┐
│             │     │             │
│    Client   │◀────│  Return     │
│ (With Tokens)     │  Tokens     │
│             │     │             │
└─────────────┘     └─────────────┘
```

### 2FA Setup

```
POST /api/v1/auth/setup-2fa
```

Response:
```json
{
  "success": true,
  "data": {
    "secretKey": "JBSWY3DPEHPK3PXP",
    "qrCodeUrl": "data:image/png;base64,iVBORw0KGgoAAAANSUh...",
    "manualEntryKey": "JBSW Y3DP EHPK 3PXP",
    "verificationRequired": true
  }
}
```

### 2FA Verification

```
POST /api/v1/auth/verify-2fa
```

Request:
```json
{
  "code": "123456"
}
```

Response:
```json
{
  "success": true,
  "data": {
    "verified": true,
    "recoveryCodes": [
      "ABCD-EFGH-IJKL-MNOP",
      "QRST-UVWX-YZ12-3456",
      "7890-ABCD-EFGH-IJKL",
      "MNOP-QRST-UVWX-YZ12"
    ]
  }
}
```

## Security Considerations

### Token Security

1. **Token Storage**:
   - Web clients: HttpOnly, Secure cookies or localStorage with proper security measures
   - Mobile clients: Secure storage (Keychain for iOS, KeyStore for Android)

2. **Token Validation**:
   - Validate issuer, audience, expiration
   - Check signature using public key
   - Validate claims and permissions

3. **Token Revocation**:
   - Maintain a blacklist of revoked tokens
   - Implement token versioning for mass revocation

### Password Security

1. **Password Requirements**:
   - Minimum 10 characters
   - Mix of uppercase, lowercase, numbers, and special characters
   - Check against common password lists
   - Implement password strength meter

2. **Password Storage**:
   - Use ASP.NET Core Identity's password hasher (PBKDF2 with HMAC-SHA256)
   - High iteration count (10,000+)

3. **Account Lockout**:
   - Implement progressive delays after failed attempts
   - Lock account after 5 failed attempts for 15 minutes
   - Notify user of suspicious login attempts

### API Security

1. **Rate Limiting**:
   - Limit authentication attempts to prevent brute force attacks
   - Implement IP-based and account-based rate limiting

2. **HTTPS Only**:
   - All authentication endpoints require HTTPS
   - Implement HSTS headers

3. **CSRF Protection**:
   - Include anti-forgery tokens for state-changing operations
   - Implement SameSite cookie attributes

## Implementation with ASP.NET Core Identity

The authentication system is implemented using ASP.NET Core Identity with the following customizations:

### Identity Configuration

```csharp
services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    // Password settings
    options.Password.RequiredLength = 10;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    
    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    
    // User settings
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    
    // Email confirmation required
    options.SignIn.RequireConfirmedEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddTokenProvider<TwoFactorTokenProvider<ApplicationUser>>("2FA");
```

### JWT Configuration

```csharp
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
        IssuerSigningKey = new RsaSecurityKey(rsaKey),
        ClockSkew = TimeSpan.FromMinutes(1)
    };
    
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Add("Token-Expired", "true");
            }
            return Task.CompletedTask;
        }
    };
});
```

### External Authentication Configuration

```csharp
services.AddAuthentication()
.AddMicrosoftAccount(options =>
{
    options.ClientId = Configuration["Authentication:Microsoft:ClientId"];
    options.ClientSecret = Configuration["Authentication:Microsoft:ClientSecret"];
})
.AddGoogle(options =>
{
    options.ClientId = Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
})
.AddApple(options =>
{
    options.ClientId = Configuration["Authentication:Apple:ClientId"];
    options.KeyId = Configuration["Authentication:Apple:KeyId"];
    options.TeamId = Configuration["Authentication:Apple:TeamId"];
    options.PrivateKey = Configuration["Authentication:Apple:PrivateKey"];
});
```

## Audit and Logging

All authentication events are logged for security and compliance purposes:

1. **Login/Logout Events**:
   - Successful and failed login attempts
   - Logout events
   - IP address and device information

2. **Token Events**:
   - Token issuance
   - Token refresh
   - Token revocation

3. **Account Events**:
   - Account creation
   - Email confirmation
   - Password changes and resets
   - Role and permission changes

Logs are stored securely and accessible only to administrators.

## Disaster Recovery

### Account Recovery

1. **Email-Based Recovery**:
   - Password reset via email
   - Account unlock via email

2. **2FA Recovery**:
   - Recovery codes for 2FA bypass
   - Administrator-assisted 2FA reset

### System Recovery

1. **Key Rotation**:
   - Regular rotation of signing keys
   - Graceful transition period for key changes

2. **Backup Authentication**:
   - Fallback authentication mechanisms
   - Emergency access procedures

## Compliance

The authentication system is designed to comply with:

1. **GDPR**:
   - Right to access personal data
   - Right to be forgotten
   - Data portability

2. **CCPA**:
   - Consumer data rights
   - Opt-out mechanisms

3. **Industry Best Practices**:
   - OWASP Top 10 security considerations
   - NIST authentication guidelines

## Testing and Quality Assurance

The authentication system undergoes rigorous testing:

1. **Unit Tests**:
   - Test individual components in isolation
   - Mock external dependencies

2. **Integration Tests**:
   - Test authentication flow end-to-end
   - Test with real external providers in sandbox mode

3. **Security Testing**:
   - Penetration testing
   - Token security analysis
   - Password security analysis

## Future Enhancements

Planned enhancements to the authentication system:

1. **WebAuthn Support**:
   - Passwordless authentication
   - Hardware security key support

2. **Adaptive Authentication**:
   - Risk-based authentication
   - Behavioral biometrics

3. **Enhanced Monitoring**:
   - Real-time threat detection
   - Advanced analytics for suspicious activities
