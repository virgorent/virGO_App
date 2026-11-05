# Tareas — 004 Cerrar sesión (logout)

| Campo | Valor |
|---|---|
| Módulo | `004-auth-logout` |
| Versión | 1.0.0 |
| Estado | Implementación local; despliegue pendiente |
| Fecha | 2026-11-03 |
| Spec / Plan / Check | [spec.md](spec.md) · [plan.md](plan.md) · [check.md](check.md) |
| Constitución | v1.2.0 |

**Estados:** ⏳ Pendiente · 🔄 En curso · ✅ Hecha (con evidencia) · ⛔ Bloqueada

**[P]** = puede ejecutarse en paralelo. **🧑** = requiere confirmación humana (Art. XI.5).

**Rama:** `feat/004-auth-logout`. Si el CI falla, se corrige en esta misma rama (Art. I.3).

---

## F0 · Contexto

| ID | Tarea | Depende de | Responsable | Verificación | Estado |
|---|---|---|---|---|---|
| T400 | Confirmar que `003` está *Cerrado*, que los MCP responden y que P-01 está resuelta con Móvil | `003`: T312 | `arquitecto` | Entrada en `memoria.md` | ⏳ |

## F1 · Dominio, aplicación y API

| ID | Tarea | Depende de | Responsable | Verificación | Estado |
|---|---|---|---|---|---|
| T401 | Eliminación de la sesión y `LogoutCommand`/`LogoutHandler` (sección 6.2 del plan); eliminación en los repositorios en memoria y MongoDB | T400 | `dev-dotnet` | Pruebas unitarias de T404 en verde | ✅ Hecha |
| T402 | `SessionValidator` y `JwtSessionEvents.OnTokenValidated` (sección 6.3) | T400 | `dev-dotnet` + `seguridad` | Un JWT sin `sid` recibe 401 en la ruta de diagnóstico | ✅ Hecha |
| T403 | `AuthController.Logout` (`[Authorize]`, 204) y documentación en OpenAPI | T401, T402 | `dev-dotnet` | `dotnet run` → login, logout = 204, mismo token = 401 | ✅ Hecha |

## F2 · Pruebas

| ID | Tarea | Depende de | Responsable | Verificación | Estado |
|---|---|---|---|---|---|
| T404 | Pruebas unitarias e integración de la sección 8 del plan | T403 | `qa` | `dotnet test` en verde (CA-01 a CA-05) | 🔄 En curso |
| T405 | Pruebas de seguridad, mutación y caso nuevo en `smoke.sh`; revisión de `seguridad` | T404 | `qa` + `seguridad` | `VirGo.SecurityTests` en verde; mutación ≥ umbral; sin hallazgos altos | 🔄 En curso |

## F3 · PR, despliegue y entrega

| ID | Tarea | Depende de | Responsable | Verificación | Estado |
|---|---|---|---|---|---|
| T406 | PR con el CI en verde, revisión y *merge*; cambio de contrato en ambas memorias | T405 | `dev-dotnet` + `arquitecto` | PR fusionado | ⏳ |
| T407 | 🧑 Desplegar con *Run workflow*; humo del logout por HTTPS; medir la latencia agregada y las operaciones por segundo en las métricas de Atlas | T406 | `devops-aws` + `qa` | 401 por HTTPS (CA-06); latencia < 50 ms (RNF-02) | ⏳ |
| T408 | Entregar a Móvil la URL y el contrato probado; completar `check.md`; marcar *Desplegado*; registrar lo aprendido | T407 | `arquitecto` + `qa` | `check.md` completo; entrada en ambas memorias | ⏳ |

---

## Matriz de trazabilidad

| Requerimiento / Criterio | Tareas |
|---|---|
| RF-01 Requiere credencial | T403, T405 |
| RF-02 Revoca solo esa sesión | T401, T404 |
| RF-03 Rechazo en toda la API | T402, T404 |
| RF-04 Token de sesión inútil | T401 (y criterio obligatorio en `006`) |
| RF-05 Otras sesiones intactas | T404 |
| RF-06 204 sin cuerpo | T403, T404 |
| RF-07 OpenAPI | T403 |
| RNF-01 … RNF-03 | T402/T404 · T407 · T405 |
| CA-01 … CA-06 | T404 · T404 · T404 · T405 · T402/T405 · T407 |
