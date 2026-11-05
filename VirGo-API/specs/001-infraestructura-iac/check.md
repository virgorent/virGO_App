# Check — 001 Infraestructura como código (IaC)

Lista de verificación del módulo (Art. I.4). Se llena conforme avanzan las tareas; cada casilla lleva su evidencia (comando y salida, enlace al PR o al workflow, captura). El módulo se marca *Cerrado* solo con todas las casillas en ✅ o ➖ justificado.

| Campo | Valor |
|---|---|
| Módulo | `001-infraestructura-iac` |
| Estado del spec | Implementación local; validación y despliegue pendientes |
| Revisó | — |
| Fecha de cierre | — |

## 1. Especificación

- [ ] El spec es atómico: solo infraestructura, sin workflows ni entidades del negocio.
- [ ] Cada RF y cada CA están cubiertos en la matriz de trazabilidad del `task.md`.
- [ ] El Constitution Check del plan no tiene ❌.
- [ ] Las preguntas abiertas P-01 y P-02 están respondidas.
- [ ] Los MCP del módulo (`aws-docs`, `mongodb`) están instalados y probados (T100).

## 2. Criterios de aceptación

| CA | Evidencia | Estado |
|---|---|---|
| CA-01 Validación sin errores | Salida de `sam validate --lint`, `cfn-lint` y `checkov` | ⏳ |
| CA-02 URL `https://` y `/health` 200 | Resumen del workflow de despliegue (`002`: T209) | ⏳ |
| CA-03 Solo servicios permitidos; Atlas M0 sin método de pago | Lista de recursos del stack y captura del nivel M0 y de la facturación de Atlas | ⏳ |
| CA-04 Presupuesto de $1 activo | Captura de AWS Budgets y del correo de confirmación | ⏳ |
| CA-05 Sin secretos en el repositorio | Salida de Gitleaks sobre todo el historial | ⏳ |
| CA-06 OIDC limitado a `main`/`production` | Política de confianza del rol y prueba de rechazo | ⏳ |
| CA-07 Borrado completo | Salida de `sam delete` y consola sin recursos de `virgo-api-test` | ⏳ |
| CA-08 Alarmas activas | Lista de alarmas, suscripción SNS confirmada y alertas de Atlas | ⏳ |
| CA-09 Usuario de la API limitado y solo TLS | `mongosh` con `virgo-api-prod` sobre otra base → denegado; sin TLS → rechazado | ⏳ |

## 3. Pruebas

| Tipo | Aplica | Evidencia | Estado |
|---|---|---|---|
| Unitarias | ➖ | No hay código de la aplicación en este módulo | ➖ |
| Integración | ➖ | Cubiertas por `000` | ➖ |
| Seguridad | ✅ | Checkov y revisión de `seguridad` (T105, T106) | ⏳ |
| Mutación | ➖ | No hay lógica que mutar | ➖ |
| Humo | ✅ | `/health` por HTTPS tras el primer despliegue (`002`: T209) | ⏳ |

## 4. PR e integración continua

- [ ] PR: enlace —
- [ ] Checks del CI en verde, incluidos `sam validate` y Checkov.
- [ ] Una revisión aprobada; sin hallazgos críticos ni altos de `seguridad`.

## 5. Despliegue

- [ ] Clúster Atlas M0 `virgo-dev` creado y documentado en `infra/atlas.md` (T101).
- [ ] Stack `virgo-bootstrap` en `CREATE_COMPLETE` (T107).
- [ ] Stack `virgo-api-prod` desplegado por el workflow manual; commit —
- [ ] URL HTTPS: —
- [ ] Costo real en AWS Cost Explorer: $0.

## 6. Memoria y aprendizaje

- [ ] Costo estimado registrado antes de crear recursos (T104).
- [ ] Entrada de cierre en `memoria.md` con lo aprendido.
- [ ] Mejoras propuestas a la constitución, `AGENTS.md`, skills o subagentes (o «ninguna»).
