# PostgreSQL Setup for TunRTC

## Prerequisites
- PostgreSQL 14 or higher installed

## Quick Setup

### 1. Create Database
```sql
CREATE DATABASE tunrtc;
```

### 2. Run Schema
```bash
psql -U postgres -d tunrtc -f schema.sql
```

### 3. Configure Connection String
Update `appsettings.json` in the Server project:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=tunrtc;Username=postgres;Password=your_password"
}
```

## Entity Framework Migrations

The project uses EF Core migrations. The database will be automatically created on first run.

### Create a new migration
```bash
cd Server
dotnet ef migrations add MigrationName
```

### Apply migrations
```bash
dotnet ef database update
```

## Default User
- **Username**: demo
- **Email**: demo@tunrtc.com
- **Password**: demo123

## Database Tables

- **Users**: User accounts
- **Sessions**: WebRTC sessions
- **SessionParticipants**: Participants in each session
- **ConnectionLogs**: Connection history and analytics
