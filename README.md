# SqlShield

SqlShield is a lightweight .NET class library that makes working with SQL in C# safer and simpler.  
It combines two powerful features:

It focuses on two things:

1. **Convention-Based Mapping for Dapper**  
   Automatically maps snake_case or kebab-case database columns to PascalCase C# properties.  
   No more writing `AS` clauses like `SELECT first_name AS FirstName`.

2. **Boilerplate-Free Query Execution**  
    Provides injectable services (`IStoredProcedureExecutor`, `IDatabaseService`) that let you call stored procedures directly, without writing repetitive ADO.NET or Dapper setup code.

---

## ✨ Features

- 🔧 **Global naming conventions** (snake_case or kebab-case) for all DTOs.
- 🏷 **Class-level overrides** with `[DapperConvention]`.
- 🎯 **Property-level overrides** with `[ColumnOverride]`.
- ⚡ **Execute stored procedures** with a single call using `IStoredProcedureExecutor`.
- 🔑 **Pass raw connection strings** per call — no config binding required.
- 🤝 **Dependency Injection ready** via extension methods.

---

## 📦 Installation

```bash
dotnet add package SqlShield
```
## Quick Start

### 1. Register SqlShield in Program.cs
```csharp
using SqlShield.Extension;

var builder = WebApplication.CreateBuilder(args);

// Pick ONE global convention:
builder.Services.AddSqlShieldWithSnakeCase();
// builder.Services.AddSqlShieldWithKebabCase();
// builder.Services.AddSqlShieldWithoutConvention();

var app = builder.Build();
```

### 2. Define your Models
```csharp
public class User
{
    public int UserId { get; set; }       // maps to user_id
    public string FirstName { get; set; } // maps to first_name
}
```

#### Attribute-Based Overrides
##### Class-level Convention
```csharp
[DapperConvention(typeof(KebabCaseConverter))]
public class Customer
{
    public int CustomerId { get; set; }  // → customer-id
    public string LastName { get; set; } // → last-name
}
```

##### Property-level Convention
```csharp
public class Order
{
    [ColumnOverride("order_number")]
    public int OrderId { get; set; }     // → order_number
}

```

## 🛠 Usage
### 1.  Use Dapper directly (with conventions applied)
Once a convention is registered, you can use Dapper:
```csharp
using var conn = new SqlConnection(connectionString);

// Snake_case columns map automatically
var users = await conn.QueryAsync<User>("SELECT * FROM users");
```

### 2. Execute stored procedures with no boilerplate
Inject IStoredProcedureExecutor anywhere (controller, service, etc.):
```csharp
public class AccountService
{
    private readonly IStoredProcedureExecutor _executor;

    public AccountService(IStoredProcedureExecutor executor)
    {
        _executor = executor;
    }

    public async Task DisableAccountAsync(int accountId)
    {
        var parameters = new Dictionary<string, object>
        {
            { "@AccountId", accountId }
        };

        await _executor.ExecuteNonQueryAsync(
            "sp_DisableAccount",
            "Server=.;Database=AppDb;Trusted_Connection=True;",
            parameters
        );
    }
}
```
If your procedure returns rows:
```csharp
var parameters = new Dictionary<string, object>
{
    { "@IsActive", true }
};

User users = await _executor.QueryAsync<User>(
    "sp_GetActiveUsers",
    "Server=.;Database=AppDb;Trusted_Connection=True;",
    parameters
);
```
👉 No more manual SqlConnection, SqlCommand, or QueryAsync boilerplate.

## 🔬 Advanced
### Precedence of Conventions
- `[ColumnOverride]` → strongest
- `[DapperConvention]` at class level
- Global convention (snake_case or kebab-case)
- Dapper’s default mapping

### Flexible Usage
- Use conventions only.
- Use stored procedure execution only
- Or combine both
SqlShield stays passive: it never forces a validation step, just helps you avoid repetitive code.

## ✅ Testing
SqlShield includes tests for:
- Global conventions (snake_case, kebab-case)
- Default Dapper mapping
- Class-level overrides
- Property-level overrides
