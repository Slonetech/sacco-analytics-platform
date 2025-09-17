# SACCO Analytics Platform - Complete Setup Script
# Run this in PowerShell as Administrator in your project root

Write-Host "🚀 Creating SACCO Analytics Platform Structure..." -ForegroundColor Green

# Create all directories
$directories = @(
    # Backend structure
    "backend/src/SaccoAnalytics.API/Controllers/v1",
    "backend/src/SaccoAnalytics.API/Middleware",
    "backend/src/SaccoAnalytics.API/Configuration",
    "backend/src/SaccoAnalytics.API/Extensions",
    
    "backend/src/SaccoAnalytics.Core/Entities/Common",
    "backend/src/SaccoAnalytics.Core/Entities/Identity", 
    "backend/src/SaccoAnalytics.Core/Entities/Tenants",
    "backend/src/SaccoAnalytics.Core/Entities/Reports",
    "backend/src/SaccoAnalytics.Core/Entities/Financial",
    "backend/src/SaccoAnalytics.Core/Interfaces/Repositories",
    "backend/src/SaccoAnalytics.Core/Interfaces/Services",
    "backend/src/SaccoAnalytics.Core/DTOs/Auth",
    "backend/src/SaccoAnalytics.Core/DTOs/Tenant",
    "backend/src/SaccoAnalytics.Core/DTOs/User",
    "backend/src/SaccoAnalytics.Core/DTOs/Report",
    "backend/src/SaccoAnalytics.Core/Exceptions",
    "backend/src/SaccoAnalytics.Core/Constants",
    
    "backend/src/SaccoAnalytics.Application/Features/Auth/Commands",
    "backend/src/SaccoAnalytics.Application/Features/Auth/Queries", 
    "backend/src/SaccoAnalytics.Application/Features/Auth/Validators",
    "backend/src/SaccoAnalytics.Application/Features/Tenants/Commands",
    "backend/src/SaccoAnalytics.Application/Features/Tenants/Queries",
    "backend/src/SaccoAnalytics.Application/Features/Tenants/Validators",
    "backend/src/SaccoAnalytics.Application/Features/Reports/Commands",
    "backend/src/SaccoAnalytics.Application/Features/Reports/Queries",
    "backend/src/SaccoAnalytics.Application/Features/Reports/Validators",
    "backend/src/SaccoAnalytics.Application/Common/Behaviors",
    "backend/src/SaccoAnalytics.Application/Common/Mappings",
    "backend/src/SaccoAnalytics.Application/Common/Extensions",
    
    "backend/src/SaccoAnalytics.Infrastructure/Data/Configurations",
    "backend/src/SaccoAnalytics.Infrastructure/Data/Migrations",
    "backend/src/SaccoAnalytics.Infrastructure/Data/Seeds",
    "backend/src/SaccoAnalytics.Infrastructure/Repositories",
    "backend/src/SaccoAnalytics.Infrastructure/Services",
    "backend/src/SaccoAnalytics.Infrastructure/Identity",
    "backend/src/SaccoAnalytics.Infrastructure/Caching",
    "backend/src/SaccoAnalytics.Infrastructure/External",
    "backend/src/SaccoAnalytics.Infrastructure/Logging",
    
    "backend/tests/SaccoAnalytics.UnitTests/Core",
    "backend/tests/SaccoAnalytics.UnitTests/Application", 
    "backend/tests/SaccoAnalytics.UnitTests/Infrastructure",
    "backend/tests/SaccoAnalytics.IntegrationTests/API",
    "backend/tests/SaccoAnalytics.IntegrationTests/Database",
    "backend/tests/SaccoAnalytics.IntegrationTests/TestFixtures",
    "backend/tests/SaccoAnalytics.E2ETests",
    
    # Frontend structure
    "frontend/src/components/common/Layout",
    "frontend/src/components/common/Loading",
    "frontend/src/components/common/ErrorBoundary", 
    "frontend/src/components/common/ProtectedRoute",
    "frontend/src/components/auth/LoginForm",
    "frontend/src/components/auth/RegisterForm",
    "frontend/src/components/auth/PasswordReset",
    "frontend/src/components/dashboard/DashboardLayout",
    "frontend/src/components/dashboard/MetricsCards",
    "frontend/src/components/dashboard/Charts",
    "frontend/src/components/dashboard/RecentActivity",
    "frontend/src/components/reports/ReportBuilder",
    "frontend/src/components/reports/ReportViewer",
    "frontend/src/components/reports/ReportsList",
    "frontend/src/components/reports/ExportOptions",
    "frontend/src/components/admin/TenantManagement",
    "frontend/src/components/admin/UserManagement",
    "frontend/src/components/admin/SystemSettings",
    "frontend/src/components/tenant/TenantSettings",
    "frontend/src/components/tenant/UserInvitation",
    "frontend/src/components/tenant/BrandingConfig",
    
    "frontend/src/hooks/auth",
    "frontend/src/hooks/api", 
    "frontend/src/hooks/common",
    
    "frontend/src/services/api",
    "frontend/src/services/auth",
    "frontend/src/services/utils",
    
    "frontend/src/store/slices",
    "frontend/src/store/middleware",
    
    "frontend/src/types",
    "frontend/src/styles",
    "frontend/src/config",
    "frontend/src/pages/auth",
    "frontend/src/pages/dashboard", 
    "frontend/src/pages/reports",
    "frontend/src/pages/admin",
    "frontend/src/pages/tenant",
    
    "frontend/tests/__mocks__",
    "frontend/tests/components",
    "frontend/tests/hooks",
    "frontend/tests/services",
    "frontend/tests/utils",
    
    # Infrastructure & DevOps
    "infrastructure/docker/backend",
    "infrastructure/docker/frontend", 
    "infrastructure/docker/nginx",
    "infrastructure/terraform/environments/dev",
    "infrastructure/terraform/environments/staging",
    "infrastructure/terraform/environments/production",
    "infrastructure/terraform/modules/networking",
    "infrastructure/terraform/modules/security",
    "infrastructure/terraform/modules/database",
    "infrastructure/terraform/modules/storage",
    "infrastructure/terraform/modules/monitoring",
    "infrastructure/terraform/global",
    "infrastructure/kubernetes/base",
    "infrastructure/kubernetes/overlays",
    "infrastructure/kubernetes/helm-charts",
    
    # Database
    "database/migrations",
    "database/seeds", 
    "database/scripts",
    
    # Documentation
    "docs/architecture",
    "docs/api",
    "docs/deployment",
    "docs/user-guides",
    
    # Monitoring & Security
    "monitoring/grafana/dashboards",
    "monitoring/grafana/datasources", 
    "monitoring/prometheus/rules",
    "monitoring/alerts",
    
    "security/policies",
    "security/certificates",
    "security/secrets",
    
    # Scripts
    "scripts/development",
    "scripts/deployment", 
    "scripts/maintenance",
    
    # CI/CD
    ".github/workflows",
    ".github/ISSUE_TEMPLATE",
    ".github/PULL_REQUEST_TEMPLATE"
)

