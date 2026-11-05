# MongoDB Atlas Bootstrap

This file documents the one-time Atlas setup without storing credentials.

## Target

- Cluster: `virgo-dev`
- Tier: M0
- Provider/region: AWS `us-east-1`
- Development database: `virgo_dev`
- Production database: `virgo`
- Network access: `0.0.0.0/0`, compensated by TLS, long random passwords and least-privilege users

## Users

| User | Database access | Use |
|---|---|---|
| `virgo-api-prod` | `readWrite` on `virgo` | Production runtime |
| `virgo-api-dev` | `readWrite` on `virgo_dev` | Development runtime |
| `virgo-mcp` | `read` on both databases | Read-only inspection |

Credentials are created outside this repository and stored only in the approved secret store.

## Alerts

Configure Atlas alerts before the first API deployment:

- Connections: alert when the cluster reaches 80% of its connection limit.
- Storage: alert when storage reaches 80% of the M0 capacity.
- Recipient: the same approved alert address passed as `AlertEmail` to the AWS stack.

Atlas alert configuration is external to CloudFormation and must be verified in the Atlas project before `T101` is marked complete.

## Verification

Run the database project's `plan`, `validate` and `verify` commands before connecting the API. The API must not create collections, validators or indexes.
