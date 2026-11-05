# Plan — 008 Crear cotizacion

## Contrato

`POST /api/v1/quotes`

```json
{"target":{"type":"venue","id":"uuid"},"eventId":"uuid","eventType":"Boda","eventDate":"2027-05-15","guests":150,"requestedServices":["Banquete"],"message":"Solicito cotización."}
```

La implementación recibe el objeto `target` del handoff y resuelve el nombre del destino en el servidor; el cliente no puede enviar `target.name` como autoridad.
