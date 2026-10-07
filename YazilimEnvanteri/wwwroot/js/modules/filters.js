(function (window) {
  "use strict";

  // multi: array-valued filters (one chip per selected value); boolean: checkbox filters.
  var FILTER_META = [
    { key: "search", label: "Arama" },
    { key: "hizmetAlani", label: "Hizmet Alanı", multi: true },
    { key: "birim", label: "Birim", multi: true },
    { key: "durum", label: "Durum", multi: true },
    { key: "kritiklik", label: "Kritiklik", multi: true },
    { key: "sadeceAktif", label: "Sadece Aktif Projeler", boolean: true },
    { key: "sadeceWebAdresiOlan", label: "Web Adresi Olanlar", boolean: true }
  ];

  function metaFor(key) {
    return FILTER_META.find(function (m) { return m.key === key; });
  }

  function isFilterActive(meta, value) {
    if (meta.multi) return value.length > 0;
    return meta.boolean ? value === true : Boolean(value);
  }

  var escapeHtml = window.AppConfig.escapeHtml;

  // Updates state only for the multi-selects - their Choices.js widgets are re-synced from state by
  // the onOptionsRefresh callback (projects.js populateFilterSelects). The search box and the
  // checkboxes are plain inputs, so they are reset right here.
  function clearFilter(state, key, value) {
    var meta = metaFor(key);

    if (meta.multi) {
      state.filters[key] = value === undefined
        ? []
        : state.filters[key].filter(function (v) { return v !== value; });
      return;
    }

    state.filters[key] = meta.boolean ? false : "";

    if (key === "search") {
      document.querySelectorAll("[data-project-search]").forEach(function (el) { el.value = ""; });
    } else {
      var control = document.querySelector('[data-project-filter="' + key + '"]');
      if (control && control.type === "checkbox") control.checked = false;
    }
  }

  function chipMarkup(key, label, value) {
    var valueAttr = value === undefined ? "" : ' data-chip-value="' + escapeHtml(value) + '"';
    return (
      '<span class="filter-chip" data-chip-key="' + key + '"' + valueAttr + ">" +
      escapeHtml(label) +
      '<button type="button" aria-label="' + escapeHtml(label) + ' filtresini kaldır">&times;</button>' +
      "</span>"
    );
  }

  function renderChips(state) {
    var container = document.querySelector("[data-filter-chips]");
    if (!container) return;

    var chips = [];
    FILTER_META.forEach(function (m) {
      var value = state.filters[m.key];
      if (m.multi) {
        value.forEach(function (v) { chips.push(chipMarkup(m.key, m.label + ": " + v, v)); });
      } else if (isFilterActive(m, value)) {
        chips.push(chipMarkup(m.key, m.boolean ? m.label : '"' + value + '"'));
      }
    });

    container.innerHTML = chips.join("");
    container.style.display = chips.length ? "flex" : "none";
  }

  // Everything that reflects state.filters outside the controls themselves. Called from
  // ProjectsApp.renderAll, which every filter change and chip removal goes through.
  function render(state) {
    renderChips(state);
  }

  function bindChipRemoval(state, onChange, onOptionsRefresh) {
    var container = document.querySelector("[data-filter-chips]");

    function refresh() {
      onOptionsRefresh();
      window.ProjectsApp.applyFilters();
      onChange();
    }

    if (container) {
      container.addEventListener("click", function (e) {
        var btn = e.target.closest("button");
        if (!btn) return;
        var chip = btn.closest("[data-chip-key]");
        if (!chip) return;
        var value = chip.hasAttribute("data-chip-value") ? chip.getAttribute("data-chip-value") : undefined;
        clearFilter(state, chip.getAttribute("data-chip-key"), value);
        refresh();
      });
    }
  }

  window.Filters = { render: render, bindChipRemoval: bindChipRemoval };
})(window);
