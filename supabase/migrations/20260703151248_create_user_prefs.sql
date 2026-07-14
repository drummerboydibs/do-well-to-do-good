-- Per-user preferences that are sensitive enough to encrypt. First use: the set
-- of faith/belief traditions a user has opted into (issue #29) — protected-class
-- info, so like everything else the value is an opaque client-encrypted payload;
-- the server only knows a row exists. One row per user (user_id is the PK, so
-- writes upsert). Mirrors the RLS shape of the other tables.
create table public.user_prefs (
    user_id uuid primary key default auth.uid() references auth.users (id),
    payload text not null check (char_length(payload) < 20000),
    updated_at timestamptz not null default now()
);

alter table public.user_prefs enable row level security;

create policy "select own prefs" on public.user_prefs
    for select using (auth.uid() = user_id);
create policy "insert own prefs" on public.user_prefs
    for insert with check (auth.uid() = user_id);
create policy "update own prefs" on public.user_prefs
    for update using (auth.uid() = user_id) with check (auth.uid() = user_id);
create policy "delete own prefs" on public.user_prefs
    for delete using (auth.uid() = user_id);
