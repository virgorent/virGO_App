# Spec — 001 Infraestructura como código (IaC)

| Campo | Valor |
|---|---|
| Módulo | `001-infraestructura-iac` |
| Versión | 1.0.0 |
| Estado | Implementación local; validación y despliegue pendientes |
| Fecha | 2026-11-03 |
| Constitución | v1.2.0. Aplica sobre todo V, VII, IX y XI |
| Plan | [plan.md](plan.md) · Tareas: [task.md](task.md) · Verificación: [check.md](check.md) |
| Origen | Se separó de `000-arquitectura-base` (antes F4, T030 y T034) para que la infraestructura sea un spec atómico (Art. I.5) |

## 1. Contexto y objetivo

La API de VirGo necesita vivir en la nube con una **URL HTTPS** pública, a **costo cero** y sin que nadie cree recursos a mano en la consola. Todo lo que exista en la nube debe estar descrito en archivos versionados, poder crearse y borrarse con un comando y avisar si genera algún gasto.

El objetivo de este módulo es dejar descrita, validada y lista para desplegar toda la infraestructura que usarán el pipeline (`002-pipeline-cicd`) y los endpoints (`003-…` en adelante).

## 2. Historias técnicas

| ID | Historia |
|---|---|
| HT-01 | Como **equipo Móvil**, quiero una URL HTTPS estable para conectar la app sin certificados ni dominios propios. |
| HT-02 | Como **responsable del proyecto**, quiero que la infraestructura cueste $0 y me avise por correo si aparece un gasto, para no recibir cargos inesperados. |
| HT-03 | Como **equipo**, quiero que la infraestructura sea código revisable en un PR, para no depender de clics en la consola. |
| HT-04 | Como **responsable de seguridad**, quiero que cada componente tenga solo los permisos que usa y que los secretos vivan fuera del repositorio. |
| HT-05 | Como **desarrollador**, quiero que el clúster de MongoDB, sus usuarios y su acceso existan antes de programar los endpoints, para que el primer *task* de usuarios cree su base y colecciones sin pasos extra. |
| HT-06 | Como **operador**, quiero alarmas de errores y registros con retención limitada, para enterarme de fallas sin pagar almacenamiento. |

## 3. Requerimientos funcionales

| ID | Requerimiento |
|---|---|
| RF-01 | Toda la infraestructura está descrita como código en el repositorio, en dos piezas: un arranque único (identidad de despliegue y presupuesto) y el stack de la aplicación. |
| RF-02 | El stack de la aplicación expone la API en una URL **HTTPS** pública; no existe ninguna entrada por HTTP. |
| RF-03 | Existe un clúster gratuito de MongoDB en la misma región que la API, con un usuario por ambiente de mínimo privilegio, un usuario de solo lectura para las herramientas de consulta, conexión cifrada y la cadena de conexión guardada fuera del repositorio. |
| RF-04 | La llave de firma de tokens y la cadena de conexión de la base se guardan cifradas en un almacén de parámetros y se entregan a la API al desplegar, nunca en el repositorio. |
| RF-05 | La puerta de entrada limita las solicitudes por segundo y aplica la lista de orígenes permitidos. |
| RF-06 | Existe una alerta de presupuesto de $1 USD al mes con aviso por correo. |
| RF-07 | Existen alarmas por correo para errores 5xx de la API, errores de la función y alertas de la base de datos (conexiones y espacio). |
| RF-08 | Los registros de la aplicación se conservan 7 días. |
| RF-09 | El pipeline entra a la nube con una identidad federada de corta duración, limitada a este repositorio y a la rama `main`. |
| RF-10 | La infraestructura se valida automáticamente (sintaxis, buenas prácticas y seguridad) sin desplegar nada. |
| RF-11 | Todo el stack de la aplicación se puede borrar con un solo comando. |
| RF-12 | Antes de crear un recurso, su costo estimado queda registrado en `memoria.md`. |

## 4. Requerimientos no funcionales

| ID | Requerimiento |
|---|---|
| RNF-01 | **Costo:** $0 USD al mes con tráfico de desarrollo y pruebas, usando solo la capa gratuita o los créditos del plan gratuito. |
| RNF-02 | **Servicios:** solo los permitidos en el Art. VII (entre 10 y 15 servicios en total, incluidos los auxiliares y la base de datos externa gratuita). |
| RNF-03 | **Seguridad:** mínimo privilegio en cada rol; cero llaves de acceso de larga duración; cifrado en reposo de los secretos. |
| RNF-04 | **Reproducibilidad:** el mismo código crea el mismo stack en cualquier cuenta con solo cambiar parámetros. |
| RNF-05 | **Región:** `us-east-1`. |

## 5. Criterios de aceptación

| ID | Criterio |
|---|---|
| CA-01 | **Dado** el repositorio, **cuando** ejecuto la validación de la infraestructura, **entonces** no hay errores de sintaxis ni hallazgos de seguridad altos o críticos sin justificar. |
| CA-02 | **Dado** el stack desplegado, **cuando** consulto la salida de la URL, **entonces** empieza con `https://` y `/health` responde 200. |
| CA-03 | **Dado** el stack desplegado, **cuando** reviso los recursos, **entonces** solo existen servicios de la lista permitida, el clúster de la base es del nivel gratuito y su organización no tiene método de pago. |
| CA-04 | **Dado** la cuenta de AWS, **cuando** reviso la facturación, **entonces** la alerta de $1 USD está activa y su correo fue confirmado. |
| CA-05 | **Dado** el repositorio y su historial, **cuando** busco secretos, **entonces** no aparecen la llave de firma, la cadena de conexión ni ninguna credencial. |
| CA-09 | **Dado** el usuario de la base de la API, **cuando** intenta leer o escribir otra base de datos o conectarse sin TLS, **entonces** se le niega. |
| CA-06 | **Dado** un workflow fuera de `main` o de otro repositorio, **cuando** intenta asumir el rol de despliegue, **entonces** AWS lo rechaza. |
| CA-07 | **Dado** un stack temporal de prueba, **cuando** ejecuto el comando de borrado, **entonces** no queda ningún recurso suyo. |
| CA-08 | **Dado** el stack desplegado, **cuando** reviso las alarmas, **entonces** existen la de errores 5xx, la de errores de la función y las alertas de la base, con aviso por correo. |

## 6. Fuera de alcance

- Los workflows de GitHub Actions y el ruleset (van en `002-pipeline-cicd`).
- Las colecciones, validadores e índices los administra `VirGo-Database`; la API solo consume el contrato existente y no ejecuta DDL al arrancar.
- Dominio propio, CDN, WAF, ambientes adicionales (*staging*) y respaldos programados.

## 7. Supuestos, dependencias y preguntas abiertas

**Supuestos**

- Hay una cuenta de AWS del equipo con la capa gratuita o los créditos del plan gratuito vigentes.
- Las tareas de código de `000` que agregan el alojamiento en Lambda están terminadas antes del primer despliegue.

**Dependencias**

- `000-arquitectura-base`: la API compila y responde `/health`.
- `002-pipeline-cicd`: ejecuta el primer despliegue real.

**Preguntas abiertas**

| # | Pregunta | Responsable |
|---|---|---|
| P-01 | ¿Qué correo recibe la alerta de presupuesto y las alarmas? (antes P-01 de `000`) | Equipo |
| P-02 | ¿Cuánto dura el crédito del plan gratuito de la cuenta (el profesor mencionó 2 meses)? Define la fecha de revisión del costo. | Equipo |
| P-03 | ¿Quién es dueño de la organización y del proyecto de MongoDB Atlas? (correo institucional del equipo, sin tarjeta) | Equipo |
