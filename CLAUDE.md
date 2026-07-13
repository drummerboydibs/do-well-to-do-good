# Agent notes — Do Well to Do Good

Project domain, stack, and constraints live in [`openspec/config.yaml`](openspec/config.yaml).
This file covers **workflow conventions** for agents working in this repo.

## OpenSpec changes ship in one PR (archive on the branch)

When a change is tracked by OpenSpec (anything under `openspec/`), keep the whole
thing in a **single pull request**:

1. `openspec new change <name>` → write proposal / design / specs / tasks.
2. Implement the feature, checking off tasks as you go.
3. Run `openspec archive <name>` **on the feature branch**. This folds the delta
   specs into `openspec/specs/` and moves the change into `openspec/changes/archive/`.
4. Open one PR containing the code, the promoted `openspec/specs/`, and the
   archived change. Merge that single PR.

**Do not** archive in a separate follow-up PR. Archiving on the branch makes the
spec update land atomically with the code, so `main` never sits in a state where
the feature exists but the living spec (`openspec/specs/`) hasn't been updated to
match. It also lets a reviewer see the final specs in the same diff as the code.

**Exception:** if a change needs proposal sign-off *before* any code is written,
the proposal may go in its own PR first; then implement + archive together in the
second PR.
