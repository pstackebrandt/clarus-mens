# Troubleshooting Guide

This document contains solutions to common issues encountered during development and testing of the Clarus Mens API.

## Table of Contents

- [.NET 9 Serialization Issues](#net-9-serialization-issues)
- [Testing Issues](#testing-issues)
- [Common Development Environment Issues](#common-development-environment-issues)

## .NET 9 Serialization Issues

### PipeWriter UnflushedBytes Exception

**Problem:**

When using `Results.Json()` or `Results.Ok()` with serialized objects in ASP.NET Core minimal APIs on .NET 9, you may encounter the following exception:

```powershell
System.InvalidOperationException: The PipeWriter 'ResponseBodyPipeWriter' does not implement PipeWriter.UnflushedBytes.
   at System.Text.Json.ThrowHelper.ThrowInvalidOperationException_PipeWriterDoesNotImplementUnflushedBytes(PipeWriter pipeWriter)
   at System.Text.Json.Serialization.Metadata.JsonTypeInfo`1.SerializeAsync(PipeWriter pipeWriter, T rootValue, Int32 flushThreshold, CancellationToken cancellationToken, Object rootValueBoxed)
   at Microsoft.AspNetCore.Http.HttpResponseJsonExtensions.<WriteAsJsonAsync>g__WriteAsJsonAsyncSlow|5_0[TValue](HttpResponse response, TValue value, JsonTypeInfo`1 jsonTypeInfo, CancellationToken cancellationToken)
   at Microsoft.AspNetCore.Http.RequestDelegateFactory.ExecuteTaskResult[T](Task`1 task, HttpContext httpContext)
```

**Cause:**

In .NET 9, there appears to be an incompatibility between System.Text.Json serialization and the ASP.NET Core HTTP response pipeline. The `PipeWriter` implementation in .NET 9 does not provide the `UnflushedBytes` property expected by the serializer.

**Solution:**

Instead of using `Results.Json()` or `Results.Ok()` with objects that need serialization, use our extension methods that safely handle JSON serialization:

```csharp
// DO NOT use this (will throw exception in .NET 9):
return Results.Json(new { property = "value" });
// DO NOT use this either (still uses problematic serialization path):
return Results.Ok(new { property = "value" });

// INSTEAD, use these methods:
// For 200 OK responses:
var response = new MyResponseType { Property = "value" };
return response.JsonSafeOk();

// For other status codes:
return new { error = "Invalid input" }.JsonSafeWithStatus(400);
```

These extension methods work by manually serializing to a JSON string and returning it with the appropriate content type, avoiding the problematic serialization path.

**Test Verification:**

The issue was identified during functional testing with endpoint tests that verify HTTP status codes and response payloads. The fix was confirmed by running the test suite and verifying all tests pass.

**References:**

- Issue discovered and fixed on: [Current Date]
- Affects: .NET 9 (Preview/Early Versions)
- Fixed in Project Version: [Your Version]

## Testing Issues

### "No test is available in ClarusMensAPI.IntegrationTests.dll"

**Problem:**

When running tests, you see this warning:

```powershell
No test is available in [...]\ClarusMensAPI.IntegrationTests.dll
```

**Cause:**

The IntegrationTests project is currently set up as a placeholder for future integration tests but doesn't contain any test methods yet.

**Solution:**

This warning can be safely ignored. When integration tests are added to the project, the warning will disappear.

### Tests Pass Locally But Fail in CI Pipeline

**Problem:**

Tests run successfully on your local machine with `dotnet test` but fail in the CI pipeline.

**Cause:**

This is often due to:

- Different runtime environment in CI
- Missing environment variables
- Path handling differences between operating systems
- Race conditions that only appear in the CI environment

**Solution:**

1. Run tests with the same configuration as CI:

   ```powershell
   dotnet test -c Testing --blame-hang-timeout 5m --logger "trx;LogFileName=test-results.trx"
   ```

2. Check for hardcoded paths or environment-specific assumptions in tests
3. Look for timing issues by increasing timeouts in tests that fail intermittently
4. Review CI logs for specific error messages

### Tests Are Too Slow

**Problem:**

The test suite takes too long to run.

**Cause:**

- Too many integration/functional tests
- Tests not running in parallel
- Inefficient test setup/teardown

**Solution:**

1. Run only unit tests during development:

   ```powershell
   dotnet test ./tests/ClarusMensAPI.UnitTests -c Testing
   ```

2. Use test categories to run specific types of tests:

   ```powershell
   dotnet test --filter "Category=API" -c Testing
   ```

3. Ensure tests can run in parallel by avoiding shared state between tests

## Common Development Environment Issues

[This section will be populated as additional issues are identified]
