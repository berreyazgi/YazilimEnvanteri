(function (window) {
  "use strict";

  var FILTER_META = [
    { key: "search", label: "Arama" },
    { key: "hizmetAlani", label: "Hizmet Alanı" },
    { key: "birim", label: "Birim" },
    { key: "durum", label: "Durum" },
    { key: "kritiklik", label: "Kritiklik" },
    { key: "sadeceAktif", label: "Sadece Aktif Projeler", boolean: true },
    { key: "sadeceWebAdresiOlan", label: "Web Adresi Olanlar", boolean: true }
  ];

  function escapeHtml(value) {
    return String(value == null ? "" : value).replace(/[&<>"']/g, function (ch) {
      return { "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[ch];
    });
  }

  function clearFilter(state, key) {
    state.filters[key] = FILTER_META.find(function (m) { return m.key === key; }).boolean ? false : "";

    if (key === "search") {
      document.querySelectorAll("[data-project-search]").forEach(function (el) { el.value = ""; });
    } else {
      var control = document.querySelector('[data-project-filter="' + key + '"]');
      if (control) {
        if (control.type === "checkbox") control.checked = false;
        else control.value = "";
      }
    }
  }

  function renderChips(state, cfg) {
    var container = document.querySelector("[data-filter-chips]");
    var clearAllBtn = document.querySelector("[data-clear-all-filters]");
    if (!container) return;

    var activeChips = FILTER_META.filter(function (m) {
      var value = state.filters[m.key];
      return m.boolean ? value === true : Boolean(value);
    });

    if (!activeChips.length) {
      container.innerHTML = "";
      container.style.display = "none";
      if (clearAllBtn) clearAllBtn.style.display = "none";
      return;
    }

    container.style.display = "flex";
    if (clearAllBtn) clearAllBtn.style.display = "inline-flex";

    container.innerHTML = activeChips.map(function (m) {
      var value = state.filters[m.key];
      var display = m.boolean ? m.label : (m.key === "search" ? '"' + value + '"' : m.label + " = " + value);
      return (
        '<span class="filter-chip" data-chip-key="' + m.key + '">' +
        escapeHtml(display) +
        '<button type="button" aria-label="' + escapeHtml(m.label) + ' filtresini kaldır">&times;</button>' +
        "</span>"
      );
    }).join("");
  }

  function bindClearAll(state, onChange, onOptionsRefresh) {
    var container = document.querySelector("[data-filter-chips]");
    var clearAllBtn = document.querySelector("[data-clear-all-filters]");

    if (container) {
      container.addEventListener("click", function (e) {
        var btn = e.target.closest("button");
        if (!btn) return;
        var chip = btn.closest("[data-chip-key]");
        if (!chip) return;
        clearFilter(state, chip.getAttribute("data-chip-key"));
        window.ProjectsApp.applyFilters();
        onOptionsRefresh();
        onChange();
      });
    }

    if (clearAllBtn) {
      clearAllBtn.addEventListener("click", function () {
        FILTER_META.forEach(function (m) { clearFilter(state, m.key); });
        window.ProjectsApp.applyFilters();
        onOptionsRefresh();
        onChange();
      });
    }
  }

  window.Filters = { renderChips: renderChips, bindClearAll: bindClearAll };
})(window);
