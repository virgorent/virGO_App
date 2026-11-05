# Plan — 003 Iniciar sesión (login)

| Campo | Valor |
|---|---|
| Módulo | `003-auth-login` |
| Versión | 1.0.0 |
| Estado | Implementación local; despliegue pendiente |
| Fecha | 2026-11-03 |
| Spec | [spec.md](spec.md) · Tareas: [task.md](task.md) · Verificación: [check.md](check.md) |
| Constitución | v1.2.0 |

## 1. Resumen técnico

Un controlador `AuthController` con la acción `Login` llama al caso de uso `LoginHandler`. El caso de uso busca a la persona por correo, verifica la contraseña con `PasswordHasher` de ASP.NET Core Identity (PBKDF2), crea una `Session` con la huella SHA-256 de un *refresh token* aleatorio y emite un JWT de 15 minutos con el `sid` de la sesión. La persistencia usa los repositorios `IUserRepository` e `ISessionRepository`, con implementaciones en memoria y en MongoDB (colecciones `users` y `sessions`).

## 2. Contexto técnico

| Aspecto | Decisión |
|---|---|
| Capas | `Api/Controllers/AuthController` · `Application/Auth/Login` · `Domain/Users`, `Domain/Sessions` · `Infrastructure/Persistence`, `Infrastructure/Security` |
| Huella de contraseña | `Microsoft.Extensions.Identity.Core` → `PasswordHasher<User>` (PBKDF2-HMAC-SHA512, formato v3) |
| Token de acceso | JWT HS256 de 15 min con la llave `Jwt:SigningKey` de `000` |
| Token de sesión | 32 bytes de `RandomNumberGenerator`, Base64Url; se guarda su SHA-256 |
| Validación | Validación de modelo de ASP.NET Core (`[ApiController]`) con mensajes en español |
| Límite por IP | Política `login` del *rate limiter*: 10 solicitudes por minuto por IP |
| MCP | `mongodb` (revisar documentos e índices de `users` y `sessions`) y `github` |

## 3. Constitution Check

| Art. | Tema | Resultado | Cómo se cumple |
|---|---|---|---|
| I | SDD | ✅ Cumple | Spec atómico de un endpoint; cuatro archivos; tareas `T3NN`. |
| II | Modularidad | ⚠️ Justificado | Se redacta antes de cerrar la base (Art. II.2); sus tareas esperan a `000`–`002`. |
| III | Arquitectura | ✅ Cumple | Controlador delgado, caso de uso en Application, entidades en Domain, repositorios por interfaz. |
| IV | Contrato | ✅ Cumple | `/api/v1/auth/login`, `camelCase`, UTC, Problem Details, OpenAPI con ejemplos. |
| V | Seguridad | ✅ Cumple | PBKDF2, respuesta genérica, bloqueo, límite por IP, huella del token, sin datos sensibles en logs. |
| VI | Pruebas | ✅ Cumple | Unitarias, integración, seguridad, mutación y humo (sección 8). |
| VII | Costo cero | ✅ Cumple | 1 lectura por índice + 2 escrituras pequeñas por login, dentro de Atlas M0. |
| VIII | CI/CD | ✅ Cumple | Entra por PR; se despliega con el workflow manual; agrega su humo. |
| IX | Observabilidad | ✅ Cumple | Log del resultado (éxito, fallo, bloqueo) con `correlationId` y el correo enmascarado. |
| X | Memoria | ✅ Cumple | Cambio de contrato en ambas memorias (T310). |
| XI | Agentes y MCP | ✅ Cumple | Responsables definidos; MCP de la lista permitida. |
| XII | Idioma | ✅ Cumple | Código en inglés; mensajes al usuario en español. |

## 4. Archivos

```text
src/VirGo.Domain/Users/User.cs, Email.cs, UserRole.cs
src/VirGo.Domain/Sessions/Session.cs
src/VirGo.Application/Auth/Login/LoginCommand.cs, LoginHandler.cs, LoginResult.cs
src/VirGo.Application/Abstractions/IUserRepository.cs, ISessionRepository.cs, IPasswordHasher.cs, ITokenService.cs
src/VirGo.Infrastructure/Security/IdentityPasswordHasher.cs, JwtTokenService.cs, RefreshTokenGenerator.cs
src/VirGo.Infrastructure/Persistence/InMemory/InMemoryUserRepository.cs, InMemorySessionRepository.cs, DevUserSeeder.cs
src/VirGo.Infrastructure/Persistence/MongoDb/MongoUserRepository.cs, MongoSessionRepository.cs, AuthIndexes.cs
src/VirGo.Api/Controllers/AuthController.cs, Contracts/Auth/LoginRequest.cs, LoginResponse.cs
tests/VirGo.UnitTests/Auth/…  tests/VirGo.IntegrationTests/Auth/LoginTests.cs
tests/VirGo.SecurityTests/Auth/LoginSecurityTests.cs  tests/smoke/smoke.sh (caso nuevo)
```

