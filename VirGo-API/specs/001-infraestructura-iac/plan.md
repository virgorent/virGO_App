# Plan — 001 Infraestructura como código (IaC)

| Campo | Valor |
|---|---|
| Módulo | `001-infraestructura-iac` |
| Versión | 1.0.0 |
| Estado | Implementación local; validación y despliegue pendientes |
| Fecha | 2026-11-03 |
| Spec | [spec.md](spec.md) · Tareas: [task.md](task.md) · Verificación: [check.md](check.md) |
| Constitución | v1.2.0 |

## 1. Resumen técnico

La infraestructura de AWS se escribe con **AWS SAM** en dos plantillas, y la base de datos es un clúster **MongoDB Atlas M0** que se crea una sola vez:

- `infra/bootstrap.yaml` (CloudFormation, se crea una sola vez a mano): proveedor OIDC de GitHub, rol de despliegue y presupuesto.
- `infra/template.yaml` (SAM, lo despliega el pipeline): HTTP API, Lambda, parámetros de SSM, alarmas y logs.
- `infra/atlas.md` (arranque documentado, una sola vez): clúster M0, usuarios y lista de acceso de Atlas, creados con el MCP de MongoDB o la interfaz de Atlas.

Se valida con `sam validate --lint`, `cfn-lint` y **Checkov** antes de desplegar. Los MCP `aws-docs` y `mongodb` se instalan antes de escribir las plantillas.

## 2. Contexto técnico

| Aspecto | Decisión |
|---|---|
| IaC | AWS SAM (`AWS::Serverless-2016-10-31`) y CloudFormation |
| Región | `us-east-1` |
| Nombre del stack | `virgo-api-prod` (aplicación) y `virgo-bootstrap` (arranque) |
| Validación | `sam validate --lint`, `cfn-lint`, `checkov -d infra` |
| Base de datos | MongoDB Atlas M0, proveedor AWS, región `us-east-1`, clúster `virgo-dev` |
| MCP de apoyo | `aws-docs` (solo lectura) y `mongodb` (usuario `virgo-mcp` de solo lectura; herramientas de Atlas solo en T101) |

## 3. Constitution Check

| Art. | Tema | Resultado | Cómo se cumple |
|---|---|---|---|
| I | SDD | ✅ Cumple | Spec atómico; cuatro archivos; tareas `T1NN`. |
| II | Modularidad | ✅ Cumple | No contiene funcionalidad del negocio; solo el clúster vacío. |
| III | Arquitectura | ➖ No aplica | No hay código de la aplicación, salvo el alojamiento en Lambda (T110). |
| IV | Contrato | ✅ Cumple | El prefijo `/api/v1` y CORS se respetan en la HTTP API. |
| V | Seguridad | ⚠️ Justificado | HTTPS, OIDC, roles de mínimo privilegio, SSM SecureString, Checkov. Atlas admite `0.0.0.0/0` porque Lambda no tiene IP fija sin NAT (prohibido); se compensa según el Art. V.11. |
| VI | Pruebas | ✅ Cumple | Validación estática en el CI y verificación del stack desplegado. |
| VII | Costo cero | ✅ Cumple | Solo servicios permitidos (sección 7); presupuesto de $1; *throttling*; Atlas M0 sin método de pago. |
| VIII | CI/CD | ✅ Cumple | El despliegue lo ejecuta el workflow manual de `002`. |
| IX | Observabilidad | ✅ Cumple | Logs de 7 días, alarmas de 5xx y de errores de la Lambda, y alertas de Atlas. |
| X | Memoria | ✅ Cumple | Costo estimado registrado antes de crear (T104). |
| XI | Agentes y MCP | ✅ Cumple | Tareas de `devops-aws` y `seguridad`; MCP en la lista permitida; 🧑 en los pasos que crean recursos. |
| XII | Idioma | ✅ Cumple | Recursos y parámetros en inglés; documentación en español. |

## 4. Estructura

