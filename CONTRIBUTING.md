# Contributing to Offline Sync Web App

Thank you for your interest in contributing! This guide will help you get started with development.

## Development Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd demo-webapp-01
   ```

2. **Install prerequisites**
   - .NET 8 SDK
   - Node.js 18+
   - Docker Desktop

3. **Start development environment**
   ```bash
   ./start-dev.sh  # or start-dev.bat on Windows
   ```

## Project Structure

```
demo-webapp-01/
├── OfflineSync.Api/              # ASP.NET Core 8 Backend
│   ├── Controllers/              # API controllers
│   ├── Data/                     # DbContext and migrations
│   ├── DTOs/                     # Data transfer objects
│   ├── Models/                   # Entity models
│   └── Program.cs                # Application entry point
│
├── OfflineSync.Client/           # Angular Frontend
│   ├── src/
│   │   ├── app/
│   │   │   ├── services/        # Angular services
│   │   │   ├── app.ts           # Root component
│   │   │   └── app.html         # Root template
│   │   └── environments/        # Environment configs
│   └── public/                   # Static assets
│
├── docker-compose.yml            # SQL Server configuration
└── README.md                     # Main documentation
```

## Coding Standards

### Backend (C#)

- Follow [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use async/await for database operations
- Add XML documentation comments to public APIs
- Keep controllers thin - move business logic to services

Example:
```csharp
/// <summary>
/// Synchronizes data between client and server
/// </summary>
/// <param name="request">Sync request containing pushed changes</param>
/// <returns>Sync response with server updates</returns>
[HttpPost("sync")]
public async Task<ActionResult<SyncResponse>> Sync([FromBody] SyncRequest request)
{
    // Implementation
}
```

### Frontend (TypeScript/Angular)

- Follow [Angular Style Guide](https://angular.io/guide/styleguide)
- Use TypeScript strict mode
- Components should be focused and single-purpose
- Services should be stateless when possible

Example:
```typescript
@Injectable({
  providedIn: 'root'
})
export class DataService {
  async createRecord(title: string, description: string): Promise<void> {
    // Implementation
  }
}
```

## Adding New Features

### Adding a New Entity

1. **Create the model** (OfflineSync.Api/Models/)
   ```csharp
   public class NewEntity
   {
       public Guid Id { get; set; }
       public string Name { get; set; } = string.Empty;
       public DateTime CreatedAt { get; set; }
   }
   ```

2. **Add to DbContext** (OfflineSync.Api/Data/AppDbContext.cs)
   ```csharp
   public DbSet<NewEntity> NewEntities { get; set; }
   ```

3. **Create migration**
   ```bash
   cd OfflineSync.Api
   dotnet ef migrations add AddNewEntity
   ```

4. **Add controller** (OfflineSync.Api/Controllers/)
   ```csharp
   [ApiController]
   [Route("api/[controller]")]
   public class NewEntityController : ControllerBase
   {
       // CRUD operations
   }
   ```

5. **Add RxDB schema** (OfflineSync.Client/src/app/services/database.service.ts)
   ```typescript
   export interface NewEntityDocument {
     id: string;
     name: string;
     createdAt: string;
   }
   ```

6. **Add to database collections**
   ```typescript
   newentities: {
     schema: {
       version: 0,
       primaryKey: 'id',
       type: 'object',
       properties: {
         id: { type: 'string', maxLength: 100 },
         name: { type: 'string' },
         createdAt: { type: 'string', format: 'date-time' }
       }
     }
   }
   ```

### Adding a New API Endpoint

1. Add method to appropriate controller
2. Include XML documentation
3. Test with api-examples.http
4. Update Swagger annotations if needed

### Adding a New Frontend Component

```bash
cd OfflineSync.Client
ng generate component components/my-component
```

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

### E2E Tests

```bash
cd OfflineSync.Client
npm run e2e
```

## Database Migrations

### Creating a Migration

```bash
cd OfflineSync.Api
dotnet ef migrations add MigrationName
```

### Applying Migrations

Migrations are automatically applied on application startup. To manually apply:

```bash
dotnet ef database update
```

### Rolling Back

```bash
dotnet ef database update PreviousMigrationName
```

## Debugging

### Backend (VS Code)

1. Open OfflineSync.Api folder
2. Press F5 or use Debug panel
3. Set breakpoints in .cs files

### Frontend (Chrome DevTools)

1. Open http://localhost:4200
2. Press F12
3. Go to Sources tab
4. Find TypeScript files and set breakpoints

### SQL Server

Connect with:
- Server: localhost,1433
- User: sa
- Password: YourStrong@Passw0rd

Tools:
- Azure Data Studio
- SQL Server Management Studio
- VS Code with SQL Server extension

## Pull Request Process

1. **Fork the repository**

2. **Create a feature branch**
   ```bash
   git checkout -b feature/my-new-feature
   ```

3. **Make your changes**
   - Write clean, documented code
   - Follow coding standards
   - Add tests for new functionality

4. **Test your changes**
   ```bash
   # Backend
   cd OfflineSync.Api && dotnet build && dotnet test
   
   # Frontend
   cd OfflineSync.Client && npm run build && npm test
   ```

5. **Commit with clear messages**
   ```bash
   git commit -m "feat: add new sync filtering capability"
   ```

   Use conventional commits:
   - `feat:` new feature
   - `fix:` bug fix
   - `docs:` documentation
   - `style:` formatting
   - `refactor:` code restructuring
   - `test:` adding tests
   - `chore:` maintenance

6. **Push and create PR**
   ```bash
   git push origin feature/my-new-feature
   ```

7. **Describe your changes**
   - What problem does it solve?
   - How did you implement it?
   - Any breaking changes?
   - Screenshots for UI changes

## Common Tasks

### Update Dependencies

Backend:
```bash
cd OfflineSync.Api
dotnet list package --outdated
dotnet add package PackageName --version x.y.z
```

Frontend:
```bash
cd OfflineSync.Client
npm outdated
npm update
```

### Generate API Client

```bash
# From Swagger/OpenAPI spec
cd OfflineSync.Client
npx @openapitools/openapi-generator-cli generate \
  -i http://localhost:5000/swagger/v1/swagger.json \
  -g typescript-angular \
  -o src/app/api
```

### Performance Profiling

Backend:
```bash
dotnet trace collect -- dotnet run
```

Frontend:
```bash
# Chrome DevTools → Performance tab → Record
```

## Code Review Checklist

- [ ] Code follows project style guidelines
- [ ] All tests pass
- [ ] New tests added for new features
- [ ] Documentation updated
- [ ] No console.log or commented code
- [ ] Error handling implemented
- [ ] Security considerations addressed
- [ ] Performance impact considered

## Getting Help

- 📖 Read the [README.md](README.md)
- 🚀 Check the [QUICKSTART.md](QUICKSTART.md)
- 🐛 Search existing issues
- 💬 Ask questions in discussions
- 📧 Contact maintainers

## License

By contributing, you agree that your contributions will be licensed under the same license as the project (MIT).
