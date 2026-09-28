## Context

Blazor WASM + Tailwind v4 with no component library. Each page hand-rolls its own card, header,
button-size overrides (`btn btn-ghost text-sm !min-h-0 !py-1 !px-2`) and inline edit and delete
buttons. The only sheet is baked into `BottomNav`. Summary widgets (`SobrietyCounters`,
`OpenGoalsReminder`, `DefinitionOfDay`, `FaithPassageOfDay`) are dropped onto several pages, and
each one handles the locked-vault case by rendering its own "Unlock…" nudge.
`CryptoService.Changed` already fires on unlock, and the widgets already subscribe to it, so an
unlock anywhere refreshes them.

This is phase 0 + 1 of a four-phase plan. Phase 2 (a new IA with Today / Journal / + / Track, and
Help and Account in the header) will replace the nav, so nothing here should change routes, the
nav catalog, or nav prefs.

## Goals / Non-Goals

**Goals:**
- A small set of reusable primitives that phase 2 will build on (`Sheet`, `ActionMenu`,
  `PageHeader`, `EmptyState`, `EncryptedBadge`, `UnlockCard`).
- Every page leads with its primary task, and on a phone its first screen is useful.
- 44px touch targets throughout.
- Zero lost functionality.

**Non-Goals:**
- Changing navigation, routes, `NavCatalog` or `NavPrefsService` (that's phase 2).
- Merging Write with My journal, or Sleep review into Sleep (phase 2).
- Onboarding and a Today dashboard redesign (phase 3). Home only gets reordered and trimmed here.
- Any data, payload, or encryption change.

## Decisions

**1. `Sheet` component extracted from `BottomNav`.**
Parameters: `Open`, `OnClose`, `Label`, `ChildContent`. It owns the scrim, the `role="dialog"
aria-modal`, the Escape handler, and focusing the sheet on open. The opener owns focus-return,
because only the opener holds the button reference. `ActionMenu` and `BottomNav` both do that
already. The existing `.sheet` CSS is reused. On `sm+`, the sheet is capped at 28rem and centred,
so row menus don't stretch across a desktop screen. `BottomNav` keeps its `sm:hidden` wrapper.
- *Alternative:* the native `<dialog>` element with `showModal()`. Rejected for now: it needs JS
  interop to open and close from Blazor, and the existing focus approach is already tested and
  working.

**2. `ActionMenu` = a ⋯ button that opens a `Sheet` of actions.**
API: `Label` (the accessible name, e.g. "Actions for entry from Mar 3"), and a list of
`MenuAction(string Text, Func<Task> Run, bool Danger = false)`. Selecting an action closes the
sheet, returns focus, then runs the action. Running the action after closing matters: the delete
actions call the JS `confirm()`, and the dialog shouldn't sit on top of an open sheet.
- *Alternative:* a desktop-style dropdown popover. Rejected: positioning without a library is
  fiddly, and a bottom action sheet is the mobile-native pattern.

**3. Touch targets via `.btn-sm`, not overrides.**
Add `.btn-sm { min-h-[2.75rem] min-w-[2.75rem] px-4 py-2 text-sm }`. Remove every
`!min-h-0 / !py-* / !px-*` override, and use `.btn-sm` where a compact button is wanted. Inline
date and number inputs drop `!min-h-0 py-1` and keep `w-auto`. `.nav-ctl` grows to 2.75rem.

**4. Progressive disclosure uses native `<details>`.**
It's accessible, keyboard-operable, and needs no state. Where it must start open because data
exists (Sleep wake-ups/naps when editing), render `open="@HasDetails"`. Blazor only re-applies the
attribute when the computed value changes, so a user's own toggle isn't fought on every render.
Secondary *forms* (log session, add counter, goal progress) use a button that toggles a bool
instead. That lets them close after a successful save and move focus into the first field.

**5. `UnlockCard` on Home only.**
It's a single card: "Your private data is locked", with an Unlock button that opens a `Sheet`
containing the existing `VaultGate`. It only renders when the user is signed in, the vault is
locked, and a keys row exists or setup is needed. It also covers the setup case, because
`VaultGate` handles both. `SobrietyCounters` and `OpenGoalsReminder` render nothing when locked.
Feature pages keep their inline `VaultGate`, since there the gate *is* the page content.

**6. Page layout order.**
- *Write:* heading + one line → textarea → "How are you feeling? (optional)" input + wheel toggle →
  actions → one-line release explainer. The guest sign-in nudge shortens to one line.
- *Home (signed in):* compact greeting (no 96px logo), [Start writing] [My journal] → `UnlockCard`
  → counters → goals → faith → word of the day → compact feature list (smaller rows, still every
  feature, which satisfies the navigation "Feature discovery on Home" requirement). The guest Home
  keeps all its sections.
- *Therapy:* header with a "Log a session" button that reveals the form. Open goals come first.
  Each goal shows its latest progress note, with older notes under a "Progress (n)" disclosure and
  "Log progress" revealing the note field. Session goal drafts start empty, behind
  "+ Add a goal from this session". Completed goals and past sessions stay in disclosures.
- *Recovery:* counters, then "Add a counter" (expanded only when there are none).
- *Sleep:* the form keeps date, bed and wake times, and thoughts. Wake-ups and naps go under
  "More details". Row actions move to the ⋯ menu.
- *Account:* `SobrietyCounters` and the redundant "Start writing / Open my journal" buttons are
  removed. The nav editor sits under a "Customize navigation" disclosure. A new "About" section
  holds the footer content, shown both signed in and signed out.

**7. Footer.**
The footer becomes `hidden sm:block`. The bottom-nav clearance moves from the footer's margin to
padding on the layout root (`.has-bottom-nav` becomes `padding-bottom` on the wrapper), so content
still clears the bar when the footer is gone.

**8. `PageHeader` / `EncryptedBadge` / `EmptyState`.**
- `PageHeader(Title, Subtitle?, Encrypted?, Actions RenderFragment?)` renders the `h1`, an optional
  one-line subtitle, the badge, and a right-aligned actions slot that wraps under the title on
  phones.
- `EncryptedBadge` is a `.badge` reading "🔒 Only you can read this".
- `EmptyState(Message, ActionText?, ActionHref?)` is a soft card with the message and an optional
  CTA.

## Risks / Trade-offs

- [Actions one tap deeper] → Mitigation: the primary action of each row stays visible where it's the
  main thing you do with it (a goal's "Log progress"). Only secondary and destructive actions go
  into ⋯.
- [Users used to the old layouts] → Mitigation: nothing is removed. Every control moved to a named
  disclosure or menu on the same page.
- [`confirm()` over a sheet] → Mitigation: close the sheet first, then run the action (Decision 2).
- [Feeling field below the text box may change how people use it] → Mitigation: the field is
  unchanged and still directly under the text box, and easy to swap back if it doesn't feel right.
- [Footer credits and "Suggest a feature" less visible on phones] → Accepted: they move to Account →
  About. Desktop is unchanged.

## Migration Plan

UI-only and shipped as one PR. Rollback is a revert of the PR. No stored data is touched.

## Open Questions

- None blocking. Phase 2 will revisit where Home's feature list and the unlock card end up once the
  Today tab exists.
