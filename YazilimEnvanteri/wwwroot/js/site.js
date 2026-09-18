(function (window, document) {
  "use strict";

  var cfg = window.AppConfig;

  function renderMasthead() {
    var titleEl = document.querySelector("[data-masthead-title]");
    var subtitleEl = document.querySelector("[data-masthead-subtitle]");
    if (titleEl) titleEl.textContent = cfg.branding.applicationName;
    if (subtitleEl) subtitleEl.textContent = cfg.branding.subtitle;

    var rightTop = document.querySelector("[data-masthead-right-top]");
    if (rightTop) {
      rightTop.innerHTML = cfg.branding.masthead.rightTop.map(function (t) { return "<span>" + t + "</span>"; }).join("");
    }

    var rightBottomLine = document.querySelector("[data-masthead-right-bottom-line]");
    if (rightBottomLine) rightBottomLine.textContent = cfg.branding.masthead.rightBottomLine;

    var rightBottomAccent = document.querySelector("[data-masthead-right-bottom-accent]");
    if (rightBottomAccent) rightBottomAccent.textContent = cfg.branding.masthead.rightBottomAccent;

    var tagline = document.querySelector("[data-sidebar-tagline]");
    if (tagline) {
      tagline.innerHTML = cfg.branding.tagline.map(function (t) { return "<span>" + t + "</span>"; }).join("");
    }
  }

  function renderSidebarNav() {
    var nav = document.querySelector("[data-sidebar-nav]");
    if (!nav) return;

    var activeKey = document.body.getAttribute("data-nav-key");

    nav.innerHTML = cfg.navigation.map(function (item) {
      var isActive = item.key === activeKey;
      var classes = "sidebar-link" + (isActive ? " active" : "");
      var disabledAttrs = item.enabled ? "" : ' aria-disabled="true" tabindex="-1"';
      return (
        '<a class="' + classes + '" href="' + item.href + '"' + disabledAttrs + (isActive ? ' aria-current="page"' : "") + ">" +
        cfg.icon(item.icon, 18) +
        '<span class="sidebar-link-label">' + item.label + "</span></a>"
      );
    }).join("");
  }

  function renderUserBlock() {
    var user = cfg.currentUser;
    var initialsEls = document.querySelectorAll("[data-user-initials]");
    initialsEls.forEach(function (el) { el.textContent = user.basHarfler; });

    var nameEl = document.querySelector("[data-user-name]");
    if (nameEl) nameEl.textContent = user.adSoyad;

    var roleEl = document.querySelector("[data-user-role]");
    if (roleEl) roleEl.textContent = user.rol;
  }

  function renderStaticIcons() {
    document.querySelectorAll("[data-icon]").forEach(function (el) {
      var key = el.getAttribute("data-icon");
      var size = Number(el.getAttribute("data-icon-size")) || 18;
      el.innerHTML = cfg.icon(key, size);
    });
  }

  document.addEventListener("DOMContentLoaded", function () {
    renderMasthead();
    renderSidebarNav();
    renderUserBlock();
    renderStaticIcons();

    window.Sidebar.init();
    window.Dropdown.init();

    var page = document.body.getAttribute("data-page");

    if (page === "projects") {
      window.ProjectsApp.init(window.__INITIAL_PROJECTS__ || [], window.__DATA_ERROR__ === true);
    }

    if (page === "project-detail") {
      window.ProjectDetail.init();
    }
  });
})(window, document);
