# API Standards and Conventions

This document outlines the standards and conventions for the Wendover HOA API, ensuring consistency across all endpoints and facilitating easier integration with frontend components and third-party systems.

## API Design Principles

The Wendover HOA API follows these core principles:

1. **RESTful Design**: Resources are represented as nouns, not verbs
2. **Clean Architecture**: API layer is separated from business logic
3. **CQRS Pattern**: Commands and queries are separated
4. **Consistent Responses**: Standard response format across all endpoints
5. **Secure by Default**: All endpoints require appropriate authentication and authorization
6. **Versioned**: API supports versioning to ensure backward compatibility
7. **Well-Documented**: All endpoints are documented with Swagger/OpenAPI

## URL Structure

### Base URL

```
https://api.wendoverhoa.org/
```

For development:
```
https://localhost:7001/api/
```

### Resource Paths

Paths follow this structure:
```
/api/v{version}/{resource}/{id?}/{sub-resource?}
```

Examples:
- `/api/v1/properties` - Get all properties
- `/api/v1/properties/123` - Get property with ID 123
- `/api/v1/properties/123/residents` - Get residents for property with ID 123

### Naming Conventions

1. **Resources**: Plural nouns in lowercase (e.g., `properties`, `residents`, `dues-transactions`)
2. **URL Parameters**: Camel case (e.g., `propertyId`, `startDate`)
3. **Query Parameters**: Camel case (e.g., `?pageSize=10&sortBy=lastName`)
4. **Multi-word Resources**: Kebab case with hyphens (e.g., `meeting-minutes`, `board-members`)

## HTTP Methods

| Method | Usage | Example |
|--------|-------|---------|
| GET | Retrieve resources | `GET /api/v1/properties` |
| POST | Create a new resource | `POST /api/v1/properties` |
| PUT | Update a resource completely | `PUT /api/v1/properties/123` |
| PATCH | Update a resource partially | `PATCH /api/v1/properties/123` |
| DELETE | Delete a resource | `DELETE /api/v1/properties/123` |

## Request Format

### Headers

All requests must include:

```
Content-Type: application/json
Accept: application/json
Authorization: Bearer {token}  (except for authentication endpoints)
```

Optional headers:
```
X-Api-Version: 1  (alternative to URL versioning)
Accept-Language: en-US
```

### Request Body (POST, PUT, PATCH)

JSON format with camelCase property names:

```json
{
  "propertyId": 123,
  "address": "123 Main St",
  "city": "Bedford",
  "state": "TX",
  "zipCode": "76021",
  "lotNumber": "A-123"
}
```

### Query Parameters

Common query parameters for collection endpoints:

| Parameter | Description | Default | Example |
|-----------|-------------|---------|---------|
| pageNumber | Page number for pagination | 1 | `?pageNumber=2` |
| pageSize | Number of items per page | 10 | `?pageSize=25` |
| sortBy | Field to sort by | id | `?sortBy=lastName` |
| sortDirection | Sort direction (asc/desc) | asc | `?sortDirection=desc` |
| filter | Filter criteria | null | `?filter=status:active` |

## Response Format

### Success Response

All successful responses follow this structure:

```json
{
  "success": true,
  "data": {
    // Resource data or collection
  },
  "pagination": {
    // Only for collection endpoints
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 5,
    "totalCount": 42,
    "hasNext": true,
    "hasPrevious": false
  },
  "links": {
    "self": "https://api.wendoverhoa.org/api/v1/properties?pageNumber=1&pageSize=10",
    "next": "https://api.wendoverhoa.org/api/v1/properties?pageNumber=2&pageSize=10",
    "previous": null
  }
}
```

For single resource endpoints, the `pagination` property is omitted.

### Error Response

All error responses follow this structure:

```json
{
  "success": false,
  "error": {
    "code": "RESOURCE_NOT_FOUND",
    "message": "The requested resource was not found",
    "details": [
      "Property with ID 123 does not exist"
    ],
    "traceId": "00-1234567890abcdef1234567890abcdef-1234567890abcdef-00"
  }
}
```

## HTTP Status Codes

| Code | Description | Usage |
|------|-------------|-------|
| 200 | OK | Successful GET, PUT, PATCH requests |
| 201 | Created | Successful POST requests |
| 204 | No Content | Successful DELETE requests |
| 400 | Bad Request | Invalid request format or parameters |
| 401 | Unauthorized | Missing or invalid authentication |
| 403 | Forbidden | Authentication valid but insufficient permissions |
| 404 | Not Found | Resource not found |
| 409 | Conflict | Resource conflict (e.g., duplicate entry) |
| 422 | Unprocessable Entity | Validation errors |
| 429 | Too Many Requests | Rate limit exceeded |
| 500 | Internal Server Error | Server-side error |

## Validation

### Validation Errors

Validation errors return a 422 status code with details:

