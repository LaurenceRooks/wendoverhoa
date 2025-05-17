# Swagger/OpenAPI Specifications

This document outlines the Swagger/OpenAPI specifications for the Wendover HOA API, providing detailed documentation for all API endpoints, request/response examples, and authentication requirements.

## Introduction to OpenAPI

The Wendover HOA API is documented using OpenAPI 3.0 (formerly known as Swagger), which provides a standardized way to describe RESTful APIs. This documentation is automatically generated and kept in sync with the actual API implementation using Swashbuckle for ASP.NET Core.

## OpenAPI Implementation

### Integration with ASP.NET Core

The Wendover HOA API uses Swashbuckle for ASP.NET Core to generate OpenAPI documentation:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Add Swagger/OpenAPI services
    services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Wendover HOA API",
            Version = "v1",
            Description = "API for Wendover Homeowners Association management system",
            Contact = new OpenApiContact
            {
                Name = "Wendover HOA Support",
                Email = "support@wendoverhoa.org",
                Url = new Uri("https://wendoverhoa.org/support")
            },
            License = new OpenApiLicense
            {
                Name = "Proprietary",
                Url = new Uri("https://wendoverhoa.org/terms")
            }
        });
        
        // Include XML comments
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        c.IncludeXmlComments(xmlPath);
        
        // Configure JWT authentication for Swagger UI
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });
        
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
                new string[] {}
            }
        });
        
        // Enable API versioning in Swagger
        c.OperationFilter<SwaggerDefaultValues>();
        c.DocumentFilter<SwaggerVersionMapping>();
        
        // Group endpoints by controller
        c.TagActionsBy(api => new[] { api.GroupName });
        c.DocInclusionPredicate((name, api) => true);
    });
    
    // Configure API versioning
    services.AddApiVersioning(options =>
    {
        options.ReportApiVersions = true;
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.ApiVersionReader = ApiVersionReader.Combine(
            new UrlSegmentApiVersionReader(),
            new HeaderApiVersionReader("X-Api-Version"));
    });
    
    services.AddVersionedApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
{
    // Enable Swagger and Swagger UI
    app.UseSwagger(c =>
    {
        c.SerializeAsV2 = false;
        c.RouteTemplate = "api-docs/{documentName}/swagger.json";
    });
    
    app.UseSwaggerUI(c =>
    {
        // Build a swagger endpoint for each discovered API version
        foreach (var description in provider.ApiVersionDescriptions)
        {
            c.SwaggerEndpoint(
                $"/api-docs/{description.GroupName}/swagger.json",
                $"Wendover HOA API {description.GroupName.ToUpperInvariant()}");
        }
        
        c.RoutePrefix = "api-docs";
        c.DocExpansion(DocExpansion.List);
        c.EnableDeepLinking();
        c.DisplayRequestDuration();
        c.EnableFilter();
    });
}
```

### XML Documentation

All API controllers and models include XML documentation comments to generate comprehensive API documentation:

```csharp
/// <summary>
/// Retrieves a property by its ID
/// </summary>
/// <param name="id">The property ID</param>
/// <remarks>
/// Sample request:
///
///     GET /api/v1/properties/123
///
/// </remarks>
/// <response code="200">Returns the property</response>
/// <response code="404">If the property is not found</response>
/// <response code="401">If the user is not authenticated</response>
/// <response code="403">If the user does not have permission to view the property</response>
[HttpGet("{id}")]
[ProducesResponseType(typeof(ApiResponse<PropertyDto>), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ApiResponse<ErrorDetails>), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ApiResponse<ErrorDetails>), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ApiResponse<ErrorDetails>), StatusCodes.Status403Forbidden)]
public async Task<ActionResult<ApiResponse<PropertyDto>>> GetProperty(int id)
{
    // Implementation
}
```

## OpenAPI Document Structure

The OpenAPI document follows this structure:

```yaml
openapi: 3.0.1
info:
  title: Wendover HOA API
  description: API for Wendover Homeowners Association management system
  contact:
    name: Wendover HOA Support
    email: support@wendoverhoa.org
    url: https://wendoverhoa.org/support
  license:
    name: Proprietary
    url: https://wendoverhoa.org/terms
  version: v1
