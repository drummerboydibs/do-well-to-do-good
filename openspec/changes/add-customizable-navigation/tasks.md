## 1. Navigation catalog and icons

- [x] 1.1 Add `Models/NavItems.cs` with `NavItem` record and `NavCatalog` (default order, fixed anchors, auth flags, blurbs)
- [x] 1.2 Add `Components/NavIcon.razor` mapping catalog icon names to line SVGs

## 2. Data model

- [x] 2.1 Apply migration `add_nav_payload_to_user_prefs`: add nullable, length-checked `nav_payload`; drop NOT NULL on `payload`

## 3. Preferences service (hybrid persistence)

- [x] 3.1 Add `Services/NavPrefsService.cs` with in-memory order/hidden + embedded timestamp
- [x] 3.2 Implement local cache load/save (localStorage) applied at startup
- [x] 3.3 Implement encrypted server read/write via PostgREST `user_prefs.nav_payload`
- [x] 3.4 Reconcile local vs server last-write-wins on vault unlock (subscribe to `CryptoService.Changed`)
- [x] 3.5 Register in `Program.cs` and initialise the local cache on boot

## 4. Responsive navigation surfaces

- [x] 4.1 Rewrite `Layout/NavMenu.razor` as a desktop-only row driven by `NavPrefsService`
- [x] 4.2 Add `Layout/BottomNav.razor`: mobile bottom tab bar (Home + tabs + `More` sheet), with active state and close-on-navigate
- [x] 4.3 Wire `MainLayout.razor`: responsive show/hide, mount `BottomNav`, bottom-bar content clearance

## 5. Home discovery and shell width

- [x] 5.1 Add Home "Explore" section: a card per feature, locked cards prompt guests to sign in
- [x] 5.2 Add `.shell` (~900px) and apply to header/footer/Home/Resources; keep prose at reading width
- [x] 5.3 Add bottom-bar, sheet, `.nav-ctl`, and `.shell` styles to `Styles/tailwind.css`

## 6. Account customization UI

- [x] 6.1 Add the "Navigation" section to `Pages/Signin.razor`: reorder (↑/↓), show/hide, reset
- [x] 6.2 Update copy to state the layout is encrypted and synced

## 7. Verification

- [x] 7.1 Build succeeds; Tailwind compiles
- [x] 7.2 Verify local-cache path in-browser (guest reorder/hide honoured across desktop row and bottom bar)
- [x] 7.3 Verify encrypted cross-device sync end-to-end with a real signed-in + unlocked session
- [x] 7.4 Add automated coverage for `NavPrefsService` reconciliation (last-write-wins, catalog reconcile)
- [x] 7.5 Accessibility pass: keyboard operation of the editor and bottom-bar/sheet, focus handling
