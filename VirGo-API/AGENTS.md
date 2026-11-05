# AGENTS.md — VirGo API (Backend)

Guía base para los agentes de IA y las personas que trabajan en este proyecto (carpeta `VirGo-API/` del repositorio `virgorent/virGO_App`). Sigue lo que dice [`constitution.md`](constitution.md); si algo aquí lo contradice, gana la constitución.

## Contexto del proyecto

- **Qué es:** la API REST de VirGo. La consume la app móvil Android (`VirGo-Movil`).
- **Stack:** .NET 10, ASP.NET Core Web API, AWS Lambda, API Gateway (HTTP API), MongoDB Atlas M0, AWS SAM y GitHub Actions.
- **Estado actual:** implementación local parcial. Existen la base `000`, las plantillas de `001`, los workflows de `002` y el vertical slice de siete endpoints (`003`–`009`); quedan pendientes la validación externa, el despliegue y el cierre formal de los specs.

## Estructura del proyecto

La estructura objetivo la define el plan de la arquitectura base:

```text
VirGo-API/
├── README.md                # Portada en inglés (Art. XII.1)
├── .gitignore               # Exclusiones DevSecOps (Art. V.10)
├── constitution.md          # Reglas globales del proyecto
├── AGENTS.md                # Este archivo
├── memoria.md               # Bitácora de actividades y decisiones
├── API-HANDOFF.md           # Contrato versionado de API y persistencia
├── opencode.json            # Servidores MCP permitidos
├── .opencode/
│   ├── agents/              # Subagentes del proyecto
│   └── skills/              # Skills internas
├── specs/                   # Un spec atómico por carpeta: spec.md, plan.md, task.md, check.md
│   ├── 000-arquitectura-base/
│   ├── 001-infraestructura-iac/
│   ├── 002-pipeline-cicd/
│   ├── 003-auth-login/
│   ├── 004-auth-logout/
│   ├── 005-auth-register/ through 009-bookings-create/ # Vertical slice
│   └── 007-esquema-mongodb/
├── db/                      # schema/ (fuente de verdad) y tools/check_schema.py
├── src/                     # VirGo.Api, VirGo.Application, VirGo.Domain, VirGo.Infrastructure
├── tests/                   # UnitTests, IntegrationTests, SecurityTests, smoke/
├── infra/                   # template.yaml (SAM), bootstrap.yaml
└── (sin .github aquí)        # GitHub solo lee `.github/` de la raíz del repositorio
```

En la raíz del repositorio están `.github/` con los workflows y el ruleset, la gobernanza global (`constitution.md`, `AGENTS.md`, `Memoria.md`, `specs/nv-*`) y `Modelo de datos VirGo.md` como contexto histórico. Todos los comandos de abajo se ejecutan dentro de `VirGo-API/`.

## Comandos

Estos comandos están disponibles para la implementación local; Python, Atlas y AWS siguen siendo dependencias externas:

```powershell
dotnet restore
dotnet build -c Release --no-restore
dotnet test -c Release --no-build            # Unitarias, integración y seguridad
dotnet tool restore && dotnet stryker        # Pruebas de mutación (Domain y Application)
python db/tools/check_schema.py              # Límites del esquema; con --live compara contra la base
dotnet run --project src/VirGo.Api          # http://localhost:5080, usa la persistencia en memoria
bash tests/smoke/smoke.sh https://<api-url>  # Pruebas de humo contra una URL desplegada
```

Orden del pipeline de CI: `restore → build → unitarias → integración → seguridad → esquema`, en paralelo con `Semgrep`, `SonarCloud`, `OWASP ZAP`, `CodeQL`, `Gitleaks`, `dependency review` e `IaC (sam validate, cfn-lint, Checkov)`. La mutación corre cada noche. El despliegue es **manual** con *Run workflow* en `api-deploy.yml`.

## Flujo de trabajo SDD (obligatorio)

