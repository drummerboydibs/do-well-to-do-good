-- Zero-knowledge journal schema.
-- The server only ever stores: salts, wrapped (encrypted) keys, and ciphertext.
-- No plaintext entry content or emotion data exists server-side.

create table public.user_keys (
  user_id              uuid primary key default auth.uid() references auth.users(id) on delete cascade,
  kdf                  text not null default 'PBKDF2-SHA256-600k',
  kek_salt             text not null,
  wrapped_dek          text not null,
  recovery_salt        text not null,
  recovery_wrapped_dek text not null,
  created_at           timestamptz not null default now()
);

alter table public.user_keys enable row level security;

create policy "select own keys"  on public.user_keys for select using (auth.uid() = user_id);
create policy "insert own keys"  on public.user_keys for insert with check (auth.uid() = user_id);
create policy "update own keys"  on public.user_keys for update using (auth.uid() = user_id) with check (auth.uid() = user_id);
create policy "delete own keys"  on public.user_keys for delete using (auth.uid() = user_id);

create table public.journal_entries (
  id         uuid primary key default gen_random_uuid(),
  user_id    uuid not null default auth.uid() references auth.users(id) on delete cascade,
  payload    text not null,          -- base64(iv || AES-GCM ciphertext) of JSON {body, emotion}
  created_at timestamptz not null default now()
);

alter table public.journal_entries enable row level security;

create policy "select own entries" on public.journal_entries for select using (auth.uid() = user_id);
create policy "insert own entries" on public.journal_entries for insert with check (auth.uid() = user_id);
create policy "update own entries" on public.journal_entries for update using (auth.uid() = user_id) with check (auth.uid() = user_id);
create policy "delete own entries" on public.journal_entries for delete using (auth.uid() = user_id);

create index journal_entries_user_created
  on public.journal_entries (user_id, created_at desc);

-- Encrypted payloads should stay reasonable (~64KB ciphertext ceiling)
alter table public.journal_entries
  add constraint payload_size check (char_length(payload) < 100000);