## 5. Decisiones técnicas (ADR)

| ADR | Decisión | Alternativas descartadas | Motivo |
|---|---|---|---|
| ADR-17 | `PasswordHasher<T>` de Identity (PBKDF2) | Argon2 con un paquete externo; BCrypt | Viene en el SDK, mantenido por Microsoft, sin dependencias nativas en Lambda |
| ADR-18 | Acceso JWT de 15 min + token de sesión opaco de 30 días con huella SHA-256 y `sid` en el JWT | Un solo JWT largo | Un token robado dura poco y la sesión se puede revocar (`004`) |
| ADR-19 | Aplicar el límite de login por IP mediante el rate limiter común | Guardar contadores en `users` | El esquema existente no contiene campos de bloqueo; el bloqueo persistente requiere un spec posterior y una decisión de producto |

## 6. Diseño

### 6.1 Contrato (JSON para el equipo Móvil)

**Petición** `POST /api/v1/auth/login` · `Content-Type: application/json`

```json
{
  "email": "ana@ejemplo.mx",
  "password": "********",
}
```

| Campo | Regla |
|---|---|
| `email` | Obligatorio, formato de correo, máximo 150 caracteres |
| `password` | Obligatorio, de 1 a 128 caracteres |

**200 OK**

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "tokenType": "Bearer",
  "expiresIn": 900,
  "refreshToken": "q3Jx...Base64Url",
  "refreshTokenExpiresAt": "2026-12-03T18:00:00Z",
  "user": {
    "id": "0b6c5d7e-2f1a-4c3b-9e8d-7a6b5c4d3e2f",
    "name": "Ana López",
    "email": "ana@ejemplo.mx",
    "role": "client"
  }
}
```

`role` ∈ `client`, `owner`, `provider` (equivalen a cliente, propietario y proveedor del modelo de datos).

**Errores** (`application/problem+json`, siempre con `traceId`)

| Estado | `title` | Cuándo |
|---|---|---|
| 400 | «La solicitud tiene datos inválidos.» + `errors` por campo | CA-06 |
| 401 | «Correo o contraseña incorrectos.» | CA-03, CA-04 |
| 429 | «Demasiados intentos. Intenta de nuevo en unos minutos.» + encabezado `Retry-After` | CA-05 y límite por IP |

### 6.2 Claims del JWT

`sub` (id de la persona), `sid` (id de la sesión), `role`, `jti`, `iat`, `exp`, `iss`, `aud`. **No** incluye el correo ni el nombre.

### 6.3 Algoritmo del caso de uso

1. Normalizar el correo (`Trim`, `ToLowerInvariant`).
2. Buscar por correo. Si no existe, verificar la contraseña contra una huella ficticia fija (para igualar el tiempo) y devolver `InvalidCredentials`.
3. Verificar la contraseña con el hasher configurado; si falla, devolver `InvalidCredentials` sin revelar si el correo existe.
4. Si `PasswordVerificationResult.SuccessRehashNeeded`, actualizar la huella.
5. Crear la `Session` (expira en 30 días), emitir el JWT y devolver el resultado.

### 6.4 Colecciones de MongoDB (base `virgo`)

**`users`**

```json
{
  "_id": "0b6c5d7e-2f1a-4c3b-9e8d-7a6b5c4d3e2f",
  "name": "Ana López",
  "email": "ana@ejemplo.mx",
  "phone": "+524421234567",
  "passwordHash": "AQAAAAIAAYagAAAAE...",
  "role": "client",
  "createdAt": "2026-11-03T18:00:00Z",
  "updatedAt": "2026-11-03T18:00:00Z"
}
```

**`sessions`**

```json
{
  "_id": "5f1d2c3b-4a59-4e6f-8a7b-9c0d1e2f3a4b",
  "userId": "0b6c5d7e-2f1a-4c3b-9e8d-7a6b5c4d3e2f",
  "tokenHash": "3q2+7w...SHA-256 en Base64",
  "expiresAt": "2026-12-03T18:00:00Z",
  "createdAt": "2026-11-03T18:00:00Z",
  "purgeAt": "2026-12-10T18:00:00Z"
}
```

| Colección | Índice | Para qué |
|---|---|---|
| `users` | `{ email: 1 }` **único** | Buscar por correo en el login e impedir correos repetidos |
| `sessions` | `{ userId: 1 }` | Sesiones de una persona; la del JWT se busca por `_id` (`sid`) y `userId` (`sub`) |
| `sessions` | `{ tokenHash: 1 }` **único** | Renovar la sesión (`006`) sin duplicados |
| `sessions` | `{ purgeAt: 1 }` con `expireAfterSeconds: 0` (TTL) | MongoDB borra la sesión 7 días después de expirar (ADR-11) |

Los nombres de campo van en inglés y `camelCase` (Art. XII.2), según `API-HANDOFF.md`. Los índices y validadores ya existen por el contrato administrado por `VirGo-Database`; la API no ejecuta DDL durante el login ni al arrancar.

### 6.5 Persona sembrada

`DevUserSeeder` crea `demo@virgo.mx` solo cuando `Persistence:Provider = InMemory` y el ambiente es `Development` o `Testing`. La contraseña viene de la configuración local (`appsettings.Development.json` sin secretos reales, o *user-secrets*), nunca del código.

## 7. Costo

Login: 1 lectura por el índice de `email` + 1 actualización de `users` + 1 inserción en `sessions`. Cada sesión ocupa menos de 1 KB, así que 100 000 sesiones vivas caben en unos 100 MB de los 512 MB de M0; el índice TTL las depura. **$0.**

## 8. Estrategia de pruebas

| Tipo | Casos |
|---|---|
| Unitarias | `Email` normaliza y rechaza formatos inválidos; `LoginHandler`: éxito, contraseña incorrecta, correo inexistente, quinto fallo bloquea, bloqueo vigente, éxito reinicia el contador, *rehash*; `JwtTokenService` incluye `sub`, `sid`, `role` y expira en 15 min |
| Integración | Repositorios `MongoUserRepository` y `MongoSessionRepository` contra Testcontainers (índice único, TTL creado); 200 con la forma exacta del JSON y `camelCase`; correo con mayúsculas; 400 con `errors`; 401 genérico; 429 al sexto intento; sesión creada en el repositorio en memoria; aparece en `/openapi/v1.json` |
| Longitudes | `email` de 150 pasa y de 151 da 400; `password` de 128 pasa y de 129 da 400; `device` de 100 pasa y de 101 da 400; 1500 caracteres en cada campo da 400 |
| Seguridad | Mismo cuerpo y estado para correo inexistente y contraseña incorrecta; diferencia de tiempo acotada; contraseña y tokens ausentes de los logs (logger de prueba); `tokenHash` ≠ `refreshToken`; contraseña de 129 caracteres → 400; *payloads* tipo inyección (`{"$ne":null}`, `' OR 1=1`) → 400 o 401, nunca 500; límite por IP → 429 |
| Mutación | Stryker sobre `LoginHandler`, `Email` y la lógica de bloqueo; sin mutantes sobrevivientes en las condiciones de bloqueo |
| Humo | `POST {ApiUrl}/api/v1/auth/login` con datos inválidos → 401 `application/problem+json` |

## 9. Riesgos y mitigaciones

| Riesgo | Mitigación |
|---|---|
| Diferencia de tiempo revela correos registrados | Verificación contra una huella ficticia (paso 2 del algoritmo) y prueba de seguridad |
| Bloqueo usado para impedir el acceso a otra persona | 15 min, no permanente; P-01 del spec |
| Inyección de operadores de MongoDB (`{"$ne": null}` en `email`) | El contrato tipa `email` como texto y el repositorio usa filtros tipados (`Builders<User>.Filter.Eq`), nunca JSON armado con texto; prueba de seguridad |

## 10. Fases

| Fase | Contenido | Tareas |
|---|---|---|
| F0 | Contexto y MCP | T300 |
| F1 | Dominio y aplicación | T301–T303 |
| F2 | Infraestructura | T304–T306 |
| F3 | API | T307 |
| F4 | Pruebas | T308–T309 |
| F5 | PR, despliegue y entrega | T310–T312 |
