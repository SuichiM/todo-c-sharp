# Todo Application

A full-stack todo application with ASP.NET Core backend and React frontend.

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (v18+) and [pnpm](https://pnpm.io/)
- [Docker](https://www.docker.com/get-started) (optional, for containerized deployment)

## Local Development

### Run the Backend API

```bash
cd TodoApi
dotnet restore
dotnet run
```

API available at `http://localhost:5296`

### Run the Frontend UI

```bash
cd TodoUI
pnpm install
pnpm dev
```

UI available at `http://localhost:5173`

### Hot Reload

Backend:

```bash
cd TodoApi
dotnet watch run
```

Frontend: Vite provides hot reload by default

## Docker Deployment

### Environment Configuration

Copy `.env.example` to `.env` and configure your settings:

```bash
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres123
POSTGRES_DB=tododb
DB_PORT=5432

ASPNETCORE_ENVIRONMENT=Development
API_PORT=8083
UI_PORT=5173
VITE_API_URL=http://localhost:${API_PORT}
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
The UI will be available at `http://localhost:5173`

## Project Structure

```
TodoApi/                  # ASP.NET Core REST API
├── Controllers/          # API endpoints
├── Models/              # Domain models
├── Repositories/        # Data access layer
├── Requests/            # DTOs and validators
└── Data/                # EF Core context and configurations

TodoUI/                   # React + TypeScript frontend
├── src/
│   ├── components/      # React components
│   ├── api/            # API client
│   ├── hooks/          # Custom hooks
│   └── types/          # TypeScript types

docker-compose.yml       # Multi-container orchestration
```

## Technologies

**Backend:**

- ASP.NET Core 9.0 - Web framework
- PostgreSQL 16 - Database
- Entity Framework Core - ORM
- FluentValidation - Request validation

**Frontend:**

- React 18 - UI library
- TypeScript - Type safety
- Vite - Build tool
- TanStack Query - Data fetching

**Infrastructure:**

- Docker - Containerization

## API Endpoints

- `GET /api/todos` - Get all todo items
- `GET /api/todos/{id}` - Get a specific todo item
- `GET /api/todos/category/{id}` - Get todo items by category
- `GET /api/todos/completed` - Get completed todo items
- `GET /api/todos/pending` - Get pending todo items
- `GET /api/todos/overdue` - Get overdue todo items

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
- [x] Basics on Request handling => DTO + Validator
- [x] Implementing RESTful API endpoints for reads Todo Items with Categories
- [x] Implementing RESTful API endpoints for creating, updating, and deleting Todo Items
- [x] Setting up React frontend with TypeScript and Vite
- [x] Implementing API client and data fetching with TanStack Query
- [x] Building todo management UI components

## videos

- [Progress on the learning process](https://www.loom.com/share/d2f13b139575463c9260c073cac589ad)
- PENDING: [Proyect structure explanation and endpoints working]()
