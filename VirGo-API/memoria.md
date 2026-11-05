# Memoria — VirGo API (Backend)

Bitácora oficial del proyecto (Art. X de [`constitution.md`](constitution.md)). Cada sesión de trabajo agrega una entrada al final de la sección «Bitácora» con la skill [`registrar-memoria`](.opencode/skills/registrar-memoria/SKILL.md).

## Estado actual

| Campo | Valor |
|---|---|
| Constitución | v1.2.0 ratificada el 2026-11-04 |
| Módulo en curso | Vertical slice local de `003`–`009`; validación cloud y despliegue pendientes |
| Tareas | Implementación local: 7 endpoints, 23 pruebas automatizadas aprobadas |
| URL de producción | Pendiente (se obtiene en `002`: T209) |
| Costo acumulado en AWS | $0 USD (todavía no se crea ningún recurso) |
| Siguiente paso | Cerrar el merge, actualizar el PR #5 y preparar validación IaC/cloud sin crear recursos aún |

## Módulos

| Módulo | Estado | Notas |
|---|---|---|
| `000-arquitectura-base` | 📝 En revisión | Base técnica sin funcionalidad del negocio; infraestructura y CI/CD se movieron a `001` y `002` |
| `001-infraestructura-iac` | ✏️ Borrador | 12 servicios de AWS, HTTPS, $0, Checkov |
| `002-pipeline-cicd` | ✏️ Borrador | Ruleset, CI con 5 tipos de prueba y DevSecOps, CD manual, humo |
| `003-auth-login` | ✏️ Borrador | `POST /api/v1/auth/login`; espera a `000`–`002` |
| `004-auth-logout` | ✏️ Borrador | `POST /api/v1/auth/logout`; espera a `003` |
| `007-esquema-mongodb` | ✏️ Borrador | Fuente de verdad del esquema; debe cerrarse antes de `003` |
| `005-auth-registro`, `006-auth-renovar-sesion` | 💡 Propuestos | Salieron del alcance de `003` y `004` |

## Decisiones (ADR)

