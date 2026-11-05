# Spec — 000 Arquitectura base del Backend

| Campo | Valor |
|---|---|
| Módulo | `000-arquitectura-base` |
| Versión | 1.1.0 |
| Estado | En revisión |
| Fecha | 2026-10-29 |
| Constitución | v1.2.0. Aplica todos los artículos; sobre todo II, III, IV, V, VII y VIII |
| Plan | [plan.md](plan.md) · Tareas: [task.md](task.md) · Verificación: [check.md](check.md) |

## 1. Contexto y objetivo

> **Cambio v1.1.0 (2026-11-03):** con los specs atómicos (constitución v1.1.0, Art. I.5), la infraestructura y el pipeline se separaron en [`001-infraestructura-iac`](../001-infraestructura-iac/spec.md) y [`002-pipeline-cicd`](../002-pipeline-cicd/spec.md). Los requerimientos y criterios marcados con ➡️ se conservan aquí como contexto global, pero se verifican en esos specs.

VirGo necesita una API que más adelante atenderá a clientes, propietarios de salones y proveedores de servicios. Antes de construir cualquier funcionalidad del negocio, hace falta una **base técnica común**:

- Estructura del proyecto, configuración y manejo de errores.
- Seguridad base y persistencia.
- Pruebas automáticas, infraestructura en la nube y despliegue continuo.

El objetivo de este módulo es que, al terminarlo, exista una API **vacía de funcionalidad pero lista para producción**: publicada en una URL HTTPS, con costo cero, probada y desplegada automáticamente desde GitHub. Así el equipo Móvil puede empezar a integrarse de inmediato.

## 2. Historias técnicas

| ID | Historia |
|---|---|
| HT-01 | Como **desarrollador**, quiero una solución organizada por capas para agregar módulos sin mezclar responsabilidades. |
| HT-02 | Como **desarrollador**, quiero ejecutar la API en mi computadora sin cuenta en la nube ni base de datos instalada para trabajar y probar rápido. |
| HT-03 | Como **equipo Móvil**, quiero una URL HTTPS estable y documentada para conectar la app desde el primer día. |
| HT-04 | Como **equipo Móvil**, quiero que todos los errores tengan el mismo formato para mostrarlos de forma consistente. |
| HT-05 | Como **responsable de seguridad**, quiero que la base ya incluya HTTPS, validación de tokens, CORS, límites de solicitudes y manejo de secretos para no agregarlos después a cada módulo. |
| HT-06 | Como **responsable del proyecto**, quiero que la infraestructura cueste $0 y avise si genera algún gasto, para no recibir cargos inesperados. |
| HT-07 | Como **equipo**, quiero que cada cambio se pruebe en un PR y se despliegue con un botón, para entregar seguido y con control. |
| HT-08 | Como **operador**, quiero un chequeo de salud y registros con identificador de solicitud para diagnosticar problemas. |

## 3. Requerimientos funcionales

| ID | Requerimiento |
|---|---|
| RF-01 | La solución separa presentación, aplicación, dominio e infraestructura en proyectos distintos, con dependencias solo hacia adentro. |
| RF-02 | La API se ejecuta igual en local y en la nube a partir del mismo código. |
| RF-03 | La API expone un chequeo de salud público que indica si el servicio está disponible, sin datos sensibles. |
| RF-04 | Todas las rutas de negocio futuras viven bajo el prefijo de versión `/api/v1`. |
| RF-05 | Cualquier error (validación, no encontrado, no autorizado, error interno) se responde con el formato estándar Problem Details e incluye un identificador de rastreo. |
| RF-06 | La API publica su documento OpenAPI para que Móvil conozca el contrato. |
| RF-07 | La persistencia se usa a través de interfaces, con dos implementaciones intercambiables por configuración: en memoria (local y pruebas) y base de datos en la nube. |
| RF-08 | La API valida tokens de acceso firmados en las rutas protegidas. La emisión de tokens (inicio de sesión) se hará en un módulo funcional posterior. |
| RF-09 | La configuración se lee de archivos sin secretos más variables de entorno o un almacén seguro, y se valida al arrancar. |
| RF-10 | Cada solicitud recibe un identificador de correlación que aparece en la respuesta y en los registros. |
| RF-11 | Existen proyectos de pruebas unitarias y de integración con al menos una prueba que verifica el chequeo de salud y el formato de errores. |
| RF-12 ➡️ `002` | Cada PR ejecuta automáticamente la compilación, las pruebas y el análisis de seguridad. |
| RF-13 ➡️ `002` | Un despliegue manual desde `main` verifica el chequeo de salud y publica la URL HTTPS resultante. |
| RF-14 ➡️ `001` | La infraestructura completa está descrita como código y se puede crear y borrar con un solo comando. |
| RF-15 ➡️ `001` | Existe una alerta de presupuesto que avisa por correo si el gasto mensual supera $1 USD. |
| RF-16 | Todo texto de entrada tiene longitud máxima, y superar ese límite responde 400 en Problem Details. |

