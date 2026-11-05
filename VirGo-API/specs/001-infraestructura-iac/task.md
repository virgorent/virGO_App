# Tareas — 001 Infraestructura como código (IaC)

| Campo | Valor |
|---|---|
| Módulo | `001-infraestructura-iac` |
| Versión | 1.0.0 |
| Estado | Implementación local parcial; tareas externas pendientes |
| Fecha | 2026-11-03 |
| Spec / Plan / Check | [spec.md](spec.md) · [plan.md](plan.md) · [check.md](check.md) |
| Constitución | v1.2.0 |

**Estados:** ⏳ Pendiente · 🔄 En curso · ✅ Hecha (con evidencia) · ⛔ Bloqueada

**[P]** = puede ejecutarse en paralelo. **🧑** = requiere confirmación humana (Art. XI.5).

**Equivalencias con `000`:** T110 = antes T020 · T102 = T021 · T103 = T022 · T104 = T023 · T105 = T024 · T107 = T030 · T108 = T034.

---

## F0 · MCP, Atlas y alojamiento

| ID | Tarea | Depende de | Responsable | Verificación | Estado |
|---|---|---|---|---|---|
| T100 | Instalar y probar los MCP `aws-docs` y `mongodb` de `opencode.json` | — | `devops-aws` | `opencode mcp list` los muestra conectados; captura en `memoria.md` | ⏳ |
| T101 | 🧑 Completar en MongoDB Atlas (organización sin método de pago; el clúster ya existe) el clúster M0 `virgo-dev` en AWS `us-east-1`; los usuarios `virgo-api-prod`, `virgo-api-dev` y `virgo-mcp` (sección 6.1 del plan); la lista de acceso y las alertas; documentarlo en `infra/atlas.md` | T100 | `devops-aws` | `mongosh` conecta por TLS con cada usuario; `virgo-mcp` no puede escribir; captura del nivel M0 (CA-03, CA-09) | ⏳ |
| T110 | Verificar si existe el runtime `dotnet10` administrado en `us-east-1` (ADR-02), registrar el resultado y agregar `AddAWSLambdaHosting(LambdaEventSource.HttpApi)` | `000`: T009 | `devops-aws` | Decisión en `memoria.md`; la app sigue arrancando en local | ⏳ |

## F1 · Plantillas

| ID | Tarea | Depende de | Responsable | Verificación | Estado |
|---|---|---|---|---|---|
| T102 | [P] `infra/bootstrap.yaml`: proveedor OIDC, rol `virgo-github-deploy` (condiciones de la sección 6.2 del plan) y presupuesto de $1 USD con correo | — | `devops-aws` | `cfn-lint infra/bootstrap.yaml` sin errores | 🔄 En curso |
| T103 | [P] `infra/template.yaml`: HTTP API (throttling 20/40, CORS), Lambda 512 MB, LogGroup de 7 días, tema SNS, alarmas `Api5xxAlarm` y `LambdaErrorsAlarm`, parámetros de la llave JWT y de la cadena de Mongo, y salida `ApiUrl` | T110 | `devops-aws` | `sam validate --lint` sin errores | 🔄 En curso |
| T104 | Registrar en `memoria.md` la tabla de costo estimado de los 12 servicios, incluido Atlas M0 (Art. VII.5) | T101, T102, T103 | `devops-aws` | Entrada con total $0 | ✅ Hecha |

## F2 · Seguridad y validación

| ID | Tarea | Depende de | Responsable | Verificación | Estado |
|---|---|---|---|---|---|
| T105 | Revisión de seguridad de IAM, OIDC, CORS, manejo de secretos y del acceso a Atlas (usuarios, roles, lista de acceso, TLS) | T101, T102, T103 | `seguridad` | Reporte sin hallazgos críticos ni altos (CA-05, CA-06) | ⏳ |
| T106 | `checkov -d infra` y `infra/.checkov.yaml` con cada regla omitida justificada | T103 | `devops-aws` | Checkov sin fallos altos sin justificar (CA-01) | ⏳ |

## F3 · Arranque en la nube

| ID | Tarea | Depende de | Responsable | Verificación | Estado |
|---|---|---|---|---|---|
| T107 | 🧑 Crear el stack `virgo-bootstrap`; registrar `AWS_ROLE_ARN` y `AWS_REGION` en el *environment* `production` de GitHub; crear `/virgo/prod/jwt-signing-key` (SecureString, 32 bytes aleatorios o más) y `/virgo/prod/mongo-uri` (SecureString, usuario `virgo-api-prod`) | T105, T106 | `devops-aws` | Stack `CREATE_COMPLETE`; correo del presupuesto confirmado (CA-04); variables visibles en GitHub | ⏳ |

> El primer despliegue del stack `virgo-api-prod` y la verificación de CA-02, CA-03 y CA-08 se hacen en `002-pipeline-cicd` (T209), porque usan el workflow manual.

## F4 · Borrado y cierre

| ID | Tarea | Depende de | Responsable | Verificación | Estado |
|---|---|---|---|---|---|
| T108 | 🧑 Prueba de borrado con el stack temporal `virgo-api-test`: `sam deploy` y después `sam delete` | T107 | `devops-aws` | Ningún recurso de `virgo-api-test` permanece (CA-07) | ⏳ |
| T109 | Completar `check.md`, ejecutar `verificar-constitucion`, marcar el módulo y registrar el cierre en `memoria.md` | T100–T108, T110, `002`: T209 | `qa` + `arquitecto` | `check.md` completo y resultado «Alineado» | ⏳ |

---

## Matriz de trazabilidad

| Requerimiento / Criterio | Tareas |
|---|---|
| RF-01 IaC | T102, T103 |
| RF-02 HTTPS | T103, `002`: T209 |
| RF-03 Clúster de MongoDB y usuarios | T101 |
| RF-04 Llave y cadena de conexión en SSM | T103, T107 |
| RF-05 Throttling y CORS | T103 |
| RF-06 Presupuesto | T102, T107 |
| RF-07 Alarmas | T101, T103 |
| RF-08 Logs 7 días | T103 |
| RF-09 OIDC | T102, T105 |
| RF-10 Validación | T103, T106 |
| RF-11 Borrado | T108 |
| RF-12 Costo registrado | T104 |
| RNF-01 … RNF-05 | T104 · T103/T105 · T105/T106 · T108 · T103 |
| CA-01 … CA-09 | T106 · `002`:T209 · T101/`002`:T209 · T107 · T105 · T105 · T108 · T101/`002`:T209 · T101 |