| ADR | Fecha | Decisión | Motivo |
|---|---|---|---|
| ADR-00 | 2026-10-29 | Backend en **.NET 10** (ASP.NET Core Web API) | Mismo stack que el proyecto de referencia del curso |
| ADR-01 | 2026-10-29 | **Lambda + API Gateway HTTP API** en lugar de EC2 + ALB | HTTPS sin dominio y costo $0; el ALB cobra por hora |
| ADR-02 | 2026-10-29 | Runtime `dotnet10` administrado o `provided.al2023` *self-contained* | Se decide en T020 tras verificar la disponibilidad |
| ADR-03 | 2026-10-29 | ~~DynamoDB provisionado~~ **Reemplazada por ADR-23** | — |
| ADR-04 | 2026-10-29 | **Persistencia en memoria** para local y pruebas | Arranque en local con un solo comando |
| ADR-05 | 2026-10-29 | **OIDC** GitHub → AWS | Sin llaves de acceso de larga duración |
| ADR-06 | 2026-10-29 | **AWS SAM** como infraestructura como código | Nativo para Lambda y API Gateway; `sam delete` borra todo |
| ADR-07 | 2026-10-29 | Secretos en **SSM Parameter Store** | Gratis; Secrets Manager cuesta $0.40 por secreto al mes |
| ADR-08 | 2026-10-29 | Prefijo fijo `/api/v1` | Simplicidad mientras solo exista la v1 |
| ADR-09 | 2026-11-03 | **CD manual** (`workflow_dispatch`, *environment* `production`) | Indicación del curso (clase del 29/10) |
| ADR-10 | 2026-11-03 | **Ruleset** de GitHub en lugar de la protección de rama clásica | Mecanismo actual; exportable a JSON |
| ADR-11 | 2026-11-03 | **Índice TTL de MongoDB** para las sesiones | Limpieza gratis |
| ADR-12 | 2026-11-03 | **Checkov** para la IaC | Revisa seguridad, no solo sintaxis |
| ADR-13 | 2026-11-03 | **SNS + CloudWatch Alarms** | Alarmas de 5xx y *throttling* gratis |
| ADR-14 | 2026-11-03 | **Stryker.NET** para mutación, primero en modo reporte | Mide la calidad de las pruebas; umbral tras medir |
| ADR-15 | 2026-11-03 | **Gitleaks + dependency review** | Funcionan sin GitHub Advanced Security |
| ADR-16 | 2026-11-03 | Proyecto **`VirGo.SecurityTests`** | Check independiente para seguridad |
| ADR-17 | 2026-11-03 | `PasswordHasher<T>` de Identity (PBKDF2) | Incluido en el SDK, sin dependencias nativas |
| ADR-18 | 2026-11-03 | JWT de 15 min + token de sesión opaco de 30 días con huella y `sid` | Tokens cortos y sesiones revocables |
| ADR-19 | 2026-11-03 | Bloqueo por cuenta guardado en la tabla | En Lambda la memoria no se comparte |
| ADR-20 | 2026-11-03 | Validar `sid` contra la tabla en cada ruta protegida | Logout efectivo en todas las instancias |
| ADR-21 | 2026-11-03 | Logout sin cuerpo; la sesión sale del `sid` | Menos datos sensibles en tránsito |
| ADR-22 | 2026-11-03 | MCP permitidos: `aws-docs`, `mongodb` (solo lectura) y `github` | La base es MongoDB, como en la clase |
| ADR-23 | 2026-11-03 | **MongoDB Atlas M0** en AWS `us-east-1` como base de datos | Decisión del equipo: la base se hace en MongoDB; M0 es gratis permanente |
| ADR-24 | 2026-11-03 | Atlas con lista de acceso `0.0.0.0/0` y controles compensatorios | Lambda no tiene IP fija sin NAT (prohibido); M0 no admite *private endpoint* |
| ADR-25 | 2026-11-03 | **Testcontainers** para probar los repositorios de MongoDB en el CI | Gratis en GitHub Actions; local sigue sin Docker (en memoria) |
| ADR-26 | 2026-11-03 | **Repositorio único** `virgorent/virGO_App` con carpetas por proyecto; `.github/` en la raíz | Decisión del equipo |
| ADR-27 | 2026-11-04 | **`db/schema/`** como fuente de verdad del esquema | Es lo único que una máquina puede comparar y aplicar |
| ADR-28 | 2026-11-04 | `_id` y referencias como **texto UUID** (provisional) | Ya estaba así en `virgo_dev` |
| ADR-29 | 2026-11-04 | `monthlyStats._id` determinista | Permite el `upsert` con `$inc` |
| ADR-30 | 2026-11-04 | **Semgrep, SonarCloud y OWASP ZAP** en cada PR | Clase del 2026-11-04 y proyecto de referencia |
| ADR-31 | 2026-11-04 | **Mutación nocturna**, fuera de los checks requeridos | Mantiene el CI corto, como la referencia |

## Cambios de contrato (compartidos con Móvil)

| Fecha | Cambio | Aviso a Móvil |
|---|---|---|
| 2026-10-29 | Contrato base definido: `/api/v1`, Problem Details, `camelCase`, UTC, JWT Bearer, `GET /health`, `GET /openapi/v1.json` | ✅ Registrado también en `VirGo-Movil/memoria.md` |
| 2026-11-03 | **Propuesto:** `POST /api/v1/auth/login` (JSON en `specs/003-auth-login/plan.md` §6.1) y `POST /api/v1/auth/logout` (`specs/004-auth-logout/plan.md` §6.1). Se confirma al desplegar | ✅ Registrado como propuesto en `VirGo-Movil/memoria.md` |

## Costos y recursos en la nube

