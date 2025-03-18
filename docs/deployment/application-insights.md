# Application Insights Integration Guide

Application Insights is an Azure service that provides application performance management (APM)
and monitoring for your web applications.
This guide explains how to integrate and use Application Insights with the ClarusMens API.

## Overview

Application Insights automatically detects performance anomalies,
helps diagnose issues,
and provides analytics about how users interact with your API.
It's particularly valuable for monitoring production deployments.

## Key Features

### 1. Automatic Detection

- Performance anomalies and bottlenecks
- Exception tracking with full stack traces
- Failed requests and dependency failures
- Server response time issues

### 2. API Monitoring

- Endpoint usage statistics
- Response times and throughput
- Dependency tracking (HTTP calls, database)
- Geographic distribution of requests

### 3. Performance Analytics

- Live metrics stream
- Custom metric tracking
- Resource utilization
- Performance counters

### 4. Diagnostics

- Detailed exception data
- Request correlation
- Log analytics
- Dependency tracking

## Implementation Steps

### 1. Install Required Package

```powershell
dotnet add package Microsoft.ApplicationInsights.AspNetCore
```

### 2. Update Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add Application Insights
builder.Services.AddApplicationInsightsTelemetry();
```

### 3. Configure Connection String

Add to `appsettings.Production.json`:

```json
{
  "ApplicationInsights": {
    "ConnectionString": "YOUR_CONNECTION_STRING"
  }
}
```

### 4. Create Azure Resource

```powershell
# Create Application Insights resource
az monitor app-insights component create --app ClarusMensInsights --location eastus --resource-group ClarusMensRG --application-type web
```

## Cost Management

### Free Tier Includes

- 5 GB data ingestion per month
- 30 days data retention
- Basic features and alerts

### Cost Optimization

1. **Sampling**
   - Reduce data volume while maintaining statistical accuracy
   - Configure adaptive sampling in high-traffic scenarios

2. **Data Filtering**
   - Filter out unnecessary telemetry
   - Focus on business-critical data

3. **Retention Settings**
   - Adjust data retention period based on needs
   - Archive important data for long-term storage

## Best Practices

### 1. Custom Telemetry

```csharp
// Inject telemetry client
private readonly TelemetryClient _telemetryClient;

// Track custom events
_telemetryClient.TrackEvent("ApiEndpointCalled", new Dictionary<string, string>
{
    { "EndpointName", "QuestionEndpoint" },
    { "QueryType", "Standard" }
});
```

### 2. Alert Configuration

- Set up alerts for:
  - Response time thresholds
  - Failed request rate
  - Exception volume
  - Dependency failures

### 3. Dashboard Setup

- Create custom dashboards for:
  - API health overview
  - Performance metrics
  - Error monitoring
  - Usage statistics

### 4. Monitoring Strategy

- Monitor key metrics:
  - Server response time
  - Request success rate
  - Dependency health
  - Resource utilization

## Security Considerations

1. **Data Privacy**
   - Review collected data for PII
   - Configure data scrubbing rules
   - Set appropriate access controls

2. **Access Management**
   - Use Azure RBAC for access control
   - Implement least-privilege principle
   - Regular access review

## Integration with Azure App Service

Application Insights works seamlessly with Azure App Service:

1. **Enable from Azure Portal**

   ```powershell
   az webapp config appsettings set --resource-group ClarusMensRG --name clarusmens-api --settings APPLICATIONINSIGHTS_CONNECTION_STRING="YOUR_CONNECTION_STRING"
   ```

2. **Container Configuration**
   - No additional configuration needed
   - Works automatically with ASP.NET Core

## Troubleshooting

### Common Issues

1. **Data not appearing**
   - Verify connection string
   - Check sampling settings
   - Ensure proper SDK initialization

2. **Performance Impact**
   - Monitor CPU usage
   - Adjust sampling rate
   - Review custom telemetry volume

### Verification Steps

1. Check live metrics stream
2. Verify telemetry ingestion
3. Test custom tracking
4. Validate alert configurations

## Resources

- [Official Documentation](https://docs.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview)
- [ASP.NET Core Monitoring](https://docs.microsoft.com/en-us/azure/azure-monitor/app/asp-net-core)
- [Pricing Details](https://azure.microsoft.com/en-us/pricing/details/monitor/)
- [Best Practices Guide](https://docs.microsoft.com/en-us/azure/azure-monitor/app/sampling)
