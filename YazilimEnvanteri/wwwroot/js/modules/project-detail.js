(function (window) {
  "use strict";

  function bindTabs() {
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

  // detail-header actions: "Düzenle" opens the same edit modal as the list's row menu, and the
  // "..." menu reuses the shared Dropdown row-menu behaviour (click/tap, one open at a time).
  function bindHeaderActions() {
    document.querySelectorAll(".detail-header-actions [data-row-menu-trigger]").forEach(function (trigger) {
      var panel = trigger.nextElementSibling;
      if (panel && panel.classList.contains("row-menu-panel")) window.Dropdown.bind(trigger, panel);
    });

    document.querySelectorAll("[data-edit-proje]").forEach(function (btn) {
      btn.addEventListener("click", function () {
        var id = Number(btn.getAttribute("data-edit-proje"));
        if (window.ProjectForm && window.ProjectForm.openEdit) window.ProjectForm.openEdit(id);
      });
    });

    document.querySelectorAll("[data-delete-proje]").forEach(function (btn) {
      btn.addEventListener("click", function () {
        window.ProjectForm.deleteProje(Number(btn.getAttribute("data-delete-proje")));
      });
    });
  }

  function init() {
    bindTabs();
    bindHeaderActions();
  }

  window.ProjectDetail = { init: init };
})(window);
