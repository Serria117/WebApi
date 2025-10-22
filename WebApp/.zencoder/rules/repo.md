---
description: Repository Information Overview
alwaysApply: true
---

# SLine Web API Information

## Summary
A .NET 8.0 Web API application providing backend services for the SLine application. The API includes authentication, authorization, data management, and various business logic services including invoice processing, payroll management, and document handling.

## Structure
- **Controllers**: API endpoints organized by domain functionality
- **Services**: Business logic implementation and service layer
- **Repositories**: Data access layer components
- **Authentication**: JWT-based authentication and permission-based authorization
- **Configuration**: Application configuration and dependency injection
- **Payloads**: Request/response models and DTOs
- **Utils**: Common utility functions and extensions
- **ScheduleTask**: Background job scheduling using Quartz
- **GlobalExceptionHandler**: Centralized exception handling middleware

## Language & Runtime
**Language**: C#
**Version**: .NET 8.0 (SDK 8.0.20)
**Build System**: MSBuild
**Package Manager**: NuGet

## Dependencies
**Main Dependencies**:
- Microsoft.AspNetCore.Authentication.JwtBearer (8.0.20)
- Microsoft.EntityFrameworkCore.Design (9.0.9)
- Quartz.Extensions.Hosting (3.15.0)
- RestSharp (112.1.0)
- Serilog (4.3.0)
- Swashbuckle.AspNetCore (6.9.0)
- BCrypt.Net-Next (4.0.3)
- FreeSpire.XLS (14.2.0)
- MailKit (4.13.0)

**Project References**:
- WebApp.Core
- WebApp.Enums
- WebApp.Mongo

## Build & Installation
```bash
dotnet restore WebApp.sln
dotnet build WebApp.sln --configuration Release
dotnet publish WebApp.csproj -c Release -o ./publish
```

## Testing
No explicit testing framework was identified in the project. The application appears to use Swagger for API documentation and testing.

## Database
**Primary Database**: SQL Server (Entity Framework Core)
**Secondary Database**: MongoDB (for specific features)
**Configuration**: Connection strings defined in application settings

## API Features
- **Authentication**: JWT-based authentication with refresh tokens
- **Authorization**: Custom permission-based authorization
- **SignalR**: Real-time communication via WebSockets
- **Swagger**: API documentation and testing interface
- **Background Jobs**: Scheduled tasks using Quartz
- **Exception Handling**: Global exception middleware
- **Logging**: Structured logging with Serilog
- **File Operations**: Document templates and export functionality

## Deployment
**Hosting**: Supports IIS Express and Kestrel
**Environment**: Development/Production configurations
**Port**: 24894 (default development port)