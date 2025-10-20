# Architecture Overview

## System Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     CLIENT DEVICES                          │
│  (Desktop Chrome, iPad Safari, Installable PWA)            │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌──────────────────────────────────────────────────────┐  │
│  │             Angular Application                       │  │
│  │  ┌────────────────────────────────────────────────┐  │  │
│  │  │  Components (UI)                               │  │  │
│  │  │  - Agent initialization                        │  │  │
│  │  │  - Record CRUD forms                          │  │  │
│  │  │  - Records list                               │  │  │
│  │  │  - Sync status display                        │  │  │
│  │  └────────────────────────────────────────────────┘  │  │
│  │                      ↕                                │  │
│  │  ┌────────────────────────────────────────────────┐  │  │
│  │  │  Services (Business Logic)                     │  │  │
│  │  │  - DatabaseService  (RxDB management)         │  │  │
│  │  │  - SyncService     (Synchronization)          │  │  │
│  │  │  - DataService     (CRUD operations)          │  │  │
│  │  └────────────────────────────────────────────────┘  │  │
│  │                      ↕                                │  │
│  │  ┌────────────────────────────────────────────────┐  │  │
│  │  │  RxDB (Offline Database)                      │  │  │
│  │  │  - Collections: datarecords, fileattachments  │  │  │
│  │  │  - Storage: IndexedDB (browser)               │  │  │
│  │  │  - Reactive queries (Observable)              │  │  │
│  │  └────────────────────────────────────────────────┘  │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                             │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Service Worker (PWA)                                │  │
│  │  - Asset caching                                     │  │
│  │  - Offline page support                             │  │
│  │  - Background sync                                  │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                            │
                            │ HTTP/REST API
                            │ (Online only)
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                     SERVER LAYER                            │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌──────────────────────────────────────────────────────┐  │
│  │       ASP.NET Core 8 Web API                         │  │
│  │                                                       │  │
│  │  ┌────────────────────────────────────────────────┐  │  │
│  │  │  Controllers                                   │  │  │
│  │  │  - AgentController  (Agent management)        │  │  │
│  │  │  - SyncController   (Synchronization logic)   │  │  │
│  │  │  - FileController   (File operations)         │  │  │
│  │  └────────────────────────────────────────────────┘  │  │
│  │                      ↕                                │  │
│  │  ┌────────────────────────────────────────────────┐  │  │
│  │  │  DTOs (Data Transfer Objects)                 │  │  │
│  │  │  - SyncRequest, SyncResponse                  │  │  │
│  │  │  - DataRecordDto, FileAttachmentDto          │  │  │
│  │  └────────────────────────────────────────────────┘  │  │
│  │                      ↕                                │  │
│  │  ┌────────────────────────────────────────────────┐  │  │
│  │  │  Entity Framework Core (ORM)                  │  │  │
│  │  │  - AppDbContext                               │  │  │
│  │  │  - Migrations                                 │  │  │
│  │  │  - Change tracking                            │  │  │
│  │  └────────────────────────────────────────────────┘  │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                             │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Models (Domain Entities)                            │  │
│  │  - Agent                                             │  │
│  │  - DataRecord                                        │  │
│  │  - FileAttachment                                    │  │
│  │  - SyncMetadata                                      │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                            │
                            │ SQL Connection
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                     DATABASE LAYER                          │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Microsoft SQL Server 2022                           │  │
│  │  (LocalDB or Docker container)                       │  │
│  │                                                       │  │
│  │  Tables:                                             │  │
│  │  ┌────────────────────────────────────────────────┐  │  │
│  │  │  Agents                                        │  │  │
│  │  │  - Id, Name, Email, IsActive, Timestamps      │  │  │
│  │  └────────────────────────────────────────────────┘  │  │
│  │  ┌────────────────────────────────────────────────┐  │  │
│  │  │  DataRecords                                   │  │  │
│  │  │  - Id, AgentId, Title, Description, Data      │  │  │
│  │  │  - Version, IsDeleted, UpdatedAt              │  │  │
│  │  └────────────────────────────────────────────────┘  │  │
│  │  ┌────────────────────────────────────────────────┐  │  │
│  │  │  FileAttachments                               │  │  │
│  │  │  - Id, AgentId, FileName, BlobPath            │  │  │
│  │  │  - Version, IsDeleted, UpdatedAt              │  │  │
│  │  └────────────────────────────────────────────────┘  │  │
│  │  ┌────────────────────────────────────────────────┐  │  │
│  │  │  SyncMetadata                                  │  │  │
│  │  │  - Id, AgentId, DeviceId, LastSyncVersion     │  │  │
│  │  └────────────────────────────────────────────────┘  │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

