-- Sobriety / recovery counters. Fully zero-knowledge: the entire meaningful
-- state (addiction name, clean-since date, best & most-recent run lengths,
-- reset count) is encrypted client-side into `payload`. Nothing here — not even
-- the date — is readable by the server or a DBA. Mirrors therapy_sessions.
create table public.sobriety_counters (
    id uuid primary key default gen_random_uuid(),
    user_id uuid not null default auth.uid() references auth.users (id),
    payload text not null check (char_length(payload) < 20000),
    created_at timestamptz not null default now()
);

alter table public.sobriety_counters enable row level security;

create policy "select own counters" on public.sobriety_counters
    for select using (auth.uid() = user_id);
create policy "insert own counters" on public.sobriety_counters
    for insert with check (auth.uid() = user_id);
create policy "update own counters" on public.sobriety_counters
    for update using (auth.uid() = user_id) with check (auth.uid() = user_id);
create policy "delete own counters" on public.sobriety_counters
    for delete using (auth.uid() = user_id);
