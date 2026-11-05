# Check — 004 Cerrar sesión (logout)

Lista de verificación del módulo (Art. I.4). Cada casilla lleva su evidencia. El spec se marca *Cerrado* solo con todas las casillas en ✅ o ➖ justificado.

| Campo | Valor |
|---|---|
| Módulo | `004-auth-logout` · `POST /api/v1/auth/logout` |
| Estado del spec | Implementación local; despliegue pendiente |
| Revisó | — |
| Fecha de cierre | — |

## 1. Especificación

- [ ] El spec es atómico: solo el logout de la sesión actual.
- [ ] Cada RF y cada CA están cubiertos en la matriz de trazabilidad.
- [ ] El Constitution Check del plan no tiene ❌.
- [ ] P-01 resuelta con el equipo Móvil.

## 2. Criterios de aceptación

| CA | Evidencia | Estado |
|---|---|---|
| CA-01 204 y documento de sesión eliminado | `AuthTests.LoginThenLogoutInvalidatesOnlyThatSession` | ✅ |
| CA-02 Token de sesión cerrada → 401 en toda la API | `AuthTests.LoginThenLogoutInvalidatesOnlyThatSession` | ✅ |
| CA-03 La otra sesión sigue funcionando | `AuthTests.LogoutKeepsASecondSessionActive` | ✅ |
| CA-04 Sin token o inválido → 401 | `BaseApiTests.ProtectedRouteWithoutTokenReturnsUnauthorizedProblemDetails` | ✅ |
| CA-05 Token sin `sid` → 401 | `JwtSecurityTests.TokenWithoutSessionIdIsRejected` | ✅ |
| CA-06 Humo 401 por HTTPS | Resumen de `api-deploy.yml` | ⏳ |

## 3. Pruebas

| Tipo | Evidencia | Estado |
|---|---|---|
| Unitarias | Cubiertas por el proyecto base; casos específicos pendientes | 🔄 |
| Integración | Incluidas en las 12 pruebas aprobadas | ✅ |
| Seguridad | 3 aprobadas · revisión de `seguridad` pendiente | 🔄 |
| Mutación | Línea base general: 36.36 % · umbral provisional 0 % | 🔄 |
| Humo | Caso de logout en `smoke.sh` en verde | ⏳ |
| Validación de longitudes | ➖ Sin cuerpo; `Authorization` de 1500 caracteres da 401 | ⏳ |

## 4. PR e integración continua

- [ ] PR: enlace —
- [ ] Todos los checks requeridos en verde y una revisión aprobada.

## 5. Despliegue y entrega a Móvil

- [ ] Ejecución de `api-deploy.yml`: enlace — · commit —
- [ ] Latencia agregada medida: — ms · operaciones por segundo en Atlas: —
- [ ] Contrato entregado a Móvil y registrado en ambas memorias.

## 6. Memoria y aprendizaje

- [ ] Spec marcado como *Desplegado* y luego *Cerrado*.
- [ ] Entrada en `memoria.md` con lo aprendido.
- [ ] Mejoras propuestas a la constitución, `AGENTS.md`, skills o subagentes (o «ninguna»).
