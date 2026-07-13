# Do Well to Do Good

A mental wellness app to get you back to your best — so you can make the world better again. **Designed from the ground up for privacy.**

Journal your thoughts, feelings, and mood — then save them privately (encrypted so only you can read them) or *"shout into the wind"* and let them go. Not sure what you're feeling? A gentle, fully keyboard-navigable **emotion wheel** helps you find the word. On each entry you get a mental-wellness tip drawn from **vetted, cited sources**.

**Live:** <https://drummerboydibs.github.io/do-well-to-do-good/>

---

## Features

- **Journaling** — write freely, name your mood, and either save an entry (encrypted, see below) or "shout into the wind" (released and cleared, never stored).
- **Accounts, no passwords** — sign in with an email **magic link** (Supabase). There's no password to remember, or for anyone to steal. Guest mode needs no account at all.
- **My journal** — your saved entries, newest first, color-tinted by emotional valence, **paginated** (10 per page, decrypting only the page you're viewing), with quick access back to writing.
- **Zero-knowledge encryption** — saved entries are encrypted in your browser with a key only you hold; the server stores nothing but ciphertext. See [Privacy & architecture](#privacy--architecture).
- **Emotion wheel** — the full three-ring feelings wheel (130 feelings, each with a plain-language definition). Tap a color to zoom into a family; fully operable by mouse, touch, and keyboard (arrow keys + Enter), with a live definition panel.
- **Cited tips** — wellness tips matched to your emotion, paraphrased in plain language and **credited with a link** to the original source (NHS, NIH — NIMH & NIA, APA, Mayo Clinic, and UC Berkeley's Greater Good). Not feeling it? A **"show me another"** shuffle offers a fresh one without repeating recent tips.
- **Therapy & goals** — signed-in users can log therapy sessions, set up to four goals (each with an optional target date), record progress over time, and get open-goal reminders on the home and writing pages — all encrypted, just like journal entries.
- **Recovery counters** — track days free from anything you're leaving behind (alcohol, nicotine, gambling, …), with AA-style **milestone** celebrations and a personal best. A setback resets *without guilt*: your longest and most-recent runs are kept, and you get a non-judgmental, research-backed message plus a link to support. Fully encrypted — even the date is invisible to the server.
- **Sleep journal** — log a night the next morning (bedtime, wake time, any night wake-ups and how long, what kept you up, daytime naps), with a live time-in-bed / estimated-sleep readout. A **review** shows your patterns as a **week-at-a-glance** bar chart and a **month** calendar heatmap, all worked out in the browser. Encrypted like everything else — only the night's date is stored in the clear.
- **Faith & belief (optional)** — opt into one or more traditions on your Account page to weave gently-worded, **cited** passages into your tips and a passage-of-the-day on Home and Write. Stocked with **Christianity** (KJV), **Islam** (Pickthall), **Judaism** (JPS 1917), and **Hinduism** (Bhagavad Gita, tr. Besant) — all public-domain translations, with wording and verse numbering verified against Bible Gateway, Quran.com, Sefaria, and Wikisource. Content is curated to be wellness-supportive and respectful of all groups, and your selection is **encrypted** — the server never learns your religion.
- **Word of the day** — a rotating feeling or wellbeing term with a plain-language definition, chosen deterministically per day and skewed toward gentler words so it never leads with a heavy one.
- **Resources** — a vetted directory of crisis lines and support (988 Suicide & Crisis Lifeline, Crisis Text Line, SAMHSA), gambling and addiction help, therapist directories, and an international fallback — reachable by anyone, signed in or not.
- **Account summary** — member-since and last-login dates (in your local time), plus your saved-entry count, "shouted into the wind" count, and current daily streak.
- **Adaptive, customizable navigation** — a horizontal bar on desktop and an app-style **bottom tab bar** on phones (with the overflow tucked under a "More" sheet). Signed-in users can choose which sections appear and in what order from their Account page; that layout is **encrypted and synced across your devices**, so the order — which could itself hint at what you're working through — never reaches the server in the clear.
- **Light / dark / system theme** — resolved before first paint (no flash), respecting your OS preference.
- **Guest mode** — use the journal, wheel, and tips without an account; nothing is saved anywhere.
- **Installable (PWA)** with a soothing, bubble-inspired design and WCAG 2.2 AA-minded accessibility (skip links, focus management, reduced-motion support).

## Tech stack

- **[Blazor WebAssembly](https://learn.microsoft.com/aspnet/core/blazor/) (.NET 8)** — runs entirely in the browser.
- **[Tailwind CSS v4](https://tailwindcss.com/)** — styling (compiled by the .NET build).
- **[Supabase](https://supabase.com/)** — email magic-link auth (GoTrue) and a Postgres database with per-user Row-Level Security, accessed directly over REST (PostgREST) — no server-side code of our own.
- **[Web Crypto API](https://developer.mozilla.org/docs/Web/API/Web_Crypto_API)** — in-browser key derivation and AES-GCM encryption (no crypto dependencies).
- **Self-hosted [Quicksand](https://fonts.google.com/specimen/Quicksand)** font — no third-party font CDN.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js](https://nodejs.org/) (LTS) — used to compile the Tailwind stylesheet

## Getting started

```bash
git clone https://github.com/drummerboydibs/do-well-to-do-good.git
cd do-well-to-do-good
dotnet run --project src/DoWellToDoGood
```

Then open the URL printed in the console. The app is wired to a live Supabase project via a **publishable** key (safe to ship — every data path is protected by Row-Level Security and entries are encrypted before they leave the browser), so both guest and signed-in experiences work out of the box.

> **Note on styles:** The compiled stylesheet `wwwroot/css/app.css` is generated by Tailwind at build time and is intentionally **not committed** (it's a build artifact). You don't need to do anything extra — an MSBuild target runs `npm install` and compiles the CSS automatically on `dotnet build` / `dotnet run`. For live style editing, run the watcher in a second terminal:
>
> ```bash
> cd src/DoWellToDoGood
> npm run watch:css
> ```

## Tests

Unit tests cover the pure logic — auth/JWT parsing, encryption-service wiring, the emotion taxonomy, tip selection and the tip library's data integrity (every tip cited, ids unique), the gentler word-of-the-day pool, streak and shout tracking, tip history, recovery-milestone math, sleep metrics (time-in-bed across midnight, estimated sleep, week/month aggregation) and encrypted-payload round-tripping, the faith passage library and its mood cascade, theming, navigation-layout reconciliation (ordering, hidden pages, last-write-wins sync), and pagination math:

```bash
dotnet test tests/DoWellToDoGood.Tests/DoWellToDoGood.Tests.csproj -p:SkipTailwind=true
```

`SkipTailwind=true` skips the Node/Tailwind step, which the tests don't need. CI runs the same suite on every push and pull request.

## Project structure

```
do-well-to-do-good/
├─ src/DoWellToDoGood/
│  ├─ Pages/        # Routable pages: Home, Journal, Entries (My journal), Sleep + Sleep review, Therapy, Recovery, Signin (Account), Resources, Palette
│  ├─ Layout/       # App shell: MainLayout, NavMenu (desktop bar), BottomNav (mobile tab bar)
│  ├─ Components/   # Reusable UI: EmotionWheel, VaultGate (encryption setup/unlock), NavIcon, OpenGoalsReminder, SobrietyCounters, DefinitionOfDay, FaithPassageOfDay
│  ├─ Models/       # Emotions taxonomy, cited Tips library, WellnessTerms vocabulary, recovery Milestones + encouragement, Sleep metrics, FaithLibrary, NavItems (nav catalog)
│  ├─ Services/     # Auth, Crypto, Entries, Therapy, Sobriety, Sleep, Faith, Stats, Theme, TipHistory, NavPrefs, Pagination, SupabaseConfig
│  ├─ Styles/       # tailwind.css (source) → wwwroot/css/app.css (generated)
│  ├─ wwwroot/      # Static assets: icons, fonts, js (theme / wheel / crypto), index.html
│  └─ package.json  # Tailwind CLI + self-hosted Quicksand
├─ tests/DoWellToDoGood.Tests/   # xUnit unit tests
├─ openspec/                     # Spec-driven change tracking: specs/ (living specs) + changes/archive/
├─ docs/                         # Contributor docs (e.g. testing authenticated pages)
├─ scripts/                      # Dev helpers (e.g. dev-login.ps1 for a local signed-in test session)
├─ CLAUDE.md                     # Contributor/agent workflow conventions (OpenSpec: one PR per change)
├─ .github/workflows/            # CI: tests.yml (dotnet test) · deploy.yml (GitHub Pages)
└─ DoWellToDoGood.sln
```

## Privacy & architecture

Privacy is the core design principle, not an afterthought:

- **Guest / "shout into the wind"** content stays in the browser, in memory only, and is wiped on submit, navigation, or session end — it never touches the network.
- **Saved entries use zero-knowledge encryption.** On first save you set an encryption passphrase; a key derived from it (Web Crypto PBKDF2) unwraps a random, per-user data key that AES-GCM-encrypts each entry's body and emotion. The server only ever stores ciphertext — unreadable by anyone, including a database administrator. A one-time **recovery code** is the only backup; losing both the passphrase and the recovery code means the entries are unrecoverable **by design**. The data key lives only in browser memory and is wiped on lock, sign-out, or page close.
- **Therapy notes are encrypted the same way.** Session notes, goals, and progress entries are AES-GCM-encrypted in the browser with that same per-user key. The only plaintext stored is a goal's optional target date — low-sensitivity metadata, kept in the clear so the app can sort and remind by it without decrypting everything.
- **Recovery counters go a step further** — even the clean-since date and day count are encrypted, not just the label. A bare row with a recent date could otherwise hint to a database admin that someone just had a setback, so nothing but an opaque blob is ever stored (id, an owner reference, and a timestamp aside).
- **Sleep logs are encrypted too.** Your bedtimes, wake-ups, what kept you up, and naps are all inside the vault; the only plaintext is the night's date, kept in the clear so the app can order nights and open one for editing without decrypting the whole history. The week/month review is computed in the browser from decrypted entries — the server never sees a single sleep figure.
- **Religion is treated as strictly private.** Your selected belief traditions are encrypted in your vault like everything else — never a plaintext column — because even knowing *which* faith someone picked is sensitive. And all scripture ships **inside the app**: it's never fetched from an outside scripture service at runtime, which would leak your faith and IP to a third party (the same reason fonts are self-hosted).
- **Even your navigation layout is private.** If you customize which sections show and their order, that preference is encrypted in your vault and synced across devices — a plaintext tab order could hint that someone leads with, say, the Recovery page. A per-device plaintext cache keeps the menu instant on load, while the server only ever stores ciphertext.
- **Passwordless auth** — email magic links only, so there's no password to leak. The database enforces per-user Row-Level Security, so a request can only ever touch its owner's rows.
- **No third-party tracking** — no profiling analytics and no third-party font CDNs (Quicksand is self-hosted), so visiting the app doesn't leak your data to outside services.

## Deployment

Pushes to `main` are published to **GitHub Pages** by [`.github/workflows/deploy.yml`](.github/workflows/deploy.yml): it publishes the Blazor WASM app to static files, rewrites the base href for the project subpath, stamps a content hash onto the `app.css` link and the hand-written JS (`crypto.js` / `theme.js` / `wheel.js`) so a fresh deploy isn't masked by a cached stylesheet or script, adds an SPA 404 fallback for deep links, and disables Jekyll so Blazor's `_framework` folder is served intact.

## Roadmap

The backlog lives in [GitHub Issues](https://github.com/drummerboydibs/do-well-to-do-good/issues). Highlights still open:

- **Search for saved entries** (privacy-preserving — a client-side or blind index, never a server-side content search).
- **.NET 10 upgrade.**

## Credits

Built by **Dylan Smith** and **Claude** in 2026.

Emotion taxonomy adapted from the Feelings Wheel (Gloria Willcox / Geoffrey Roberts). Wellness tips are credited inline to their sources.

## License

[MIT](LICENSE) © 2026 Dylan Smith
