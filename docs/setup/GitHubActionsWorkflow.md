# GitHub Actions Workflow Configuration

This document outlines the GitHub Actions workflow configuration for the Wendover HOA web application's CI/CD pipeline. The workflow is designed to automate building, testing, and deploying the application while ensuring code quality and security using GitHub's free tools.

## Workflow Overview

The CI/CD pipeline consists of the following stages:
1. **Build** - Compile the application
2. **Test** - Run unit and integration tests
3. **Analyze** - Perform code quality and security checks
4. **Deploy** - Deploy to the appropriate environment

## Workflow Files

The GitHub Actions workflows are defined in YAML files located in the `.github/workflows` directory of the repository:

### Main Workflow (`main.yml`)

```yaml
name: WendoverHOA CI/CD Pipeline

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]
  workflow_dispatch:

jobs:
  build:
    runs-on: windows-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '9.0.x'
        
    - name: Restore dependencies
      run: dotnet restore
      
    - name: Build
      run: dotnet build --no-restore --configuration Release
      
    - name: Test
      run: dotnet test --no-build --configuration Release --verbosity normal --collect:"XPlat Code Coverage"
      
    - name: Upload build artifacts
      uses: actions/upload-artifact@v3
      with:
        name: build-artifacts
        path: |
          **/bin/Release/net9.0/
          !**/bin/Release/net9.0/ref/
```

### Code Analysis Workflow (`code-analysis.yml`)

```yaml
name: Code Analysis

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]
  schedule:
    - cron: '0 0 * * 0'  # Run weekly on Sunday at midnight
  workflow_dispatch:

jobs:
  analyze:
    runs-on: ubuntu-latest
    permissions:
      actions: read
      contents: read
      security-events: write
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Initialize CodeQL
      uses: github/codeql-action/init@v2
      with:
        languages: 'csharp, javascript'
        
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '9.0.x'
        
    - name: Build
      run: dotnet build --configuration Release
      
    - name: Perform CodeQL Analysis
      uses: github/codeql-action/analyze@v2
      
    - name: Run dotnet format
      run: dotnet format --verify-no-changes --verbosity diagnostic
      
    - name: Dependency Review
      uses: actions/dependency-review-action@v3
      with:
        fail-on-severity: moderate
```

### Deployment Workflow (`deploy.yml`)

```yaml
name: Deploy

on:
  workflow_run:
    workflows: ["WendoverHOA CI/CD Pipeline"]
    branches: [main, develop]
    types: [completed]
  workflow_dispatch:

jobs:
  deploy-dev:
    if: ${{ github.event.workflow_run.conclusion == 'success' && github.ref == 'refs/heads/develop' }}
    runs-on: ubuntu-latest
    environment: development
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Download build artifacts
      uses: actions/download-artifact@v3
      with:
        name: build-artifacts
        
    - name: Deploy to Development
      uses: webfactory/ssh-agent@v0.7.0
      with:
        ssh-private-key: ${{ secrets.SSH_PRIVATE_KEY }}
      
    - name: Copy files to server
      run: |
        scp -r -P ${{ secrets.SSH_PORT }} ./publish/* ${{ secrets.SSH_USER }}@${{ secrets.SSH_HOST }}:${{ secrets.DEPLOY_PATH }}
        
    - name: Restart application
      run: |
        ssh -p ${{ secrets.SSH_PORT }} ${{ secrets.SSH_USER }}@${{ secrets.SSH_HOST }} "${{ secrets.RESTART_COMMAND }}"
        
  deploy-prod:
    if: ${{ github.event.workflow_run.conclusion == 'success' && github.ref == 'refs/heads/main' }}
    runs-on: ubuntu-latest
    environment: production
    needs: deploy-dev
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Download build artifacts
      uses: actions/download-artifact@v3
      with:
        name: build-artifacts
        
    - name: Deploy to Production
      uses: webfactory/ssh-agent@v0.7.0
      with:
        ssh-private-key: ${{ secrets.SSH_PRIVATE_KEY }}
      
    - name: Copy files to server
      run: |
        scp -r -P ${{ secrets.SSH_PORT }} ./publish/* ${{ secrets.SSH_USER }}@${{ secrets.SSH_HOST }}:${{ secrets.DEPLOY_PATH }}
        
    - name: Restart application
      run: |
        ssh -p ${{ secrets.SSH_PORT }} ${{ secrets.SSH_USER }}@${{ secrets.SSH_HOST }} "${{ secrets.RESTART_COMMAND }}"
```

## GitHub Security Features

The CI/CD pipeline leverages GitHub's free security and code quality tools:

### 1. CodeQL Analysis
CodeQL is GitHub's semantic code analysis engine that automatically discovers vulnerabilities in your code. It's configured in the `code-analysis.yml` workflow to scan C# and JavaScript code.

### 2. Dependency Scanning
The Dependency Review action checks for vulnerable dependencies in pull requests, helping prevent the introduction of known vulnerabilities.

### 3. Secret Scanning
GitHub automatically scans repositories for known types of secrets to prevent fraudulent use of credentials that were accidentally committed.

### 4. Code Style Enforcement
The workflow uses `dotnet format` to ensure consistent code style across the codebase, making it more maintainable and readable.

## Environment Configuration

The deployment workflow uses GitHub Environments to manage environment-specific secrets and variables:

### Development Environment
- URL: dev.wendoverhoa.org
- Protected: No
- Required reviewers: None

### Production Environment
- URL: wendoverhoa.org
- Protected: Yes
- Required reviewers: At least one board member

## Required Secrets

The following secrets need to be configured in GitHub:

- `SSH_PRIVATE_KEY`: SSH key for deployment
- `SSH_USER`: Username for SSH connection
- `SSH_HOST`: Hostname for SSH connection
- `SSH_PORT`: Port for SSH connection
- `DEPLOY_PATH`: Path on the server where files should be deployed
- `RESTART_COMMAND`: Command to restart the application on the server

## Workflow Triggers

- **Push to main/develop**: Triggers the build, test, and analysis workflows
- **Pull Request to main/develop**: Triggers the build, test, and analysis workflows
- **Successful workflow completion**: Triggers the deployment workflow
- **Manual trigger**: All workflows can be triggered manually using the workflow_dispatch event
- **Weekly schedule**: The code analysis workflow runs weekly to check for new vulnerabilities

## Best Practices

1. **Never commit secrets**: Use GitHub Secrets for storing sensitive information
2. **Review security alerts**: Regularly check GitHub's security alerts
3. **Keep dependencies updated**: Regularly update dependencies to patch security vulnerabilities
4. **Test before deployment**: Ensure all tests pass before deploying to any environment
5. **Use protected branches**: Configure branch protection rules for main and develop branches
6. **Require pull request reviews**: Enforce code reviews before merging to protected branches
7. **Monitor deployments**: Set up monitoring to detect issues after deployment
