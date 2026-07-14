-- Goal end dates are low-sensitivity metadata (like created_at, already plaintext)
-- and benefit from server-side sorting/filtering. Move them out of the encrypted
-- payload into a real date column. Goal titles + everything else stay encrypted.
alter table public.goals add column if not exists end_date date;
create index if not exists goals_user_done_end on public.goals (user_id, done, end_date);
