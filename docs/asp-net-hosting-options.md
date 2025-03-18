# ASP.NET Core Hosting Options

This document outlines various options for hosting ASP.NET Core applications, from cloud providers to self-hosting solutions.

## Cloud Hosting Options

### 1. Microsoft Azure

#### Azure App Service

- **Description**: Fully managed platform for building, deploying, and scaling web apps
- **Pricing**: Free tier available (F1) with limitations; paid tiers starting ~$14/month
- **Benefits**:

  - Native integration with .NET and SQL Server
  - Auto-scaling capabilities
  - Deployment slots for testing
  - Integrated with Azure DevOps for CI/CD
- **Limitations**:
  - Free tier has CPU/memory limitations
  - Costs can increase with additional services

#### Azure Container Apps

- **Description**: Serverless container service that lets you run microservices
- **Pricing**: Pay-per-use model; can be cost-effective for low-traffic apps
- **Benefits**:
  - Built-in Kubernetes-style orchestration
  - Scale to zero capability (cost savings)
  - Dapr integration
- **Limitations**:
  - More complex setup than App Service

### 2. Amazon Web Services (AWS)

#### AWS Elastic Beanstalk

- **Description**: Platform-as-a-service for deploying web applications
- **Pricing**: Free tier available (750 hours/month of t2.micro instance)
- **Benefits**:
  - Simple deployment workflow
  - Auto-scaling capabilities
  - Managed platform updates
- **Limitations**:
  - AWS-specific configuration learning curve

#### AWS App Runner

- **Description**: Fully managed container application service
- **Pricing**: Pay-per-compute-second; starts around $5/month
- **Benefits**:
  - Automatic scaling
  - Simplified deployment from container images
- **Limitations**:
  - Less mature than other AWS services

### 3. Google Cloud Platform (GCP)

#### Google Cloud Run

- **Description**: Fully managed container platform
- **Pricing**: Pay-per-use model with generous free tier (2 million requests/month)
- **Benefits**:
  - Scale to zero capability
  - Only pay for actual usage time
  - Simple deployment
- **Limitations**:
  - Stateless design required
  - Request timeout limits

### 4. Render

- **Description**: Modern cloud provider built for developers
- **Pricing**: Free tier for web services; paid plans start at $7/month
- **Benefits**:
  - Simple deployment model
  - Automatic SSL certificates
  - Git-based deployments
- **Limitations**:
  - Free tier has sleep periods after inactivity

### 5. DigitalOcean App Platform

- **Description**: PaaS offering from DigitalOcean
- **Pricing**: Starting at $5/month
- **Benefits**:
  - Simple UI and deployment
  - Global CDN included
  - Automatic HTTPS
- **Limitations**:
  - Higher cost for scaling compared to some alternatives

### 6. Heroku

- **Description**: Platform-as-a-service for easy application deployment
- **Pricing**: Free tier available; paid tiers start at $7/month
- **Benefits**:
  - Simple Git-based deployment
  - Add-ons ecosystem
- **Limitations**:
  - Free dynos sleep after 30 minutes of inactivity
  - Not .NET specialized (requires buildpacks)

### 7. Railway

- **Description**: Modern deployment platform focused on simplicity
- **Pricing**: Free tier available; $5/month starter plan
- **Benefits**:
  - Very simple deployment
  - Automatic preview environments
  - Integrated database options
- **Limitations**:
  - Usage-based pricing can be unpredictable

## Self-Hosting Options

### 8. Docker + VPS Solutions

- **Description**: Deploying containerized applications to virtual private servers
- **Providers**: Linode, DigitalOcean, Vultr, OVH
- **Pricing**: Starting at ~$5/month for basic VPS
- **Benefits**:
  - Full control over infrastructure
  - Potentially lower costs for stable workloads
  - Learning DevOps skills
- **Limitations**:
  - Requires manual server management
  - Responsibility for security and updates

### 9. Kubernetes-based Hosting

- **Description**: Container orchestration for more complex applications
- **Providers**: Self-hosted K8s, managed K8s services (AKS, EKS, GKE)
- **Pricing**: Varies widely based on setup and provider
- **Benefits**:
  - Advanced orchestration features
  - Scalability and resilience
  - Industry-standard deployment
- **Limitations**:
  - Steep learning curve
  - Complexity overhead

### 10. Static Site + API Hybrid

#### GitHub Pages + Serverless API

- **Description**: Host frontend statically, backend as serverless functions
- **Pricing**: Free tier for GitHub Pages; function costs vary by provider
- **Benefits**:
  - Cost-effective for many use cases
  - Scales automatically
  - Simple deployment
- **Limitations**:
  - Requires separating frontend/backend
  - Cold start times for functions

## Comparison Factors

When choosing a hosting option, consider:

1. **Cost**: Monthly fees vs. pay-per-use models
2. **Scalability**: Automatic scaling capabilities
3. **Ease of deployment**: CI/CD integration
4. **Management overhead**: Managed vs. self-managed
5. **Performance**: Geographic distribution, cold starts
6. **Development experience**: Local to production parity
7. **Database integration**: Managed database options
8. **Learning curve**: Required knowledge to deploy