| Fecha | Recurso | Acción | Costo estimado |
|---|---|---|---|
| — | — | Aún no se crean recursos | $0 |
| (planeado) | MongoDB Atlas M0 `virgo-cluster` | Se crea en `001`: T101, sin método de pago | $0 |

---

## Bitácora

### 2026-10-29 · Análisis del proyecto de referencia

- **Autor:** equipo + Claude Code
- **Módulo / tareas:** — (preparación)
- **Qué se hizo:**
  - Se revisó el repositorio de referencia del profesor (`edelomeza/WebAPIDevSecOpsScalling`): .NET 10 Web API, `AGENTS.md`, archivos `Memoria*.md`, GitHub Actions y AWS EC2 + ALB con CloudFormation.
  - Se identificó que el ALB genera costo (alrededor de $7–10 USD cada 2 semanas), lo que contradice el requisito de costo cero.
- **Evidencia:** `MemoriaAWS.md` del repositorio de referencia, sección «Cálculo 14 días».
- **Decisiones:** ADR-00 y ADR-01 (mantener .NET y cambiar a serverless).
- **Costo:** $0, sin cambios.
- **Pendiente:** crear los archivos SDD.

### 2026-10-29 · Archivos de gobernanza y specs de la arquitectura base

- **Autor:** equipo + Claude Code (`arquitecto`)
- **Módulo / tareas:** `specs/000-arquitectura-base` · ninguna ejecutada
- **Qué se hizo:**
  - Se crearon `constitution.md` v1.0.0, `AGENTS.md` y esta `memoria.md`.
  - Se crearon 5 subagentes en `.opencode/agents/`: `arquitecto`, `dev-dotnet`, `qa`, `seguridad` y `devops-aws`.
  - Se crearon 4 skills internas en `.opencode/skills/`: `sdd-flujo`, `registrar-memoria`, `verificar-constitucion` y `desplegar-aws`, más la lista filtrada de skills externas en `AGENTS.md`.
  - Se redactaron `spec.md` (8 historias técnicas, 15 RF, 7 RNF y 10 CA), `plan.md` (Constitution Check, 8 ADR, diseño y costo de $0) y `task.md` (35 tareas en 8 fases, con matriz de trazabilidad).
- **Evidencia:** archivos en el repositorio. El Constitution Check del plan tiene los 12 artículos en ✅.
- **Decisiones:** ADR-02 a ADR-08.
- **Costo:** $0, sin recursos creados.
- **Pendiente:**
  - Revisión del profesor y del equipo.
  - Responder las preguntas abiertas P-01 (correo del presupuesto) y P-02 (quién aprueba los PR).
  - Al aprobarse, empezar por T001. **No se ejecutan tareas hasta la aprobación.**

### 2026-11-03 · Lineamientos de clase: specs atómicos, check.md, DevSecOps, CD manual y MCP

- **Autor:** equipo + Claude Code (`arquitecto`)
- **Módulo / tareas:** gobernanza · `specs/000`–`004` · ninguna tarea ejecutada
- **Qué se hizo:**
  - Se revisaron las grabaciones de clase del 28 y 29 de octubre y se aplicaron sus lineamientos.
  - Se propuso la enmienda **v1.1.0** de `constitution.md`: `check.md` y specs atómicos, redacción anticipada de specs funcionales, `.gitignore` y escaneos DevSecOps, cinco tipos de prueba con mutación, servicios auxiliares de AWS, ruleset, **CD manual**, aprendizaje tras el despliegue, MCP y README en inglés.
  - Se crearon `README.md` (inglés), `.gitignore` DevSecOps y `opencode.json` (MCP `aws-docs`, `dynamodb`, `github`).
  - La infraestructura y el CI/CD de `000` se separaron en `001-infraestructura-iac` y `002-pipeline-cicd`; `000` conserva el contexto global y la tabla de equivalencias de tareas.
  - Se redactaron `003-auth-login` y `004-auth-logout` con su JSON de contrato para Móvil.
  - Se agregó `check.md` a `000`–`004` y se actualizaron `AGENTS.md`, los 5 subagentes y las skills `sdd-flujo`, `verificar-constitucion`, `desplegar-aws` y `registrar-memoria`.