foreach ($dir in $directories) {
    New-Item -ItemType Directory -Path $dir -Force | Out-Null
    Write-Host "✅ Created: $dir" -ForegroundColor Gray
}

# Create essential files
Write-Host "`n📝 Creating essential files..." -ForegroundColor Yellow

# .gitignore
@"
# .NET
bin/
obj/
out/
*.user
*.userosscache
*.sln.docstates
.vscode/
.vs/
*.log
appsettings.Development.json
appsettings.Local.json

# React/Node
node_modules/
npm-debug.log*
yarn-debug.log*
yarn-error.log*
.env.local
.env.development.local
.env.test.local
.env.production.local
build/
dist/

# Database
*.db
*.sqlite
*.sqlite3

# Secrets & Keys
*.pem
*.key
*.p12
*.pfx
secrets/
.env
.env.local

# OS
.DS_Store
Thumbs.db
*.swp
*.swo

# IDE
.idea/
*.suo
*.ntvs*
*.njsproj
*.sln.docstates

# Logs
logs/
*.log
"@ | Out-File -FilePath ".gitignore" -Encoding UTF8

# docker-compose.yml
@"
version: '3.8'

services:
  database:
    image: postgres:15-alpine
    container_name: sacco-postgres
    environment:
      POSTGRES_DB: sacco_analytics
      POSTGRES_USER: dev_user
      POSTGRES_PASSWORD: dev_password
    ports:
      - "5433:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
      - ./database/scripts:/docker-entrypoint-initdb.d
    networks:
      - sacco_network
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U dev_user -d sacco_analytics"]
      interval: 10s
      timeout: 5s
      retries: 5

  redis:
    image: redis:7-alpine
    container_name: sacco-redis
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data
    networks:
      - sacco_network
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 10s
      timeout: 3s
      retries: 5

  backend:
    build:
      context: ./backend
      dockerfile: ../infrastructure/docker/backend/Dockerfile
    container_name: sacco-backend
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Host=database;Port=5432;Database=sacco_analytics;Username=dev_user;Password=dev_password
      - Redis__ConnectionString=redis:6379
    depends_on:
      database:
        condition: service_healthy
      redis:
        condition: service_healthy
    networks:
      - sacco_network

  frontend:
    build:
      context: ./frontend
      dockerfile: ../infrastructure/docker/frontend/Dockerfile
    container_name: sacco-frontend
    ports:
      - "3000:80"
    environment:
      - REACT_APP_API_URL=http://localhost:5000/api
    depends_on:
      - backend
    networks:
      - sacco_network

