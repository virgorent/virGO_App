# Plan — 002 Pipeline CI/CD con DevSecOps

| Campo | Valor |
|---|---|
| Módulo | `002-pipeline-cicd` |
| Versión | 1.0.0 |
| Estado | Implementación local; ejecución en GitHub pendiente |
| Fecha | 2026-11-03 |
| Spec | [spec.md](spec.md) · Tareas: [task.md](task.md) · Verificación: [check.md](check.md) |
| Constitución | v1.2.0 |

## 1. Resumen técnico

Cinco workflows de GitHub Actions y un ruleset, todos en `.github/` de la **raíz** de `virgorent/virGO_App` (GitHub ignora `.github/` en subcarpetas) y compartidos con Móvil:

- `api-ci.yml` (PR y *push* a `main`): compilación, pruebas, Semgrep, SonarCloud, OWASP ZAP, IaC, esquema y comentario de calidad.
- `api-mutation-nightly.yml` (cada noche y `workflow_dispatch`): Stryker.
- `security.yml` (común, todos los PR): secretos y dependencias.
- `api-codeql.yml` (PR, *push* a `main` y semanal): análisis de C#.
- `api-deploy.yml` (**solo `workflow_dispatch`**, botón *Run workflow*): OIDC, `sam build`/`sam deploy`, humo y URL en el resumen.
- Ruleset `main-protection` sobre `main`, uno solo para todo el repositorio.

## 2. Contexto técnico

| Aspecto | Decisión |
|---|---|
| Plataforma | GitHub Actions, `ubuntu-latest` |
| .NET | `actions/setup-dotnet` con SDK 10.0.x y caché de NuGet |
| Mutación | Stryker.NET como *dotnet tool* local (`.config/dotnet-tools.json`) |
| SAST y calidad | Semgrep (`.semgrep/semgrep.yaml`, reglas propias y `p/csharp`), SonarCloud con quality gate y cobertura |
| DAST | OWASP ZAP en modo *baseline* contra la API levantada en el runner |
| Secretos | Gitleaks (CLI) sobre el diff del PR y el historial completo en `main` |
| Dependencias | `actions/dependency-review-action` y Dependabot |
| IaC | `sam validate --lint`, `cfn-lint`, Checkov |
| Despliegue | `aws-actions/configure-aws-credentials` con OIDC, AWS SAM CLI |
| MCP de apoyo | `github` (leer PRs, checks y ejecuciones de workflows) |

> Las acciones de terceros se fijan por **SHA del commit**, no por etiqueta, para evitar ataques a la cadena de suministro. Dependabot las actualiza.

## 3. Constitution Check

| Art. | Tema | Resultado | Cómo se cumple |
|---|---|---|---|
| I | SDD | ✅ Cumple | Spec atómico; un CI en rojo se corrige sobre el mismo spec. |
| II | Modularidad | ✅ Cumple | Sin funcionalidad del negocio. |
| III | Arquitectura | ➖ No aplica | |
| IV | Contrato | ✅ Cumple | El humo verifica `/health`, `/openapi/v1.json` y un 401 en Problem Details. |
| V | Seguridad | ✅ Cumple | CodeQL, *dependency review*, Gitleaks, Checkov, OIDC y `permissions:` mínimos. |
| VI | Pruebas | ✅ Cumple | Unitarias, integración, seguridad, mutación y humo. |
| VII | Costo cero | ✅ Cumple | Minutos gratuitos de Actions. |
| VIII | CI/CD | ✅ Cumple | Ruleset, CI en cada PR y CD manual con humo y URL HTTPS. |
| IX | Observabilidad | ✅ Cumple | El resumen del workflow guarda URL, commit y resultado. |
| X | Memoria | ✅ Cumple | T210 registra la URL en ambas memorias. |
| XI | Agentes y MCP | ✅ Cumple | Tareas de `devops-aws`, `seguridad` y `qa`; 🧑 en ruleset y despliegue. |
| XII | Idioma | ✅ Cumple | Nombres de jobs en inglés; documentación en español. |

## 4. Estructura

