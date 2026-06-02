# Setup Plan: Stub Server Project

## Overview
This project is a multi-service environment managed by Docker. It includes a MySQL database, multiple Node.js application instances (Stub, Staging, Web), and a management UI (phpMyAdmin).

## Prerequisites
- [ ] Install **Docker Desktop** for Windows.
- [ ] Install **Git**.
- [ ] Ensure ports `3000`, `3001`, `3002`, `3306`, and `8080` are available on your host machine.

## Phase 1: Environment Setup
1. **Clone the repository**:
   ```bash
   git clone <repo-url>
   cd stub-server
   ```
2. **Environment Configuration**:
   - The project uses predefined environment variables in `docker-compose.yml`.
   - Check `./web/.env` if manual overrides are needed for the dashboard.

## Phase 2: Launching Services
1. **Build and start containers**:
   ```bash
   docker-compose up -d --build
   ```
2. **Verify container status**:
   ```bash
   docker-compose ps
   ```

## Phase 3: Accessing Services
- **Stub Server**: [http://localhost:3000](http://localhost:3000)
- **Staging Server**: [http://localhost:3001](http://localhost:3001)
- **Web Dashboard**: [http://localhost:3002](http://localhost:3002)
- **phpMyAdmin**: [http://localhost:8080](http://localhost:8080) (User: `root`, Password: `aA@123456`)

## Phase 4: Troubleshooting
- Check logs for specific services: `docker-compose logs -f <service_name>`
- Common services: `stub`, `staging`, `web`, `mysql`.