```json
{
  "success": false,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Validation failed",
    "details": [
      {
        "field": "address",
        "message": "Address is required"
      },
      {
        "field": "zipCode",
        "message": "Zip code must be in format XXXXX or XXXXX-XXXX"
      }
    ],
    "traceId": "00-1234567890abcdef1234567890abcdef-1234567890abcdef-00"
  }
}
```

### Common Validation Rules

| Field Type | Validation Rules |
|------------|------------------|
| Strings | Min/max length, required, pattern |
| Numbers | Min/max value, required |
| Dates | Format (ISO 8601), min/max date, required |
| Emails | Format, required |
| Phone Numbers | Format (E.164), required |
| Enums | Valid values, required |

## API Versioning

### URL-Based Versioning

The primary versioning strategy is URL-based:

```
/api/v1/properties
/api/v2/properties
```

### Header-Based Versioning (Alternative)

Clients can also specify version via header:

```
X-Api-Version: 1
```

### Version Lifecycle

1. **Current Version**: The latest stable version
2. **Supported Versions**: Versions still supported but not latest
3. **Deprecated Versions**: Versions scheduled for removal
4. **Sunset Versions**: Versions no longer supported

Versions are supported for at least 12 months after a new version is released.

## Pagination

### Offset Pagination

The default pagination strategy is offset-based:

```
GET /api/v1/properties?pageNumber=2&pageSize=10
```

Response includes pagination metadata:

```json
{
  "pagination": {
    "pageNumber": 2,
    "pageSize": 10,
    "totalPages": 5,
    "totalCount": 42,
    "hasNext": true,
    "hasPrevious": true
  }
}
```

### Cursor Pagination (for Large Collections)

For large collections, cursor-based pagination is available:

```
GET /api/v1/audit-logs?cursor=eyJpZCI6MTAwfQ==&limit=10
```

Response includes cursor information:

```json
{
  "pagination": {
    "nextCursor": "eyJpZCI6MTEwfQ==",
    "previousCursor": "eyJpZCI6OTB9==",
    "limit": 10
  }
}
```

## Filtering and Sorting

### Filtering

Basic filtering:
```
GET /api/v1/properties?filter=status:active
```

Multiple filters:
```
GET /api/v1/properties?filter=status:active,city:Bedford
```

Operators:
```
GET /api/v1/dues-transactions?filter=amount:gt:100,dueDate:lt:2025-06-01
```

Supported operators: `eq` (equal), `neq` (not equal), `gt` (greater than), `gte` (greater than or equal), `lt` (less than), `lte` (less than or equal), `contains`, `startswith`, `endswith`.

### Sorting

Basic sorting:
```
GET /api/v1/properties?sortBy=address
```

Sort direction:
```
GET /api/v1/properties?sortBy=address&sortDirection=desc
```

Multiple sort fields:
```
GET /api/v1/properties?sortBy=city,address
```

## Field Selection

Clients can request specific fields to reduce payload size:

```
GET /api/v1/properties?fields=id,address,city,zipCode
```

## HATEOAS Links

Responses include hypermedia links for navigation:

```json
{
  "links": {
    "self": "https://api.wendoverhoa.org/api/v1/properties/123",
    "residents": "https://api.wendoverhoa.org/api/v1/properties/123/residents",
    "dues": "https://api.wendoverhoa.org/api/v1/properties/123/dues"
  }
}
```

## Data Types and Formats

| Data Type | Format | Example |
|-----------|--------|---------|
| Dates | ISO 8601 (UTC) | `"2025-05-15T10:30:00Z"` |
| Times | ISO 8601 (UTC) | `"10:30:00Z"` |
| Timestamps | ISO 8601 (UTC) | `"2025-05-15T10:30:00.123Z"` |
| Durations | ISO 8601 | `"PT1H30M"` (1 hour 30 minutes) |
| Booleans | `true`/`false` | `true` |
| Enums | String | `"active"` |
| Money | Decimal with currency code | `{"amount": 100.50, "currency": "USD"}` |
| Phone Numbers | E.164 | `"+12345678901"` |
| Email Addresses | RFC 5322 | `"user@example.com"` |
| URLs | RFC 3986 | `"https://wendoverhoa.org"` |
| GUIDs | RFC 4122 | `"123e4567-e89b-12d3-a456-426614174000"` |

## Idempotency

For non-idempotent operations (POST), clients can ensure idempotency by providing an idempotency key:

```
X-Idempotency-Key: 123e4567-e89b-12d3-a456-426614174000
```

The server will return the same response for duplicate requests with the same idempotency key.

## Bulk Operations

Bulk operations are supported for create, update, and delete:

```
POST /api/v1/properties/bulk
```

Request body:
```json
{
  "items": [
    {
      "address": "123 Main St",
      "city": "Bedford",
      "state": "TX",
      "zipCode": "76021"
    },
    {
      "address": "456 Oak Ave",
      "city": "Bedford",
      "state": "TX",
      "zipCode": "76021"
    }
  ]
}
```

