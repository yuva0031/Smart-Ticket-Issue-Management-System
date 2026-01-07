# Smart Ticket & Issue Management System

An full-stack Issue & Incident Management Platform built using ASP.NET Core 8, Angular 17, and Lucene.NET. Designed to solve real-world IT support challenges, this system delivers intelligent ticket categorization, automated agent assignment, and real-time communication with production-ready architecture.

## Business Problem

Organizations struggle with fragmented, manual ticket handling that leads to delayed resolutions, poor visibility, and SLA violations.

## Key Features

- Centralized ticket lifecycle management
- AI-assisted ticket categorization using Lucene.NET
- Real-time updates via SignalR
- Secure authentication with JWT
- SLA tracking and breach detection
- Clean Architecture for scalability and maintainability

## System Architecture

The system follows a layered architecture pattern:

```
Angular 17 (UI)
   ↓ REST + SignalR
ASP.NET Core 8 Web API
   ↓
Application Layer (Business Logic)
   ↓
Infrastructure Layer (EF Core, Lucene, SignalR)
   ↓
SQL Server
```

## Prerequisites

- .NET SDK 8.0 or higher
- SQL Server (LocalDB, Express, or Full)
- Node.js 18 or higher
- npm 9 or higher
- Angular CLI 17 or higher

Install Angular CLI globally:
```bash
npm install -g @angular/cli
```

## Installation Guide

### 1. Clone the Repository

```bash
git clone https://github.com/yuva0031/Smart-Ticket-Issue-Management-System.git
cd Smart-Ticket-Issue-Management-System
```

### 2. Backend Setup

Navigate to the backend directory:
```bash
cd SmartTicketSystem
```

Configure the database connection in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=SmartTicketSystemDb;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

Note: You can use SQL Server LocalDB or Express for local development.

Apply database migrations:
```bash
dotnet ef database update
```

Run the API:
```bash
dotnet run
```

The backend will start at: `https://localhost:7074`

### 3. Frontend Setup

Open a new terminal window and navigate to the frontend directory:
```bash
cd smart-ticket-ui
```

Install dependencies:
```bash
npm install
```

Run the Angular application:
```bash
ng serve
```

The frontend will be available at: `http://localhost:4200`

## Application Workflow

### User Onboarding
- Supported roles: End User, Agent, Manager
- Agent and Manager accounts require Admin approval

### Ticket Creation
- End Users submit tickets with issue details

### Intelligent Processing
- Lucene.NET analyzes ticket descriptions
- Automatically categorizes issues (IT, Network, Hardware, etc.)
- Background services assign tickets based on:
  - Agent skills
  - Current workload
  - Priority level

### Real-Time Updates
- Status changes pushed instantly via SignalR
- Users and agents receive notifications without page refresh

### SLA Monitoring
- SLA timers start automatically
- System flags breaches and escalates issues

## Technology Stack

### Frontend
- Angular 17 (Signals, RxJS, Angular Material)

### Backend
- ASP.NET Core 8 (Clean Architecture)
- Entity Framework Core
- JWT Authentication

### Database
- SQL Server

### Search and Categorization
- Lucene.NET 4.8.0

### Real-Time Communication
- SignalR

## Architecture Pattern

The backend follows Clean Architecture principles:
- Domain Layer
- Application Layer
- Infrastructure Layer
- API Layer

Key principles:
- SOLID principles
- Asynchronous programming throughout
- Background workers for automation

The frontend implements:
- Angular Signals for reactive state management
- Modular, standalone components
- ESLint for code quality

## Configuration Notes

### JWT Configuration

Ensure the JWT secret is at least 32 characters in `appsettings.json`:
```json
{
  "JwtSettings": {
    "Secret": "YOUR_SUPER_SECURE_32_CHAR_SECRET_KEY"
  }
}
```

### HTTPS Certificate Issue

If you encounter SSL errors during development:
```bash
dotnet dev-certs https --trust
```

## Code Quality

### Backend Linting
The backend follows SOLID principles and Clean Architecture patterns.

### Frontend Linting
Run Angular lint check:
```bash
ng lint
```

## Why Lucene.NET?

Lucene.NET provides:
- Full-text search indexing
- Keyword relevance scoring
- Fast categorization without ML overhead

Ideal for:
- Rule-less classification
- Scalable enterprise search
- Low-latency processing

## Project Type

Full Stack Enterprise Application Development - Capstone Project
\
