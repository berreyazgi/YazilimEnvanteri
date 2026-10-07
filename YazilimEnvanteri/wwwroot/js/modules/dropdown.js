(function (window) {
  "use strict";

  var OPEN_SELECTOR = ".dropdown-panel.open, .row-menu-panel.open";
  var VIEWPORT_GAP = 8;

  function closePanel(panel) {
    panel.classList.remove("open");
    if (panel._dropdownTrigger) panel._dropdownTrigger.setAttribute("aria-expanded", "false");
  }

  function closeAll() {
    document.querySelectorAll(OPEN_SELECTOR).forEach(closePanel);
  }

  // Row "..." menus are positioned against the viewport (position: fixed) instead of their row,
  // so the table's overflow-x scroll container can't clip them and they can't render off-screen:
  // right-aligned under the trigger, flipped above it when there's no room below.
  function positionFloatingPanel(trigger, panel) {
    var rect = trigger.getBoundingClientRect();
    var width = panel.offsetWidth;
    var height = panel.offsetHeight;
    var viewportWidth = document.documentElement.clientWidth;
    var viewportHeight = window.innerHeight;

    var left = Math.min(rect.right - width, viewportWidth - width - VIEWPORT_GAP);
    left = Math.max(VIEWPORT_GAP, left);

    var top = rect.bottom + 6;
    if (top + height > viewportHeight - VIEWPORT_GAP && rect.top - height - 6 >= VIEWPORT_GAP) {
      top = rect.top - height - 6;
    }

    panel.style.left = left + "px";
    panel.style.top = top + "px";
  }

  // Plain click handlers only (no hover), so the same code works for mouse and touch.
  function bind(trigger, panel) {
    panel._dropdownTrigger = trigger;
    trigger.setAttribute("aria-expanded", "false");
    trigger.setAttribute("aria-haspopup", "true");

    trigger.addEventListener("click", function (e) {
      e.stopPropagation();
      var isOpen = panel.classList.contains("open");
      closeAll();
      if (!isOpen) {
        panel.classList.add("open");
        trigger.setAttribute("aria-expanded", "true");
        if (panel.classList.contains("row-menu-panel")) positionFloatingPanel(trigger, panel);
      }
    });

    // Picking an action (edit, copy, open link...) closes its menu.
    panel.addEventListener("click", function (e) {
      if (e.target.closest("a, button")) closePanel(panel);
    });
  }

  function initStatic() {
    document.querySelectorAll("[data-dropdown-trigger]").forEach(function (trigger) {
      var targetId = trigger.getAttribute("data-dropdown-trigger");
      var panel = document.getElementById(targetId);
      if (panel) bind(trigger, panel);
    });

    document.addEventListener("click", function (e) {
      document.querySelectorAll(OPEN_SELECTOR).forEach(function (panel) {
        if (!panel.contains(e.target)) closePanel(panel);
      });
    });

    document.addEventListener("keydown", function (e) {
      if (e.key === "Escape") closeAll();
    });

    // A fixed-position row menu would otherwise stay put while its row scrolls away.
    function closeFloatingPanels() {
      document.querySelectorAll(".row-menu-panel.open").forEach(closePanel);
    }
    window.addEventListener("scroll", closeFloatingPanels, true);
    window.addEventListener("resize", closeFloatingPanels);
  }

  window.Dropdown = { init: initStatic, bind: bind, closeAll: closeAll };
})(window);