- **Evidencia:** archivos en el repositorio.
- **Decisiones:** ADR-09 a ADR-22.
- **Costo:** $0, sin recursos creados.
- **Aprendizaje:** el ejemplo de clase usa MongoDB; se mantuvo DynamoDB por costo cero y se eligió su MCP equivalente.
- **Pendiente:**
  - Aprobar la enmienda v1.1.0 (la aprueba el equipo, Art. XI.2).
  - Responder: `001` P-01 (correo de alertas) y P-02 (vigencia de créditos), `002` P-01 (quién aprueba PR) y P-02 (repositorio público o privado), `003` P-01 (bloqueo) y P-02 (agregar `intentos_fallidos` y `bloqueado_hasta` al modelo de datos).
  - Fijar las versiones de los MCP al instalarlos (`001`: T100, `002`: T200).

### 2026-11-03 · Cambio de base de datos: DynamoDB → MongoDB Atlas M0

- **Autor:** equipo + Claude Code (`arquitecto`)
- **Módulo / tareas:** gobernanza · `specs/000`–`004` · ninguna tarea ejecutada
- **Qué se hizo:**
  - Por decisión del equipo, la base de datos será **MongoDB** (ADR-23 reemplaza a ADR-03).
  - Se ajustó la propuesta de enmienda v1.1.0: Art. III.4 (MongoDB e índices creados por la app), V.11 (seguridad de Atlas) y VII (Atlas M0 como única excepción externa, sin método de pago).
  - `000`: proveedor `MongoDb`, `MongoClient` único e inicializador de índices (T019). `001`: nueva T101 para crear el clúster M0, los usuarios y el acceso; la antigua T020 pasa a ser T110; la cadena de conexión va a SSM. `003` y `004`: colecciones `users` y `sessions`, índices (único en `email`, TTL en `purgeAt`) y pruebas con Testcontainers.
  - MCP: `dynamodb` → `mongodb` (`mongodb-mcp-server`, solo lectura).
- **Decisiones:** ADR-23, ADR-24 y ADR-25; ADR-11 pasa a índice TTL de MongoDB.
- **Costo:** $0.
- **Aprendizaje:** el aislamiento de red de Atlas (*private endpoint*, IP fija) requiere servicios de pago; se documentaron controles compensatorios.
  - `Modelo de datos VirGo.md` pasó al borrador 3 para MongoDB: 22 colecciones con sus índices (únicos, TTL y `2dsphere`), `$jsonSchema` y transacciones en lugar de la tabla única de DynamoDB.
- **Pendiente:** `001` P-03 (dueño de la organización de Atlas); `003` P-02 (confirmar `failedLoginCount` y `lockoutEndsAt` en `users`).

### 2026-11-03 · Repositorio único `Karasu0001/virGO_App`

- **Autor:** equipo + Claude Code (`arquitecto`)
- **Módulo / tareas:** gobernanza · `specs/000`, `001`, `002` · ninguna tarea ejecutada
- **Qué se hizo:**
  - El equipo usará un solo repositorio con carpetas `VirGo-API/` y `VirGo-Movil/` en `main`. La base SDD de Cesar Augusto (raíz, `nv-00` y `nv-01`) queda como gobernanza global y esta constitución se subordina a ella en los principios generales.
  - `.github/` pasa a la raíz; workflows con prefijo `api-`, `security.yml` común, ruleset único y job `changes` en lugar de filtros `paths`.
  - El rol OIDC queda limitado a `Karasu0001/virGO_App`.
- **Evidencia:** PR desde la rama `docs/sdd-virgo`.
- **Decisiones:** ADR-26.
- **Costo:** $0.
- **Aprendizaje:** un check requerido omitido por `paths` deja el PR pendiente; en repositorio único hay que usar jobs condicionales.
- **Pendiente:** `002` P-02 (el repositorio parece privado y el ruleset no aplicaría) y P-03 (uso de `rama-apis`); `.specify/` lo exige la constitución global, pero no existe en el repositorio.

