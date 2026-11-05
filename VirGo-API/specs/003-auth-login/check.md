# Check — 003 Iniciar sesión (login)

Lista de verificación del módulo (Art. I.4). Cada casilla lleva su evidencia. El spec se marca *Cerrado* solo con todas las casillas en ✅ o ➖ justificado.

| Campo | Valor |
|---|---|
| Módulo | `003-auth-login` · `POST /api/v1/auth/login` |
| Estado del spec | Implementación local; despliegue pendiente |
| Revisó | — |
| Fecha de cierre | — |

## 1. Especificación

- [ ] El spec es atómico: solo el login (sin registro, renovación ni logout).
- [ ] Cada RF y cada CA están cubiertos en la matriz de trazabilidad.
- [ ] El Constitution Check del plan no tiene ❌.
- [ ] P-01 (bloqueo) respondida; los campos se verifican contra `API-HANDOFF.md` y `VirGo-Database`.
- [ ] Los MCP `mongodb` y `github` responden (T300).

## 2. Criterios de aceptación

| CA | Evidencia | Estado |
|---|---|---|
| CA-01 200 con tokens y datos básicos | `AuthTests.LoginThenLogoutInvalidatesOnlyThatSession` | ✅ |
| CA-02 Correo con mayúsculas o espacios | `AuthTests.LoginNormalizesEmailCaseAndWhitespace` | ✅ |
| CA-03 Contraseña incorrecta → 401 genérico | `AuthTests.UnknownEmailReturnsGenericUnauthorizedResponse` | ✅ |
| CA-04 Correo inexistente → mismo 401 | `AuthTests.UnknownEmailReturnsGenericUnauthorizedResponse`; comparación con contraseña incorrecta pendiente | 🔄 |
| CA-05 Límite por IP → 429 | `RateLimitTests.SecondRequestInWindowReturnsProblemDetails429` | ✅ |
| CA-06 Datos inválidos → 400 con `errors` | `AuthTests.LoginRejectsInvalidInputWithFieldErrors` | ✅ |
| CA-07 Sesión creada; token no en texto plano | Login/logout de integración; inspección MCP pendiente | 🔄 |
| CA-08 Sin datos sensibles en logs | Prueba de seguridad con logger de prueba | ⏳ |
| CA-09 Humo 401 por HTTPS | Resumen de `api-deploy.yml` | ⏳ |

## 3. Pruebas

| Tipo | Evidencia | Estado |
|---|---|---|
| Unitarias | 2 aprobadas / 0 fallidas | ✅ |
| Integración | 12 aprobadas / 0 fallidas | ✅ |
| Seguridad | 2 aprobadas · Revisión de `seguridad` pendiente | 🔄 |
| Mutación | Línea base: 36.36 % · umbral 0 % provisional | 🔄 |
| Humo | Caso de login en `smoke.sh` en verde | ⏳ |
| Validación de longitudes | `email`, `password` y `device`: límite, límite + 1 y 1500 caracteres | ⏳ |

## 4. PR e integración continua

- [ ] PR: enlace —
- [ ] Todos los checks requeridos en verde.
- [ ] Una revisión aprobada.

## 5. Despliegue y entrega a Móvil

- [ ] Ejecución de `api-deploy.yml`: enlace — · commit —
- [ ] JSON de petición y respuestas 200/400/401/429 entregado a Móvil.
- [ ] Cambio de contrato en `memoria.md` de ambos proyectos.

## 6. Memoria y aprendizaje

- [ ] Spec marcado como *Desplegado* y luego *Cerrado*.
- [ ] Entrada en `memoria.md` con lo aprendido.
- [ ] Mejoras propuestas a la constitución, `AGENTS.md`, skills o subagentes (o «ninguna»).
