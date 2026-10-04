# PRP-012 — Shared Family Date fields and retention

## Metadata

- **Proposal ID:** `PRP-012`
- **Status:** `ACCEPTED; CANONICAL CTX-1.9`
- **Date:** 2026-09-26
- **Requested by:** Project owner
- **Context baseline:** `CTX-1.8`
- **Required context:** `README.md`, `project-status.md`, `REQ-FAM-010`, `REQ-PRIV-017`, `DEC-040`, and `DEC-041`
- **Affected authority:** Shared Family Date content and finite retention policy
- **Model tier:** `DEEP`

## Request

Define a small v1 shared-date content set and an explicit finite retention duration after a date becomes unavailable in normal Family views.

## Accepted recommendation

- Provide a title, description, and date field. The title and date are required; description requiredness remains undecided.
- Exclude notes distinct from the description, attachments, notification settings, and other additional content fields from v1.
- Retain a shared-date record for 30 calendar days from the time it first becomes unavailable in normal Family views, then delete the retained date data.
- Keep retained copies unavailable to Family Group members through normal product behavior, consistent with `REQ-PRIV-017`.
- Apply this only to shared Family Date data. Do not alter Adult Profile birthday handling or other product-data retention.

## Still undecided

- Date-value semantics, including whether a year is recorded and whether dates recur annually.
- Audit-record retention, technical backup retention, and cleanup mechanics.

These open matters are not inferred and block implementation planning for affected behavior.

## Decision and authorization

- **Decision:** Recommendation approved as `CTX-1.9`; title and date are required, a description field is included, and whether description is required remains open.
- **Decision ID:** `DEC-042`
- **Approved by:** Project owner
- **Execution authorized:** Candidate context preparation and verification only; no application implementation.
- **Baseline:** `CTX-1.9` is approved and active under `DEC-043`.