1. **Spec:** el agente `arquitecto` redacta `spec.md` (qué y por qué) y lo aprueba una persona.
2. **Plan:** el agente `arquitecto` redacta `plan.md` y llena el «Constitution Check». Lo aprueba una persona.
3. **Tareas:** se generan en `task.md` con un ID con el número del módulo (`T001` en `000`, `T101` en `001`, `T301` en `003`…), dependencias, el subagente responsable y su criterio de verificación.
4. **Check:** se crea `check.md` con la evidencia que hará falta para cada CA, cada tipo de prueba, el PR, el despliegue y la memoria.
5. **Implementación:** se avanza una tarea a la vez, en el orden del `task.md`, en la rama `feat/NNN-modulo`. Si el CI falla, se corrige en la misma rama y sobre el mismo spec.
6. **Despliegue:** una persona lo lanza con *Run workflow*; el humo debe quedar en verde.
7. **Cierre y aprendizaje:** `qa` completa el `check.md`, el spec pasa a *Desplegado*/*Cerrado* y se registra en `memoria.md` lo aprendido y las mejoras para la constitución, este archivo, las skills y los subagentes.

Un spec es **atómico**: un endpoint, la infraestructura o el pipeline (Art. I.5).

Usa la skill [`sdd-flujo`](.opencode/skills/sdd-flujo/SKILL.md) para crear o revisar specs.

## Subagentes

Están definidos en `.opencode/agents/`. Cada tarea del `task.md` indica cuál la ejecuta.

| Subagente | Archivo | Responsabilidad | No puede |
|---|---|---|---|
| `arquitecto` | [arquitecto.md](.opencode/agents/arquitecto.md) | Redactar specs y planes, revisar la alineación con la constitución y diseñar capas y contratos | Escribir código de producción |
| `dev-dotnet` | [dev-dotnet.md](.opencode/agents/dev-dotnet.md) | Implementar las tareas de código en .NET 10 siguiendo el plan | Cambiar el spec o el plan sin aprobación |
| `qa` | [qa.md](.opencode/agents/qa.md) | Escribir y ejecutar pruebas, y verificar los criterios de aceptación de cada tarea | Marcar una tarea como hecha sin evidencia |
| `seguridad` | [seguridad.md](.opencode/agents/seguridad.md) | Revisar secretos, autenticación, encabezados, IAM, dependencias y análisis estático | Desactivar controles de seguridad |
| `devops-aws` | [devops-aws.md](.opencode/agents/devops-aws.md) | Definir la infraestructura con SAM, los workflows de GitHub Actions y el costo cero | Crear recursos fuera de la lista permitida (Art. VII) |

**Reglas comunes a todos los subagentes:**

- Lee `constitution.md`, el spec, el plan y el `task.md` del módulo antes de actuar.
- Trabaja solo en las tareas que te asignó el `task.md`.
- Antes de cambiar un archivo, revisa su contenido actual.
- No despliegues, no borres recursos en la nube y no hagas `push --force` sin confirmación humana.
- Al terminar, deja evidencia (comando ejecutado, salida y archivos tocados) y agrega una entrada en `memoria.md` con la skill [`registrar-memoria`](.opencode/skills/registrar-memoria/SKILL.md).

## Skills

### Internas, en `.opencode/skills/`

| Skill | Cuándo se usa |
|---|---|
| [`sdd-flujo`](.opencode/skills/sdd-flujo/SKILL.md) | Crear o revisar `spec.md`, `plan.md` y `task.md` de un módulo |
| [`registrar-memoria`](.opencode/skills/registrar-memoria/SKILL.md) | Agregar una entrada a `memoria.md` al terminar una sesión o tarea |
| [`verificar-constitucion`](.opencode/skills/verificar-constitucion/SKILL.md) | Revisar que un plan, PR o archivo cumpla con la constitución |
| [`desplegar-aws`](.opencode/skills/desplegar-aws/SKILL.md) | Validar y desplegar la infraestructura con SAM a costo cero |

### Externas: lista filtrada

Solo se usan las skills externas de esta lista, porque cubren una necesidad real del proyecto. Cualquier otra se evalúa y se agrega aquí antes de usarse (Art. XI).

| Skill externa | Estado | Motivo |
|---|---|---|
| `code-review` / `security-review` | ✅ Permitida | Revisión de PRs (Art. V y VI) |
| `simplify` | ✅ Permitida | Limpieza de código ya funcional |
| `docx` / `pdf` | ✅ Permitida | Entregables escritos para la materia |
| Skills de Figma | ⛔ No aplica | El diseño vive en el proyecto Móvil y en la maqueta |
| `claude-api` | ⛔ No aplica | La API todavía no integra modelos de IA |
| Skills de otros proveedores de nube (Azure, GCP, DigitalOcean) | ⛔ Prohibida | Contradice el Art. VII (solo AWS) |

## Servidores MCP

Se configuran en [`opencode.json`](opencode.json) y se instalan **antes** de codificar el módulo que los necesita (Art. XI.6). Los tokens y perfiles se leen de variables de entorno; nunca se escriben en el archivo.

| MCP | Estado | Para qué | Credencial | Quién lo usa |
|---|---|---|---|---|
| `aws-docs` (`awslabs.aws-documentation-mcp-server`) | ✅ Permitido, activo | Consultar la documentación oficial de AWS (SAM, Lambda, SSM, capa gratuita) | Ninguna | `devops-aws`, `arquitecto` |
| `mongodb` (`mongodb-mcp-server`, oficial de MongoDB) | ✅ Permitido, se activa en `001`: T100 | Consultar la base de datos: colecciones, documentos e índices; verificar que cada spec deja los documentos con la forma de su plan | `VIRGO_MONGO_MCP_URI`: usuario `virgo-mcp` de **solo lectura**; el MCP corre en modo solo lectura | `dev-dotnet`, `qa`, `devops-aws` |
| `mongodb` con herramientas de Atlas | 🧑 Solo en el arranque (`001`: T101) | Crear el clúster M0, los usuarios y la lista de acceso | Cuenta de servicio de Atlas en variables de entorno, desactivada al terminar | `devops-aws` con confirmación humana |
| `github` (servidor remoto oficial) | ✅ Permitido, se activa en `002`: T200 | Leer PRs, checks, ejecuciones de workflows y alertas de seguridad | `GITHUB_MCP_PAT`: token de grano fino solo de este repositorio | Todos |
| `dynamodb` (`awslabs.dynamodb-mcp-server`) | ⛔ No aplica | La base de datos es MongoDB (ADR-23) | — | — |
| MCP de otras nubes | ⛔ Prohibido | Contradice el Art. VII | — | — |

Un MCP nuevo se evalúa (necesidad real, mantenedor, permisos que pide) y se agrega a esta tabla antes de usarse. Las versiones se fijan al instalarlos y se registran en `memoria.md`.

## Convenciones

- **Código** en inglés y **documentación** en español (Art. XII).
- **Commits:** `tipo(TNNN): descripción`, por ejemplo `feat(T014): agregar endpoint /health` o `feat(T307): agregar POST /auth/login`.
- **Ramas:** el backend se integra en `rama-backend`. Las ramas `feat/NNN-modulo`, `fix/…` y `docs/…` salen de ahí, y `rama-backend` entra a `main` por PR.
- **Errores:** siempre Problem Details. **Rutas:** `/api/v1/...`. **JSON:** `camelCase` (Art. IV).
- **Configuración:** `appsettings.json` sin secretos; los secretos por variables de entorno o SSM (Art. V).

## Coordinación con el proyecto Móvil

- El contrato de la API (Art. IV) es compartido. Cualquier cambio se registra en `memoria.md` de **ambos** proyectos.
- La URL HTTPS de producción que publica el workflow de despliegue es la `API_BASE_URL` del móvil.
- Al cerrar cada endpoint se entrega a Móvil la URL y el **JSON probado** (petición y respuestas) que aparece en la sección «Contrato» de su `plan.md`.
