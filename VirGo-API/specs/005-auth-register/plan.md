# Plan — 005 Registro de cuenta

## Contrato

`POST /api/v1/auth/register`

```json
{"name":"Ana López","email":"ana@example.com","password":"contraseña-segura","phone":"+525500000000"}
```

Respuesta `201`:

```json
{"id":"uuid","name":"Ana López","email":"ana@example.com","phone":"+525500000000","role":"client","isVerified":false}
```

## Reglas

- No se acepta `role`, `providerId` ni `isVerified` del cliente.
- La API usa el hash configurado y la colección `users` existente; no ejecuta DDL.
- La unicidad final la garantiza el índice `users.email_1_unique`.