```text
virGO_App/                           # Raíz del repositorio
├── .github/
│   ├── workflows/
│   │   ├── api-ci.yml
│   │   ├── api-codeql.yml
│   │   ├── api-deploy.yml
│   │   ├── api-mutation-nightly.yml
│   │   └── security.yml             # Común con Móvil
│   ├── rulesets/main-protection.json
│   ├── dependabot.yml               # Un bloque por carpeta
│   └── pull_request_template.md
├── .gitleaks.toml                   # Reglas extra y rutas permitidas
├── .semgrep/semgrep.yaml
└── VirGo-API/
    ├── .config/dotnet-tools.json    # dotnet-stryker
    ├── stryker-config.json
    └── tests/
        ├── VirGo.UnitTests/
        ├── VirGo.IntegrationTests/
        ├── VirGo.SecurityTests/     # Nuevo
        └── smoke/smoke.sh           # Pruebas de humo contra la URL desplegada
```

## 5. Decisiones técnicas (ADR)

| ADR | Decisión | Alternativas descartadas | Motivo |
|---|---|---|---|
| ADR-09 | **CD manual** con `workflow_dispatch` y *environment* `production` | Despliegue automático en cada *merge* | Indicación del curso; la persona decide cuándo sale una versión |
| ADR-10 | **Ruleset** de GitHub en lugar de la protección de rama clásica | Branch protection rules | Los rulesets son el mecanismo actual, se pueden exportar a JSON y versionar |
| ADR-14 | **Stryker.NET** para mutación, cada noche y primero en modo reporte | Sin pruebas de mutación | Mide la calidad real de las pruebas; el umbral se fija después de medir (Art. VI.4) |
| ADR-15 | **Gitleaks + dependency review** | Solo el *secret scanning* de GitHub | Funcionan en repositorios privados sin GitHub Advanced Security |
| ADR-16 | Proyecto **`VirGo.SecurityTests`** separado | Mezclar en integración | Se puede ejecutar y reportar como check independiente |
| ADR-30 | **Semgrep, SonarCloud y OWASP ZAP** en cada PR | Solo CodeQL | Lo pidió la clase del 2026-11-04 y lo usa la referencia; SonarCloud es gratis en repositorio público |
| ADR-31 | **Mutación nocturna**, fuera de los checks requeridos | Mutación en cada PR | Mantiene el CI bajo 15 minutos, como la referencia |
| ADR-26 | **Un solo repositorio** con carpetas `VirGo-API/` y `VirGo-Movil/`; job `changes` en lugar de filtros `paths` | Dos repositorios; un proyecto por rama | Decisión del equipo. Un job omitido por `if` cuenta como exitoso; un workflow omitido por `paths` deja el check requerido pendiente y bloquea el PR |

## 6. Diseño

### 6.1 `api-ci.yml`

| Job | Pasos | Requerido en el ruleset |
|---|---|---|
| `changes` | Detecta si el PR toca `VirGo-API/**` o el propio workflow; los demás jobs usan `needs: changes` e `if` | — |
| `api-build-test` | restore → build (`-warnaserror` para seguridad) → `VirGo.UnitTests` → `VirGo.IntegrationTests` (incluye los repositorios de MongoDB con Testcontainers; el runner trae Docker) → `VirGo.SecurityTests` → cobertura como artefacto | ✅ |
| `api-semgrep` | Semgrep con `.semgrep/semgrep.yaml` y `p/csharp` | ✅ |
| `api-sonarcloud` | Análisis, cobertura y quality gate | ✅ |
| `api-zap` | Levanta la API con persistencia en memoria y ejecuta ZAP *baseline* | ✅ |
| `api-schema` | `python db/tools/check_schema.py` | ✅ |
| `pr-quality-gate` | Con `if: always()`, comenta en el PR qué job falló y el comando para reproducirlo; el agente corrige sobre el mismo spec y rama | — |
| `api-iac` | `sam validate --lint` → `cfn-lint infra/*.yaml` → `checkov -d infra` | ✅ |

Todos con `defaults.run.working-directory: VirGo-API`, `permissions: contents: read` y `concurrency` para cancelar ejecuciones obsoletas.

`security.yml` corre siempre, sin depender de la carpeta, con los jobs `gitleaks` (sobre los commits del PR) y `dependency review` (falla con vulnerabilidades altas o licencias no permitidas); ambos son requeridos.

### 6.2 `api-deploy.yml`

