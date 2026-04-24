## MBET Dependency Manifest

Execute these commands in the Package Manager Console or via Visual Studio NuGet Manager for the respective projects.

### 1. MBET.Core

- Microsoft.AspNetCore.Identity.EntityFrameworkCore (10.0.0-preview+)

### 2. MBET.Infrastructure

- Microsoft.EntityFrameworkCore (10.0.0-preview+)

- Microsoft.EntityFrameworkCore.SqlServer (10.0.0-preview+)

- Npgsql.EntityFrameworkCore.PostgreSQL (10.0.0-preview+)

- Microsoft.AspNetCore.Identity.EntityFrameworkCore (10.0.0-preview+)

- Microsoft.AspNetCore.DataProtection (10.0.0-preview+)

### 3. MBET.Shared (Razor Class Library)

- MudBlazor (8.15.0)

- Microsoft.Extensions.Localization (10.0.0-preview+)

### 4. MBET.Web (Blazor Server)

- MudBlazor (8.15.0)

- Serilog.AspNetCore (Latest)

- Serilog.Sinks.File (Latest)

- Microsoft.AspNetCore.Authentication.Google (10.0.0-preview+)

### 5. MBET.Tests (xUnit Test Project)

- Microsoft.EntityFrameworkCore.InMemory (Latest)

- Microsoft.EntityFrameworkCore.Sqlite (Latest)

- Moq (Latest)

- xunit (Latest)

- xunit.runner.visualstudio (Latest)

### 6. Project References (The Chain)

- MBET.Infrastructure -> References MBET.Core

- MBET.Shared -> References MBET.Core

- MBET.Web -> References MBET.Infrastructure and MBET.Shared

- MBET.Tests -> References MBET.Core, MBET.Infrastructure, and MBET.Web