# Plan — 007 Catalogo publico de venues

## Contrato

`GET /api/v1/venues?zone=Centro&category=Elegante`

Respuesta `200`:

```json
{"data":[{"id":"uuid","name":"Casa VirGo","shortName":"Casa","description":"...","address":"...","zone":"Centro","capacity":150,"price":{"baseAmount":45000,"unit":"day"},"categories":[],"amenities":[],"photos":[],"rating":{"average":5,"count":1}}]}
```

La consulta usa el contrato existente de `venues`; la API no crea colecciones ni índices.
