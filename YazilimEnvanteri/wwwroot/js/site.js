(function () {
  "use strict";

  function initSidebar() {
    var links = document.querySelectorAll(".sidebar-link");
    links.forEach(function (link) {
      link.addEventListener("click", function (e) {
        if (link.dataset.navDisabled === "true") {
          e.preventDefault();
        }
        links.forEach(function (l) { l.classList.remove("active"); });
        link.classList.add("active");
      });
    });
  }

  function initDropdown(triggerSelector, panelSelector) {
    var trigger = document.querySelector(triggerSelector);
    var panel = document.querySelector(panelSelector);
    if (!trigger || !panel) return;

    trigger.addEventListener("click", function (e) {
      e.stopPropagation();
      document.querySelectorAll(".dropdown-panel.open").forEach(function (p) {
        if (p !== panel) p.classList.remove("open");
      });
      panel.classList.toggle("open");
    });

    document.addEventListener("click", function (e) {
      if (!panel.contains(e.target) && e.target !== trigger) {
        panel.classList.remove("open");
      }
    });
  }

  document.addEventListener("DOMContentLoaded", function () {
    initSidebar();
    initDropdown("#userMenuTrigger", "#userMenuPanel");
    initDropdown("#bellTrigger", "#bellPanel");
  });
})();