```text
infra/
├── bootstrap.yaml      # OIDC, rol virgo-github-deploy, presupuesto de $1 USD
├── template.yaml       # SAM: HTTP API, Lambda, parámetros SSM, alarmas, SNS, LogGroup
├── atlas.md            # Arranque de MongoDB Atlas: clúster, usuarios, acceso y alertas
├── samconfig.toml      # Parámetros no secretos del despliegue (stack, región, resolve_s3)
└── .checkov.yaml       # Reglas omitidas con su justificación
```

## 5. Decisiones técnicas (ADR)

Se conservan ADR-01 (Lambda + HTTP API), ADR-02 (runtime), ADR-05 (OIDC), ADR-06 (SAM) y ADR-07 (SSM) de `000`. ADR-03 se reemplaza por ADR-23. Se agregan:

| ADR | Decisión | Alternativas descartadas | Motivo |
|---|---|---|---|
| ADR-23 | **MongoDB Atlas M0** en AWS `us-east-1` | DynamoDB, RDS, Atlas Flex | El equipo trabaja la base en MongoDB; M0 es gratis permanente y queda en la misma región que la Lambda |
| ADR-24 | Lista de acceso de Atlas `0.0.0.0/0` con controles compensatorios | NAT Gateway con IP fija; *private endpoint* | El NAT cuesta por hora (prohibido) y M0 no admite *private endpoints* ni *peering* |
| ADR-11 | **Índice TTL de MongoDB** en `sessions.purgeAt` | Borrado programado con otra Lambda | Gratis; las sesiones expiradas se borran solas |
| ADR-12 | **Checkov** para analizar la IaC | Solo `cfn-lint` | `cfn-lint` revisa sintaxis; Checkov revisa seguridad (cifrado, IAM, logs) |
| ADR-13 | **SNS + CloudWatch Alarms** para alertas | Sin alarmas | 10 alarmas y 1 000 correos al mes son gratis |

## 6. Diseño

### 6.1 Servicios de AWS (12)

| # | Servicio | Recurso | Para qué |
|---|---|---|---|
| 1 | API Gateway HTTP API | `VirGoHttpApi` | Entrada HTTPS, *throttling* 20 rps / ráfaga 40, CORS |
| 2 | Lambda | `VirGoApiFunction` (512 MB, 15 s, arm64) | Ejecuta `VirGo.Api` |
| 3 | MongoDB Atlas M0 (externo) | `virgo-dev`, bases `virgo` y `virgo_dev` | 512 MB; TLS obligatorio; cifrado en reposo por defecto; usuarios `virgo-api-prod` (`readWrite@virgo`), `virgo-api-dev` (`readWrite@virgo_dev`) y `virgo-mcp` (`read` en ambas) |
| 4 | SSM Parameter Store | `/virgo/prod/jwt-signing-key`, `/virgo/prod/mongo-uri` | Llave JWT (≥ 32 bytes) y cadena de conexión de Atlas, ambas SecureString |
| 5 | KMS | Llave administrada `aws/ssm` | Cifra el SecureString, sin costo |
| 6 | IAM | `virgo-github-deploy`, rol de ejecución de la Lambda | Mínimo privilegio |
| 7 | STS | `AssumeRoleWithWebIdentity` | Credenciales temporales para GitHub Actions |
| 8 | CloudFormation | `virgo-bootstrap`, `virgo-api-prod` | Motor de SAM |
| 9 | S3 | Bucket administrado por SAM (`--resolve-s3`) | Paquetes de despliegue |
| 10 | CloudWatch Logs | `/aws/lambda/virgo-api-prod` | Registros JSON, 7 días |
| 11 | CloudWatch Alarms + alertas de Atlas | `Api5xxAlarm`, `LambdaErrorsAlarm`; en Atlas: conexiones > 400 y espacio > 400 MB | Avisos de fallas |
| 12 | SNS + AWS Budgets | `virgo-alerts` y presupuesto de $1 | Correo de alarmas y de gasto |

