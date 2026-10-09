(function (window, document) {
  "use strict";

  // Light/dark switch for every [data-theme-toggle] button (sidebar, mobile header, login page).
  // The initial theme is already on <html> (Views/Shared/_ThemeInit.cshtml); this only flips it,
  // remembers the choice in localStorage and keeps the buttons' icon/label in sync.
  // Self-contained (own icons) because the Login/Register layout doesn't load app-config.js.
  var SVG = '<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">';
  var SUN = SVG + '<circle cx="12" cy="12" r="4" /><path d="M12 2v2M12 20v2M4.9 4.9l1.4 1.4M17.7 17.7l1.4 1.4M2 12h2M20 12h2M4.9 19.1l1.4-1.4M17.7 6.3l1.4-1.4" /></svg>';
  var MOON = SVG + '<path d="M21 12.8A9 9 0 1 1 11.2 3a7 7 0 0 0 9.8 9.8Z" /></svg>';

  function current() {
    return document.documentElement.getAttribute("data-theme") === "dark" ? "dark" : "light";
  }

  function storedTheme() {
    try { return window.localStorage.getItem("theme"); } catch (e) { return null; }
  }

  function render() {
    var dark = current() === "dark";
    // The button shows what clicking it switches *to*.
    var label = dark ? "Açık temaya geç" : "Koyu temaya geç";
    document.querySelectorAll("[data-theme-toggle]").forEach(function (button) {
      var withText = button.hasAttribute("data-theme-toggle-label");
      button.innerHTML = (dark ? SUN : MOON) + (withText ? "<span>" + (dark ? "Açık Tema" : "Koyu Tema") + "</span>" : "");
      button.setAttribute("aria-label", label);
      button.setAttribute("title", label);
    });
  }

  function apply(theme, persist) {
    document.documentElement.setAttribute("data-theme", theme);
    if (persist) {
      try { window.localStorage.setItem("theme", theme); } catch (e) { /* private mode: session-only */ }
    }
    render();
  }

  function init() {
    render();

    document.addEventListener("click", function (e) {
      if (e.target.closest("[data-theme-toggle]")) apply(current() === "dark" ? "light" : "dark", true);
    });

    // No explicit choice yet: keep following the OS setting while the page is open.
    if (window.matchMedia) {
      var query = window.matchMedia("(prefers-color-scheme: dark)");
      var onChange = function (e) { if (!storedTheme()) apply(e.matches ? "dark" : "light", false); };
      if (query.addEventListener) query.addEventListener("change", onChange); else query.addListener(onChange);
    }
  }

  window.Theme = { apply: apply, current: current };
  document.addEventListener("DOMContentLoaded", init);
})(window, document);
