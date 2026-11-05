# Spec — 009 Crear reservacion

| Campo | Valor |
|---|---|
| Endpoint | `POST /api/v1/bookings` |
| Fuente de datos | `API-HANDOFF.md` §4.4 `bookings`, `counters`, `venueDates` y §9.9 |
| Estado | Implementación local; despliegue pendiente |

## Criterios de aceptación

- Solo un cliente autenticado puede crear una reservación.
- El venue debe estar publicado y la fecha debe ser válida.
- El servidor calcula total, genera folio `VG-AAAA-NNNN` y crea estado `pending_payment`.
- Una reservación pendiente no bloquea `venueDates`, según el handoff.
- La operación inválida devuelve `400` o `404` sin filtrar detalles internos.