## 4. Requerimientos no funcionales

| ID | Requerimiento |
|---|---|
| RNF-01 | **Costo:** $0 USD al mes dentro de la capa gratuita, con tráfico de desarrollo y pruebas. |
| RNF-02 | **Seguridad:** solo HTTPS; ningún secreto en el repositorio; la llave de firma de tokens mide 256 bits o más; CORS con orígenes explícitos; acceso de CI/CD a la nube sin llaves permanentes. |
| RNF-03 | **Disponibilidad:** el chequeo de salud responde en menos de 3 s incluso en el primer arranque (arranque en frío). |
| RNF-04 | **Protección contra abuso:** límite de solicitudes por segundo en la puerta de entrada. |
| RNF-05 | **Mantenibilidad:** la compilación no tiene advertencias de seguridad; el formato se rige por `.editorconfig`. |
| RNF-06 | **Trazabilidad:** cada despliegue se asocia a un commit y queda registrado en `memoria.md`. |
| RNF-07 | **Portabilidad local:** levantar la API en local toma un solo comando, sin servicios externos. |

## 5. Criterios de aceptación

| ID | Criterio |
|---|---|
| CA-01 | **Dado** el repositorio clonado, **cuando** ejecuto el comando de arranque local, **entonces** la API responde 200 en el chequeo de salud sin configurar nada en la nube. |
| CA-02 | **Dado** una ruta inexistente, **cuando** la consulto, **entonces** recibo un 404 en formato Problem Details con `traceId`. |
| CA-03 | **Dado** un error inesperado, **cuando** ocurre, **entonces** recibo un 500 en Problem Details, sin trazas internas, y el error queda en los registros con su identificador de correlación. |
| CA-04 | **Dado** una ruta protegida de prueba, **cuando** la consulto sin token o con un token inválido, **entonces** recibo un 401 en Problem Details. |
| CA-05 | **Dado** que la llave de firma mide menos de 256 bits, **cuando** la API arranca, **entonces** se detiene con un mensaje claro. |
| CA-06 ➡️ `002` | **Dado** un PR, **cuando** se abre, **entonces** el pipeline compila, ejecuta las pruebas y el análisis de seguridad, y bloquea la fusión si algo falla. |
| CA-07 ➡️ `002` | **Dado** un despliegue manual desde `main`, **cuando** termina el pipeline, **entonces** la API queda desplegada, el chequeo de salud responde 200 por HTTPS y la URL aparece en el resumen del pipeline. |
| CA-08 ➡️ `001` | **Dado** el stack desplegado, **cuando** reviso los recursos y la configuración de facturación, **entonces** solo existen servicios permitidos, dentro de la capa gratuita, y la alerta de $1 USD está activa. |
| CA-09 | **Dado** el documento OpenAPI publicado, **cuando** Móvil lo consulta, **entonces** describe el chequeo de salud, el formato de errores y el esquema de seguridad. |
| CA-10 ➡️ `001` | **Dado** el comando de borrado de la infraestructura, **cuando** se ejecuta, **entonces** desaparecen todos los recursos del stack. |

## 6. Fuera de alcance

- **Cualquier funcionalidad del negocio:** registro e inicio de sesión de usuarios, salones, proveedores, cotizaciones, reservaciones, pagos, eventos e IA. Se especifican en módulos posteriores (`001-…`).
- Las tablas y entidades del negocio. Este módulo solo deja lista la infraestructura de persistencia.
- Dominio propio, correo transaccional y notificaciones push.
- Ambientes adicionales (*staging*). Por ahora hay un solo ambiente en la nube: producción de pruebas.

## 7. Supuestos, dependencias y preguntas abiertas

**Supuestos**

- Hay una cuenta de AWS a nombre del equipo, con la capa gratuita o los créditos del plan gratuito.
- Hay un repositorio en GitHub con permisos de administración para configurar secretos, entornos y protección de ramas.

**Dependencias**

- El proyecto Móvil consumirá la URL de CA-07 como `API_BASE_URL`.

**Preguntas abiertas**

| # | Pregunta | Responsable |
|---|---|---|
| P-01 | Se movió a `001-infraestructura-iac` (P-01). | — |
| P-02 | Se movió a `002-pipeline-cicd` (P-01). | — |
