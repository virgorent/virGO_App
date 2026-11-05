# Spec — 006 Perfil propio

| Campo | Valor |
|---|---|
| Endpoint | `GET /api/v1/users/me` |
| Fuente de datos | `API-HANDOFF.md` §4.4 `users` y §9.3 |
| Estado | Implementación local; despliegue pendiente |

## Criterios de aceptación

- Una sesión válida recibe el perfil propio sin `passwordHash`.
- Una sesión inválida recibe `401` en Problem Details.
- La respuesta expone solo `id`, `name`, `email`, `phone`, `role` e `isVerified`.
