# Quick Start Guide

Get your Offline Sync Web App up and running in 5 minutes!

## Prerequisites Check

Before starting, ensure you have:

- ✅ [Docker Desktop](https://www.docker.com/products/docker-desktop) installed and running
- ✅ [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) installed
- ✅ [Node.js 18+](https://nodejs.org/) and npm installed

Quick check:
```bash
docker --version
dotnet --version
node --version
npm --version
```

## Option 1: Automated Start (Recommended)

### On Linux/Mac:
```bash
./start-dev.sh
```

### On Windows:
```batch
start-dev.bat
```

This will:
1. Start SQL Server in Docker
2. Start the ASP.NET Core API on port 5000
3. Start the Angular app on port 4200

## Option 2: Manual Start

### Step 1: Start SQL Server
```bash
docker-compose up -d
```

Wait 10 seconds for SQL Server to initialize.

### Step 2: Start Backend API
```bash
cd OfflineSync.Api
dotnet run
```

The API will be available at `http://localhost:5000`

### Step 3: Start Frontend (in a new terminal)
```bash
cd OfflineSync.Client
npm install  # Only needed first time
npm start
```

The app will be available at `http://localhost:4200`

## First Time Setup

### 1. Create an Agent

Using curl:
```bash
curl -X POST http://localhost:5000/api/agent \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Test Agent",
    "email": "test@example.com"
  }'
```

Or using PowerShell:
```powershell
Invoke-RestMethod -Uri http://localhost:5000/api/agent `
  -Method Post `
  -ContentType "application/json" `
  -Body '{"name":"Test Agent","email":"test@example.com"}'
```

**Save the returned `id` - you'll need it!**

### 2. Open the App

1. Navigate to `http://localhost:4200`
2. Enter your Agent ID (the `id` from step 1)
3. Click "Initialize"

### 3. Create Your First Record

1. Fill in the "Create New Record" form
2. Click "Create Record"
3. Watch it appear in the records list below!

### 4. Test Offline Functionality

1. Open DevTools (F12)
2. Go to Network tab
3. Check "Offline" mode
4. Create more records - they still work!
5. Uncheck "Offline" to go back online
6. Click "Sync Now" to sync your offline changes

## Testing Multi-Device Sync

### Same Device, Different Browser Tab:
1. Open `http://localhost:4200` in a second tab
2. Use the **same Agent ID**
3. Create records in either tab
4. Click "Sync Now" in both tabs
5. See records appear in both! 🎉

### Different Device:
1. Open the app on another device (iPad, phone, another computer)
2. Make sure both devices can reach your computer's IP (e.g., `http://192.168.1.100:4200`)
3. Use the same Agent ID
4. Watch data sync between devices!

## Troubleshooting

### SQL Server won't start
```bash
# Check logs
docker-compose logs sqlserver

# Restart
docker-compose down
docker-compose up -d
```

### Backend API errors
```bash
# Check if port 5000 is already in use
netstat -an | grep 5000  # Linux/Mac
netstat -an | findstr 5000  # Windows

# Try a different port by editing OfflineSync.Api/Properties/launchSettings.json
```

### Frontend errors
```bash
# Clear node_modules and reinstall
cd OfflineSync.Client
rm -rf node_modules package-lock.json
npm install
```

### Database connection issues
Check `OfflineSync.Api/appsettings.json` and ensure the SQL Server password matches the one in `docker-compose.yml` (default: `YourStrong@Passw0rd`)

## Next Steps

- 📖 Read the full [README.md](README.md) for architecture details
- 🔒 Review [Security Considerations](README.md#security-considerations) before production
- 🧪 Try the API examples in [api-examples.http](api-examples.http)
- 📱 Install the PWA on your device (Chrome menu → Install app)

## Stopping the Application

### Automated scripts:
- **Linux/Mac**: Press Ctrl+C in the terminal running start-dev.sh
- **Windows**: Press any key in the command prompt running start-dev.bat

### Manual:
```bash
# Stop backend and frontend (Ctrl+C in each terminal)
# Stop SQL Server
docker-compose down
```

## Quick Reference

| Service | URL | Description |
|---------|-----|-------------|
| Frontend | http://localhost:4200 | Angular app |
| Backend API | http://localhost:5000 | REST API |
| Swagger | http://localhost:5000/swagger | API documentation |
| SQL Server | localhost:1433 | Database (sa/YourStrong@Passw0rd) |

---

Need help? Check the [README.md](README.md) or create an issue!