### 2026-11-04 · El repositorio pasa a la organización `virgorent`

- **Autor:** equipo + Claude Code (`arquitecto`)
- **Módulo / tareas:** gobernanza · `specs/000`, `001`, `002` · ninguna tarea ejecutada
- **Qué se hizo:** no se pudo transferir `Karasu0001/virGO_App`, así que se creó `virgorent/virGO_App` vacío y se subieron `main`, `rama-apis`, `rama-backend`, `rama-movil` y `docs/sdd-virgo` con su historial. Las referencias vigentes, incluido el `sub` de OIDC, apuntan al repositorio nuevo; las entradas anteriores de esta bitácora conservan el nombre de entonces.
- **Evidencia:** `git ls-remote` muestra los mismos commits en ambos repositorios.
- **Decisiones:** `virgorent/virGO_App` es el repositorio oficial.
- **Costo:** $0.
- **Aprendizaje:** ninguno.
- **Pendiente:** archivar `Karasu0001/virGO_App` cuando el equipo confirme; `002` P-02 (el repositorio es privado y el ruleset no aplicaría).

### 2026-11-04 · El repositorio ya es público

- **Autor:** equipo + Claude Code (`arquitecto`)
- **Módulo / tareas:** gobernanza · ninguna tarea ejecutada
- **Qué se hizo:** se comprobó que `virgorent/virGO_App` es público, con lo que el ruleset y los revisores de los *environments* ya están disponibles en el plan Free. Se corrigió el conteo de tareas de `001` (11, por la T101 de Atlas).
- **Evidencia:** la API pública de GitHub responde `visibility: public`. `Karasu0001/virGO_App` responde «not found».
- **Decisiones:** se resuelve `VirGo-API` `002` P-02.
- **Costo:** $0.
- **Aprendizaje:** al ser público, todo lo que se suba es visible; Gitleaks y la protección de push son prioritarios.
- **Pendiente:** crear el ruleset (`002`: T207) y aclarar `002` P-03 (`rama-apis`).

### 2026-11-04 · Saneamiento tras la clase: esquema, pipeline y validaciones

- **Autor:** equipo + Claude Code (`arquitecto`)
- **Módulo / tareas:** `007`: T702 y T703 · `000`, `002`, `003`, `004` actualizados
- **Qué se hizo:**
  - Se revisó `virgo_dev` en modo lectura: 22 colecciones y 59 índices, pero contradecía al modelo, y 0 de 77 campos de texto libre tenían `maxLength`.
  - Se creó `db/schema/` y `db/tools/check_schema.py`, con correcciones y longitudes. **Todavía no se aplica a la base** (T704).
  - `002` suma Semgrep, SonarCloud, OWASP ZAP, comentario de calidad en el PR y mutación nocturna: 13 herramientas.
  - Se agregó la regla «ningún campo sin límite» (Art. VI.6, RF-16, T036).
  - Se crearon la plantilla de PR, `CHECKLIST_PR.md`, Dependabot, el ruleset exportado, `.editorconfig`, `Directory.Build.props`, `appsettings.Example.json` y `.specify/templates/`.
  - Se comprobó que existen los paquetes de los MCP y se fijaron sus versiones: `mongodb-mcp-server@3.2.2 --readOnly` y `awslabs.aws-documentation-mcp-server@1.2.4`. Falta instalarlos y probarlos (T100).
- **Evidencia:** `check_schema.py`: 0 diferencias sin credenciales; `--live`: 25 diferencias, pendientes de T704.
- **Decisiones:** ADR-27 a ADR-31.
- **Costo:** $0.
- **Aprendizaje (5 porqués):** la causa raíz fue que nada comparaba los documentos con la realidad; `verificar-constitucion` ahora lo exige.
- **Pendiente:** `007` P-01 a P-05 (T701), T700 y T704; D5 (Redis y colas) y D6 (Pact y pruebas de base de datos) con el profesor; revisión de otra persona antes de aprobar v1.1.0.

