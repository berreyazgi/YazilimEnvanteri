(function (window) {
  "use strict";

  function init() {
    var tabs = document.querySelectorAll("[data-tab]");
    if (!tabs.length) return;

    tabs.forEach(function (tab) {
      tab.addEventListener("click", function () {
        var target = tab.getAttribute("data-tab");

        tabs.forEach(function (t) {
          var isActive = t === tab;
          t.classList.toggle("active", isActive);
          t.setAttribute("aria-selected", String(isActive));
        });

        document.querySelectorAll("[data-tab-panel]").forEach(function (panel) {
          panel.classList.toggle("active", panel.getAttribute("data-tab-panel") === target);
        });
      });
    });
  }

  window.ProjectDetail = { init: init };
})(window);