servers:
  - url: https://api.wendoverhoa.org/api/v1
    description: Production server
  - url: https://staging-api.wendoverhoa.org/api/v1
    description: Staging server
  - url: https://localhost:7001/api/v1
    description: Local development server
paths:
  # API paths defined here
components:
  # Schemas, security schemes, etc. defined here
security:
  - Bearer: []
tags:
  # Tags for grouping endpoints
```

## Sample OpenAPI Specifications

Below are sample OpenAPI specifications for key API endpoints in the Wendover HOA system.

### Properties API

```yaml
paths:
  /properties:
    get:
      tags:
        - Properties
      summary: Get all properties
      description: Retrieves a paginated list of properties
      operationId: getProperties
      parameters:
        - name: pageNumber
          in: query
          description: Page number
          schema:
            type: integer
            default: 1
            minimum: 1
        - name: pageSize
          in: query
          description: Page size
          schema:
            type: integer
            default: 10
            minimum: 1
            maximum: 100
        - name: sortBy
          in: query
          description: Field to sort by
          schema:
            type: string
            default: address
        - name: sortDirection
          in: query
          description: Sort direction
          schema:
            type: string
            enum: [asc, desc]
            default: asc
        - name: filter
          in: query
          description: Filter criteria
          schema:
            type: string
      responses:
        '200':
          description: Successful operation
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/PropertyCollectionResponse'
              example:
                success: true
                data:
                  - id: 1
                    address: "123 Main St"
                    city: "Bedford"
                    state: "TX"
                    zipCode: "76021"
                    lotNumber: "A-123"
                    status: "Occupied"
                    resident:
                      id: 1
                      firstName: "John"
                      lastName: "Doe"
                  - id: 2
                    address: "456 Oak Ave"
                    city: "Bedford"
                    state: "TX"
                    zipCode: "76021"
                    lotNumber: "A-124"
                    status: "Occupied"
                    resident:
                      id: 2
                      firstName: "Jane"
                      lastName: "Smith"
                pagination:
                  pageNumber: 1
                  pageSize: 10
                  totalPages: 5
                  totalCount: 42
                  hasNext: true
                  hasPrevious: false
                links:
                  self: "https://api.wendoverhoa.org/api/v1/properties?pageNumber=1&pageSize=10"
                  next: "https://api.wendoverhoa.org/api/v1/properties?pageNumber=2&pageSize=10"
                  previous: null
        '401':
          $ref: '#/components/responses/Unauthorized'
        '403':
          $ref: '#/components/responses/Forbidden'
      security:
        - Bearer: []
    post:
      tags:
        - Properties
      summary: Create a new property
      description: Creates a new property
      operationId: createProperty
      requestBody:
        description: Property to create
        required: true
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/CreatePropertyCommand'
            example:
              address: "789 Pine St"
              city: "Bedford"
              state: "TX"
              zipCode: "76021"
              lotNumber: "A-125"
              status: "Vacant"
      responses:
        '201':
          description: Property created
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/PropertyResponse'
              example:
                success: true
                data:
                  id: 3
                  address: "789 Pine St"
                  city: "Bedford"
                  state: "TX"
                  zipCode: "76021"
                  lotNumber: "A-125"
                  status: "Vacant"
                  resident: null
        '400':
          $ref: '#/components/responses/BadRequest'
        '401':
          $ref: '#/components/responses/Unauthorized'
        '403':
          $ref: '#/components/responses/Forbidden'
        '422':
          $ref: '#/components/responses/ValidationError'
      security:
        - Bearer: []
  /properties/{id}:
    get:
      tags:
        - Properties
      summary: Get property by ID
      description: Retrieves a property by its ID
      operationId: getProperty
      parameters:
        - name: id
          in: path
          description: Property ID
          required: true
          schema:
            type: integer
            format: int32
      responses:
        '200':
          description: Successful operation
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/PropertyResponse'
              example:
                success: true
                data:
                  id: 1
                  address: "123 Main St"
                  city: "Bedford"
                  state: "TX"
                  zipCode: "76021"
                  lotNumber: "A-123"
                  status: "Occupied"
                  resident:
                    id: 1
                    firstName: "John"
                    lastName: "Doe"
        '401':
          $ref: '#/components/responses/Unauthorized'
        '403':
          $ref: '#/components/responses/Forbidden'
        '404':
          $ref: '#/components/responses/NotFound'
      security:
        - Bearer: []
    put:
      tags:
        - Properties
      summary: Update property
      description: Updates a property
      operationId: updateProperty
      parameters:
        - name: id
          in: path
          description: Property ID
          required: true
          schema:
            type: integer
            format: int32
      requestBody:
        description: Property to update
        required: true
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/UpdatePropertyCommand'
            example:
              address: "123 Main St"
              city: "Bedford"
              state: "TX"
              zipCode: "76022"
              lotNumber: "A-123"
              status: "Occupied"
      responses:
        '200':
          description: Property updated
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/PropertyResponse'
              example:
                success: true
                data:
                  id: 1
                  address: "123 Main St"
                  city: "Bedford"
                  state: "TX"
                  zipCode: "76022"
                  lotNumber: "A-123"
                  status: "Occupied"
                  resident:
                    id: 1
                    firstName: "John"
                    lastName: "Doe"
        '400':
          $ref: '#/components/responses/BadRequest'
        '401':
          $ref: '#/components/responses/Unauthorized'
        '403':
          $ref: '#/components/responses/Forbidden'
        '404':
          $ref: '#/components/responses/NotFound'
        '422':
          $ref: '#/components/responses/ValidationError'
      security:
        - Bearer: []
    delete:
      tags:
        - Properties
      summary: Delete property
      description: Deletes a property
      operationId: deleteProperty
      parameters:
        - name: id
          in: path
          description: Property ID
          required: true
          schema:
            type: integer
            format: int32
      responses:
        '204':
          description: Property deleted
        '401':
          $ref: '#/components/responses/Unauthorized'
        '403':
          $ref: '#/components/responses/Forbidden'
        '404':
          $ref: '#/components/responses/NotFound'
      security:
        - Bearer: []
