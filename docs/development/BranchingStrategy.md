# Branching Strategy

This document outlines the branching strategy for the Wendover HOA project.

## Overview

We follow a simplified GitFlow approach with two main branches and feature branches for development work.

## Branch Structure

### Main Branches

- **`main`** - Production-ready code that has been thoroughly tested and is ready for deployment
- **`develop`** - Integration branch for features before they're ready for production

### Supporting Branches

- **`feature/*`** - Feature branches for new functionality (branched from and merged back to `develop`)
- **`fix/*`** - Hotfix branches for urgent production fixes (branched from `main` and merged to both `main` and `develop`)

## Workflow

### Feature Development

1. Create a feature branch from `develop`:
   ```bash
   git checkout develop
   git pull origin develop
   git checkout -b feature/my-new-feature
   ```

2. Implement the feature with regular commits

3. When the feature is complete:
   - Push the feature branch to GitHub
   - Create a pull request to merge into `develop`
   - After code review and approval, merge the PR
   - Delete the feature branch after merging

### Release Process

1. When `develop` contains all features for a release:
   - Create a pull request from `develop` to `main`
   - Run all tests and perform final validation
   - After approval, merge to `main`

2. Tag the release in `main`:
   ```bash
   git checkout main
   git pull origin main
   git tag -a v1.0.0 -m "Version 1.0.0"
   git push origin v1.0.0
   ```

### Hotfixes

1. For urgent production fixes:
   ```bash
   git checkout main
   git pull origin main
   git checkout -b fix/critical-issue
   ```

2. Implement the fix

3. Create pull requests to merge into both `main` and `develop`

## Best Practices

- Keep feature branches short-lived (1-2 weeks maximum)
- Regularly pull changes from `develop` into feature branches to reduce merge conflicts
- Write descriptive commit messages
- Include the issue/task number in commit messages when applicable
- Delete branches after they've been merged

## CI/CD Integration

- All branches are automatically built and tested by GitHub Actions
- PRs to `develop` and `main` require passing CI checks before merging
- Merges to `main` trigger the deployment pipeline
