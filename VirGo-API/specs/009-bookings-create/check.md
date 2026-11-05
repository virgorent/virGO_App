# Check — 009 Crear reservacion

| Evidencia | Estado |
|---|---|
| Creacion autenticada en integracion | ✅ |
| Folio atomico contra MongoDB | ⏳ |
| Idempotencia y concurrencia | ✅ `VerticalSliceTests` repite la clave secuencial y con `Task.WhenAll`; recibe un `id` y folio |
| Humo HTTPS | ⏳ |
