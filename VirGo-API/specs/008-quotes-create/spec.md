# Spec — 008 Crear cotizacion

| Campo | Valor |
|---|---|
| Endpoint | `POST /api/v1/quotes` |
| Fuente de datos | `API-HANDOFF.md` §4.4 `quotes` y §9.8 |
| Estado | Implementación local; despliegue pendiente |

## Criterios de aceptación

- Solo un cliente autenticado puede crear una cotización.
- El destino `venue` debe existir y estar publicado.
- El servidor genera `clientName`, `target.name`, fechas, UUID y estado `sent`.
- La entrada inválida devuelve `400`; venue inexistente devuelve `404`.
