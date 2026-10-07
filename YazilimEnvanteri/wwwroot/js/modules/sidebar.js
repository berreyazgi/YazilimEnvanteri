(function (window) {
  "use strict";

  function init() {
    var toggle = document.querySelector("[data-sidebar-toggle]");
    var sidebar = document.querySelector("[data-sidebar]");
    var overlay = document.querySelector("[data-sidebar-overlay]");

    function closeSidebar() {
      if (!sidebar || !overlay) return;
      sidebar.classList.remove("open");
      overlay.classList.remove("open");
      document.body.classList.remove("sidebar-open");
      if (toggle) toggle.setAttribute("aria-expanded", "false");
    }

    function openSidebar() {
      if (!sidebar || !overlay) return;
      sidebar.classList.add("open");
      overlay.classList.add("open");
      document.body.classList.add("sidebar-open");
      if (toggle) toggle.setAttribute("aria-expanded", "true");
    }

    if (toggle && sidebar && overlay) {
      toggle.addEventListener("click", function () {
        if (sidebar.classList.contains("open")) {
          closeSidebar();
        } else {
          openSidebar();
        }
      });
      overlay.addEventListener("click", closeSidebar);
    }

    document.querySelectorAll(".sidebar-link").forEach(function (link) {
      link.addEventListener("click", function (e) {
        if (link.getAttribute("aria-disabled") === "true") {
          e.preventDefault();
          return;
        }
        closeSidebar();
      });
    });

    // Leaving drawer mode (e.g. rotating a tablet to a wide landscape) while the drawer is open
    // would otherwise leave the body scroll lock stuck. Drawer mode is capability-based (see
    // AppConfig.media.touchCompact), so resizing a desktop window never enters or leaves it.
    var drawerQuery = window.matchMedia(window.AppConfig.media.touchCompact);
    var onDrawerModeChange = function (e) { if (!e.matches) closeSidebar(); };
    if (drawerQuery.addEventListener) {
      drawerQuery.addEventListener("change", onDrawerModeChange);
    } else {
      drawerQuery.addListener(onDrawerModeChange);
    }
  }

  window.Sidebar = { init: init };
})(window);