volumes:
  postgres_data:
  redis_data:

networks:
  sacco_network:
    driver: bridge
"@ | Out-File -FilePath "docker-compose.yml" -Encoding UTF8

# Makefile for easy commands
@"
.PHONY: setup dev build test clean help

help: ## Show this help message
	@echo 'Usage: make [target]'
	@echo ''
	@echo 'Targets:'
	@awk 'BEGIN {FS = ":.*?## "} /^[a-zA-Z_-]+:.*?## / {printf "  %-15s %s\n", $$1, $$2}' $(MAKEFILE_LIST)

setup: ## Setup development environment
	@echo "🚀 Setting up development environment..."
	docker-compose up -d database redis
	cd backend && dotnet restore
	cd frontend && npm install

dev: ## Start development servers
	@echo "🔥 Starting development servers..."
	docker-compose up -d database redis
	start /B cmd /c "cd backend && dotnet watch run --project src/SaccoAnalytics.API"
	start /B cmd /c "cd frontend && npm run dev"
	@echo "Backend: http://localhost:5000"
	@echo "Frontend: http://localhost:5173"

build: ## Build all services
	@echo "🔨 Building all services..."
	cd backend && dotnet build
	cd frontend && npm run build
	docker-compose build

test: ## Run all tests
	@echo "🧪 Running tests..."
	cd backend && dotnet test
	cd frontend && npm test

clean: ## Clean up everything
	@echo "🧹 Cleaning up..."
	docker-compose down -v
	cd backend && dotnet clean
	cd frontend && if exist node_modules rmdir /s /q node_modules
	cd frontend && if exist build rmdir /s /q build

db-migrate: ## Run database migrations
	@echo "📊 Running database migrations..."
	cd backend && dotnet ef database update --project src/SaccoAnalytics.Infrastructure --startup-project src/SaccoAnalytics.API

db-reset: ## Reset database
	@echo "🔄 Resetting database..."
	docker-compose down database
	docker volume rm sacco-analytics-platform_postgres_data
	docker-compose up -d database
"@ | Out-File -FilePath "Makefile" -Encoding UTF8

# README.md
@"
# SACCO Analytics Platform

A secure, multi-tenant SaaS platform for Savings and Credit Cooperative Organizations (SACCOs) to manage financial reporting and analytics.

## 🎯 Vision
Empower SACCOs with modern, AI-driven financial insights while maintaining the highest standards of data security and regulatory compliance.

## 🏗️ Architecture
- **Backend**: C# .NET 8 Web API with Clean Architecture
- **Frontend**: React 18 + TypeScript + Vite
- **Database**: PostgreSQL with Row-Level Security  
- **Authentication**: JWT + Refresh Tokens
- **Infrastructure**: Docker + Kubernetes
- **Monitoring**: Grafana + Prometheus

## 🚀 Quick Start

### Prerequisites
- .NET 8 SDK
- Node.js 18+
- Docker & Docker Compose
- VS Code with C# extension

