# SqlShield: .NET Data Access Layer

SqlShield is a reusable .NET class library designed to provide a secure, centralized, and easy-to-use data access layer for any .NET application, such as ASP.NET Core MVC or Web API projects. It handles database connection management, credential decryption, and simplifies executing stored procedures using Dapper.

## The Problem It Solves

In many applications, data access logic and connection strings are scattered, duplicated, and often insecure. This library solves that by:

* **Centralizing Logic**: All database execution logic is in one reusable place.
* **Securing Credentials**: The encryption key is **never stored on disk**. It's provided at application startup. While connection passwords are encrypted in the configuration file, they can only be decrypted by the person running the application who can provide the secret key.
* **Simplifying Development**: Provides a clean, injectable service that allows developers to execute complex stored procedures with a single line of code, without worrying about ADO.NET boilerplate.

## Key Features ✨

* **Runtime Key Provider**: The master encryption key is provided at application startup, ensuring it's never exposed in source code or configuration files.
* **Modern Auth Support**: Natively supports both traditional password-based connections and modern, passwordless authentication methods (e.g., Microsoft Entra ID, IAM).
* **Configuration-Driven**: Reads connection details and settings from the host application's `appsettings.json`.
* **Dynamic Stored Procedure Execution**: Uses Dapper to safely and efficiently execute stored procedures with dynamic parameters.
* **Dependency Injection Ready**: Designed from the ground up with interfaces and services for easy integration into modern .NET applications.

## Getting Started

Follow these steps to integrate the SqlShield library into your .NET application.

### 1. Configure `appsettings.json`

The library is powered by a `SqlShield` section in your main application's `appsettings.json`. Add the following configuration and customize it for your environment.

```json
{
  "SqlShield": {
    "Iterations": 100000,
    "Connections": {
      "DefaultConnection": {
        "ConnectionString": "Data Source=YourServer;Initial Catalog=YourDatabase;User ID=YourUser;Password={0}",
        "ConnectionPassword": "AnEncryptedPasswordString=="
      },
      "AnalyticsConnection": {
        "ConnectionString": "Server=myServerAddress.database.windows.net;Authentication=Active Directory Integrated;Database=myDataBase;"
      }
    }
  }
}
```

### 2. Register Services in `Program.cs`

In your application's `Program.cs` file, you will retrieve the master key and then call the `AddDatabaseServices` extension method to register all services.

```csharp
using SqlShield; // Your library's namespace

var builder = WebApplication.CreateBuilder(args);

// ... other services like AddControllersWithViews()

// --- SqlShield Setup ---
// 1. Retrieve the master key from your preferred secure source (e.g., Key Vault, user prompt).
string userProvidedKey = GetKeyFromSecureSource(); 

// 2. This single line registers all SqlShield services with the provided key.
//    It will automatically read other settings like 'Iterations' from the configuration.
builder.Services.AddDatabaseServices(
    builder.Configuration,
    userProvidedKey
);
// --- End SqlShield Setup ---

var app = builder.Build();

// ... rest of your application
```

### 3. Inject and Use

You can now inject any of the library's services into your controllers or other classes. The most common service to use is `IStoredProcedureExecutor`.

```csharp
using Microsoft.AspNetCore.Mvc;
using SqlShield.Interface; // The library's interfaces
using YourApp.Models;      // Your application's models

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

| Interface                | Description                                                                                                      |
| ------------------------ | ---------------------------------------------------------------------------------------------------------------- |
| **`ICryptography`** | A standalone service that handles cryptographic operations. It's configured at startup with the user-provided key. |
| **`IDatabaseService`** | Consumes `ICryptography` to build a fully decrypted, ready-to-use database connection string when needed.        |
| **`IStoredProcedureExecutor`** | The primary service for application developers. It consumes `IDatabaseService` to provide high-level methods (`QueryAsync`, `ExecuteNonQueryAsync`) for running stored procedures. |
