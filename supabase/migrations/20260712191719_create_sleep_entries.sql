-- Sleep journal: one encrypted record per night, mirroring the app's
-- privacy model (journal_entries / therapy_sessions). The only unencrypted
-- metadata is night_date (the morning the sleep is attributed to), which the
-- app needs to order/group/edit nights without decrypting; everything else
-- (bedtimes, wake-ups, thoughts, naps) lives inside the encrypted payload.
create table public.sleep_entries (
  id uuid primary key default gen_random_uuid(),
  user_id uuid not null default auth.uid() references auth.users (id) on delete cascade,
  night_date date not null,
  payload text not null check (char_length(payload) < 100000),
  created_at timestamptz not null default now(),
  unique (user_id, night_date)
);

alter table public.sleep_entries enable row level security;

create policy "sleep_entries owner can select"
  on public.sleep_entries for select
  using (auth.uid() = user_id);

create policy "sleep_entries owner can insert"
  on public.sleep_entries for insert
  with check (auth.uid() = user_id);

create policy "sleep_entries owner can update"
  on public.sleep_entries for update
  using (auth.uid() = user_id)
  with check (auth.uid() = user_id);

create policy "sleep_entries owner can delete"
  on public.sleep_entries for delete
  using (auth.uid() = user_id);

create index sleep_entries_user_night_idx
  on public.sleep_entries (user_id, night_date desc);
