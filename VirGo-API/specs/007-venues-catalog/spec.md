# Spec — 007 Catalogo publico de venues

| Campo | Valor |
|---|---|
| Endpoint | `GET /api/v1/venues` |
| Fuente de datos | `API-HANDOFF.md` §4.4 `venues` y §9.5 |
| Estado | Implementación local; despliegue pendiente |

## Criterios de aceptación

- Solo se devuelven venues con `status: published`.
- Acepta filtros `zone` y `category`.
- No requiere autenticación.
- No expone campos internos ni secretos.
