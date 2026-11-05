# Tareas — 000 Arquitectura base del Backend

| Campo | Valor |
|---|---|
| Módulo | `000-arquitectura-base` |
| Versión | 1.1.0 |
| Estado | Implementación local parcial; cierre pendiente de evidencias externas |
| Fecha | 2026-10-29 |
| Spec / Plan / Check | [spec.md](spec.md) · [plan.md](plan.md) · [check.md](check.md) |
| Constitución | v1.2.0 |

**Estados:** ⏳ Pendiente · 🔄 En curso · ✅ Hecha (con evidencia) · ⛔ Bloqueada

**[P]** = puede ejecutarse en paralelo con otras tareas [P] de la misma fase.

**🧑 Requiere confirmación humana** = la tarea crea recursos en la nube o cambia la configuración de GitHub (Art. XI.5).

---

## F0 · Preparación del repositorio y la gobernanza

| ID | Tarea | Depende de | Responsable | Verificación | Estado |
|---|---|---|---|---|---|
| T001 | Confirmar que `VirGo-API/` entró a `main` del repositorio `virgorent/virGO_App` con su `.gitignore` DevSecOps (Art. V.10) y su `README.md` en inglés | — | `devops-aws` | `git remote -v` muestra el repositorio; `git status` limpio tras el primer commit | ⏳ |
| T002 | ➡️ Movida a `002-pipeline-cicd` como **T207** (ruleset `main-protection`) | — | — | — | ➡️ |
| T003 | [P] Crear `.editorconfig` y `Directory.Build.props` (net10.0, `Nullable`, `AnalysisLevel=latest-recommended`, advertencias de seguridad como error) | T001 | `dev-dotnet` | Los archivos existen y la compilación los aplica (T006) | ✅ Hecha |
| T004 | [P] Revisar la alineación de `constitution.md`, `AGENTS.md`, subagentes, skills y specs con la skill `verificar-constitucion` | — | `arquitecto` | Tabla de resultados «Alineado» registrada en `memoria.md` | ⏳ |
| T005 | [P] Agregar en la raíz del repositorio `.github/pull_request_template.md` (lista con el ID de tarea y los artículos revisados) y `.github/dependabot.yml` (NuGet semanal, Actions mensual) | T001 | `devops-aws` | Archivos creados; activación de Dependabot pendiente de GitHub | ✅ Hecha |

## F1 · Solución y capas

| ID | Tarea | Depende de | Responsable | Verificación | Estado |
|---|---|---|---|---|---|
| T006 | Crear `VirGo.slnx` y los proyectos `VirGo.Api`, `VirGo.Application`, `VirGo.Domain` y `VirGo.Infrastructure` (.NET 10) | T003 | `dev-dotnet` | `dotnet build -c Release` termina sin errores | ✅ Hecha |
| T007 | Configurar las referencias según la regla de dependencias (`Api → Application, Infrastructure`; `Infrastructure → Application`; `Application → Domain`) | T006 | `dev-dotnet` | `dotnet build` OK; `Domain.csproj` sin referencias a proyectos ni paquetes | ✅ Hecha |
| T008 | [P] Crear `tests/VirGo.UnitTests` y `tests/VirGo.IntegrationTests` (xUnit, `Microsoft.AspNetCore.Mvc.Testing`) y agregarlos a la solución | T006 | `qa` | `dotnet test` ejecuta 0 pruebas sin errores | ✅ Hecha |
| T009 | `Program.cs` mínimo: controladores, `MapHealthChecks("/health")`, `MapOpenApi()`, JSON en `camelCase` y `public partial class Program` | T007 | `dev-dotnet` | `dotnet run` → `GET /health` = 200 y `GET /openapi/v1.json` = 200 | ✅ Hecha |
| T010 | [P] Prueba de arquitectura: `VirGo.Domain` no depende de otros ensamblados del proyecto ni de paquetes externos | T008 | `qa` | La prueba pasa y falla si se agrega una referencia indebida | ✅ Hecha |

## F2 · Núcleo transversal