## Data Flow Diagrams

### 1. Offline Record Creation

```
User Action                Client (Offline)              Server
    │                            │                         │
    ├─── Create Record ─────────►│                         │
    │                            ├─ Save to IndexedDB      │
    │                            ├─ Generate ID & Version  │
    │◄─── Show Success ──────────┤                         │
    │                            │                         │
    │                            │  (Record stored locally)│
```

### 2. Synchronization Process

```
Client                                    Server
  │                                         │
  ├─── Check Online Status ──────►         │
  │                                         │
  ├─── Prepare Sync Request ──────►        │
  │    - lastSyncVersion: 1000             │
  │    - pushedRecords: [Record A, B]      │
  │    - pushedFiles: []                   │
  │                                         │
  │                                         ├─ Validate Agent
  │                                         ├─ Process Pushed Records
  │                                         │  - Compare versions
  │                                         │  - Insert or update
  │                                         │  - Resolve conflicts
  │                                         │
  │                                         ├─ Query Server Updates
  │                                         │  WHERE AgentId = X
  │                                         │    AND Version > 1000
  │                                         │
  │                                         ├─ Update SyncMetadata
  │                                         │
  │◄─── Sync Response ──────────────────────┤
  │    - currentVersion: 2000               │
  │    - updatedRecords: [Record C]         │
  │    - updatedFiles: []                   │
  │                                         │
  ├─ Apply Server Updates                  │
  │  - Upsert to IndexedDB                  │
  │  - Update local version                 │
  │                                         │
  ├─ Update lastSyncVersion = 2000         │
  │                                         │
  │◄─── Sync Complete ──────────────────────┤
```

### 3. Multi-Device Scenario

```
Device A (Desktop)     Server          Device B (iPad)
      │                  │                   │
      ├─ Create Rec1 ───►│                   │
      │  (offline)        │                   │
      │                   │                   │
      │                   │◄─── Sync ────────┤
      │                   ├─ Send: nothing    │
      │                   ├─ Receive: Rec1 ──►│
      │                   │                   ├─ Apply Rec1
      │                   │                   │
      ├─── Sync ─────────►│                   │
      ├─ Push: Rec1       │                   │
      │◄─ No updates ─────┤                   │
      │                   │                   │
      │                   │                   ├─ Update Rec1
      │                   │◄─── Sync ────────┤
      │                   ├─ Push: Rec1 v2    │
      │                   ├─ Send: OK ───────►│
      │                   │                   │
      ├─── Sync ─────────►│                   │
      │◄─ Receive: Rec1v2─┤                   │
      ├─ Apply update     │                   │
```

## Component Interactions

### Frontend Component Flow

```
┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│   App.ts     │────►│ DataService  │────►│DatabaseService│
│  (UI Layer)  │     │(Business Logic)    │(RxDB Manager) │
└──────────────┘     └──────────────┘     └──────────────┘
       │                    │                     │
       │                    │                     ↓
       │                    │              ┌──────────────┐
       │                    │              │   RxDB       │
       │                    │              │ (IndexedDB)  │
       │                    │              └──────────────┘
       │                    ↓
       │             ┌──────────────┐
       │             │ SyncService  │
       │             │(Sync Logic)  │
       │             └──────────────┘
       │                    │
       │                    ↓
       │             ┌──────────────┐
       │             │  HttpClient  │
       │             │  (REST API)  │
       │             └──────────────┘
       │                    │
       └────────────────────┴──────────────► Server API
```

### Backend Component Flow

```
HTTP Request
     │
     ↓
┌─────────────────┐
│  Controllers    │
│ - Route         │
│ - Validate      │
└─────────────────┘
     │
     ↓
┌─────────────────┐
│  Business Logic │
│ - Process sync  │
│ - Apply changes │
└─────────────────┘
     │
     ↓
┌─────────────────┐
│ EF Core Context │
│ - Track changes │
│ - Generate SQL  │
└─────────────────┘
     │
     ↓
┌─────────────────┐
│  SQL Server DB  │
│ - Store data    │
│ - Execute query │
└─────────────────┘
     │
     ↓
HTTP Response
```

## Technology Stack Details

### Client-Side Technologies

| Technology | Version | Purpose |
|------------|---------|---------|
| Angular | 19.x | Frontend framework |
| TypeScript | 5.x | Type-safe JavaScript |
| RxDB | 16.9.3 | Offline database |
| RxJS | 7.x | Reactive programming |
| IndexedDB | Browser native | Persistent storage |
| Service Worker | PWA | Offline support |
| SCSS | Latest | Styling |

### Server-Side Technologies

