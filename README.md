# Feed App

A full-stack RSS feed reader built with **ASP.NET Core**. Users can subscribe to RSS feeds, browse and read articles, and mark content as read. The application is structured as a decoupled Web API backend with MVC frontend, and background services that continuously poll and ingest feed content.

---

## Features

**Feed Management**
- Add, update, and delete RSS feed subscriptions
- Retrieve feed lists and individual feeds
- Browse articles from subscribed feeds
- Mark feeds and individual articles as read

**Authentication & Authorization**
- JWT-based authentication with access and refresh token workflow
- Dedicated auth database context keeping identity data separate from application data

**Background Processing**
- Hosted background services periodically poll subscribed RSS feeds
- Automatic article ingestion and deduplication
- Scheduled cleanup and maintenance tasks

**Observability**
- Dedicated logging database context (`LoggingDbContext`) for structured log persistence

---

## Tech Stack

| Technology | Purpose |
|---|---|
| ASP.NET Core Web API | Backend API |
| ASP.NET Core MVC | Server-rendered web frontend |
| Entity Framework Core | ORM and database migrations |
| SQL Server | Primary data store |
| JWT (JSON Web Tokens) | Authentication |
| xUnit | Unit testing |
| Microsoft.Extensions.Logging | Structured logging |
| Azure Pipelines | CI/CD |

---

## Database Contexts

The API uses three separate EF Core DbContexts, each bundled as its own migration artifact in the CI pipeline:

| Context | Purpose |
|---|---|
| `AppDbContext` | Core application data — feeds, articles, subscriptions |
| `AuthDbContext` | Identity and authentication data — users, tokens |
| `LoggingDbContext` | Persisted application logs |

---

## Authentication Flow

1. User registers or logs in via the API
2. A short-lived **access token** and long-lived **refresh token** are issued
3. The access token is used to authenticate subsequent API requests
4. When the access token expires, the refresh token is exchanged for a new one without requiring re-login

---

## License

This project is licensed under the MIT License.
