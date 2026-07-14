# Database schema

The app's Postgres schema lives in the **do-well-to-do-good** Supabase project
(email magic-link auth + PostgREST). This folder brings that schema under version
control so the database is reproducible from the repo and reviewable alongside the
code that depends on it (issue #43).

`migrations/` uses the [Supabase CLI](https://supabase.com/docs/guides/local-development)
layout — one timestamped `.sql` file per change, applied in filename order. The
files here were exported verbatim from the live project's migration history, so
they are the same statements that built the current database.

## Zero-knowledge model

Every user-content table stores an opaque, client-encrypted `payload`
(`base64(iv || AES-GCM ciphertext)`) plus only the structural columns the app
needs to sort, group, or relate rows without decrypting them. The server — and
anyone with database access — never sees plaintext. Row-Level Security scopes
every row to its owner via `auth.uid() = user_id`, and each table has the same
four owner-only policies (select / insert / update / delete).

The only columns intentionally left in the clear are non-sensitive metadata:
timestamps, foreign keys, the goal `done` flag, goal `end_date`, and sleep
`night_date`. None of them reveal entry content.

## Tables

| Table | Purpose | Clear-text columns beyond `id` / `user_id` / `payload` |
| --- | --- | --- |
| `user_keys` | Per-user envelope-encryption root: KDF name, salts, and the two wrapped (encrypted) data-encryption keys. No `id`/`payload` — see note below. One row per user. | `kdf`, `kek_salt`, `recovery_salt`, `created_at` |
| `journal_entries` | Encrypted journal entries `{ body, emotion }`. | `created_at`, `updated_at` |
| `therapy_sessions` | Encrypted therapy session notes `{ date, notes }`. | `created_at`, `updated_at` |
| `goals` | Encrypted goal `{ title }`, optionally linked to a session. | `session_id`, `done`, `end_date`, `created_at` |
| `goal_progress` | Encrypted progress notes `{ date, note }` on a goal. | `goal_id`, `created_at` |
| `sobriety_counters` | Encrypted recovery/sobriety counter state. | `created_at` |
| `sleep_entries` | Encrypted per-night sleep record; unique per `(user_id, night_date)`. | `night_date`, `created_at` |
| `user_prefs` | Encrypted preferences: faith traditions (`payload`) and nav layout (`nav_payload`). One row per user. | `updated_at` |

> **`user_keys` is the exception to the `payload` shape above.** It has no
> `payload` blob, and its primary key is `user_id` (not `id`). It stores the KDF
> name (`kdf`) and two salts (`kek_salt`, `recovery_salt`) — non-secret by
> design — alongside two wrapped data-encryption keys (`wrapped_dek`,
> `recovery_wrapped_dek`) that are themselves ciphertext, the payload-equivalent.

## Applying / staying in sync

These migrations are already applied to the live project — this folder mirrors
it. To reproduce the schema on a fresh Supabase project:

```bash
# A fresh clone has migrations/ but no config.toml, so initialize first.
supabase init                       # creates supabase/config.toml (skip if present)
supabase link --project-ref <project-ref>
supabase db push                    # applies migrations/ in order
```

No CLI? Run each file in `migrations/` in filename order in the Supabase SQL
editor (or `psql`) — they're plain SQL.

When changing the schema, add a **new** timestamped migration here rather than
editing an existing file, and apply it to the project so the repo and database
never drift.