```

### Component Schemas

```yaml
components:
  schemas:
    ApiResponse:
      type: object
      properties:
        success:
          type: boolean
          description: Indicates if the request was successful
        data:
          type: object
          description: Response data
        pagination:
          $ref: '#/components/schemas/PaginationMetadata'
        links:
          type: object
          additionalProperties:
            type: string
          description: HATEOAS links
        error:
          $ref: '#/components/schemas/ErrorDetails'
    
    PaginationMetadata:
      type: object
      properties:
        pageNumber:
          type: integer
          description: Current page number
        pageSize:
          type: integer
          description: Number of items per page
        totalPages:
          type: integer
          description: Total number of pages
        totalCount:
          type: integer
          description: Total number of items
        hasNext:
          type: boolean
          description: Indicates if there is a next page
        hasPrevious:
          type: boolean
          description: Indicates if there is a previous page
    
    ErrorDetails:
      type: object
      properties:
        code:
          type: string
          description: Error code
        message:
          type: string
          description: Error message
        details:
          type: array
          items:
            type: string
          description: Additional error details
        traceId:
          type: string
          description: Trace ID for debugging
    
    PropertyDto:
      type: object
      properties:
        id:
          type: integer
          format: int32
          description: Property ID
        address:
          type: string
          description: Property address
        city:
          type: string
          description: Property city
        state:
          type: string
          description: Property state
        zipCode:
          type: string
          description: Property ZIP code
        lotNumber:
          type: string
          description: Property lot number
        status:
          type: string
          enum: [Vacant, Occupied, ForSale, UnderConstruction]
          description: Property status
        resident:
          $ref: '#/components/schemas/ResidentDto'
    
    ResidentDto:
      type: object
      properties:
        id:
          type: integer
          format: int32
          description: Resident ID
        firstName:
          type: string
          description: Resident first name
        lastName:
          type: string
          description: Resident last name
        email:
          type: string
          format: email
          description: Resident email
        phoneNumber:
          type: string
          description: Resident phone number
    
    CreatePropertyCommand:
      type: object
      required:
        - address
        - city
        - state
        - zipCode
      properties:
        address:
          type: string
          description: Property address
        city:
          type: string
          description: Property city
        state:
          type: string
          description: Property state
        zipCode:
          type: string
          description: Property ZIP code
        lotNumber:
          type: string
          description: Property lot number
        status:
          type: string
          enum: [Vacant, Occupied, ForSale, UnderConstruction]
          default: Vacant
          description: Property status
    
    UpdatePropertyCommand:
      type: object
      required:
        - address
        - city
        - state
        - zipCode
      properties:
        address:
          type: string
          description: Property address
        city:
          type: string
          description: Property city
        state:
          type: string
          description: Property state
        zipCode:
          type: string
          description: Property ZIP code
        lotNumber:
          type: string
          description: Property lot number
        status:
          type: string
          enum: [Vacant, Occupied, ForSale, UnderConstruction]
          description: Property status
    
    PropertyResponse:
      allOf:
        - $ref: '#/components/schemas/ApiResponse'
        - type: object
          properties:
            data:
              $ref: '#/components/schemas/PropertyDto'
    
    PropertyCollectionResponse:
      allOf:
        - $ref: '#/components/schemas/ApiResponse'
        - type: object
          properties:
            data:
              type: array
              items:
                $ref: '#/components/schemas/PropertyDto'
  
  responses:
    BadRequest:
      description: Bad request
      content:
        application/json:
          schema:
            $ref: '#/components/schemas/ApiResponse'
          example:
            success: false
            error:
              code: "BAD_REQUEST"
              message: "The request is invalid"
              details:
                - "The request could not be understood"
              traceId: "00-1234567890abcdef1234567890abcdef-1234567890abcdef-00"
    
    Unauthorized:
      description: Unauthorized
      content:
        application/json:
          schema:
            $ref: '#/components/schemas/ApiResponse'
          example:
            success: false
            error:
              code: "UNAUTHORIZED"
              message: "Authentication is required"
              details:
                - "Missing or invalid authentication token"
              traceId: "00-1234567890abcdef1234567890abcdef-1234567890abcdef-00"
    
    Forbidden:
      description: Forbidden
      content:
        application/json:
          schema:
            $ref: '#/components/schemas/ApiResponse'
          example:
            success: false
            error:
              code: "FORBIDDEN"
              message: "Access denied"
              details:
                - "You do not have permission to access this resource"
              traceId: "00-1234567890abcdef1234567890abcdef-1234567890abcdef-00"
    
    NotFound:
      description: Not found
      content:
        application/json:
          schema:
            $ref: '#/components/schemas/ApiResponse'
          example:
            success: false
            error:
              code: "RESOURCE_NOT_FOUND"
              message: "The requested resource was not found"
              details:
                - "Property with ID 123 does not exist"
              traceId: "00-1234567890abcdef1234567890abcdef-1234567890abcdef-00"
    
    ValidationError:
      description: Validation error
      content:
        application/json:
          schema:
            $ref: '#/components/schemas/ApiResponse'
          example:
            success: false
            error:
              code: "VALIDATION_ERROR"
              message: "Validation failed"
              details:
                - "Address is required"
                - "ZIP code must be in format XXXXX or XXXXX-XXXX"
              traceId: "00-1234567890abcdef1234567890abcdef-1234567890abcdef-00"
  
  securitySchemes:
    Bearer:
      type: apiKey
      name: Authorization
      in: header
      description: JWT Authorization header using the Bearer scheme
