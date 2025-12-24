# Todo API

A REST API built with ASP.NET Core and PostgreSQL.

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/get-started) (optional, for containerized deployment)

## Local Development

### Run the API

```bash
cd TodoApi
dotnet restore
dotnet run
```

The API will be available at `http://localhost:5296`

### Hot Reload (Auto-restart on changes)

```bash
dotnet watch run
```

## Docker Deployment

### Environment Configuration

Copy `.env.example` to `.env` and configure your settings:

```bash
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres123
POSTGRES_DB=tododb
```

### Build and Run with Docker Compose

```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

The API will be available at `http://localhost:8083`

## Project Structure

```
TodoApi/
├── Controllers/          # API endpoints
├── Models/              # Data models
├── Program.cs           # Application entry point
├── appsettings.json     # Configuration
└── TodoApi.csproj       # Project file

docker-compose.yml       # Multi-container setup
Dockerfile              # Container image definition
.env                    # Environment variables (not committed)
```

## API Endpoints

### Weather (Example)

- `GET /weatherforecast` - Get weather forecast

## Technologies

- **ASP.NET Core 9.0** - Web framework
- **PostgreSQL 16** - Database
- **Entity Framework Core** - ORM
- **Docker** - Containerization

## Endpoints

- `GET /api/todos` - Get all todo items
- `GET /api/todos/{id}` - Get a specific todo item
- `GET /api/todos/category/{id}` - Get todo items by category
- `GET /api/todos/completed` - Get completed todo items
- `GET /api/todos/pending` - Get pending todo items
- `GET /api/todos/overdue` - Get overdue todo items
-

## Learning checkpoints

- [x] Setting up ASP.NET Core Web API
- [x] Initial WheaterForecast endpoint
- [x] Configuring Entity Framework Core with PostgreSQL
- [x] Basics on Entity framework vs Eloquent
- [x] Creating Models Items and Categories and applying migrations
- [x] Defining data models and relationships
- [x] Setting up Docker and Docker Compose
- [x] Environment variable management with .env files
- [x] Repository pattern for data access
- [x] Implementing Categories model and repository
- [x] DTOs for responses shapes similar to Laravel Resources
- [ ] Basics on Request handling => DTO + Validator
- [ ] Implementing RESTful API endpoints for reads Todo Items with Categories
- [ ] Implementing RESTful API endpoints for creating, updating, and deleting Todo Items
