#MBET Development Roadmap

Status: Mid-Phase 4 (The Shopping Engine) & Phase 5 (Admin Foundation)
Current Version: 0.6.0 (White-Label Engine & Dynamic Catalog)

## ✅ Phase 1: Foundation & Architecture (Completed)

- [x] Solution Structure: Clean Architecture (Core, Infrastructure, Web, Shared).

- [x] Database Engine: 
    - [x] MBETDbContext with switchable SQL Server/PostgreSQL logic.
    - [x] IDbContextFactory configured for Blazor Server concurrency.

- [x] Security Core:
    - [x] AES-256 Encryption at rest for PII (Phone, Addresses, Names).
    - [x] ICurrentUserService for audit tracking.

- [x] Localization: 
    - [x] Middleware configured for EN/FR/AR.
    - [x] RTL layout support via MudRTLProvider.

## ✅ Phase 2: Identity & Visual Experience (Completed)

- [x] Design System: 
    - [x] "Future Tech" Dark Mode aesthetic (MBETTheme.cs).
    - [x] Glassmorphism Layouts (MainLayout, GlassPanel CSS).

- [x] Identity System:
    - [x] Custom ApplicationUser with Commerce fields (Billing/Shipping, VIP status).
    - [x] AccountController for cookie-based Authentication (Login/Register/Logout).
    - [x] RevalidatingIdentityAuthenticationStateProvider for session security.
    - [x] Role Seeding (SuperAdmin, Customer, ShopManager) via DbInitializer.

- [x] User Experience:
    - [x] Profile Dashboard: Tabbed interface for Personal Info, Addresses, and Security.
    - [x] Landing Page: Modular, localized components (Hero, Features, JoinCollective).
    - [x] Navigation: Dynamic Appbar with User Profile dropdown and Notifications.

## ✅ Phase 3: Administration & Operations (Foundation Completed)

Goal: Allow non-technical management of the store.

- [x] Admin Layout: Distinct layout for /admin routes (Sidebar navigation).

- [x] User Management:

- [x] Data Grid with Actions (Edit, Ban, Reset Password).

- [x] Manual User Creation (Admin/Users/Create).

- [x] Role Management: Create/Delete Roles.

- [x] Global Settings: Centralized configuration page (/admin/settings) for branding, SEO, and business logic.

## ✅ Phase 4: White-Label Engine (Completed - Refactored)

- [x] Global Settings System:
    - [x] Database-driven GlobalSettings entity (Branding, Socials, SEO).
    - [x] SettingsService with in-memory caching for high performance.
    - [x] Admin Settings Page for managing all site configuration.

- [x] Dynamic UI Components:
    - [x] AppLogo, AppFooter, HeroSection refactored to consume DB settings.
    - [x] Removed hard-coded CSS in favor of dynamic MudBlazor theming.

- [x] Feature Section Engine:
    - [x] Dynamic "Why Choose Us" cards (Icon/Title/Desc) managed via Admin.

## ✅ Phase 5: The Shopping Engine (CURRENT FOCUS)

Goal: Move from static demo data to real database interactions.

### 5.1 Product Architecture

- [x] Product Core:
    - [x] Entities: Finalize Product, Category, ProductImage, and Inventory models in Core.
    - [x] Repository: Implement IProductRepository with filtering logic (Price, Category, Search).

- [x] Landing Page Logic:
    - [x] ProductDisplayMode (Newest, Priority, Random) configurable in Settings.
    - [x] ProductService connecting UI to Repository logic.
    - [x] "Fresh Drops" section with independent visibility toggle.

- [ ] Shopping Cart (Logic):
    - [x] Cart Entity & State Service.
    - [ ] Refine Checkout Logic (Stock Deduction integration).

### 5.2 Public Catalog

Catalog Page: Create /catalog with:

- [x] Sidebar Filters (Category, Price Range, In Stock).

- [x] Sort Options (Newest, Price: Low-High).

- [x] Product Details: Create /product/{id} with gallery, specs, and "Add to Cart".

- [x] Landing Page Integration: Connect Home.razor to fetch "Fresh Drops" from the DB.

### 5.3 Shopping Cart (Logic)

- [x] Cart Entity: Design Cart and CartItem for database persistence (User) and LocalStorage (Guest).

- [x] Cart State: Implement a Blazor CartState service to update the UI badge in real-time.

- [x] Cart UI: Management page with total calculation.

- [x] Checkout Logic: Order creation, address validation, and stock deduction.

## 🛠️ Phase 6: Administration & Operations

Goal: Allow non-technical management of the store.

- [x] Admin Layout: Distinct layout for /admin routes.

- [x] System Diagnostics:

    - [x] SystemHealth.razor for debugging environment paths and permissions.

- [ ] Product Management:
    - [x] Data Grid with CRUD actions for product (Create, Read, Update, Delete).
    - [x] Image Upload System:
        - [x] LocalStorageService refactored for folder organization (uploads/branding, uploads/products).
        - [x] IIS/SmarterASP compatibility fix (Explicit UseStaticFiles configuration).

- [x] Order Management: View incoming orders and update status.

## 🚀 Phase 7: Deployment & Polish

- [ ] Email Notifications: Send welcome emails and password resets via SMTP/SendGrid.

- [ ] Performance: Implement Caching for the Product Catalog.

- [ ] CI/CD: GitHub Actions pipeline for SmarterASP.NET deployment.

## 📝 Design & Code Rules (Refresher)

- No Magic Strings: Use Localization (L["Key"]) for all text.

- MudBlazor First: Avoid custom CSS unless strictly necessary for Glassmorphism.

- Secure by Default: PII must be encrypted. Controller actions must validate models.

- Async Patterns: Use await for all Database operations to prevent thread starvation.