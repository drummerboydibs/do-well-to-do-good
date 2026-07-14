-- Track when a journal entry or therapy session was last edited, so the UI can
-- surface "edited on …". Left NULL on insert and set only when a row is edited,
-- which distinguishes an edited row from a never-edited one. Low-sensitivity
-- metadata like created_at, so it stays in the clear.
alter table public.journal_entries add column if not exists updated_at timestamptz;
alter table public.therapy_sessions add column if not exists updated_at timestamptz;