### Setup
``````bash
# Clone repository
git clone https://github.com/Slonetech/sacco-analytics-platform.git
cd sacco-analytics-platform

# Setup everything (Windows)
make setup

# Start development servers
make dev

# Access applications
# Frontend: http://localhost:5173
# Backend API: http://localhost:5000
# Swagger: http://localhost:5000/swagger
``````

## 📊 Current Status
🟡 **Phase 1**: Foundation & Architecture (In Progress)
- [x] Repository structure
- [x] Docker infrastructure  
- [ ] Backend API foundation
- [ ] Frontend foundation
- [ ] Database schema
- [ ] Authentication system

## 🛠️ Development Commands
- `make setup` - Setup development environment
- `make dev` - Start development servers
- `make build` - Build all services
- `make test` - Run all tests  
- `make clean` - Clean up everything
- `make db-migrate` - Run database migrations
- `make help` - Show all commands

## 🏗️ Project Structure
``````
sacco-analytics-platform/
├── backend/                    # .NET 8 Web API
│   ├── src/                   # Source code
│   │   ├── SaccoAnalytics.API/        # Web API layer
│   │   ├── SaccoAnalytics.Core/       # Domain layer  
│   │   ├── SaccoAnalytics.Application/ # Application layer
│   │   └── SaccoAnalytics.Infrastructure/ # Infrastructure layer
│   └── tests/                 # Test projects
├── frontend/                   # React TypeScript SPA
│   ├── src/                   # Source code
│   └── tests/                 # Frontend tests
├── infrastructure/             # Infrastructure as Code
├── database/                   # Database scripts & migrations
├── docs/                       # Documentation
└── scripts/                    # Automation scripts
``````

## 🔐 Security Features
- Multi-tenant data isolation
- Row-Level Security (RLS)
- JWT authentication with refresh tokens
- Role-based access control (RBAC)
- Audit logging
- Data encryption at rest

## 🚀 Deployment
- **Development**: Docker Compose
- **Staging/Production**: Kubernetes + Helm
- **Infrastructure**: Terraform
- **CI/CD**: GitHub Actions

## 📖 Documentation
- [Architecture Overview](docs/architecture/)
- [API Documentation](docs/api/)
- [Deployment Guide](docs/deployment/)
- [User Guides](docs/user-guides/)

## 🤝 Contributing
1. Fork the repository
2. Create feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open Pull Request

## 📄 License
This project is licensed under the MIT License - see [LICENSE](LICENSE) file for details.

## 🆘 Support
- Create an issue for bug reports
- Start a discussion for questions
- Check [docs/](docs/) for detailed guides

---
Built with ❤️ for the SACCO community
"@ | Out-File -FilePath "README.md" -Encoding UTF8

# Create placeholder files to preserve folder structure
$placeholderFiles = @(
    "backend/src/SaccoAnalytics.Core/Entities/Common/.gitkeep",
    "frontend/src/components/common/Layout/.gitkeep", 
    "infrastructure/terraform/environments/dev/.gitkeep",
    "database/migrations/.gitkeep",
    "docs/architecture/.gitkeep",
    "monitoring/grafana/dashboards/.gitkeep",
    "security/secrets/.gitkeep",
    "scripts/development/.gitkeep"
)

foreach ($file in $placeholderFiles) {
    New-Item -ItemType File -Path $file -Force | Out-Null
}

Write-Host "`n✅ Project structure created successfully!" -ForegroundColor Green
Write-Host "✅ Essential files created:" -ForegroundColor Green
Write-Host "   - .gitignore" -ForegroundColor Gray
Write-Host "   - docker-compose.yml" -ForegroundColor Gray  
Write-Host "   - Makefile" -ForegroundColor Gray
Write-Host "   - README.md" -ForegroundColor Gray

Write-Host "`n🎯 Next Steps:" -ForegroundColor Yellow
Write-Host "1. Run: make setup" -ForegroundColor White
Write-Host "2. Run: make dev" -ForegroundColor White
Write-Host "3. Open VS Code in this folder" -ForegroundColor White
Write-Host "4. Start building! 🚀" -ForegroundColor White

Write-Host "`n💡 Tip: Run 'make help' to see all available commands" -ForegroundColor Cyan