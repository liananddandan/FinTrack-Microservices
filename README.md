## Authentication Model

FinTrack uses a two-stage authentication model designed for multi-tenant systems.

A user account may belong to multiple tenants. Authentication is therefore separated into **account identity** and **tenant context**.

### Token Types

The system issues several JWT token types:

- **AccountAccessToken**  
  Identifies the user account. Used to fetch user profile and tenant memberships.

- **TenantAccessToken**  
  Represents access to a specific tenant. Required for all tenant-scoped APIs.

- **RefreshToken**  
  Used to obtain a new access token when the current one expires.

- **InviteToken**  
  Used during tenant invitation flows when a user accepts an organization invitation.

### Login Flow

1. User logs in with email and password.
2. The system issues an **AccountAccessToken** and **RefreshToken**.
3. The client fetches memberships using `/api/account/me`.
4. The client selects a tenant using `/api/account/select-tenant`.
5. The server issues a **TenantAccessToken**.
6. All tenant-related APIs require the **TenantAccessToken**.

This model ensures that user identity and tenant authorization remain clearly separated, which simplifies multi-tenant security and permission management.