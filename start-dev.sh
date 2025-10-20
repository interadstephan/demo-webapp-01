#!/bin/bash

echo "🚀 Starting Offline Sync Application..."
echo ""

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker is not running. Please start Docker and try again."
    exit 1
fi

# Start SQL Server
echo "📦 Starting SQL Server..."
docker-compose up -d

# Wait for SQL Server to be ready
echo "⏳ Waiting for SQL Server to be ready..."
sleep 10

# Start backend
echo "🔧 Starting ASP.NET Core API..."
cd OfflineSync.Api
dotnet run &
BACKEND_PID=$!
cd ..

# Wait for backend to start
sleep 5

# Start frontend
echo "🎨 Starting Angular frontend..."
cd OfflineSync.Client
npm start &
FRONTEND_PID=$!
cd ..

echo ""
echo "✅ Application started!"
echo "   Backend API: http://localhost:5000"
echo "   Frontend UI: http://localhost:4200"
echo ""
echo "Press Ctrl+C to stop all services"

# Wait for Ctrl+C
trap "echo ''; echo '🛑 Stopping services...'; docker-compose down; kill $BACKEND_PID $FRONTEND_PID 2>/dev/null; exit" INT
wait