| Technology | Version | Purpose |
|------------|---------|---------|
| ASP.NET Core | 8.0 | Web API framework |
| C# | 12.0 | Programming language |
| Entity Framework Core | 8.0 | ORM |
| SQL Server | 2022/LocalDB | Database (LocalDB for Windows dev, Docker for production/Linux/Mac) |
| Docker | Latest | Container platform (production & non-Windows) |

## Security Architecture

```
┌──────────────────────────────────────────────────────────┐
│                    Security Layers                       │
├──────────────────────────────────────────────────────────┤
│                                                          │
│  ┌────────────────────────────────────────────────────┐ │
│  │  Transport Layer (HTTPS)                           │ │
│  │  - TLS 1.3 encryption                              │ │
│  │  - Certificate validation                          │ │
│  └────────────────────────────────────────────────────┘ │
│                         ↓                                │
│  ┌────────────────────────────────────────────────────┐ │
│  │  Authentication (TODO: Production)                 │ │
│  │  - JWT Bearer tokens                               │ │
│  │  - Agent identity verification                     │ │
│  └────────────────────────────────────────────────────┘ │
│                         ↓                                │
│  ┌────────────────────────────────────────────────────┐ │
│  │  Authorization                                     │ │
│  │  - Agent-specific data filtering                  │ │
│  │  - Query WHERE AgentId = current                  │ │
│  └────────────────────────────────────────────────────┘ │
│                         ↓                                │
│  ┌────────────────────────────────────────────────────┐ │
│  │  Input Validation                                  │ │
│  │  - Model validation attributes                     │ │
│  │  - SQL injection prevention (EF Core)              │ │
│  └────────────────────────────────────────────────────┘ │
│                         ↓                                │
│  ┌────────────────────────────────────────────────────┐ │
│  │  Data Layer                                        │ │
│  │  - Parameterized queries                           │ │
│  │  - Soft deletes (audit trail)                      │ │
│  └────────────────────────────────────────────────────┘ │
└──────────────────────────────────────────────────────────┘
```

## Deployment Architecture

```
┌────────────────────────────────────────────────────────────┐
│                    Production Setup                        │
├────────────────────────────────────────────────────────────┤
│                                                            │
│  ┌──────────────────────────────────────────────────────┐ │
│  │  CDN / Static Hosting                                │ │
│  │  - Angular build artifacts                           │ │
│  │  - Service worker                                    │ │
│  │  - PWA assets                                        │ │
│  └──────────────────────────────────────────────────────┘ │
│                         │                                  │
│                         │ (API calls)                      │
│                         ↓                                  │
│  ┌──────────────────────────────────────────────────────┐ │
│  │  Load Balancer / API Gateway                         │ │
│  │  - SSL termination                                   │ │
│  │  - Rate limiting                                     │ │
│  │  - API routing                                       │ │
│  └──────────────────────────────────────────────────────┘ │
│                         │                                  │
│                         ↓                                  │
│  ┌──────────────────────────────────────────────────────┐ │
│  │  App Service / Container Instances                   │ │
│  │  - ASP.NET Core API (multiple instances)             │ │
│  │  - Auto-scaling                                      │ │
│  │  - Health monitoring                                 │ │
│  └──────────────────────────────────────────────────────┘ │
│                         │                                  │
│                         ↓                                  │
│  ┌──────────────────────────────────────────────────────┐ │
│  │  Database Cluster                                    │ │
│  │  - SQL Server (primary + replicas)                  │ │
│  │  - Automated backups                                 │ │
│  │  - Point-in-time recovery                           │ │
│  └──────────────────────────────────────────────────────┘ │
│                                                            │
│  ┌──────────────────────────────────────────────────────┐ │
│  │  Monitoring & Logging                                │ │
│  │  - Application Insights                              │ │
│  │  - Log Analytics                                     │ │
│  │  - Alerts & dashboards                              │ │
│  └──────────────────────────────────────────────────────┘ │
└────────────────────────────────────────────────────────────┘
```

## Scalability Considerations

### Horizontal Scaling
- **API Servers**: Stateless design allows multiple instances
- **Database**: Read replicas for query load distribution
- **CDN**: Static assets served from edge locations

### Vertical Scaling
- **Database**: Increase CPU/RAM for complex queries
- **API**: Increase resources for compute-heavy operations

### Data Partitioning
- Agent-based partitioning (each agent is independent)
- Time-based archival for old data
- File storage on separate blob storage

---

For more details, see:
- [README.md](README.md) - Full documentation
- [QUICKSTART.md](QUICKSTART.md) - Getting started
- [IMPLEMENTATION_SUMMARY.md](IMPLEMENTATION_SUMMARY.md) - Technical details
