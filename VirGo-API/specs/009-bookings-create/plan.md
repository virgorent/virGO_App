# Plan — 009 Crear reservacion

## Contrato

`POST /api/v1/bookings` con `Idempotency-Key` requerido antes del despliegue.

```json
{"quoteId":"uuid","eventId":"uuid","venueId":"uuid","date":"2027-05-15","hours":8,"deposit":"30000.00"}
```

El primer corte crea `pending_payment`; la confirmación, bloqueo de fecha, pagos y cancelación transaccional requieren sus propios specs y no se inventan en este endpoint.