| ID | Tarea | Depende de | Responsable | Verificación | Estado |
|---|---|---|---|---|---|
| T011 | Opciones tipadas (`Persistence`, `Jwt`, `Cors`, `RateLimiting`) con `ValidateOnStart`; la llave JWT de menos de 32 bytes detiene el arranque | T009 | `dev-dotnet` | Arrancar con una llave corta falla con un mensaje claro (CA-05) | ✅ Hecha |
| T012 | Manejo global de excepciones y respuestas Problem Details (404, 401, 500, validación) con `traceId` y sin trazas | T009 | `dev-dotnet` | Ruta inexistente → 404 `application/problem+json` (CA-02) | ✅ Hecha |
| T013 | [P] `CorrelationIdMiddleware` y logging JSON estructurado con el *scope* de correlación | T009 | `dev-dotnet` | La respuesta trae `X-Correlation-Id`; el log de consola en JSON lo incluye | ✅ Hecha |
| T014 | [P] `SecurityHeadersMiddleware` y política CORS `MobileAndWeb` desde la configuración | T011 | `dev-dotnet` | Los encabezados aparecen en `/health`; un origen no permitido no recibe `Access-Control-Allow-Origin` | ✅ Hecha |
| T015 | [P] Rate limiting global por IP configurable (429 en Problem Details) | T012 | `dev-dotnet` | Superar el límite configurado en pruebas devuelve 429 | ✅ Hecha |
| T016 | Autenticación JWT Bearer (emisor, audiencia, expiración y llave) y ruta `/api/v1/_diagnostics/secure` solo en el ambiente `Testing`; revisión de `seguridad` | T011, T012 | `dev-dotnet` + `seguridad` | Sin token → 401 Problem Details (CA-04); reporte de `seguridad` sin hallazgos altos | 🔄 En curso |

## F3 · Persistencia base

| ID | Tarea | Depende de | Responsable | Verificación | Estado |
|---|---|---|---|---|---|
| T017 | Abstracciones en `VirGo.Application/Abstractions` (`IRepository<T>`, `IClock`) y `Result<T>` y paginación en `Common` | T007 | `dev-dotnet` | Compila; sin referencias a AWS en Application | ✅ Hecha |
| T018 | [P] Implementación en memoria (`Persistence/InMemory`), segura para concurrencia | T017 | `dev-dotnet` | Pruebas unitarias del repositorio en memoria en verde (T025) | ✅ Hecha |
| T019 | [P] Implementación base de MongoDB (`Persistence/MongoDb`): `MongoClient` singleton, opciones (`DatabaseName`, `ConnectionString`), repositorios y selector `Persistence:Provider` en `AddInfrastructure()`; sin DDL al arrancar | T017 | `dev-dotnet` | Con `Provider=InMemory` la app arranca sin Atlas; con `MongoDb` se conecta al contrato existente sin crear colecciones ni índices | 🔄 En curso |

## F4 · Alojamiento en Lambda e infraestructura ➡️ `001-infraestructura-iac`

| Antes | Ahora | Tarea |
|---|---|---|
| T020 | `001`: T110 | Runtime de Lambda y `AddAWSLambdaHosting` |
| T021 | `001`: T102 | `infra/bootstrap.yaml` |
| T022 | `001`: T103 | `infra/template.yaml` |
| T023 | `001`: T104 | Costo estimado en `memoria.md` |
| T024 | `001`: T105 | Revisión de seguridad de la infraestructura |

## F5 · Pruebas

| ID | Tarea | Depende de | Responsable | Verificación | Estado |
|---|---|---|---|---|---|
| T025 | [P] Pruebas unitarias: validación de opciones (llave corta, orígenes vacíos en producción) y repositorio en memoria | T011, T018 | `qa` | `dotnet test tests/VirGo.UnitTests` en verde | ⏳ |
| T026 | [P] Pruebas de integración: `/health` 200; 404, 401 y 500 en Problem Details sin trazas; encabezados de seguridad; `X-Correlation-Id`; 429; `/openapi/v1.json` 200 | T012–T016 | `qa` | `dotnet test tests/VirGo.IntegrationTests` en verde (CA-02, CA-03, CA-04, CA-09) | ✅ Hecha |
| T027 | Prueba de arranque local con un solo comando y sin AWS (CA-01, RNF-07) | T019, T026 | `qa` | `dotnet run --project src/VirGo.Api` → `/health` 200 con `Provider=InMemory` | ✅ Hecha |

