@echo off
echo Creating SACCO Analytics Platform structure...

REM Create main directories
mkdir backend\src\SaccoAnalytics.API
mkdir backend\src\SaccoAnalytics.Core
mkdir backend\src\SaccoAnalytics.Application  
mkdir backend\src\SaccoAnalytics.Infrastructure
mkdir backend\tests\SaccoAnalytics.UnitTests
mkdir backend\tests\SaccoAnalytics.IntegrationTests
mkdir backend\scripts

mkdir frontend\src
mkdir frontend\public

mkdir docs\architecture
mkdir docs\api
mkdir docs\deployment

mkdir infrastructure\docker
mkdir infrastructure\terraform
mkdir infrastructure\kubernetes

mkdir database\migrations
mkdir database\seeds
mkdir database\scripts

mkdir scripts\development
mkdir scripts\deployment

echo Creating essential files...

REM Create .gitignore
(
echo # .NET
echo bin/
echo obj/
echo out/
echo *.user
echo *.userosscache
echo *.sln.docstates
echo .vscode/
echo .vs/
echo *.log
echo appsettings.Development.json
echo appsettings.Local.json
echo.
echo # React/Node
echo node_modules/
echo npm-debug.log*
echo yarn-debug.log*
echo yarn-error.log*
echo .env.local
echo .env.development.local
echo .env.test.local
echo .env.production.local
echo build/
echo dist/
echo.
echo # Database
echo *.db
echo *.sqlite
echo *.sqlite3
echo.
echo # Secrets ^& Keys
echo *.pem
echo *.key
echo *.p12
echo *.pfx
echo secrets/
echo .env
echo .env.local
echo.
echo # OS
echo .DS_Store
echo Thumbs.db
echo *.swp
echo *.swo
echo.
echo # IDE
echo .idea/
echo *.suo
echo *.ntvs*
echo *.njsproj*
echo *.sln.docstates
echo.
echo # Logs
echo logs/
echo *.log
) > .gitignore

REM Create docker-compose.yml
(
echo version: '3.8'
echo.
echo services:
echo   database:
echo     image: postgres:15-alpine
echo     environment:
echo       POSTGRES_DB: sacco_analytics
echo       POSTGRES_USER: dev_user
echo       POSTGRES_PASSWORD: dev_password
echo     ports:
echo       - "5433:5432"
echo     volumes:
echo       - postgres_data:/var/lib/postgresql/data
echo       - ./database/init:/docker-entrypoint-initdb.d
echo     networks:
echo       - sacco_network
echo.
echo   redis:
echo     image: redis:7-alpine
echo     ports:
echo       - "6379:6379"
echo     volumes:
echo       - redis_data:/data
echo     networks:
echo       - sacco_network
echo.
echo volumes:
echo   postgres_data:
echo   redis_data:
echo.
echo networks:
echo   sacco_network:
echo     driver: bridge
) > docker-compose.yml

REM Create README.md content
(
echo # SACCO Analytics Platform
echo.
echo A secure, multi-tenant SaaS platform for Savings and Credit Cooperative Organizations ^(SACCOs^) to manage financial reporting and analytics.
echo.
echo ## 🎯 Vision
echo Empower SACCOs with modern, AI-driven financial insights while maintaining the highest standards of data security and regulatory compliance.
echo.
echo ## 🏗️ Architecture
echo - **Backend**: C# .NET 8 Web API
echo - **Frontend**: React 18 + TypeScript + Vite  
echo - **Database**: PostgreSQL with Row-Level Security
echo - **Authentication**: JWT + Refresh Tokens
echo - **Infrastructure**: Docker + Azure/AWS
echo.
echo ## 🚀 Quick Start
echo ```bash
echo # Start infrastructure
echo docker-compose up -d
echo.
echo # Backend
echo cd backend
echo dotnet restore
echo dotnet run --project src/SaccoAnalytics.API
echo.
echo # Frontend
echo cd frontend
echo npm install  
echo npm run dev
echo ```
echo.
echo ## 📊 Current Status
echo 🟡 **Phase 1**: Foundation ^& Architecture ^(In Progress^)
echo - [x] Repository structure
echo - [ ] Backend API foundation
echo - [ ] Frontend foundation
echo - [ ] Database schema
echo - [ ] Authentication system
echo.
echo ## 🔧 Development Requirements
echo - .NET 8 SDK
echo - Node.js 18+
echo - Docker ^& Docker Compose
echo - PostgreSQL ^(via Docker^)
echo - VS Code with C# extension
echo.
echo ## 📖 Documentation  
echo - [Architecture Overview]^(docs/architecture/^)
echo - [API Documentation]^(docs/api/^)
echo - [Deployment Guide]^(docs/deployment/^)
echo.
echo ## 🤝 Contributing
echo See [CONTRIBUTING.md]^(CONTRIBUTING.md^)
echo.
echo ## 📄 License
echo MIT License - See [LICENSE]^(LICENSE^)
) > README.md

echo.
echo ✅ Project structure created successfully!
echo ✅ .gitignore created
echo ✅ docker-compose.yml created  
echo ✅ README.md updated
echo.
echo Next steps:
echo 1. Start Docker containers: docker-compose up -d
echo 2. Setup .NET solution: cd backend ^&^& [run .NET setup commands]
echo 3. Setup React frontend: cd frontend ^&^& [run React setup commands]
echo.
pause