# Constitution — VirGo API (Backend)

**Versión:** 1.2.0 · **Ratificada:** 2026-11-04 · **Última enmienda:** 2026-11-04

Este documento norma el proyecto Backend de VirGo. Todos los demás archivos (`AGENTS.md`, `memoria.md`, subagentes, skills y specs) deben estar alineados con él. Si un archivo lo contradice, prevalece este documento y el otro archivo se corrige.

VirGo es una plataforma que conecta a quien organiza un evento con salones y proveedores de servicios. Este proyecto vive en la carpeta `VirGo-API/` del repositorio [`virgorent/virGO_App`](https://github.com/virgorent/virGO_App) y contiene solo la API que consumen la app móvil (`VirGo-Movil/`) y, en el futuro, la web.

Se subordina a la constitución global de la raíz (`../constitution.md`): esta desarrolla sus principios para el proyecto. En lo técnico del proyecto manda este documento; en los principios generales, la global.

---

## Artículo I. Desarrollo guiado por especificaciones (SDD)

1. **No se escribe código sin especificación.** Todo trabajo sigue el flujo `spec.md → plan.md → task.md → implementación`.
2. Cada etapa requiere aprobación antes de pasar a la siguiente:
   - El **spec** dice *qué* y *por qué*, sin detalles técnicos.
   - El **plan** dice *cómo*, y debe pasar el control constitucional (sección «Constitution Check»).
   - Las **tareas** son pasos pequeños, verificables y con un responsable.
3. Si durante la implementación aparece algo no previsto, se actualiza primero el spec o el plan y después el código.
4. Las especificaciones viven en `specs/NNN-nombre-del-modulo/` con exactamente cuatro archivos: `spec.md`, `plan.md`, `task.md` y `check.md`. El `check.md` es la lista de verificación del módulo: reúne la evidencia de cada criterio de aceptación, de cada tipo de prueba, del PR, del despliegue y de la memoria.
5. **Specs atómicos.** Cada spec cubre una sola capacidad: un endpoint o recurso (por ejemplo, `login` y `logout` van en specs separados), la infraestructura o el pipeline. Si un spec necesita «y además…», se divide.
6. Los IDs de tarea llevan el número del módulo para no repetirse: el módulo `000` usa `T001…T099`, el `001` usa `T101…T199`, el `002` usa `T201…` y así sucesivamente.
7. Un spec pasa por los estados *Borrador → En revisión → Aprobado → Desplegado → Cerrado*. Solo se marca *Cerrado* cuando su `check.md` está completo, su código está desplegado en la URL HTTPS y quedó registrado en `memoria.md`.

## Artículo II. Modularidad e iteraciones

1. El módulo `000-arquitectura-base` se construye primero. No contiene funcionalidad del negocio: solo la estructura, la configuración, la seguridad base, la infraestructura y el despliegue.
2. Los specs funcionales se pueden **redactar** (spec, plan, task y check) antes de terminar la base, porque alimentan el contexto de la arquitectura global y de los agentes. Sus tareas **no se ejecutan** hasta que `000-arquitectura-base`, `001-infraestructura-iac` y `002-pipeline-cicd` estén cerrados.
3. Cada módulo funcional define sus endpoints y declara qué subagentes y skills necesita.
4. Un módulo se cierra solo cuando todas sus tareas están verificadas y quedaron registradas en `memoria.md`. Así no quedan brechas estructurales entre iteraciones.

## Artículo III. Arquitectura

1. **Plataforma:** .NET 10 con ASP.NET Core Web API y controladores.
2. **Capas:** la dependencia solo va hacia adentro.

   | Capa | Proyecto | Responsabilidad |
   |---|---|---|
   | Presentación | `VirGo.Api` | Controladores, middleware, configuración, arranque |
   | Aplicación | `VirGo.Application` | Casos de uso, DTOs, validaciones, interfaces |
   | Dominio | `VirGo.Domain` | Entidades y reglas de negocio puras, sin dependencias externas |
   | Infraestructura | `VirGo.Infrastructure` | Persistencia (MongoDB), servicios de AWS, implementaciones |

3. **Inyección de dependencias** con el contenedor nativo de .NET. No se instancian servicios con `new` dentro de los controladores.
4. **Persistencia detrás de interfaces.** Hay una implementación en memoria para desarrollo y pruebas, y otra de **MongoDB** (driver oficial `MongoDB.Driver`) para la nube. El proveedor se elige por configuración. La fuente de verdad de campos, validadores e índices es `db/schema/`, mantenida por `VirGo-Database`; la API consume ese contrato y no ejecuta DDL al arrancar.
5. **Ejecución dual:** la misma aplicación corre localmente con Kestrel y en AWS Lambda. No se mantienen dos bases de código.

## Artículo IV. Contrato de la API (acuerdo con Móvil)

1. REST con JSON. Las propiedades van en `camelCase`.
2. **Versionado en la ruta:** `/api/v1/...`. Un cambio que rompa el contrato obliga a crear `/api/v2`.
3. Los errores se devuelven siempre como **Problem Details (RFC 9457)**, `application/problem+json`, con un `traceId`.
4. Las fechas van en ISO-8601 y en UTC. Los montos van en MXN como número decimal.
5. El documento **OpenAPI** generado por la API es la fuente de verdad del contrato y se publica en cada despliegue.
6. La paginación usa los parámetros `page` y `pageSize`, con un máximo de 100.
7. Cualquier cambio de contrato se anota en `memoria.md` y se avisa al proyecto Móvil.

## Artículo V. Seguridad (DevSecOps)

1. **Ningún secreto en el repositorio.** Las llaves y contraseñas viven en GitHub Secrets y en AWS SSM Parameter Store (SecureString).
2. **Solo HTTPS.** No se expone ningún endpoint por HTTP en la nube.
3. **Autenticación** con JWT firmado. La llave mide al menos 256 bits y la aplicación no arranca si es más corta.
4. **Mínimo privilegio:** cada función y rol de IAM tiene solo los permisos que usa. GitHub Actions entra a AWS con **OIDC**, sin llaves de acceso de larga duración.
5. Toda entrada se valida. Los errores no exponen trazas internas.
6. Hay límites de solicitudes en API Gateway y en la aplicación.
7. Se envían los encabezados de seguridad y CORS con una lista explícita de orígenes, nunca `*` en producción.
8. Los logs no contienen datos personales, contraseñas ni tokens.
9. Las dependencias se revisan con Dependabot y *dependency review*, y el código con análisis estático (CodeQL) en cada PR. El código se analiza además con Semgrep y SonarCloud, la API en ejecución con OWASP ZAP, la infraestructura como código con `cfn-lint` y Checkov, y el repositorio se escanea en busca de secretos (Gitleaks y la protección de push de GitHub).
10. El `.gitignore` existe desde el primer commit y excluye secretos (`.env`, llaves, certificados, credenciales de AWS, `appsettings.*.local.json`), artefactos de compilación y reportes. Agregar una exclusión de seguridad no requiere enmienda; quitarla, sí.
11. **MongoDB Atlas:** conexión solo por TLS; un usuario por ambiente con el rol mínimo (`readWrite` solo sobre su base de datos; el MCP usa un usuario de solo lectura); la cadena de conexión vive solo en SSM SecureString y en variables de entorno locales. Como Lambda no tiene IP fija sin un NAT Gateway (prohibido por costo), la lista de acceso de red de Atlas admite `0.0.0.0/0`; esto se compensa con TLS, contraseñas aleatorias de 32 caracteres o más, usuarios de mínimo privilegio y alertas de Atlas.

## Artículo VI. Calidad y pruebas

1. Cada tarea define cómo se verifica antes de empezarla.
2. Cada endpoint tiene cinco tipos de prueba, cada uno en su carpeta:

   | Tipo | Carpeta o herramienta | Qué verifica |
   |---|---|---|
   | Unitarias | `tests/VirGo.UnitTests` (xUnit) | Reglas del dominio y casos de uso |
   | Integración | `tests/VirGo.IntegrationTests` (`WebApplicationFactory`) | El endpoint completo con la persistencia en memoria |
   | Seguridad | `tests/VirGo.SecurityTests` | Autenticación, autorización, validación de entradas, encabezados, CORS, límites y que los errores no expongan datos |
   | Mutación | Stryker.NET (`stryker-config.json`) | Que las pruebas detecten cambios en el código del dominio y la aplicación |
   | Humo | `tests/smoke/` en el CD | El endpoint desplegado responde por HTTPS después de cada despliegue |
3. La compilación no debe dejar advertencias nuevas. Las advertencias de seguridad se tratan como errores.
4. Los umbrales de cobertura y de mutación se fijan después de medirlos, nunca antes. La mutación corre cada noche y bajo demanda, no en cada PR.
5. No se fusiona un PR con pruebas en rojo.
6. **Ningún campo sin límite:** todo texto de entrada tiene longitud máxima, se valida en el DTO y en `db/schema/`, y tiene pruebas de seguridad en el límite, en el límite + 1 y con 1500 caracteres.

## Artículo VII. Infraestructura a costo cero

1. Se usan **solo servicios serverless de AWS** dentro de la capa gratuita, en la región `us-east-1`, más una sola excepción externa: la base de datos **MongoDB Atlas M0** (gratis permanente), alojada en AWS `us-east-1`.

   | Permitido | Uso |
   |---|---|
   | API Gateway (HTTP API) | Entrada HTTPS pública |
   | AWS Lambda | Ejecución de la API |
   | MongoDB Atlas **M0** (externo a la cuenta de AWS; 512 MB) | Base de datos |
   | SSM Parameter Store (estándar) | Secretos y configuración |
   | CloudWatch Logs (retención de 7 días) | Registros |
   | AWS Budgets | Alerta de gasto |
   | IAM y STS | Rol de despliegue con OIDC y rol de ejecución de Lambda |
   | CloudFormation | Motor de AWS SAM |
   | S3 (bucket de artefactos de SAM) | Paquetes de despliegue, menos de 50 MB |
   | KMS (llave administrada `aws/ssm`) | Cifrado de los parámetros SecureString, sin llaves propias |
   | CloudWatch Alarms (hasta 10) y SNS (correo) | Alarmas de errores 5xx y de errores de la Lambda |

2. **Prohibido** sin enmendar este documento: NAT Gateway, ALB/ELB, EC2 encendido 24/7, RDS, ECS/Fargate, nombres de dominio de pago, cualquier recurso con costo por hora y cualquier nivel de Atlas distinto de M0 (Flex, M10 o superiores, respaldos o funciones de pago).
3. **Obligatorio:** una alerta de AWS Budgets de **$1 USD** con aviso por correo, throttling configurado en API Gateway y la organización de Atlas **sin método de pago registrado**, para que no pueda generar cargos.
4. Toda la infraestructura de AWS se define como código (AWS SAM/CloudFormation). Nada se crea a mano en la consola, salvo el arranque inicial documentado en el plan: el stack `bootstrap` y el clúster M0 de Atlas con sus usuarios y lista de acceso (creados una sola vez, con el MCP de MongoDB o la interfaz de Atlas, y registrados en `memoria.md`).
5. Antes de crear un recurso nuevo se registra su costo estimado en `memoria.md`.

## Artículo VIII. Integración y entrega continuas

1. **GitHub** aloja el código. La rama `main` está protegida con un **ruleset** de GitHub: PR obligatorio con una revisión, *status checks* del CI requeridos, sin `push --force` y sin borrar la rama.
2. **CI** en cada PR: restaurar, compilar, pruebas unitarias, de integración y de seguridad, análisis de seguridad (CodeQL, Semgrep, SonarCloud, OWASP ZAP, *dependency review*, Gitleaks y Checkov), validación de la infraestructura y del esquema. Si el CI falla, un comentario en el PR resume qué falló, y se corrige en la misma rama y sobre el mismo spec antes de fusionar.
3. **CD manual:** el despliegue se lanza a mano desde GitHub con el botón *Run workflow* (`workflow_dispatch`), solo desde `main` y con el CI en verde. Usa el *environment* `production`, sus *secrets* y variables, entra a AWS por OIDC, despliega con SAM, ejecuta las pruebas de humo y publica la **URL HTTPS** en el resumen del workflow.
4. La URL HTTPS de producción se registra en `memoria.md` y se comparte con el proyecto Móvil como `API_BASE_URL`.
5. Cada despliegue es reproducible y trazable a su commit.
6. **Repositorio único:** los workflows, el ruleset y Dependabot viven en `.github/` de la raíz del repositorio, compartidos con Móvil; los de este proyecto llevan el prefijo `api-`. Ningún workflow requerido usa filtros `paths`: cada uno decide con un job `changes`, porque un check requerido omitido por ruta deja el PR pendiente para siempre.
7. Después de un despliegue exitoso se registra en `memoria.md` lo aprendido, el spec se marca como *Desplegado* y se proponen las mejoras a la constitución, `AGENTS.md`, las skills y los subagentes que haya revelado.

## Artículo IX. Observabilidad

1. Los logs son estructurados, en JSON, con un `correlationId` en cada solicitud.
2. Hay un endpoint `/health` público, sin datos sensibles.
3. Los errores 5xx quedan registrados con contexto suficiente para reproducirlos.

## Artículo X. Memoria y trazabilidad

1. `memoria.md` es la bitácora oficial. Cada sesión de trabajo, de una persona o de un agente, agrega una entrada con la fecha, lo hecho, las decisiones, la evidencia y lo pendiente.
2. Los commits siguen *Conventional Commits* y citan el ID de la tarea (por ejemplo, `feat(T012): …`).
3. Las decisiones de arquitectura se registran como ADR breves dentro de `memoria.md`.

## Artículo XI. Gobierno de agentes y skills

1. Los subagentes y sus responsabilidades se definen en `AGENTS.md` y en `.opencode/agents/`. Ningún agente actúa fuera de su rol.
2. Ningún agente modifica este documento. Las enmiendas las aprueba el equipo humano.
3. Solo se usan skills externas que estén en la lista permitida de `AGENTS.md`. Una skill nueva se evalúa y se agrega a la lista antes de usarse.
4. Los agentes no ejecutan tareas que no estén en un `task.md` aprobado.
5. Ningún agente despliega a producción, borra recursos en la nube ni fuerza un push sin confirmación humana.
6. **Servidores MCP.** Solo se usan los MCP de la lista permitida de `AGENTS.md`, configurados en `opencode.json`. Se instalan antes de empezar a codificar el módulo que los necesita, usan credenciales de mínimo privilegio que se leen de variables de entorno (nunca escritas en el archivo) y no escriben en recursos de producción sin confirmación humana.

## Artículo XII. Idioma y estilo

1. La documentación, los specs, la memoria y los mensajes de la API dirigidos al usuario van en **español**. La excepción es `README.md`, que va en **inglés** porque es la portada pública del repositorio.
2. El código, los identificadores y los nombres técnicos van en **inglés**, siguiendo las convenciones de .NET.
3. El formato del código se define en `.editorconfig` y es obligatorio.

---

## Enmiendas

- Una enmienda se propone con un PR que modifica este archivo e incluye el motivo, el impacto en los demás archivos y una nueva versión (*SemVer*: mayor = cambia un principio, menor = agrega uno, parche = redacción).
- Al aprobarse, se revisan en el mismo PR `AGENTS.md`, los subagentes, las skills y los specs afectados.
- Cada enmienda se registra en `memoria.md`.

| Versión | Fecha | Cambio |
|---|---|---|
| 1.0.0 | 2026-10-29 | Versión inicial |
| 1.1.0 | 2026-11-03 | **Propuesta, pendiente de aprobación del equipo.** Lineamientos de la clase del 28 y 29 de octubre: `check.md` y specs atómicos (I.4–I.7), redacción anticipada de specs funcionales (II.2), `.gitignore` y escaneos DevSecOps (V.9–V.10), cinco tipos de prueba con mutación nocturna y longitudes obligatorias (VI.2, VI.4, VI.6), servicios auxiliares de AWS y **MongoDB Atlas M0 como base de datos** en lugar de DynamoDB (III.4, V.11, VII), ruleset, CD manual, repositorio único y aprendizaje tras el despliegue (VIII), servidores MCP (XI.6) y README en inglés (XII.1). |
| 1.2.0 | 2026-11-04 | Aprobada por el equipo: `VirGo-Database` administra colecciones, validadores e índices; `VirGo-API` no ejecuta DDL ni modifica la estructura de MongoDB al arrancar, conforme a `API-HANDOFF.md`. |
