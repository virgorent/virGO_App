# Spec — 004 Cerrar sesión (logout)

| Campo | Valor |
|---|---|
| Módulo | `004-auth-logout` |
| Endpoint | `POST /api/v1/auth/logout` |
| Versión | 1.0.0 |
| Estado | Implementación local; despliegue pendiente |
| Fecha | 2026-11-03 |
| Constitución | v1.2.0. Aplica sobre todo IV, V y VI |
| Plan | [plan.md](plan.md) · Tareas: [task.md](task.md) · Verificación: [check.md](check.md) |
| Pantallas | «Cerrar sesión» del perfil (C39) y de los menús de propietario y proveedor |
| Datos | Colección `sessions` del contrato `API-HANDOFF.md` (secciones 4.4 y 9.2); `VirGo-Database` es la autoridad ejecutable |

## 1. Contexto y objetivo

Cuando una persona cierra sesión en su teléfono, esa sesión debe dejar de servir de inmediato, también en el servidor. Si alguien copió sus credenciales, ya no deben funcionar.

El objetivo es que cerrar sesión invalide la sesión del dispositivo en el servidor y que cualquier solicitud posterior con esas credenciales sea rechazada.

## 2. Historias de usuario

| ID | Historia |
|---|---|
| HU-01 | Como **persona con sesión abierta**, quiero cerrar sesión para que nadie más use mi cuenta en este teléfono. |
| HU-02 | Como **persona**, quiero que al cerrar sesión en un teléfono mis otros dispositivos sigan con su sesión. |
| HU-03 | Como **responsable de seguridad**, quiero que una credencial de una sesión cerrada deje de funcionar en toda la API, no solo en la app. |

## 3. Requerimientos funcionales

| ID | Requerimiento |
|---|---|
| RF-01 | Solo una persona con una credencial de acceso válida puede cerrar sesión. |
| RF-02 | Cerrar sesión elimina **solo** la sesión a la que pertenece la credencial usada. |
| RF-03 | Después de cerrar sesión, cualquier solicitud a una ruta protegida con la credencial de esa sesión es rechazada, aunque no haya expirado. |
| RF-04 | La credencial de sesión de larga duración de esa sesión tampoco vuelve a servir. |
| RF-05 | Las demás sesiones de la persona no cambian. |
| RF-06 | La respuesta exitosa no tiene cuerpo. |
| RF-07 | El endpoint se publica en el documento OpenAPI con el esquema de seguridad Bearer. |

## 4. Requerimientos no funcionales

| ID | Requerimiento |
|---|---|
| RNF-01 | **Seguridad:** la revocación es efectiva en menos de 1 s en todas las instancias de la API. |
| RNF-02 | **Rendimiento:** la verificación de sesión agrega menos de 50 ms a cada ruta protegida con la instancia iniciada. |
| RNF-03 | **Privacidad:** los registros no contienen credenciales. |

## 5. Criterios de aceptación

| ID | Criterio |
|---|---|
| CA-01 | **Dado** una sesión abierta, **cuando** la persona cierra sesión, **entonces** recibe 204 sin cuerpo y el documento de esa sesión deja de existir. |
| CA-02 | **Dado** una sesión cerrada, **cuando** se usa su credencial de acceso en cualquier ruta protegida (incluido el mismo logout), **entonces** se recibe 401 en Problem Details. |
| CA-03 | **Dado** una persona con dos sesiones, **cuando** cierra una, **entonces** la otra sigue funcionando. |
| CA-04 | **Dado** una solicitud sin credencial o con una inválida, **cuando** intenta cerrar sesión, **entonces** recibe 401 en Problem Details. |
| CA-05 | **Dado** una credencial firmada correctamente pero sin identificador de sesión, **cuando** se usa, **entonces** se recibe 401. |
| CA-06 | **Dado** la URL de producción, **cuando** la prueba de humo llama al logout sin credencial, **entonces** recibe 401 por HTTPS. |

## 6. Fuera de alcance

- Cerrar sesión en todos los dispositivos.
- Renovar la credencial de acceso (`006-auth-renovar-sesion`). Ese spec deberá rechazar las sesiones revocadas.
- Borrar los tokens del teléfono: lo hace la app móvil en su propio spec de pantalla.

## 7. Supuestos, dependencias y preguntas abiertas

**Dependencias:** `003-auth-login` (sesiones y `sid` en el JWT).

**Preguntas abiertas**

| # | Pregunta | Responsable |
|---|---|---|
| P-01 | ¿La app debe mostrar algún mensaje si el logout falla por falta de red? (La recomendación es borrar los tokens locales de todas formas.) | Equipo Móvil |
