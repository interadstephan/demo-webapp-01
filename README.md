# Offline Sync Web Application

A multi-client offline-capable web application built with ASP.NET Core 8, RxDB, and Angular. This application enables hundreds of agents to sync their subset of data (including files) with a central SQL Server database, supporting offline-first functionality across desktop Chrome and iPad devices.

📚 **[Quick Start Guide](QUICKSTART.md)** | 🤝 **[Contributing Guide](CONTRIBUTING.md)** | 🔧 **[API Examples](api-examples.http)**

## Features

- **Offline-First Architecture**: Full functionality even without internet connection
- **Real-time Synchronization**: Automatic bi-directional sync between client and server
- **Multi-Device Support**: Same data available across multiple devices per agent
- **Agent-Specific Data Isolation**: Each agent only syncs their own data subset
- **File Attachment Support**: Upload and sync files along with data records
- **Conflict Resolution**: Version-based conflict resolution for data consistency
- **Cross-Platform**: Works on desktop Chrome and iPad browsers

## Tech Stack

### Backend
- **ASP.NET Core 8**: Web API framework
- **Entity Framework Core**: ORM for database operations
- **SQL Server**: Central database for all agent data
- **RESTful API**: For client-server communication

### Frontend
- **Angular 19+**: Modern web framework
- **RxDB**: Reactive offline-first database (IndexedDB)
- **RxJS**: Reactive programming library
- **TypeScript**: Type-safe JavaScript
- **SCSS**: Advanced CSS preprocessing

## Architecture

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│  Agent Device 1 │     │  Agent Device 2 │     │  Agent Device 3 │
│   (Chrome/iPad) │     │   (Chrome/iPad) │     │   (Chrome/iPad) │
│                 │     │                 │     │                 │
│  ┌───────────┐  │     │  ┌───────────┐  │     │  ┌───────────┐  │
│  │   RxDB    │  │     │  │   RxDB    │  │     │  │   RxDB    │  │
│  │ IndexedDB │  │     │  │ IndexedDB │  │     │  │ IndexedDB │  │
│  └───────────┘  │     │  └───────────┘  │     │  └───────────┘  │
└────────┬────────┘     └────────┬────────┘     └────────┬────────┘
         │                       │                       │
         │  Sync API (HTTP)      │  Sync API (HTTP)      │
         │                       │                       │
         └───────────────────────┴───────────────────────┘
                                 │
                         ┌───────▼────────┐
                         │  ASP.NET Core  │
                         │    Web API     │
                         │                │
                         │  ┌──────────┐  │
                         │  │  EF Core │  │
                         │  └─────┬────┘  │
                         └────────┼───────┘
                                  │
                         ┌────────▼────────┐
                         │   SQL Server    │
                         │  Central DB     │
                         └─────────────────┘
```

## Prerequisites

- .NET 8 SDK
- Node.js 18+ and npm
- Docker and Docker Compose (for SQL Server)
- Modern browser (Chrome, Safari on iPad)

## Setup Instructions

### 1. Clone the Repository

```bash
git clone <repository-url>
cd demo-webapp-01
```

### 2. Start SQL Server

Using Docker Compose:

```bash
docker-compose up -d
```

Wait for SQL Server to be ready (check with `docker-compose logs -f sqlserver`).

### 3. Run the Backend API

```bash
cd OfflineSync.Api
dotnet restore
dotnet run
```

The API will start at `http://localhost:5000` and `https://localhost:5001`.

The database will be automatically created and migrated on first run.

### 4. Run the Frontend Application

```bash
cd OfflineSync.Client
npm install
npm start
```

The Angular app will start at `http://localhost:4200`.

### 5. Create an Agent

Before using the application, you need to create an agent via the API:

```bash
curl -X POST http://localhost:5000/api/agent \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Agent 1",
    "email": "agent1@example.com"
  }'
```

The response will include an `id` (GUID) that you'll use to initialize the client app.

### 6. Initialize the Client

1. Open `http://localhost:4200` in your browser
2. Enter the Agent ID from step 5
3. Click "Initialize"
4. The app will initialize the local RxDB database and start syncing

## Usage

### Creating Records

1. Fill in the "Create New Record" form with:
   - Title (required)
   - Description (optional)
   - Data (optional)
2. Click "Create Record"
3. The record is saved locally and automatically synced to the server

### Viewing Records

- All records are displayed in the "Records" section
- Records are automatically updated when synced from other devices
- Record count is shown in the section header

### Deleting Records

1. Click the "Delete" button on any record
2. Confirm the deletion
3. The record is marked as deleted and synced across all devices

### Offline Mode

- The app works fully offline
- A status badge shows "Online" or "Offline"
- Changes made offline are queued and synced when connection is restored
- Data persists in the browser's IndexedDB

### Synchronization

- **Automatic Sync**: Every 30 seconds when online
- **Manual Sync**: Click "Sync Now" button
- **On Reconnect**: Automatic sync when coming back online
- Last sync time is displayed in the header

