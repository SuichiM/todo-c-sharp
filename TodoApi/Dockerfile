# Development stage - use SDK for hot reload
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS development
WORKDIR /app

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy everything else
COPY . ./

# Expose port
EXPOSE 8080

# Set environment variable
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV ASPNETCORE_ENVIRONMENT=Development

# Run with hot reload (watch mode)
ENTRYPOINT ["dotnet", "watch", "run", "--non-interactive"]
