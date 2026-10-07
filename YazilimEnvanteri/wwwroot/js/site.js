(function (window, document) {
  "use strict";

  var cfg = window.AppConfig;

  function renderSidebarNav() {
    var nav = document.querySelector("[data-sidebar-nav]");
    if (!nav) return;

    var activeKey = document.body.getAttribute("data-nav-key");

    // Unfinished sections stay in AppConfig.navigation (enabled: false) but aren't shown, so the
    // menu never offers dead links.
    nav.innerHTML = cfg.navigation.filter(function (item) { return item.enabled; }).map(function (item) {
      var isActive = item.key === activeKey;
      var classes = "sidebar-link" + (isActive ? " active" : "");
      return (
        '<a class="' + classes + '" href="' + item.href + '"' + (isActive ? ' aria-current="page"' : "") + ">" +
        cfg.icon(item.icon, 18) +
        '<span class="sidebar-link-label">' + item.label + "</span></a>"
      );
    }).join("");
  }

  function renderUserBlock() {
    var user = cfg.currentUser;
    var initialsEls = document.querySelectorAll("[data-user-initials]");
    initialsEls.forEach(function (el) { el.textContent = user.basHarfler; });

    document.querySelectorAll("[data-user-name]").forEach(function (el) { el.textContent = user.adSoyad; });
    document.querySelectorAll("[data-user-role]").forEach(function (el) { el.textContent = user.rol; });
  }

  function renderStaticIcons() {
    document.querySelectorAll("[data-icon]").forEach(function (el) {
      var key = el.getAttribute("data-icon");
      var size = Number(el.getAttribute("data-icon-size")) || 18;
      el.innerHTML = cfg.icon(key, size);
    });
  }

  // Create/Edit finish with a full page reload (see project-form.js reloadWithFlag) rather than
  // a client-side redirect, so the success toast is surfaced here via a one-shot querystring flag
  // instead of TempData - there's no server-rendered redirect step in that AJAX-modal flow to hang
  // TempData off of. The flag is stripped immediately so a manual refresh won't re-show the toast.
  function showPendingToast() {
    var params = new URLSearchParams(window.location.search);
    var toastMessage = null;

    if (params.get("created") === "1") {
      toastMessage = "Projeniz başarıyla oluşturuldu.";
    } else if (params.get("updated") === "1") {
      toastMessage = "Projeniz başarıyla güncellendi.";
    } else if (params.get("deleted") === "1") {
      toastMessage = "Proje silindi.";
    }

    if (!toastMessage) return;

    params.delete("created");
    params.delete("updated");
    params.delete("deleted");
    var query = params.toString();
    var newUrl = window.location.pathname + (query ? "?" + query : "") + window.location.hash;
    window.history.replaceState({}, "", newUrl);

    if (window.Toast) window.Toast.show(toastMessage, "success");
  }

  document.addEventListener("DOMContentLoaded", function () {
    renderSidebarNav();
    renderUserBlock();
    renderStaticIcons();
    showPendingToast();

    window.Sidebar.init();
    window.Dropdown.init();

    var page = document.body.getAttribute("data-page");

    if (page === "projects") {
      window.ProjectsApp.init(window.__INITIAL_PROJECTS__ || [], window.__DATA_ERROR__ === true);
      window.ProjectForm.init();
    }

    if (page === "project-detail") {
      window.ProjectDetail.init();
      window.ProjectForm.init();
    }
  });
})(window, document);
