# IONOS Deployment Guide

This document provides detailed instructions for deploying the Wendover HOA application to the IONOS Web Hosting Business Windows plan. It covers initial setup, deployment process, configuration, and maintenance procedures.

## IONOS Web Hosting Business Windows Plan Overview

The IONOS Web Hosting Business Windows plan provides a Windows Server environment with the following features:

- Windows Server 2019
- IIS 10.0
- SQL Server 2019 Express
- .NET Framework 4.8
- .NET Core/.NET 9 support
- 100 GB SSD storage
- Unlimited traffic
- 1 free domain
- 500 email accounts
- SSL certificate included

## Prerequisites

Before deploying to IONOS, ensure you have:

1. IONOS Web Hosting Business Windows plan account credentials
2. FTP/FTPS credentials for file uploads
3. SQL Server connection details
4. Domain configuration completed
5. SSL certificate installed or ready for installation
6. Published version of the Wendover HOA application

## Initial Server Setup

### 1. Accessing the IONOS Control Panel

1. Navigate to the IONOS Control Panel at [https://my.ionos.com/](https://my.ionos.com/)
2. Log in with your IONOS account credentials
3. Select your Web Hosting Business Windows plan

### 2. Domain Configuration

1. In the Control Panel, navigate to "Domains & SSL"
2. Select "Assign Domain" if not already assigned
3. Choose your domain (e.g., wendoverhoa.org)
4. Configure DNS settings:
   - A record: Point to your IONOS server IP
   - CNAME record: Add www subdomain
   - MX records: Configure for email services if needed

### 3. SSL Certificate Setup

1. In the Control Panel, navigate to "Domains & SSL" > "SSL Certificates"
2. Select "Activate SSL" for your domain
3. Choose between:
   - Free DV certificate (included with plan)
   - Purchase OV or EV certificate for enhanced validation
4. Complete the certificate request process
5. Verify domain ownership if required
6. Wait for certificate issuance (typically 1-24 hours)

### 4. Database Setup

1. In the Control Panel, navigate to "Databases"
2. Select "Create Database" > "Microsoft SQL Server"
3. Configure database settings:
   - Database name: `WendoverHOA`
   - Collation: `SQL_Latin1_General_CP1_CI_AS`
   - Initial size: `100 MB`
   - Growth rate: `10%`
4. Note the connection string details:
   - Server name
   - Database name
   - Username
   - Password

## Application Deployment

### 1. Preparing the Application for Deployment

1. Update the production connection string in `appsettings.json` or use environment variables
2. Build the application in Release mode:
   ```bash
   dotnet publish -c Release -o ./publish
   ```
3. Verify the published output in the `./publish` directory

### 2. Deploying via FTP/FTPS

1. Connect to your IONOS server using an FTP client (e.g., FileZilla, WinSCP)
   - Host: Your FTP hostname from IONOS (e.g., `ftp.wendoverhoa.org`)
   - Username: Your FTP username
   - Password: Your FTP password
   - Port: 21 (FTP) or 990 (FTPS)
   - Protocol: FTP or FTPS (recommended)

2. Navigate to the web root directory (typically `/httpdocs` or `/www`)

3. Upload the contents of the `./publish` directory to the web root

4. Verify file permissions:
   - All files: Read permission
   - Executable files (.dll, .exe): Execute permission
   - Configuration files: Read permission
   - Log directory: Write permission

### 3. Deploying via Web Deploy

For more streamlined deployments, configure Web Deploy:

1. In Visual Studio, right-click the project and select "Publish"
2. Select "IIS, FTP, etc." as the publish target
3. Configure the publish profile:
   - Publish method: Web Deploy
   - Server: Your IONOS server name
   - Site name: Default Web Site or your configured site name
   - Username: Your deployment username
   - Password: Your deployment password
   - Destination URL: Your website URL (e.g., https://wendoverhoa.org)

4. Configure deployment settings:
   - Database: Update connection string to production
   - File Publish Options: Remove additional files at destination
   - Precompile during publishing: Enabled

5. Save the profile and click "Publish"

### 4. Configuring IIS

1. Access the IONOS Plesk Control Panel
2. Navigate to "Websites & Domains" > your domain
3. Select "IIS Settings"
4. Configure the application pool:
   - .NET CLR Version: No Managed Code
   - Managed Pipeline Mode: Integrated
   - Identity: ApplicationPoolIdentity
   - Enable 32-Bit Applications: False

5. Configure the website:
   - Physical Path: Point to the deployed application
   - Binding: Ensure HTTPS binding is configured with SSL certificate
   - Default Document: Add `index.html` and `default.aspx` if not present

6. Configure web.config transformations if needed

### 5. Database Deployment

1. Generate SQL migration script:
   ```bash
   dotnet ef migrations script --output migration.sql --idempotent
   ```

2. Execute the migration script against the production database:
   - Use SQL Server Management Studio (SSMS) to connect to your IONOS database
   - Open and execute the migration script
   - Verify database objects are created correctly

3. Seed initial data if required:
   - Execute seed data scripts
   - Verify data integrity

## Post-Deployment Configuration

### 1. Environment Variables

Configure environment variables through the IONOS Control Panel:

1. Navigate to "Websites & Domains" > your domain
2. Select "Web Application Settings"
3. Add the following environment variables:
   - `ASPNETCORE_ENVIRONMENT`: `Production`
   - `ConnectionStrings__DefaultConnection`: Your database connection string
   - `JWT__Secret`: Your JWT secret key (generate a secure random string)
   - `JWT__Issuer`: Your domain (e.g., `wendoverhoa.org`)
   - `JWT__Audience`: Your domain (e.g., `wendoverhoa.org`)
   - `JWT__ExpiryMinutes`: `15`
   - `SMTP__Server`: Your SMTP server
   - `SMTP__Port`: SMTP port (typically 587)
   - `SMTP__Username`: SMTP username
   - `SMTP__Password`: SMTP password
   - `SMTP__FromEmail`: Sender email address
   - `SMTP__FromName`: Sender name

### 2. Application Initialization

1. Navigate to your website URL (e.g., https://wendoverhoa.org)
2. Verify the application loads correctly
3. Log in with administrator credentials
4. Complete any required initialization steps:
   - Verify database connection
   - Check email configuration
   - Test authentication system
   - Create initial administrator account if not seeded

### 3. SSL/TLS Configuration

1. In the IONOS Control Panel, navigate to "Websites & Domains" > your domain
2. Select "Security" > "SSL/TLS Settings"
3. Configure SSL/TLS settings:
   - Minimum TLS version: TLS 1.2
   - Enable HSTS: Yes
   - HSTS max age: 31536000 (1 year)
   - Include subdomains: Yes
   - Preload: Yes

4. Configure SSL/TLS cipher suites:
   - Enable modern cipher suites
   - Disable outdated cipher suites
   - Prioritize forward secrecy

## Monitoring and Maintenance

### 1. Application Monitoring

1. Configure Application Insights:
   - Add Application Insights connection string to environment variables
   - Verify telemetry is being received

2. Set up email alerts for:
   - Application errors
   - Performance degradation
   - Availability issues
   - Security events

3. Configure regular status checks:
   - Health endpoint monitoring
   - Database connectivity checks
   - External service dependency checks

### 2. Backup Procedures

1. Database Backups:
   - Configure automated SQL Server backups through IONOS Control Panel
   - Schedule daily full backups
   - Schedule hourly transaction log backups
   - Verify backup integrity regularly
   - Store backups in multiple locations

2. File System Backups:
   - Configure automated file system backups through IONOS Control Panel
   - Schedule weekly full backups
   - Schedule daily incremental backups
   - Verify backup integrity regularly
   - Store backups in multiple locations

### 3. Update Procedures

1. Application Updates:
   - Build and test updates in staging environment
   - Schedule maintenance window for production updates
   - Create backup before updating
   - Deploy update using Web Deploy or FTP
   - Verify application functionality after update
   - Monitor for any issues

2. Database Schema Updates:
   - Generate idempotent migration script
   - Test migration in staging environment
   - Schedule maintenance window for production migration
   - Create database backup before migration
   - Apply migration to production
   - Verify data integrity after migration

3. Server Updates:
   - Monitor IONOS notifications for server updates
   - Schedule maintenance window for server updates
   - Create backups before server updates
   - Verify application functionality after server updates

## Troubleshooting

### 1. Common Issues and Solutions

1. **Application Not Starting**:
   - Check application logs in `logs` directory
   - Verify correct .NET version is installed
   - Verify web.config configuration
   - Check application pool settings

2. **Database Connection Issues**:
   - Verify connection string is correct
   - Check SQL Server is running
   - Verify firewall settings
   - Check user permissions

3. **SSL/TLS Issues**:
   - Verify certificate is valid and not expired
   - Check certificate binding in IIS
   - Verify correct hostname configuration
   - Check for mixed content warnings

4. **Performance Issues**:
   - Check server resource utilization
   - Analyze database query performance
   - Check for memory leaks
   - Verify caching configuration

### 2. Support Resources

1. IONOS Support:
   - Phone: 1-866-991-2631
   - Email: support@ionos.com
   - Support Portal: https://www.ionos.com/help

2. Microsoft Documentation:
   - IIS: https://docs.microsoft.com/en-us/iis/
   - SQL Server: https://docs.microsoft.com/en-us/sql/
   - .NET: https://docs.microsoft.com/en-us/dotnet/

3. Wendover HOA Support:
   - Internal documentation
   - Development team contacts
   - Escalation procedures

## Disaster Recovery

### 1. Recovery Procedures

1. **Application Recovery**:
   - Identify the issue causing the failure
   - Restore application files from backup if corrupted
   - Verify configuration settings
   - Restart application pool
   - Verify application functionality

2. **Database Recovery**:
   - Identify the issue causing the failure
   - Stop application to prevent further data corruption
   - Restore database from latest backup
   - Apply transaction logs if available
   - Verify data integrity
   - Restart application
   - Verify application functionality

3. **Complete System Recovery**:
   - Contact IONOS support for server restoration if needed
   - Restore application files from backup
   - Restore database from backup
   - Reconfigure environment variables
   - Verify SSL/TLS configuration
   - Test application functionality

### 2. Recovery Time Objectives (RTO)

- Minor issues: < 1 hour
- Major issues: < 4 hours
- Catastrophic failure: < 24 hours

### 3. Recovery Point Objectives (RPO)

- Database: < 1 hour data loss
- Application files: < 24 hours data loss

## Security Considerations

### 1. Secure Configuration

1. Remove development files and tools from production
2. Disable detailed error messages for end users
3. Configure proper HTTP security headers
4. Implement IP restrictions for administrative areas if needed
5. Enable request filtering and URL rewriting for security

### 2. Regular Security Audits

1. Schedule monthly security scans
2. Review application logs for suspicious activity
3. Audit user accounts and permissions regularly
4. Update dependencies to address security vulnerabilities
5. Perform penetration testing annually

## Conclusion

This deployment guide provides comprehensive instructions for deploying and maintaining the Wendover HOA application on the IONOS Web Hosting Business Windows plan. Following these procedures will ensure a secure, reliable, and performant application deployment.

For additional assistance, contact the development team or IONOS support.
