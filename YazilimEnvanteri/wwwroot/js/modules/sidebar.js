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
      if (toggle) toggle.setAttribute("aria-expanded", "false");
    }

    function openSidebar() {
      if (!sidebar || !overlay) return;
      sidebar.classList.add("open");
      overlay.classList.add("open");
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
  }

  window.Sidebar = { init: init };
})(window);
