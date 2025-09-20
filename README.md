# sacco-analytics-platform
# Update README.md
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

## 🚀 Quick Start

### Prerequisites
- .NET 8 SDK
- Node.js 18+
- Docker & Docker Compose
- VS Code with C# extension

### Setup
``````bash
# Start infrastructure
docker-compose up -d

# Backend
cd backend
dotnet restore
dotnet run --project src/SaccoAnalytics.API

# Frontend
cd frontend
npm install
npm run dev