# Spec — 003 Iniciar sesión (login)

| Campo | Valor |
|---|---|
| Módulo | `003-auth-login` |
| Endpoint | `POST /api/v1/auth/login` |
| Versión | 1.0.0 |
| Estado | Implementación local; despliegue pendiente |
| Fecha | 2026-11-03 |
| Constitución | v1.2.0. Aplica sobre todo IV, V y VI |
| Plan | [plan.md](plan.md) · Tareas: [task.md](task.md) · Verificación: [check.md](check.md) |
| Pantallas | A4 «Iniciar sesión» y A5 (sesión persistente) del checklist de VirGo |
| Datos | Colecciones `users` y `sessions` de `db/schema/` y `API-HANDOFF.md`; `VirGo-Database` es la autoridad ejecutable |

## 1. Contexto y objetivo

Clientes, propietarios de salones y proveedores entran a la app con su correo y su contraseña. Al entrar, la app recibe una credencial de corta duración para usar la API y otra de larga duración para mantener la sesión abierta en el dispositivo.

El objetivo es que una persona registrada pueda iniciar sesión de forma segura, sin que nadie pueda adivinar contraseñas por fuerza bruta ni averiguar qué correos están registrados.

## 2. Historias de usuario

| ID | Historia |
|---|---|
| HU-01 | Como **persona registrada**, quiero entrar con mi correo y contraseña para usar VirGo. |
| HU-02 | Como **persona registrada**, quiero que mi sesión siga abierta en mi teléfono para no escribir mi contraseña cada vez. |
| HU-03 | Como **persona registrada**, quiero que nadie pueda entrar a mi cuenta probando contraseñas una y otra vez. |
| HU-04 | Como **app móvil**, quiero saber el rol de la persona al entrar para mostrarle la navegación de cliente, propietario o proveedor. |

## 3. Requerimientos funcionales

| ID | Requerimiento |
|---|---|
| RF-01 | La persona envía su correo y su contraseña; el correo se compara sin distinguir mayúsculas ni espacios al inicio o al final. |
| RF-02 | Si los datos son correctos, la respuesta incluye una credencial de acceso de 15 minutos, una credencial de sesión de 30 días y los datos básicos de la persona (identificador, nombre, correo y rol). |
| RF-03 | Cada inicio de sesión crea una sesión nueva asociada al dispositivo que la persona indique (opcional). |
| RF-04 | Si el correo no existe o la contraseña es incorrecta, la respuesta es **la misma** en ambos casos: «Correo o contraseña incorrectos.» |
| RF-05 | Tras 5 intentos fallidos seguidos, la cuenta se bloquea 15 minutos; durante el bloqueo, la respuesta indica que hay demasiados intentos. Un inicio de sesión correcto reinicia el contador. |
| RF-06 | Si el correo tiene un formato inválido o falta algún dato, la respuesta enumera los errores de cada campo. |
| RF-07 | La credencial de sesión nunca se guarda tal cual: se guarda una huella irreversible. |
| RF-08 | El endpoint se publica en el documento OpenAPI con ejemplos de petición y respuesta. |

## 4. Requerimientos no funcionales

| ID | Requerimiento |
|---|---|
| RNF-01 | **Seguridad:** la contraseña se verifica contra una huella lenta y con sal; nunca aparece en registros, respuestas ni errores. |
| RNF-02 | **Privacidad:** los registros no contienen el correo completo ni las credenciales. |
| RNF-03 | **Anti-enumeración:** un correo inexistente y una contraseña incorrecta tardan un tiempo similar en responder. |
| RNF-04 | **Rendimiento:** responde en menos de 1 s con la instancia ya iniciada. |
| RNF-05 | **Abuso:** además del bloqueo por cuenta, hay un límite de solicitudes por dirección IP. |

## 5. Criterios de aceptación

| ID | Criterio |
|---|---|
| CA-01 | **Dado** una persona registrada, **cuando** envía su correo y contraseña correctos, **entonces** recibe 200 con las dos credenciales, su tiempo de vida y sus datos básicos. |
| CA-02 | **Dado** una persona registrada, **cuando** escribe su correo con mayúsculas o espacios, **entonces** también entra. |
| CA-03 | **Dado** una contraseña incorrecta, **cuando** intenta entrar, **entonces** recibe 401 en Problem Details con el mensaje genérico. |
| CA-04 | **Dado** un correo no registrado, **cuando** intenta entrar, **entonces** recibe exactamente el mismo 401 que en CA-03. |
| CA-05 | **Dado** 5 intentos fallidos seguidos, **cuando** intenta por sexta vez, aunque sea con la contraseña correcta, **entonces** recibe 429 en Problem Details hasta que pasen 15 minutos. |
| CA-06 | **Dado** un correo con formato inválido o una contraseña vacía, **cuando** intenta entrar, **entonces** recibe 400 en Problem Details con los errores por campo. |
| CA-07 | **Dado** un inicio de sesión exitoso, **cuando** reviso la base de datos, **entonces** existe una sesión nueva y la credencial de sesión no aparece en texto plano. |
| CA-08 | **Dado** cualquier intento, **cuando** reviso los registros, **entonces** no aparecen la contraseña, las credenciales ni el correo completo. |
| CA-09 | **Dado** la URL de producción, **cuando** la prueba de humo envía datos inválidos, **entonces** recibe 401 por HTTPS. |

## 6. Fuera de alcance

- Registro de cuentas (A2): spec atómico propuesto `005-auth-registro`. Mientras tanto, las pruebas usan una persona sembrada solo en los ambientes `Development` y `Testing`.
- Renovar la credencial de acceso con la de sesión: spec propuesto `006-auth-renovar-sesion`.
- Cerrar sesión: `004-auth-logout`.
- Recuperar contraseña (A6, A7) e inicio con Google o Apple.

## 7. Supuestos, dependencias y preguntas abiertas

**Dependencias:** `000` (JWT, Problem Details, límites), `001` (tabla con TTL), `002` (CI, humo).

**Preguntas abiertas**

| # | Pregunta | Responsable |
|---|---|---|
| P-01 | ¿El bloqueo de 15 minutos tras 5 intentos es aceptable para el equipo de producto? | Equipo |
| P-02 | `failedLoginCount` y `lockoutEndsAt` ya están en el modelo y en `db/schema/users.json`. ¿El equipo lo confirma? | Equipo |
