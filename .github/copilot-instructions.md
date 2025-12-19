1. Project Identity & SDK Standards
Unified ID Strategy: Use the Solution name (e.g., PoAppName) as the master identifier for Azure resources. Use the short prefix (PoApp) for project namespaces.
Modern SDK Standards: Target .NET 10 exclusively. Enforce central package management via Directory.Packages.props.
C# 14+ Syntax: Leverage primary constructors and collection expressions as the default. Leverage the field keyword to eliminate backing fields. Enable <IsAotCompatible>true</IsAotCompatible> to ensure lean, reflection-free code. TreatWarningsAsErrors. 
2. Architecture & Logic Flow
Vertical Slice Architecture: Organize code by feature folders. Each folder contains the Minimal API endpoint, DTOs, and Business Logic. Logic should be self-contained within the slice.
Result Pattern: Use the ErrorOr library for flow control. Minimal APIs must use the .Match() extension to return TypedResults (e.g., 200 OK or 400 BadRequest).
Safety: <Nullable>enable</Nullable> and <ImplicitUsings>enable</ImplicitUsings> are mandatory across all projects.
3. Data & Mapping
Source-Generated Mapping: Use Mapperly for DTO-to-Entity conversions to ensure compile-time safety and high performance. Reflection-based mappers are permitted only for dynamic, non-performance-critical internal utilities.
Queryable Mapping: Use lightweight expression visitors to map DTO filters directly to Azure Table Storage TableClient, keeping the application logic storage-agnostic without heavy ORMs.
Resilience: Apply Polly pipelines (Retry with jitter, Circuit Breaker) directly to the storage and outbound HTTP clients.
4. Authentication & UI (BFF Pattern)
BFF Pattern (Backend-for-Frontend): The .Api host acts as the security proxy for the Blazor WASM client. The client uses Secure Cookies only; it never handles or stores JWTs.
Claims-Based Auth: Use IClaimsTransformation on the server to inject specific application claims (e.g., IsAdmin) after Google Auth login.
UI Hierarchy: Follow the Smart vs. Dumb component pattern. Page-level components handle data fetching, while child components remain purely presentational.
State Management: Use a Scoped StateContainer with an OnChange event for shared state (e.g., user preferences) to avoid deep parameter drilling.
Fingerprinted Static Assets: All JS/CSS references must use the .NET 10 Static Web Assets system with automatic fingerprinting to ensure immediate cache busting upon deployment.
Hydration: Use [PersistentComponentState] to eliminate flickering during SSR to WASM transitions.
5. Infrastructure & DevOps
Provisioning: Use Azure Deployment Stacks with modular Bicep files targeting Azure App Service (Linux).
Secret Management: Use Azure Key Vault via DefaultAzureCredential. Use Azurite locally for Table/Blob emulation.
Environment Configuration: Appsettings.json must only contain non-sensitive metadata. All keys and secrets must reside in Key Vault and be injected into App Service via Key Vault References (@Microsoft.KeyVault(...)).
6. Health & Monitoring
DIAG Page: Maintain a /diag page using .NET Health Checks. Configure App Service Health Check to monitor this endpoint for automated instance recycling.
Lean Logging: Use Serilog with a "Context-First" policy. In Production, log only Information and above.
Custom Metrics: Use OpenTelemetry exported directly to Azure Monitor (Application Insights) for performance tracking and distributed tracing.
Budgeting: Set a $5/month hard limit with an 80% alert sent to the registered email.
7. Development Workflow
Local Simulation: Use launchSettings.json profiles to manage local environment variables. Run the API and Client as separate startup projects.
Testing: Apply TDD. Integration tests run against Azurite; E2E tests use Chromium and mobile emulators.
Manual Debugging: Explicitly maintain ports 5000 (HTTP) and 5001 (HTTPS) in launchsettings.json to ensure consistency across local dev environments.


