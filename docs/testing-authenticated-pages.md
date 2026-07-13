# Testing authenticated pages (dev only)

The app signs in with **magic links only**, which is awkward to automate. For
local testing of signed-in pages, use a dedicated Supabase **test account** with
a password. Nothing secret is committed — real credentials live only in a
git-ignored file, and the `service_role` key is never used.

> Password sign-in is enabled here **only** for this one test account. Real users
> stay magic-link only; the app UI never offers a password.

## One-time setup

1. **Create the test user** — Supabase dashboard → Authentication → Users →
   **Add user → Create new user**:
   - Email: a dedicated test address (it never needs to receive mail).
   - Password: a strong password.
   - Tick **Auto Confirm User**.
   - Make sure the **Email** provider (with password) is enabled under
     Authentication → Providers.
2. **Store the credentials outside the repo** — so they're shared across sessions
   on this machine and can never be committed. Create `~/.dwtdg/dev-credentials.json`
   from the template:
   ```sh
   mkdir -p ~/.dwtdg
   cp dev-credentials.example.json ~/.dwtdg/dev-credentials.json
   ```
   Set `email`, `password`, and a `vaultPassphrase` (≥ 10 characters) of your
   choosing. Leave `recoveryCode` blank for now.

   `dev-login.ps1` finds this automatically. (A git-ignored
   `dev-credentials.local.json` in the repo root is also accepted as a fallback,
   or point `DWTDG_DEV_CREDENTIALS` at any path.)
3. **Set up the vault once** (see below). When the "Set up your private key" step
   shows a one-time recovery code, paste it into `recoveryCode` in the local file.

## Signing in for a test session

1. Start the preview (the `dwtdg` config, or `dotnet run --project src/DoWellToDoGood`).
2. Run the helper:
   ```sh
   pwsh scripts/dev-login.ps1
   ```
   It mints a session via Supabase's password grant (publishable key only) and
   prints a magic-link-style URL.
3. Open that URL in the preview. `AuthService` picks the tokens out of the URL
   fragment exactly as it would after a real magic link — you're now signed in.
   (It runs at app **startup**, so open the URL in a fresh tab or trigger a full
   reload — changing only the `#…` hash on an already-loaded page won't re-run it.)

## Unlocking the vault

Most signed-in pages (My journal, Therapy, Recovery, faith content) also require
the encryption vault to be unlocked:

- **First time:** the "Set up your private key" panel appears — enter your
  `vaultPassphrase`, then save the recovery code into the local file.
- **Later:** enter the same `vaultPassphrase` to unlock.

The passphrase never leaves the browser and is never stored on the server, so it
can't be recovered from the database — keep it (and the recovery code) in the
local file.

## Security notes

- Credentials live at `~/.dwtdg/dev-credentials.json` — **outside the repo**, so
  they're never committed and are shared across sessions on this machine. (The
  repo-root `dev-credentials.local.json` fallback is git-ignored either way.)
- Only the publishable key (already shipped in the client) is used; the
  `service_role` key is never used or stored.
- If you rotate the test account, just update the local file.
