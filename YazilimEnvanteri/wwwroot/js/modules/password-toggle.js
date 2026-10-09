(function (window, document) {
  "use strict";

  // Show/hide button for every <input type="password"> on the page - no per-form markup needed,
  // so future password fields get it automatically. Self-contained (own icons) because the
  // Login/Register layout doesn't load app-config.js.
  var SVG = '<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">';
  var EYE = SVG + '<path d="M2 12s3.6-7 10-7 10 7 10 7-3.6 7-10 7S2 12 2 12Z" /><circle cx="12" cy="12" r="3" /></svg>';
  var EYE_OFF = SVG + '<path d="M3 3l18 18" /><path d="M10.6 5.1A10.8 10.8 0 0 1 12 5c6.4 0 10 7 10 7a17 17 0 0 1-3.2 4.1" /><path d="M6.6 6.6A17.4 17.4 0 0 0 2 12s3.6 7 10 7a10 10 0 0 0 5.4-1.6" /><path d="M9.9 9.9a3 3 0 0 0 4.2 4.2" /></svg>';

  function setVisible(input, button, visible) {
    input.type = visible ? "text" : "password";
    button.innerHTML = visible ? EYE_OFF : EYE;
    button.setAttribute("aria-label", visible ? "Şifreyi gizle" : "Şifreyi göster");
    button.setAttribute("aria-pressed", String(visible));
  }

  function enhance(input) {
    if (input.hasAttribute("data-password-toggle")) return;
    input.setAttribute("data-password-toggle", "");

    var wrapper = document.createElement("span");
    wrapper.className = "password-field";
    input.parentNode.insertBefore(wrapper, input);
    wrapper.appendChild(input);

    // type="button": never submits the form. Only `type` changes, so the value stays put.
    var button = document.createElement("button");
    button.type = "button";
    button.className = "password-toggle";
    if (input.id) button.setAttribute("aria-controls", input.id);
    wrapper.appendChild(button);
    setVisible(input, button, false);

    button.addEventListener("click", function () {
      setVisible(input, button, input.type === "password");
    });

    // Back to type="password" before submitting, so password managers still recognise the field
    // when offering to save it.
    if (input.form) {
      input.form.addEventListener("submit", function () { setVisible(input, button, false); });
    }
  }

  function init(root) {
    (root || document).querySelectorAll('input[type="password"]').forEach(enhance);
  }

  window.PasswordToggle = { init: init };
  document.addEventListener("DOMContentLoaded", function () { init(); });
})(window, document);
