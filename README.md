# SqlShield: .NET Data Access Layer
SqlShield is a reusable .NET class library designed to provide a secure, centralized, and easy-to-use data access layer for any .NET application, such as ASP.NET Core MVC or Web API projects. It handles database connection management, credential decryption, and simplifies executing stored procedures using Dapper.

## The Problem It Solves
In many applications, data access logic and connection strings are scattered, duplicated, and often insecure. This library solves that by:

Centralizing Logic: All database execution logic is in one reusable place.

Securing Credentials: Database passwords are encrypted in the configuration file and decrypted at runtime, never sitting in memory as plain text.

Simplifying Development: Provides a clean, injectable service that allows developers to execute complex stored procedures with a single line of code, without worrying about ADO.NET boilerplate.

## Key Features ✨
Configuration-Driven: Reads all settings from the host application's appsettings.json.

Built-in Cryptography: Includes services to handle symmetric encryption/decryption for credentials.

Dynamic Stored Procedure Execution: Uses Dapper to safely and efficiently execute stored procedures with dynamic parameters.

Dependency Injection Ready: Designed from the ground up with interfaces and services for easy integration into modern .NET applications.

One-Line Setup: A single extension method call in Program.cs wires up the entire library.

## Getting Started
Follow these steps to integrate the SqlShield library into your .NET application.

### 1. Reference the Library
In your main application (e.g., your ASP.NET MVC project), add a project reference to the SqlShield class library.

### 2. Configure appsettings.json
The library is powered by a SqlShield section in your main application's appsettings.json. Add the following configuration and customize it for your environment.

```
{
  "SqlShield": {
    "CryptoKey": "your-secret-key-that-is-not-weak",
    "Connections": {
      "DefaultConnection": {
        "ConnectionString": "Data Source=YourServer;Initial Catalog=YourDatabase;Integrated Security=False;User ID=YourUser;",
        "ConnectionPassword": "AnEncryptedPasswordString=="
      },
      "AnalyticsConnection": {
        "ConnectionString": "Data Source=AnalyticsServer;Initial Catalog=AnalyticsDb;...",
        "ConnectionPassword": "AnotherEncryptedPassword=="
      }
    }
  }
}
```
### 3. Register Services in Program.cs
In your application's Program.cs file, call the AddDatabaseServices extension method to register all of the library's services with the dependency injection container.

```
using SqlShield; // Your library's namespace

var builder = WebApplication.CreateBuilder(args);

// ... other services like AddControllersWithViews()

// This single line registers all SqlShield services
builder.Services.AddDatabaseServices(builder.Configuration);

var app = builder.Build();
```
// ...
### 4. Inject and Use
You can now inject any of the library's services into your controllers or other classes. The most common service to use is IStoredProcedureExecutor.

```
using Microsoft.AspNetCore.Mvc;
using SqlShield.Interface; // The library's interfaces
using YourApp.Models;     // Your application's models

public class HomeController : Controller
{
    private readonly IStoredProcedureExecutor _executor;

    public HomeController(IStoredProcedureExecutor executor)
    {
        _executor = executor;
    }

    public async Task<IActionResult> Index()
    {
        string procedureName = "sp_GetActiveUsers";
        string connectionName = "DefaultConnection"; // Must match a key in appsettings.json

        var parameters = new Dictionary<string, object>
        {
            { "@IsActive", true },
            { "@MinimumReputation", 500 }
        };

        // Execute the stored procedure and map the results to a list of User objects
        var activeUsers = await _executor.QueryAsync<User>(procedureName, connectionName, parameters);

        // Pass the data to a Razor View
        return View(activeUsers);
    }
}
```
## Services Overview
This library provides the following injectable services:

Interface	Description
ICryptography	Handles cryptographic operations. Used internally to decrypt database passwords.
IDatabaseService Consumes ICryptography to build a fully decrypted, ready-to-use ADO.NET connection string.
IStoredProcedureExecutor Consumes IDatabaseService. Provides high-level methods (QueryAsync, ExecuteNonQueryAsync) to run stored procedures using Dapper.

Export to Sheets
⚠️ Security Note
The current implementation uses TripleDES, which is considered a legacy and cryptographically weak algorithm. For production use, it is strongly recommended to upgrade the CryptographyService to use a modern standard like AES.
