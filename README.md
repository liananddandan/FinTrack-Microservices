# FinTrack Microservices

FinTrack is a microservices-based financial tracking platform designed to manage donations, procurements, and audit logs for organizations.

The system is built using a distributed architecture with ASP.NET Core services, message-driven communication, and containerized deployment via Docker.

This project demonstrates a production-style backend architecture including API Gateway routing, event-driven messaging, and containerized development workflows.

FinTrack is primarily designed as a **portfolio project to demonstrate modern backend architecture patterns**, including microservices, event-driven communication, multi-tenant authentication, and containerized development environments.

## Tech Stack

### Backend
- .NET 9 / ASP.NET Core
- Entity Framework Core
- MediatR (CQRS pattern)

### Infrastructure
- MySQL
- Redis
- RabbitMQ (event messaging via CAP)

### Architecture
- Microservices
- API Gateway
- Event-driven communication

### DevOps
- Docker & Docker Compose
- GitHub Actions (CI)
- Automatic EF Core migrations

### Frontend
- Web Admin
- Web Portal

## Architecture
```
Web Portal / Web Admin
          │
          ▼
       Gateway
          │
 ┌────────┼────────┐
 ▼        ▼        ▼
Identity Transact  AuditLog
          │
          ▼
       RabbitMQ
          │
          ▼
      Notification
```
The **Gateway service** acts as the single entry point for all client requests.

Internal services communicate using both:

- **HTTP APIs** (synchronous communication)
- **RabbitMQ events via CAP** (asynchronous communication)

---
# Project Structure
```
FinTrack-Microservices
│
├─ src
│  ├─ Services
│  │  ├─ GatewayService
│  │  ├─ IdentityService
│  │  ├─ TransactionService
│  │  ├─ AuditLogService
│  │  └─ NotificationService
│  │
│  ├─ Web
│  │  ├─ web-admin
│  │  └─ web-portal
│
├─ docker-compose.yml
├─ README.md
```
---

# Service Responsibilities

### GatewayService
- API entry point
- Routes requests to internal services
- Handles basic authentication validation
- Provides development utilities such as seed endpoints

### IdentityService
- User accounts
- Tenant management
- Authentication and authorization
- JWT token generation

### TransactionService
- Financial transactions
- Donation and procurement tracking

### AuditLogService
- Stores system activity logs
- Records important domain events

### NotificationService
- Handles asynchronous notifications
- Consumes events from RabbitMQ

---

## Running Locally

Prerequisites

- Docker
- Docker Compose

Start the entire stack:

```bash
docker compose up -d
```
Services will start automatically and apply database migrations on startup.

Web interfaces:
- Admin: http://localhost:3001
- Portal: http://localhost:3000

API Gateway:
- http://localhost:5193

## Development Seed
For development and testing, FinTrack provides a seed endpoint that initializes demo data including a tenant and example users.
```
POST /api/dev/seed
```
After triggering the seed endpoint from the Web Admin login page, the following demo accounts become available.
Admin account:
Email: admin@fintrack.local
Password: Admin123!

Member account:
Email: member@fintrack.local
Password: Member123!

These accounts are intended for development environments only.

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

## Docker Services

The Docker environment includes the following containers:

- gateway
- identity-service
- transaction-service
- auditlog-service
- notification-service
- mysql
- redis
- rabbitmq
- mailhog
- web-admin
- web-portal

All services run within a shared Docker network and communicate using service names.