### 6.2 Rol de despliegue (`virgo-github-deploy`)

- Confianza: `token.actions.githubusercontent.com`, condición `sub = repo:virgorent/virGO_App:ref:refs/heads/main` y `aud = sts.amazonaws.com`. El *environment* `production` de `002` usa `sub = repo:virgorent/virGO_App:environment:production`; se permite solo ese valor.
- Permisos: CloudFormation sobre `virgo-api-*`, `iam:PassRole` solo del rol de la Lambda, S3 del bucket de SAM y `ssm:GetParameter` de `/virgo/prod/*`. Nada de `*:*`.

### 6.3 Llave JWT y cadena de conexión

Ambas se crean una vez con `aws ssm put-parameter --type SecureString` (T107). La cadena de conexión usa `mongodb+srv://`, el usuario `virgo-api-prod` y `tls=true`. La plantilla las recibe como referencia dinámica `{{resolve:ssm-secure:/virgo/prod/jwt-signing-key}}` cuando el tipo de recurso lo permite; si no, la Lambda las lee al arrancar con permiso `ssm:GetParameter` sobre esos dos parámetros.

## 7. Costo estimado

| Recurso | Capa gratuita | Costo estimado al mes |
|---|---|---|
| Lambda | 1 M solicitudes y 400 000 GB-s (permanente) | $0 |
| HTTP API | 1 M llamadas (12 meses o créditos) | $0 |
| MongoDB Atlas M0 | 512 MB, gratis permanente; organización sin método de pago | $0 |
| SSM estándar + KMS `aws/ssm` | Gratis | $0 |
| CloudWatch Logs (7 días) | 5 GB | $0 |
| CloudWatch Alarms (2) y alertas de Atlas | 10 alarmas; alertas de Atlas incluidas | $0 |
| SNS (correo) | 1 000 correos | $0 |
| Budgets | 2 presupuestos | $0 |
| S3 de SAM | 5 GB (12 meses o créditos) | ≈ $0 |
| IAM, STS, CloudFormation | Sin costo | $0 |
| **Total** | | **$0 USD** |

## 8. Estrategia de pruebas

| Nivel | Herramienta | Qué cubre |
|---|---|---|
| Estática | `sam validate --lint`, `cfn-lint` | Sintaxis y propiedades válidas |
| Seguridad | `checkov -d infra` | Cifrado, IAM sin comodines, logs con retención |
| Base de datos | MCP `mongodb` y `mongosh` | El usuario de la API no accede a otra base; sin TLS se rechaza la conexión (CA-09) |
| Despliegue | Prueba de humo de `002` | URL `https://` y `/health` 200 |
| Borrado | Stack temporal `virgo-api-test` | `sam delete` no deja recursos |

## 9. Riesgos y mitigaciones

| Riesgo | Mitigación |
|---|---|
| Se agotan los créditos del plan gratuito | Presupuesto de $1, fecha de revisión (P-02) y `sam delete` |
| El rol de despliegue tiene permisos de más | Revisión de `seguridad` (T105) y Checkov |
| Un MCP con credenciales escribe en producción | Usuario `virgo-mcp` con rol `read` y MCP en modo solo lectura (Art. XI.6) |
| Se llena el espacio de M0 (512 MB) | Alerta a 400 MB, índice TTL en sesiones y revisión del tamaño al cerrar cada módulo |
| Alguien registra una tarjeta en Atlas y sube de nivel | Art. VII.3: organización sin método de pago; revisión en el `check.md` |

## 10. Fases

| Fase | Contenido | Tareas |
|---|---|---|
| F0 | MCP, Atlas y alojamiento | T100, T101, T110 |
| F1 | Plantillas | T102–T104 |
| F2 | Seguridad y validación | T105–T106 |
| F3 | Arranque en la nube | T107 |
| F4 | Borrado y cierre | T108–T109 |
