# Plan — 006 Perfil propio

## Contrato

`GET /api/v1/users/me` con `Authorization: Bearer <accessToken>`.

Respuesta `200`:

```json
{"id":"uuid","name":"Ana López","email":"ana@example.com","phone":"+525500000000","role":"client","isVerified":false}
```

La API obtiene el usuario por `sub`; nunca acepta un `userId` enviado por el cliente para este recurso.