## F6 · CI/CD y entrega de la URL HTTPS ➡️ `002-pipeline-cicd`

| Antes | Ahora | Tarea |
|---|---|---|
| T028 | `002`: T204 | `api-ci.yml` (ahora con pruebas de seguridad, mutación, Gitleaks, *dependency review* y Checkov) |
| T029 | `002`: T205 | CodeQL |
| T030 | `001`: T107 | 🧑 Stack `bootstrap`, variables de GitHub y llave JWT en SSM |
| T031 | `002`: T208 | `api-deploy.yml` **manual** (`workflow_dispatch`) y `smoke.sh` |
| T032 | `002`: T209 | 🧑 Primer despliegue y verificación |
| T033 | `002`: T210 | Entrega de la URL HTTPS a Móvil |

## F7 · Cierre del módulo

| ID | Tarea | Depende de | Responsable | Verificación | Estado |
|---|---|---|---|---|---|
| T034 | ➡️ Movida a `001-infraestructura-iac` como **T108** (prueba de borrado) | — | — | — | ➡️ |
| T036 | Regla global de longitudes: atributos de longitud en todos los DTO con los límites de `db/schema/`, y 400 en Problem Details al superarlos | T012 | `dev-dotnet` | Prueba de seguridad: un campo de 1500 caracteres recibe 400 | ⏳ |
| T035 | Completar `check.md`; verificación final con `verificar-constitucion`; marcar el spec, el plan y las tareas como *Cerrado*; entrada de cierre en `memoria.md` | T001–T027, T036, `001`: T109, `002`: T211, `007`: T706 | `qa` + `arquitecto` | Resultado «Alineado» y todos los criterios de aceptación con evidencia | ⏳ |

---

## Matriz de trazabilidad

Los IDs con prefijo `001:` o `002:` están en esos módulos.

| Requerimiento / Criterio | Tareas |
|---|---|
| RF-01 Capas | T006, T007, T010 |
| RF-02 Ejecución dual local/nube | T009, `001`:T110, T027, `002`:T209 |
| RF-03 Chequeo de salud | T009, T026, `002`:T209 |
| RF-04 Prefijo `/api/v1` | T016, T026 |
| RF-05 Problem Details | T012, T015, T026 |
| RF-06 OpenAPI | T009, T026, `002`:T210 |
| RF-07 Persistencia intercambiable | T017, T018, T019 |
| RF-08 Validación de tokens | T016, T026 |
| RF-09 Configuración validada | T011, T025 |
| RF-10 Correlación | T013, T026 |
| RF-11 Proyectos de pruebas | T008, T025, T026, `002`:T202 |
| RF-12 CI en PR ➡️ | `002`:T204, `002`:T205 |
| RF-13 CD y URL HTTPS ➡️ | `002`:T208, `002`:T209 |
| RF-14 Infraestructura como código ➡️ | `001`:T102, `001`:T103, `001`:T108 |
| RF-15 Alerta de presupuesto ➡️ | `001`:T102, `001`:T107 |
| RF-16 Longitudes | T036, `007`:T702 |
| RNF-01 Costo $0 | `001`:T103, `001`:T104, `002`:T209 |
| RNF-02 Seguridad | `002`:T207, T014, T016, `001`:T105, `002`:T205, `001`:T107 |
| RNF-03 Arranque < 3 s | `002`:T209 |
| RNF-04 Límite de solicitudes | T015, `001`:T103 |
| RNF-05 Sin advertencias de seguridad | T003, `002`:T204 |
| RNF-06 Trazabilidad | `001`:T104, `002`:T209, T035 |
| RNF-07 Arranque local simple | T027 |
| CA-01 … CA-10 | T027 · T026 · T026 · T016/T026 · T011/T025 · `002`:T204 · `002`:T209 · `002`:T209 · T026/`002`:T210 · `001`:T108 |
