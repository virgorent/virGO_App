# API Handoff

Versioned backend contract snapshot for the VirGo API vertical slice. The
MongoDB definitions in `db/schema/` remain the executable authority for field
types, limits, validators and indexes. The database project must reconcile
future changes with this file before deployment.

## 4.4 Collections

- `users`: accounts identified by a UUID string. The API writes `name`,
  `email`, `phone`, `passwordHash`, `role`, verification and login lockout
  fields.
- `venues`: published venue catalog documents. The API reads `name`,
  `shortName`, `description`, `address`, `zone`, `capacity`, `price`,
  `categories`, `amenities`, `photos` and `rating`.
- `quotes`: client requests containing `clientId`, `clientName`, `target`,
  event data, requested services, status and timestamps.
- `bookings`: pending reservations containing `clientId`, `clientName`,
  `venue`, date, hours, calculated amounts, folio, status, payments and
  timestamps. `idempotencyKey` is unique per client when present.
- `counters`: atomic yearly folio counters.
- `venueDates`: reserved dates used by the future confirmation transaction;
  creating a pending booking does not reserve a date.

## 9.2 Authentication

- `POST /api/v1/auth/register` accepts `name`, `email`, `password` and
  `phone`; it returns `201` with the public client account.
- `POST /api/v1/auth/login` accepts `email` and `password`; it returns a JWT
  bearer token and refresh-session data.
- `POST /api/v1/auth/logout` requires the current bearer token and returns
  `204` when its session is revoked.
- `GET /api/v1/users/me` requires the current bearer token and returns only
  the authenticated user's public profile.

## 9.5 Venue catalog

`GET /api/v1/venues` is public. It accepts optional `zone` and `category`
filters and returns only documents with `status: published`. Internal fields
such as `ownerId` and `status` are not exposed.

## 9.8 Quotes

`POST /api/v1/quotes` requires authentication and accepts:

```json
{"target":{"type":"venue","id":"uuid"},"eventId":"uuid","eventType":"Boda","eventDate":"2027-05-15","guests":150,"requestedServices":["Banquete"],"message":"Solicito cotizacion."}
```

The server resolves the published venue and its name. The client cannot make
`target.name` authoritative. Event dates cannot be in the past.

## 9.9 Bookings

`POST /api/v1/bookings` requires authentication and the `Idempotency-Key`
header. It accepts:

```json
{"quoteId":"uuid","eventId":"uuid","venueId":"uuid","date":"2027-05-15","hours":8,"deposit":"30000.00"}
```

The server verifies a published venue, validates the minimum hours, calculates
the total from `price.base` and `price.unit`, generates a yearly `VG-AAAA-NNNN`
folio and creates `pending_payment`. If `deposit` is omitted, the server uses
30 percent of the calculated total; an amount below zero or above the total is
rejected. Repeating the same key for the same client returns the original
booking. Confirmation, payments and date blocking belong to later specs.

All invalid input uses Problem Details. JSON uses `camelCase`, dates use
`yyyy-MM-dd`, and monetary values are MXN decimals.
