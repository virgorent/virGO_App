# Check — 000 Arquitectura base del Backend

Lista de verificación del módulo (Art. I.4). Cada casilla lleva su evidencia. El módulo se marca *Cerrado* solo con todas las casillas en ✅ o ➖ justificado. Los criterios marcados con ➡️ se verifican en `001` o `002`; aquí solo se enlaza su evidencia.

| Campo | Valor |
|---|---|
| Módulo | `000-arquitectura-base` |
| Estado del spec | Implementación local; cierre pendiente |
| Revisó | — |
| Fecha de cierre | — |

## 1. Especificación y gobernanza

- [ ] Spec, plan y tareas aprobados por el profesor y el equipo.
- [ ] El Constitution Check del plan no tiene ❌ (constitución v1.2.0 aprobada).
- [ ] `README.md` en inglés y `.gitignore` DevSecOps en el primer commit (T001).
- [ ] `verificar-constitucion` sobre la gobernanza: «Alineado» (T004).
- [ ] Los MCP de `opencode.json` están instalados antes de codificar (`001`: T100, `002`: T200).

## 2. Criterios de aceptación

| CA | Evidencia | Estado |
|---|---|---|
| CA-01 Arranque local sin nube | `dotnet run --project src/VirGo.Api --no-restore --urls http://127.0.0.1:5180`; `curl /health` = 200 | ✅ |
| CA-02 404 en Problem Details | `VirGo.IntegrationTests.BaseApiTests.MissingRouteReturnsProblemDetailsWithTraceId` | ✅ |
| CA-03 500 sin trazas y con correlación | `VirGo.IntegrationTests.BaseApiTests.UnexpectedFailureReturnsSafeProblemDetails` | ✅ |
| CA-04 401 sin token | `VirGo.IntegrationTests.BaseApiTests.ProtectedRouteWithoutTokenReturnsUnauthorizedProblemDetails` | ✅ |
| CA-05 Llave corta detiene el arranque | `dotnet run` con `Jwt__SigningKey=short` termina con `OptionsValidationException` | ✅ |
| CA-06 ➡️ CI bloquea la fusión | `002`: check CA-02 | ⏳ |
| CA-07 ➡️ Despliegue con URL HTTPS | `002`: check CA-05 | ⏳ |
| CA-08 ➡️ Solo servicios permitidos y presupuesto | `001`: check CA-03 y CA-04 | ⏳ |
| CA-09 OpenAPI publicado | `VirGo.IntegrationTests.BaseApiTests.OpenApiIsPublished`; entrega a Móvil pendiente | 🔄 |
| CA-10 ➡️ Borrado de la infraestructura | `001`: check CA-07 | ⏳ |

## 3. Pruebas

| Tipo | Evidencia | Estado |
|---|---|---|
| Unitarias | `dotnet test tests/VirGo.UnitTests` · total: 2 | ✅ |
| Integración | `dotnet test tests/VirGo.IntegrationTests` · total: 18 | ✅ |
| Seguridad | `dotnet test tests/VirGo.SecurityTests` · total: 3; ampliación y CI en `002`: T202 | 🔄 |
| Mutación | Stryker sobre `Domain` y `Application` (`002`: T203) · puntaje: — % | ⏳ |
| Humo | `smoke.sh` tras el primer despliegue (`002`: T209) | ⏳ |
| Arquitectura | Prueba de dependencias de `VirGo.Domain` (T010) | ⏳ |
| Validación de longitudes | Regla global en DTO y `db/schema/` (T036) | ⏳ |

## 4. PR e integración continua

- [ ] PRs del módulo: enlaces —
- [ ] Todos con los checks requeridos en verde y una revisión.

## 5. Despliegue

- [ ] URL HTTPS: —
- [ ] Commit desplegado: —
- [ ] Arranque en frío medido: — s (RNF-03)

## 6. Memoria y aprendizaje

- [ ] Entrada de cierre en `memoria.md` con lo aprendido.
- [ ] Mejoras propuestas a la constitución, `AGENTS.md`, skills o subagentes (o «ninguna»).