Response includes individual results:
```json
{
  "success": true,
  "data": {
    "results": [
      {
        "success": true,
        "id": 124,
        "status": "created"
      },
      {
        "success": false,
        "error": {
          "code": "VALIDATION_ERROR",
          "message": "Validation failed",
          "details": [
            {
              "field": "address",
              "message": "Address already exists"
            }
          ]
        }
      }
    ],
    "summary": {
      "total": 2,
      "successful": 1,
      "failed": 1
    }
  }
}
```

## Caching

API responses may be cached using standard HTTP caching headers:

```
Cache-Control: max-age=3600, public
ETag: "33a64df551425fcc55e4d42a148795d9f25f89d4"
Last-Modified: Wed, 15 May 2025 10:30:00 GMT
```

Clients can use conditional requests:

```
If-None-Match: "33a64df551425fcc55e4d42a148795d9f25f89d4"
If-Modified-Since: Wed, 15 May 2025 10:30:00 GMT
```

## Rate Limiting

Rate limits are enforced and communicated via headers:

```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 1589547426
```

When rate limit is exceeded, a 429 response is returned with:

```json
{
  "success": false,
  "error": {
    "code": "RATE_LIMIT_EXCEEDED",
    "message": "Rate limit exceeded. Try again in 60 seconds.",
    "details": [
      "Limit: 100 requests per hour"
    ]
  }
}
```

## Cross-Origin Resource Sharing (CORS)

The API supports CORS with the following headers:

```
Access-Control-Allow-Origin: https://wendoverhoa.org
Access-Control-Allow-Methods: GET, POST, PUT, PATCH, DELETE, OPTIONS
Access-Control-Allow-Headers: Content-Type, Authorization, X-Api-Version
Access-Control-Max-Age: 86400
```

## Implementation with ASP.NET Core

### Controller Naming

Controllers follow this naming convention:
```csharp
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class PropertiesController : ControllerBase
{
    // Controller actions
}
```

### Action Methods

Action methods follow this naming convention:
```csharp
// GET /api/v1/properties
[HttpGet]
public async Task<ActionResult<ApiResponse<IEnumerable<PropertyDto>>>> GetProperties([FromQuery] PropertyQueryParameters parameters)

// GET /api/v1/properties/{id}
[HttpGet("{id}")]
public async Task<ActionResult<ApiResponse<PropertyDto>>> GetProperty(int id)

// POST /api/v1/properties
[HttpPost]
public async Task<ActionResult<ApiResponse<PropertyDto>>> CreateProperty([FromBody] CreatePropertyCommand command)

// PUT /api/v1/properties/{id}
[HttpPut("{id}")]
public async Task<ActionResult<ApiResponse<PropertyDto>>> UpdateProperty(int id, [FromBody] UpdatePropertyCommand command)

// DELETE /api/v1/properties/{id}
[HttpDelete("{id}")]
public async Task<ActionResult<ApiResponse<bool>>> DeleteProperty(int id)
```

### Response Wrapper

A standard response wrapper is used for all endpoints:
```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; } = true;
    public T Data { get; set; }
    public PaginationMetadata Pagination { get; set; }
    public Dictionary<string, string> Links { get; set; }
    public ErrorDetails Error { get; set; }
}

public class PaginationMetadata
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public bool HasNext { get; set; }
    public bool HasPrevious { get; set; }
}

public class ErrorDetails
{
    public string Code { get; set; }
    public string Message { get; set; }
    public List<string> Details { get; set; }
    public string TraceId { get; set; }
}
```

## Best Practices

1. **Use Nouns, Not Verbs**: Resources should be nouns (e.g., `/properties`, not `/getProperties`)
2. **Use Plural Resource Names**: Use plural for collection resources (e.g., `/properties`, not `/property`)
3. **Use HTTP Methods Appropriately**: Use GET for retrieval, POST for creation, etc.
4. **Use Appropriate Status Codes**: Return appropriate HTTP status codes for different scenarios
5. **Provide Meaningful Error Messages**: Error responses should be informative and actionable
6. **Include Pagination for Collections**: All collection endpoints should support pagination
7. **Support Filtering and Sorting**: Allow clients to filter and sort data as needed
8. **Use Consistent Naming Conventions**: Follow consistent naming conventions for all resources and parameters
9. **Document All Endpoints**: Use Swagger/OpenAPI to document all endpoints
10. **Implement HATEOAS**: Include hypermedia links for navigation
11. **Support Versioning**: Version all APIs to ensure backward compatibility
12. **Implement Rate Limiting**: Protect the API from abuse with rate limiting
13. **Use Proper Authentication and Authorization**: Secure all endpoints appropriately
14. **Validate All Input**: Validate all input parameters and request bodies
15. **Log All Requests and Responses**: Log all API interactions for debugging and auditing
