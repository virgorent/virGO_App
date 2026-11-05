# VirGo API

REST API for **VirGo**, a platform that connects people organizing an event with venues and service providers. This folder of the [`virGO_App`](https://github.com/virgorent/virGO_App) repository contains only the backend; the Android app lives in `../VirGo-Movil`.

> **Status:** local implementation of the architecture, infrastructure, pipeline and seven-endpoint vertical slice. Cloud validation and deployment remain pending.

## Tech stack

| Area | Choice |
|---|---|
| Runtime | .NET 10, ASP.NET Core Web API (controllers) |
| Architecture | Clean layers: `Api → Application → Domain`, `Infrastructure → Application` |
| Hosting | AWS Lambda behind API Gateway HTTP API (HTTPS, no custom domain) |
| Database | MongoDB Atlas M0 (free forever, AWS `us-east-1`), official `MongoDB.Driver` |
| Secrets | AWS SSM Parameter Store (SecureString) and GitHub Secrets |
| Infrastructure as Code | AWS SAM / CloudFormation |
| CI/CD | GitHub Actions with OIDC to AWS: CI on every PR, manual deploy |
| Security (DevSecOps) | CodeQL, Semgrep, SonarCloud, OWASP ZAP, Dependabot, dependency review, Gitleaks, Checkov, GitHub rulesets |
| Tests | xUnit (unit, integration, security), Stryker.NET (mutation), smoke tests after deploy |

## Architecture

```text
Mobile app ──HTTPS──▶ API Gateway (HTTP API, throttling) ──▶ Lambda (VirGo.Api) ──TLS──▶ MongoDB Atlas M0
                                                                 ├──▶ SSM Parameter Store (JWT key, Mongo URI)
                                                                 └──▶ CloudWatch Logs
GitHub Actions ──OIDC──▶ least-privilege deploy role ──▶ CloudFormation / SAM
AWS Budgets ($1 USD alert) ──▶ team email
```

Target monthly cost: **$0 USD** (AWS free tier and the Atlas M0 free cluster).

## Repository layout

```text
VirGo-API/
├── constitution.md   # Project rules (Spanish). Everything else must align with it
├── AGENTS.md         # Guide for AI agents and contributors
├── memoria.md        # Project log: decisions (ADR), costs, contract changes
├── API-HANDOFF.md      # Versioned API and persistence contract snapshot
├── opencode.json     # Approved MCP servers
├── .opencode/        # Sub-agents and internal skills
├── specs/            # One folder per module: spec.md, plan.md, task.md, check.md
├── src/              # VirGo.Api, VirGo.Application, VirGo.Domain, VirGo.Infrastructure
├── tests/            # Unit, Integration, Security, smoke
├── db/               # MongoDB schema (source of truth) and check tool
├── infra/            # SAM template and bootstrap stack
```

GitHub workflows live in `.github/` at the repository root (`api-*`, `movil-*` and `security.yml`). Commands below run inside `VirGo-API/`.

## Specs

The project follows **Spec-Driven Development**: no code without an approved spec. Each spec is atomic (one capability).

| Module | Scope | Status |
|---|---|---|
| [`000-arquitectura-base`](specs/000-arquitectura-base/spec.md) | Solution layers, errors, configuration, base security, persistence | In review |
| [`001-infraestructura-iac`](specs/001-infraestructura-iac/spec.md) | AWS infrastructure as code, HTTPS URL, $0 cost | Draft |
| [`002-pipeline-cicd`](specs/002-pipeline-cicd/spec.md) | CI with DevSecOps checks, rulesets, manual CD, smoke tests | Draft |
| [`003-auth-login`](specs/003-auth-login/spec.md) | `POST /api/v1/auth/login` | Local |
| [`004-auth-logout`](specs/004-auth-logout/spec.md) | `POST /api/v1/auth/logout` | Local |
| [`005-auth-register`](specs/005-auth-register/spec.md) | `POST /api/v1/auth/register` | Local |
| [`006-users-me`](specs/006-users-me/spec.md) | `GET /api/v1/users/me` | Local |
| [`007-venues-catalog`](specs/007-venues-catalog/spec.md) | `GET /api/v1/venues` | Local |
| [`008-quotes-create`](specs/008-quotes-create/spec.md) | `POST /api/v1/quotes` | Local |
| [`009-bookings-create`](specs/009-bookings-create/spec.md) | `POST /api/v1/bookings` | Local |
| [`007-esquema-mongodb`](specs/007-esquema-mongodb/spec.md) | MongoDB schema as the single source of truth | Draft |

## Getting started

Run these commands from `VirGo-API/`:

```bash
dotnet restore
dotnet build -c Release --no-restore
dotnet test -c Release --no-build
dotnet run --project src/VirGo.Api     # http://localhost:5080, in-memory persistence, no AWS needed
```

For local execution, provide a development-only JWT signing key through the environment; production reads it from SSM:

```powershell
$env:Jwt__SigningKey = "set-a-local-value-with-at-least-32-characters"
dotnet run --project src/VirGo.Api
```

Mutation tests: `dotnet tool restore && dotnet stryker`.

## API contract

- Base path `/api/v1`, JSON in `camelCase`, dates in ISO-8601 UTC, amounts in MXN.
- Errors follow **Problem Details (RFC 9457)** with a `traceId`.
- Auth: JWT Bearer in the `Authorization` header.
- OpenAPI document: `GET /openapi/v1.json`. Health check: `GET /health`.

## Contributing

1. Pick a task from an approved `task.md`.
2. Branch from `rama-backend` (`feat/NNN-module`), commit with Conventional Commits citing the task ID (`feat(T301): ...`).
3. Merge into `rama-backend`, then open a PR to `main`. CI must be green and one review is required.
4. Fill in the module's `check.md` and add an entry to `memoria.md`.

Project documentation (constitution, specs, log) is written in Spanish.