```

## Swagger UI Customization

The Swagger UI is customized to match the Wendover HOA branding and to provide a better user experience:

```csharp
app.UseSwaggerUI(c =>
{
    // Configure Swagger UI
    c.SwaggerEndpoint("/api-docs/v1/swagger.json", "Wendover HOA API V1");
    c.RoutePrefix = "api-docs";
    
    // UI customization
    c.DocExpansion(DocExpansion.List);
    c.DefaultModelsExpandDepth(0);
    c.EnableDeepLinking();
    c.DisplayRequestDuration();
    c.EnableFilter();
    
    // Custom CSS
    c.InjectStylesheet("/swagger-ui/custom.css");
    
    // Custom JavaScript
    c.InjectJavascript("/swagger-ui/custom.js");
    
    // OAuth configuration for Swagger UI
    c.OAuthClientId("swagger-ui");
    c.OAuthAppName("Swagger UI");
});
```

## API Documentation Generation

The OpenAPI documentation is generated automatically from the API code and kept in sync with the implementation. This ensures that the documentation is always up-to-date and accurate.

### Documentation Generation Process

1. **Code First Approach**: The API is implemented first, with proper XML documentation comments.
2. **Automatic Generation**: Swashbuckle generates the OpenAPI documentation from the code.
3. **Manual Enhancements**: Additional documentation, such as examples and descriptions, is added manually.
4. **Validation**: The generated documentation is validated against the OpenAPI specification.
5. **Publication**: The documentation is published to the API documentation portal.

### Documentation Export

The OpenAPI documentation can be exported in various formats:

1. **JSON**: For machine-readable documentation
2. **YAML**: For human-readable documentation
3. **HTML**: For web-based documentation
4. **PDF**: For printable documentation

## API Client Generation

The OpenAPI documentation can be used to generate API clients for various programming languages:

1. **C#**: Using NSwag or AutoRest
2. **TypeScript**: Using NSwag or OpenAPI Generator
3. **JavaScript**: Using Swagger Codegen
4. **Python**: Using OpenAPI Generator
5. **Java**: Using Swagger Codegen

Example of generating a TypeScript client:

```bash
npx openapi-generator-cli generate -i https://api.wendoverhoa.org/api-docs/v1/swagger.json -g typescript-fetch -o ./src/api-client
```

## API Testing with Swagger

The Swagger UI provides a convenient way to test the API directly from the documentation:

1. **Authentication**: Log in using the Authorize button
2. **Request Building**: Build requests using the UI
3. **Request Execution**: Execute requests and view responses
4. **Response Validation**: Validate responses against the schema

## Best Practices for API Documentation

1. **Keep Documentation Up-to-Date**: Ensure that the documentation is always in sync with the implementation.
2. **Provide Examples**: Include examples for all requests and responses.
3. **Document Error Responses**: Document all possible error responses and their meanings.
4. **Use Clear Descriptions**: Provide clear and concise descriptions for all endpoints and parameters.
5. **Include Authentication Requirements**: Document authentication requirements for all endpoints.
6. **Document Rate Limits**: Include information about rate limits and throttling.
7. **Version Documentation**: Maintain documentation for all API versions.
8. **Include Deprecation Notices**: Clearly mark deprecated endpoints and provide migration paths.

## Conclusion

The Swagger/OpenAPI documentation provides a comprehensive and interactive way to explore and understand the Wendover HOA API. It serves as a single source of truth for API consumers and helps ensure that the API is used correctly and efficiently.
