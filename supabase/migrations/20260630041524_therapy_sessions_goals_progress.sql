-- Therapy session logging with goals and progress.
-- Same zero-knowledge model as journal_entries: every payload is ciphertext
-- the server cannot read. Only structural columns (done flag, fks, timestamps)
-- are in the clear, and none of them reveal entry content.

create table public.therapy_sessions (
  id         uuid primary key default gen_random_uuid(),
  user_id    uuid not null default auth.uid() references auth.users(id) on delete cascade,
  payload    text not null,          -- encrypted JSON { date, notes }
  created_at timestamptz not null default now()
);
alter table public.therapy_sessions enable row level security;
create policy "select own sessions" on public.therapy_sessions for select using (auth.uid() = user_id);
create policy "insert own sessions" on public.therapy_sessions for insert with check (auth.uid() = user_id);
create policy "update own sessions" on public.therapy_sessions for update using (auth.uid() = user_id) with check (auth.uid() = user_id);
create policy "delete own sessions" on public.therapy_sessions for delete using (auth.uid() = user_id);
create index therapy_sessions_user_created on public.therapy_sessions (user_id, created_at desc);
alter table public.therapy_sessions add constraint session_payload_size check (char_length(payload) < 100000);

create table public.goals (
  id         uuid primary key default gen_random_uuid(),
  user_id    uuid not null default auth.uid() references auth.users(id) on delete cascade,
  session_id uuid references public.therapy_sessions(id) on delete set null,
  payload    text not null,          -- encrypted JSON { title, end }
  done       boolean not null default false,
  created_at timestamptz not null default now()
);
alter table public.goals enable row level security;
create policy "select own goals" on public.goals for select using (auth.uid() = user_id);
create policy "insert own goals" on public.goals for insert with check (auth.uid() = user_id);
create policy "update own goals" on public.goals for update using (auth.uid() = user_id) with check (auth.uid() = user_id);
create policy "delete own goals" on public.goals for delete using (auth.uid() = user_id);
create index goals_user_open on public.goals (user_id, done, created_at desc);
alter table public.goals add constraint goal_payload_size check (char_length(payload) < 20000);

create table public.goal_progress (
  id         uuid primary key default gen_random_uuid(),
  user_id    uuid not null default auth.uid() references auth.users(id) on delete cascade,
  goal_id    uuid not null references public.goals(id) on delete cascade,
  payload    text not null,          -- encrypted JSON { date, note }
  created_at timestamptz not null default now()
);
alter table public.goal_progress enable row level security;
create policy "select own progress" on public.goal_progress for select using (auth.uid() = user_id);
create policy "insert own progress" on public.goal_progress for insert with check (auth.uid() = user_id);
create policy "update own progress" on public.goal_progress for update using (auth.uid() = user_id) with check (auth.uid() = user_id);
create policy "delete own progress" on public.goal_progress for delete using (auth.uid() = user_id);
create index goal_progress_goal_created on public.goal_progress (goal_id, created_at desc);
alter table public.goal_progress add constraint progress_payload_size check (char_length(payload) < 50000);
