(function (window) {
  "use strict";

  function bind(trigger, panel) {
    trigger.setAttribute("aria-expanded", "false");
    trigger.setAttribute("aria-haspopup", "true");

    trigger.addEventListener("click", function (e) {
      e.stopPropagation();
      var isOpen = panel.classList.contains("open");
      document.querySelectorAll(".dropdown-panel.open, .row-menu-panel.open").forEach(function (p) {
        p.classList.remove("open");
      });
      if (!isOpen) {
        panel.classList.add("open");
        trigger.setAttribute("aria-expanded", "true");
      } else {
        trigger.setAttribute("aria-expanded", "false");
      }
    });
  }

  function initStatic() {
    document.querySelectorAll("[data-dropdown-trigger]").forEach(function (trigger) {
      var targetId = trigger.getAttribute("data-dropdown-trigger");
      var panel = document.getElementById(targetId);
      if (panel) bind(trigger, panel);
    });

    document.addEventListener("click", function (e) {
      document.querySelectorAll(".dropdown-panel.open, .row-menu-panel.open").forEach(function (panel) {
        if (!panel.contains(e.target)) {
          panel.classList.remove("open");
          var trigger = document.querySelector('[data-dropdown-trigger="' + panel.id + '"], [aria-controls="' + panel.id + '"]');
          if (trigger) trigger.setAttribute("aria-expanded", "false");
        }
      });
    });

    document.addEventListener("keydown", function (e) {
      if (e.key === "Escape") {
        document.querySelectorAll(".dropdown-panel.open, .row-menu-panel.open").forEach(function (panel) {
          panel.classList.remove("open");
        });
      }
    });
  }

  window.Dropdown = { init: initStatic, bind: bind };
})(window);
