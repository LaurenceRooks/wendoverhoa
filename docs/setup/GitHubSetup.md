# GitHub Repository Setup

This document outlines the setup and configuration of the GitHub repository for the Wendover HOA project, taking into account the limitations of GitHub's free tier.

## Repository Configuration

### Repository Structure

The Wendover HOA project uses a standard repository structure following Clean Architecture principles:

- `/src`: Contains the source code
- `/tests`: Contains test projects
- `/docs`: Contains documentation
- `/.github`: Contains GitHub-specific configuration files

### Branch Strategy

While GitHub's free tier doesn't support enforced branch protection rules, we follow these practices:

1. **Main Branch**: Represents the production-ready code
   - All changes must go through pull requests
   - Direct pushes to main are discouraged
   - CI/CD workflows run on all pushes and pull requests

2. **Feature Branches**: Used for development of new features
   - Created from main
   - Named with the format `feature/feature-name`
   - Merged back to main via pull requests

3. **Bugfix Branches**: Used for fixing bugs
   - Created from main
   - Named with the format `bugfix/bug-name`
   - Merged back to main via pull requests

4. **Release Branches**: Used for release preparation
   - Created from main
   - Named with the format `release/version-number`
   - Merged back to main via pull requests

### Pull Request Process

1. Create a feature, bugfix, or release branch
2. Make your changes
3. Push your branch to GitHub
4. Create a pull request to merge into main
5. Ensure all CI/CD checks pass
6. Request a review from a team member
7. Address any feedback
8. Merge the pull request

## CI/CD Workflows

The repository includes several GitHub Actions workflows to ensure code quality and security:

1. **CI/CD**: Builds, tests, and deploys the application
2. **CodeQL Analysis**: Scans code for security vulnerabilities
3. **Dependency Scanning**: Checks for vulnerable dependencies
4. **Secret Scanning**: Prevents credential leaks
5. **Code Quality**: Enforces coding standards
6. **Documentation Generation**: Generates API documentation
7. **PR Validation**: Ensures pull requests meet quality standards

## Security Features

The repository uses GitHub's free security features:

1. **CodeQL**: Automated code scanning for security vulnerabilities
2. **Dependency Graph**: Tracks dependencies and their versions
3. **Dependabot Alerts**: Notifies about vulnerable dependencies
4. **Secret Scanning**: Detects accidentally committed secrets

## Environment Configuration

The project uses GitHub Environments for deployment:

1. **Development**: For testing new features
   - URL: https://dev.wendoverhoa.org
   - Automatically deployed on merges to main

2. **Production**: For the live application
   - URL: https://wendoverhoa.org
   - Requires manual approval for deployment

## Documentation

Project documentation is stored in the `/docs` directory and includes:

1. **Requirements**: Feature requirements and specifications
2. **Architecture**: System architecture documentation
3. **Setup**: Setup and installation guides
4. **Implementation**: Implementation details and guides

## Best Practices

Since we cannot enforce branch protection rules on GitHub's free tier, we follow these best practices:

1. **Code Reviews**: Always have code reviewed before merging
2. **CI/CD Checks**: Ensure all checks pass before merging
3. **Semantic Versioning**: Follow semantic versioning for releases
4. **Commit Messages**: Use descriptive commit messages
5. **Documentation**: Keep documentation up-to-date
6. **Testing**: Write tests for all new features and bug fixes
