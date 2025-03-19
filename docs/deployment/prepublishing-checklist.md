# Pre-Publishing Checklist (Training Project)

> **Focus**: This checklist covers essential steps for publishing a training/template .NET API project.
> It focuses on demonstrating deployment practices while keeping things simple.

## Table of Contents

- [Table of Contents](#table-of-contents)
- [Code Preparation](#code-preparation)
- [Docker Setup](#docker-setup)
- [Documentation](#documentation)
- [Testing](#testing)

## Code Preparation

Essential steps to prepare your code:

✅ Update project purpose in documentation to reflect training/template focus  
✅ Ensure basic API endpoint works (`/api/question`)  
✅ Configure proper error handling  
✅ Set up health check endpoint  

- [ ] Review and remove any hardcoded values or secrets  
✅ Ensure proper logging is configured  

## Docker Setup

Docker configuration for deployment:

✅ Create multi-stage Dockerfile  
✅ Configure container health checks  
✅ Set up proper base images  
✅ Add `.dockerignore` file  

- [ ] Test Docker build locally  
- [ ] Test Docker container locally  

## Documentation

Essential documentation updates:

✅ Update README with project purpose  
✅ Document API endpoints  
✅ Add deployment documentation  

- [ ] Add deployment URLs once available  
- [ ] Document any environment-specific settings  

## Testing

Verify everything works:

✅ Unit tests passing  
✅ Functional tests passing  

- [ ] Test Docker container locally:

  ```bash
  docker build -t clarusmens-api .
  docker run -p 5000:80 clarusmens-api
  ```

- [ ] Test endpoints in container:
  - [ ] Health check endpoint
  - [ ] Main API endpoint