### 2026-11-05 · Vertical slice y alineación con el esquema versionado

- **Autor:** equipo + OpenCode (`dev-dotnet`)
- **Módulo / tareas:** `003`–`009` · PR #5
- **Qué se hizo:**
  - Se implementaron siete endpoints de autenticación, perfil, catálogo, cotizaciones y reservaciones con persistencia en memoria y MongoDB.
  - Se incorporó `db/schema/` desde `main` como contrato versionado y se ajustaron `users`, `quotes` y `bookings` para respetar campos requeridos y propiedades opcionales.
  - Se resolvió el merge de `origin/main` en la rama `feat/vertical-slice-7-apis` y se publicó el resultado en el PR #5.
- **Evidencia:** `dotnet build VirGo.slnx -c Release --no-restore -warnaserror` sin advertencias; `dotnet test VirGo.slnx -c Release`: 28 pruebas aprobadas (2 unitarias, 23 integración, 3 seguridad).
- **Costo:** $0; no se crearon recursos AWS ni Atlas.
- **Pendiente:** ejecutar `db/tools/check_schema.py` cuando Python esté disponible; completar la validacion live contra Atlas y el despliegue autorizado.

### 2026-11-05 · Revisión del PR y corrección de bloqueos

- **Autor:** equipo + OpenCode (`qa`, `seguridad`)
- **Módulo / tareas:** `003`, `009`, `002` · PR #5
- **Qué se corrigió:**
  - Se implementó idempotencia persistida para reservaciones mediante `idempotencyKey` y un índice único por cliente.
  - Se agregó bloqueo de cuenta tras cinco fallos consecutivos, con respuesta 429 y `Retry-After` en el siguiente intento.
  - Se alinearon validaciones de entrada con los límites de `db/schema/`, se eliminó la clave JWT de `appsettings.json` y se corrigió la validación de sesión/correlación señalada por CodeQL.
  - Se corrigieron CloudFormation, CORS, la ruta del template generado en el despliegue y los gates de esquema/dependencias del CI.
- **Evidencia local:** build estricto sin advertencias; 19 pruebas de integración, 2 unitarias y 3 de seguridad aprobadas.
- **Pendiente:** confirmar los nuevos checks remotos de CodeQL, IaC, esquema y seguridad; no hacer merge sin aprobación humana.

### 2026-11-05 · Correcciones de contrato y pruebas del vertical slice

- **Autor:** equipo + OpenCode (`dev-dotnet`, `qa`)
- **Módulo / tareas:** `005`–`009` · PR #5
- **Qué se corrigió:**
  - Se rechazaron elementos nulos en servicios de cotizacion y se diferenciaron errores `400` de venues inexistentes `404`.
  - El catalogo local usa categorias y amenities permitidos por `db/schema/venues.json`.
  - Las reservaciones respetan `price.unit` y `minHours`, calculan el anticipo por defecto al 30% y rechazan importes fuera de rango.
  - Se agregaron pruebas de validacion, autenticacion, filtros, fecha, venue inexistente, payload nulo, importe invalido y concurrencia de idempotencia.
  - Se versiono `API-HANDOFF.md` como snapshot auditable del contrato citado por los specs.
- **Evidencia:** `dotnet test VirGo.slnx -c Release`: 35 pruebas aprobadas (30 integracion, 2 unitarias, 3 seguridad); CI remoto previo verde para build, esquema, IaC, mutacion, CodeQL, Gitleaks y dependency review.
- **Costo:** $0; no se crearon recursos AWS ni Atlas.
- **Pendiente:** persistencia live contra Atlas, prueba de humo HTTPS, OIDC y aprobacion humana del PR; no hacer merge automaticamente.
