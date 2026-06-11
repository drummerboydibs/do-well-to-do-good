// Theme preference (light / dark / system) for Do Well to Do Good.
//
// The *preference* is stored in localStorage; the *effective* theme is
// resolved to a concrete "light" or "dark" and written to data-theme on
// <html>, which the stylesheet keys off. When the preference is "system"
// we follow the OS and live-update if it flips. An inline bootstrap in
// index.html applies the stored preference before first paint (no flash);
// this module keeps it in sync and exposes get/set to Blazor.
window.dwtdgTheme = (() => {
    const KEY = "dwtdg.theme";
    const mq = window.matchMedia("(prefers-color-scheme: dark)");

    function get() {
        try { return localStorage.getItem(KEY) || "system"; }
        catch { return "system"; }
    }

    function effective(pref) {
        return pref === "dark" || (pref !== "light" && mq.matches) ? "dark" : "light";
    }

    function apply(pref) {
        document.documentElement.setAttribute("data-theme", effective(pref));
    }

    function set(pref) {
        try { localStorage.setItem(KEY, pref); } catch { /* private mode — still apply for this session */ }
        apply(pref);
    }

    // Follow the OS while the user is on "system".
    mq.addEventListener("change", () => { if (get() === "system") apply("system"); });

    apply(get()); // belt-and-suspenders in case the inline bootstrap didn't run
    return { get, set, apply };
})();