```text
on: workflow_dispatch
job deploy:
  if: github.ref == 'refs/heads/main'
  environment: production
  permissions: { id-token: write, contents: read }
  1. checkout, setup-dotnet, setup-sam
  2. configure-aws-credentials (OIDC, role-to-assume: vars.AWS_ROLE_ARN)
  3. sam build → sam deploy --no-confirm-changeset --no-fail-on-empty-changeset
  4. ApiUrl ← salida del stack
  5. VirGo-API/tests/smoke/smoke.sh "$ApiUrl"
  6. URL, commit y resultado → $GITHUB_STEP_SUMMARY
```

### 6.3 Pruebas de humo (`tests/smoke/smoke.sh`)

| Prueba | Esperado |
|---|---|
| `GET {ApiUrl}/health` | 200 y `"status":"Healthy"` |
| `GET {ApiUrl}/openapi/v1.json` | 200 |
| `GET http://…` (sin TLS) | No responde (API Gateway solo HTTPS) |
| Ruta protegida sin token | 401 `application/problem+json` |
| Encabezados | `Strict-Transport-Security` y `X-Correlation-Id` presentes |

Cada módulo funcional agrega su propia prueba de humo a este script (por ejemplo, `003` agrega un login inválido → 401).

### 6.4 Pruebas de seguridad base (`VirGo.SecurityTests`)

JWT con `alg: none`, firma inválida, expirado, emisor o audiencia incorrectos → 401; CORS con origen no permitido sin `Access-Control-Allow-Origin`; cuerpo mayor al límite → 413; 500 sin trazas ni nombres de clases; encabezados de seguridad presentes; 429 al superar el límite.

### 6.5 Ruleset `main-protection`

Objetivo: rama por defecto. Reglas: *Restrict deletions*, *Block force pushes*, *Require a pull request* (1 aprobación, descartar aprobaciones al subir cambios, resolver conversaciones), *Require status checks* (`api-build-test`, `api-semgrep`, `api-sonarcloud`, `api-zap`, `api-iac`, `api-schema`, `api-codeql`, `gitleaks`, `dependency review` y los de Móvil: `movil-build-test` y `movil-codeql`). Se exporta a `.github/rulesets/main-protection.json` como evidencia.

## 7. Costo

| Servicio | Costo |
|---|---|
| GitHub Actions | $0 (2 000 min/mes en privado; ilimitado en público). Un CI estimado de 8–12 min |
| Herramientas | $0 (Stryker, Semgrep, ZAP, Gitleaks, Checkov, cfn-lint y CodeQL son gratuitas para este uso; SonarCloud lo es en repositorios públicos) |

## 8. Estrategia de pruebas del pipeline

| Verificación | Cómo |
|---|---|
| Ruleset | PR de prueba y *push* directo rechazado (CA-01) |
| Bloqueo por pruebas | PR con una prueba que falla a propósito (CA-02), se revierte |
| Secretos | PR con una llave falsa con formato AWS (CA-03), se cierra sin fusionar |
| Despliegue | Ejecución manual desde `main` y desde otra rama (CA-05, CA-06) |

## 9. Riesgos y mitigaciones

| Riesgo | Mitigación |
|---|---|
| Stryker tarda demasiado | Solo `Domain` y `Application`, y corre de noche |
| Al ser público, un secreto subido por error queda expuesto de inmediato | Gitleaks en cada PR, protección de push activa y rotar de inmediato cualquier secreto filtrado |
| Un check requerido queda pendiente en un PR que solo toca la otra carpeta | Job `changes` con `if`, nunca `paths` a nivel de workflow (ADR-26) |
| Una acción de terceros comprometida | Fijada por SHA y actualizada por Dependabot |
| El humo pasa pero el endpoint de negocio falla | Cada spec funcional agrega su prueba de humo |

## 10. Fases

| Fase | Contenido | Tareas |
|---|---|---|
| F0 | MCP y plantillas del repositorio | T200–T201 |
| F1 | Pruebas de seguridad y mutación | T202–T203 |
| F2 | CI y análisis de seguridad | T204–T206 |
| F3 | Ruleset | T207 |
| F4 | CD manual y humo | T208–T210 |
| F5 | Análisis extendido | T212–T216 |
| F6 | Cierre | T211 |
