<p align="center">
  <i>This project is part of my personal portfolio. Feel free to browse the code.</i><br>
  <b>For commercial inquiries or private licensing, please contact me directly.</b>
</p>

# MBET Prototype (Modular Blazor E-Commerce Template)

### 1. Project Goal
A **Personal Project** prototype providing a modular, white-label foundation for rapid e-commerce deployment. This MVP focuses on a secure, server-rendered architecture optimized for Windows-based hosting (e.g., MonsterASP, SmarterASP).

### 2. Core Tech Stack
- **Framework:** Blazor Server (.NET 10) with [MudBlazor 8.15](https://mudblazor.com)
- **Data:** EF Core (PostgreSQL/SQL Server) with Repository Pattern
- **Auth:** ASP.NET Identity (Planned: Google OAuth + TOTP 2FA)
- **Security:** AES-256 encryption at rest for PII (**Algerian Data Privacy** compliance ready)

### 3. Simplified Architecture
The prototype follows a three-tier **Clean Architecture**:
- **Shop.Core:** Domain entities (Product, Order, User) and service interfaces.
- **Shop.Infrastructure:** Data access logic, switchable DB providers, and encryption services.
- **Shop.Web:** Blazor UI, dynamic theme providers, and Admin/User interfaces.

### 4. Key Prototype Rules
- **Dynamic Branding:** UI elements (Logos, Titles, Socials) are driven by a `GlobalSettings` table for instant white-labeling.
- **Service-Only Access:** Razor components use injected services; direct DB context calls are strictly prohibited.
- **Encrypted Storage:** Interceptors handle encryption for fields marked for protection.
- **Deployment Ready:** Runtime folders are pre-configured in `Program.cs` for IIS/SmarterASP static file compatibility.

---

### ⚖️ License

This project is licensed under the **GNU General Public License v3.0 (GPL-3.0)**.

- **Transparency:** You are free to view, copy, and modify this code.
- **Reciprocity:** If you distribute a modified version of this project, you **must** also make your source code public under the same GPL-3.0 license.
- **No Warranty:** This software is provided "as is" for portfolio demonstration purposes.

For the full legal text, please see the [LICENSE](./LICENSE) file.