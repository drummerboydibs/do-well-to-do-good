-- Encrypted, per-user navigation layout (page order + hidden pages), synced
-- across devices. Client-side envelope-encrypted like the rest of user data,
-- so a DBA sees only ciphertext (can't tell someone leads with the Recovery tab).
alter table public.user_prefs
  add column if not exists nav_payload text
    check (nav_payload is null or char_length(nav_payload) < 20000);

-- A prefs row may now exist with only a nav layout (and no faith payload yet),
-- so the original faith payload column must be allowed to be null.
alter table public.user_prefs
  alter column payload drop not null;
