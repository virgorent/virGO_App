# Spec — 002 Pipeline CI/CD con DevSecOps

| Campo | Valor |
|---|---|
| Módulo | `002-pipeline-cicd` |
| Versión | 1.0.0 |
| Estado | Implementación local; ejecución en GitHub pendiente |
| Fecha | 2026-11-03 |
| Constitución | v1.2.0. Aplica sobre todo V, VI, VIII y XI |
| Plan | [plan.md](plan.md) · Tareas: [task.md](task.md) · Verificación: [check.md](check.md) |
| Origen | Se separó de `000-arquitectura-base` (antes T002, F6 y T032) para que el pipeline sea un spec atómico (Art. I.5) |

## 1. Contexto y objetivo

Cada cambio al backend debe entrar por un PR que pruebe el código con los cinco tipos de prueba y lo revise en busca de problemas de seguridad. El despliegue a producción es **manual**: una persona lo lanza con un botón desde GitHub, solo si todo está en verde, y al terminar el pipeline comprueba que la API responde y publica la URL HTTPS.

El objetivo es que el flujo **spec → rama → PR → CI → merge → deploy manual → humo → memoria** funcione de punta a punta antes del primer endpoint de negocio.

## 2. Historias técnicas

| ID | Historia |
|---|---|
| HT-01 | Como **equipo**, quiero que ningún cambio llegue a `main` sin PR, revisión y CI en verde, para no romper producción. |
| HT-02 | Como **responsable de seguridad**, quiero que cada PR busque secretos, dependencias vulnerables, código inseguro e infraestructura insegura. |
| HT-03 | Como **QA**, quiero que el CI ejecute pruebas unitarias, de integración, de seguridad y de mutación, para saber si las pruebas realmente detectan errores. |
| HT-04 | Como **responsable del proyecto**, quiero desplegar a mano con un botón, para decidir cuándo sale una versión. |
| HT-05 | Como **equipo Móvil**, quiero que cada despliegue publique la URL HTTPS y confirme que la API responde, para integrarme sin preguntar. |
| HT-06 | Como **equipo**, quiero que un CI en rojo se corrija en la misma rama y sobre el mismo spec, para no perder el hilo de la tarea. |

## 3. Requerimientos funcionales

| ID | Requerimiento |
|---|---|
| RF-01 | La rama principal solo acepta cambios por PR, con una revisión aprobada y los checks requeridos en verde; no admite *push* forzado ni borrado. |
| RF-02 | Cada PR compila la solución y ejecuta las pruebas unitarias, de integración y de seguridad. |
| RF-03 | Cada noche y bajo demanda se ejecutan pruebas de mutación sobre el dominio y la aplicación, y se publica su reporte. |
| RF-04 | Cada PR ejecuta análisis estático de código, revisión de dependencias nuevas, búsqueda de secretos y análisis de seguridad de la infraestructura. |
| RF-05 | Cada PR valida las plantillas de infraestructura sin desplegarlas. |
| RF-06 | El despliegue a producción se lanza manualmente, solo desde la rama principal, y usa credenciales temporales. |
| RF-07 | Tras desplegar, el pipeline ejecuta pruebas de humo contra la URL pública y falla si alguna no pasa. |
| RF-08 | El resumen del despliegue muestra la URL HTTPS, el commit desplegado y el resultado de las pruebas de humo. |
| RF-09 | Las dependencias se actualizan con PRs automáticos semanales. |
| RF-10 | La plantilla de PR pide el ID de la tarea, el spec, los artículos revisados y el estado del `check.md`. |
| RF-11 | Cada PR analiza el código con dos herramientas de análisis estático y una de calidad, y la API en ejecución con una prueba dinámica. |
| RF-12 | Si algo falla en un PR, un comentario resume qué falló para que se corrija sobre el mismo spec. |

## 4. Requerimientos no funcionales

| ID | Requerimiento |
|---|---|
| RNF-01 | **Costo:** $0 (minutos gratuitos de GitHub Actions). |
| RNF-02 | **Tiempo:** el CI de un PR tarda menos de 15 minutos. |
| RNF-03 | **Seguridad:** ninguna llave de acceso de larga duración; los workflows tienen permisos mínimos (`permissions:` explícito). |
| RNF-04 | **Trazabilidad:** cada despliegue queda asociado a un commit y a una ejecución del workflow. |

## 5. Criterios de aceptación

| ID | Criterio |
|---|---|
| CA-01 | **Dado** un *push* directo a `main`, **cuando** se intenta, **entonces** GitHub lo rechaza. |
| CA-02 | **Dado** un PR con una prueba en rojo, **cuando** termina el CI, **entonces** no se puede fusionar. |
| CA-03 | **Dado** un PR que agrega un secreto de prueba, **cuando** corre el CI, **entonces** el escaneo de secretos falla. |
| CA-04 | **Dado** un PR, **cuando** termina el CI, **entonces** aparecen los checks de pruebas, CodeQL, Semgrep, SonarCloud, OWASP ZAP, revisión de dependencias, secretos, infraestructura y esquema. |
| CA-05 | **Dado** `main` en verde, **cuando** presiono *Run workflow* en el despliegue, **entonces** se despliega, las pruebas de humo pasan y el resumen muestra la URL `https://`. |
| CA-06 | **Dado** una rama distinta de `main`, **cuando** intento lanzar el despliegue, **entonces** el workflow se niega. |
| CA-07 | **Dado** un despliegue cuya prueba de humo falla, **cuando** termina, **entonces** el workflow queda en rojo. |
| CA-08 | **Dado** el despliegue exitoso, **cuando** reviso `memoria.md` de ambos proyectos, **entonces** aparecen la URL y el enlace al OpenAPI. |
| CA-09 | **Dado** la ejecución nocturna, **cuando** termina, **entonces** el reporte de mutación queda como artefacto. |
| CA-10 | **Dado** un PR con un hallazgo de Semgrep, **cuando** termina el CI, **entonces** no se puede fusionar y un comentario indica qué falló. |

## 6. Fuera de alcance

- Despliegue automático en cada *merge* (se descartó por indicación del curso: el CD es manual).
- Ambientes de *staging* y despliegues *canary*.
- Pruebas de carga.

## 7. Supuestos, dependencias y preguntas abiertas

**Dependencias**

- `000-arquitectura-base`: solución y proyectos de pruebas.
- `001-infraestructura-iac`: plantillas, rol OIDC y variables del *environment* `production`.

**Preguntas abiertas**

| # | Pregunta | Responsable |
|---|---|---|
| P-01 | ¿Quién aprueba los PR a `main`? (antes P-02 de `000`) | Equipo |
| P-02 | ✅ Resuelta (2026-11-04): `virgorent/virGO_App` es público, así que el ruleset y los revisores de los *environments* funcionan en el plan Free. | — |
| P-03 | ¿Para qué se usa `rama-apis`, si ya existe `rama-backend`? | Equipo |
