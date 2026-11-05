#!/usr/bin/env bash
set -euo pipefail

base_url="${1:?Usage: smoke.sh https://api-url}"
base_url="${base_url%/}"

health="$(curl --fail --silent --show-error "$base_url/health")"
grep -q '"status":"Healthy"' <<<"$health"
curl --fail --silent --show-error "$base_url/openapi/v1.json" >/dev/null

headers_file="$(mktemp)"
trap 'rm -f "$headers_file"' EXIT
status="$(curl --silent --show-error --output /dev/null --dump-header "$headers_file" --write-out '%{http_code}' "$base_url/api/v1/_diagnostics/secure")"
test "$status" = "401"
grep -qi '^strict-transport-security:' "$headers_file"
grep -qi '^x-correlation-id:' "$headers_file"

login_status="$(curl --silent --show-error --output /dev/null --write-out '%{http_code}' \
  -H 'content-type: application/json' \
  -d '{"email":"invalid@example.invalid","password":"invalid"}' \
  "$base_url/api/v1/auth/login")"
test "$login_status" = "401"

register_status="$(curl --silent --show-error --output /dev/null --write-out '%{http_code}' \
  -H 'content-type: application/json' \
  -d '{}' \
  "$base_url/api/v1/auth/register")"
test "$register_status" = "400"

venues_status="$(curl --silent --show-error --output /dev/null --write-out '%{http_code}' \
  "$base_url/api/v1/venues")"
test "$venues_status" = "200"

profile_status="$(curl --silent --show-error --output /dev/null --write-out '%{http_code}' \
  "$base_url/api/v1/users/me")"
test "$profile_status" = "401"

quote_status="$(curl --silent --show-error --output /dev/null --write-out '%{http_code}' \
  -H 'content-type: application/json' \
  -d '{}' \
  "$base_url/api/v1/quotes")"
test "$quote_status" = "401"

booking_status="$(curl --silent --show-error --output /dev/null --write-out '%{http_code}' \
  -H 'content-type: application/json' \
  -d '{}' \
  "$base_url/api/v1/bookings")"
test "$booking_status" = "401"

logout_status="$(curl --silent --show-error --output /dev/null --write-out '%{http_code}' \
  -X POST "$base_url/api/v1/auth/logout")"
test "$logout_status" = "401"

echo "Smoke tests passed for $base_url"