### Multi-Device Support

- Use the same Agent ID on multiple devices
- Each device maintains its own local database
- Changes are synchronized bidirectionally
- Conflicts are resolved using version numbers

## API Endpoints

### Agent Management

- `GET /api/agent` - List all agents
- `GET /api/agent/{id}` - Get agent by ID
- `POST /api/agent` - Create new agent
- `PUT /api/agent/{id}` - Update agent
- `DELETE /api/agent/{id}` - Soft delete agent

### Synchronization

- `POST /api/sync/sync` - Perform sync operation
- `GET /api/sync/agent/{agentId}/status` - Get sync status

### File Management

- `POST /api/file/upload` - Upload file
- `GET /api/file/download/{fileId}` - Download file
- `GET /api/file/agent/{agentId}` - List agent files

## Configuration

### Backend (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=OfflineSyncDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true"
  },
  "FileStorage": {
    "Path": "uploads"
  }
}
```

### Frontend (sync.service.ts)

```typescript
private apiUrl = 'http://localhost:5000/api/sync';
```

Change this to your production API URL when deploying.

## Database Schema

### Tables

- **Agents**: User/agent information
- **DataRecords**: Main data records with versioning
- **FileAttachments**: File metadata and references
- **SyncMetadata**: Sync tracking per device

### Key Features

- **Version Tracking**: Every record has a version number for conflict resolution
- **Soft Deletes**: Records are marked as deleted, not physically removed
- **Indexes**: Optimized for sync queries (AgentId + UpdatedAt)
- **Foreign Keys**: Maintain referential integrity

## Deployment

### Backend

1. Publish the .NET application:
   ```bash
   dotnet publish -c Release
   ```

2. Configure production database connection string

3. Deploy to your hosting service (Azure, AWS, etc.)

### Frontend

1. Build the Angular app:
   ```bash
   npm run build
   ```

2. Deploy the `dist/` folder to a web server or CDN

3. Update API URL in the build configuration

### SQL Server

- Use Azure SQL Database, AWS RDS, or on-premises SQL Server
- Ensure network connectivity from application servers
- Configure backup and monitoring

## Testing

### Backend Tests

```bash
cd OfflineSync.Api
dotnet test
```

### Frontend Tests

```bash
cd OfflineSync.Client
npm test
```

## Troubleshooting

### SQL Server Connection Issues

- Verify SQL Server is running: `docker-compose ps`
- Check connection string in appsettings.json
- Ensure firewall allows port 1433

### RxDB Initialization Errors

- Clear browser storage and reload
- Check browser console for specific errors
- Verify IndexedDB is enabled in browser

### Sync Failures

- Check API logs for errors
- Verify agent ID is valid
- Ensure network connectivity
- Check CORS configuration

## Performance Considerations

- **Batch Size**: Adjust sync batch sizes for large datasets
- **Sync Frequency**: Balance between freshness and battery/bandwidth
- **Indexing**: Ensure database indexes are optimized
- **File Size**: Consider chunked uploads for large files

## Security Considerations

⚠️ **Important**: This application is configured for development. Before deploying to production, make the following security changes:

### Critical Security Updates Required

1. **CORS Configuration** (OfflineSync.Api/Program.cs)
   - Replace `AllowAnyOrigin()` with specific allowed origins
   ```csharp
   builder.Services.AddCors(options =>
   {
       options.AddPolicy("AllowAngular",
           builder => builder
               .WithOrigins("https://your-production-domain.com")
               .AllowAnyMethod()
               .AllowAnyHeader());
   });
   ```

2. **Database Password** (docker-compose.yml & appsettings.json)
   - Use environment variables instead of hardcoded passwords
   - Store passwords in Azure Key Vault, AWS Secrets Manager, or similar
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=OfflineSyncDb;User Id=sa;Password=${DB_PASSWORD};TrustServerCertificate=true"
   }
   ```

3. **API URL Configuration** (OfflineSync.Client/src/environments/environment.prod.ts)
   - Update the production API URL
   ```typescript
   export const environment = {
     production: true,
     apiUrl: 'https://api.your-domain.com/api'
   };
   ```

4. **Authentication**: Add JWT bearer authentication for production
5. **Authorization**: Implement agent-specific data access controls
6. **HTTPS**: Use HTTPS in production (required for PWA features)
7. **SQL Injection**: Entity Framework provides protection
8. **Input Validation**: Validate all inputs on server and client
9. **File Upload Limits**: Configure appropriate file size limits
10. **Rate Limiting**: Implement rate limiting on API endpoints

## Future Enhancements

- [ ] JWT authentication
- [ ] Real-time sync with SignalR
- [ ] Conflict resolution UI
- [ ] File chunking for large files
- [ ] Progressive Web App (PWA) support
- [ ] Service Worker for better offline support
- [ ] Data compression for sync payloads
- [ ] Incremental sync optimization

## License

MIT

## Support

For issues and questions, please create an issue in the repository.