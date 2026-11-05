# Spec — 005 Registro de cuenta

| Campo | Valor |
|---|---|
| Endpoint | `POST /api/v1/auth/register` |
| Fuente de datos | `API-HANDOFF.md` §4.4 `users` y §9.2 |
| Estado | Implementación local; despliegue pendiente |

## Objetivo

Crear una cuenta de cliente con los campos obligatorios del contrato (`name`, `email`, `phone`, `passwordHash`, `role`, `isVerified`, fechas). El rol público inicial siempre es `client`.

## Criterios de aceptación

- Datos válidos devuelven `201` sin contraseña ni hash.
- El correo se normaliza a minúsculas y espacios externos.
- Un correo duplicado devuelve `409`.
- Datos inválidos devuelven `400` en Problem Details.
- La contraseña se persiste únicamente como hash.
