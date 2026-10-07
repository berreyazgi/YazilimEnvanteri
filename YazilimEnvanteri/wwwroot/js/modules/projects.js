(function (window) {
  "use strict";

  var cfg = window.AppConfig;

  var state = {
    projects: [],
    filteredProjects: [],
    dataError: false,

    // hizmetAlani/birim/durum/kritiklik are multi-select: values inside one array are OR-ed,
    // the different filters are AND-ed (same rule as ProjeController.ApplyFilter for exports).
    filters: {
      search: "",
      hizmetAlani: [],
      birim: [],
      durum: [],
      kritiklik: [],
      sadeceAktif: false,
      sadeceWebAdresiOlan: false
    },

    pagination: {
      page: 1,
      pageSize: cfg.defaultPageSize
    },

    sort: {
      field: null,
      direction: "asc"
    }
  };

  // Multi-select filter key -> the project field it matches against.
  var MULTI_FILTER_FIELDS = {
    hizmetAlani: "projeHizmetAlani",
    birim: "birim",
    durum: "projeDurum",
    kritiklik: "projeKritiklik"
  };

  // Choices.js instances by filter key (absent when the library didn't load - the native
  // <select multiple> then keeps working on its own).
  var filterChoices = {};
  var syncingFilterControls = false;

  function normalize(value) {
    return String(value == null ? "" : value).toLocaleLowerCase("tr-TR");
  }

  function distinctValues(list, key) {
    var seen = {};
    var out = [];
    list.forEach(function (item) {
      var value = item[key];
      if (value && !seen[value]) {
        seen[value] = true;
        out.push(value);
      }
    });
    out.sort(function (a, b) { return a.localeCompare(b, "tr-TR"); });
    return out;
  }

  function matchesSearch(project, term) {
    if (!term) return true;
    var haystacks = [
      project.projeAdi, project.projeKodu, project.projeHizmetAlani,
      project.sunucu, project.websiteUrl, project.birim,
      project.yazilimUzmaniAdSoyad, project.backendTeknoloji, project.frontendTeknoloji
    ];
    return haystacks.some(function (h) { return normalize(h).indexOf(term) !== -1; });
  }

  function applyFilters() {
    var f = state.filters;
    var term = normalize(f.search);

    state.filteredProjects = state.projects.filter(function (p) {
      if (!matchesSearch(p, term)) return false;

      var failsMultiFilter = Object.keys(MULTI_FILTER_FIELDS).some(function (key) {
        var selected = f[key];
        return selected.length > 0 && selected.indexOf(p[MULTI_FILTER_FIELDS[key]]) === -1;
      });
      if (failsMultiFilter) return false;

      if (f.sadeceAktif && !p.projeAktifMi) return false;
      if (f.sadeceWebAdresiOlan && !p.websiteUrl) return false;
      return true;
    });

    applySort();
    state.pagination.page = 1;
  }

  function applySort() {
    var field = state.sort.field;
    if (!field) return;
    var direction = state.sort.direction === "desc" ? -1 : 1;
    // Durum/Kritiklik sort by their enum order (Analiz -> Yayında, Düşük -> Yüksek), not alphabetically.
    var themes = { projeDurum: cfg.statusThemes, projeKritiklik: cfg.criticalityThemes }[field];
    var key = function (p) {
      var theme = themes && themes[p[field]];
      return theme ? theme.value : p[field];
    };

    state.filteredProjects.sort(function (a, b) {
      var av = key(a);
      var bv = key(b);
      if (typeof av === "number" && typeof bv === "number") {
        return (av - bv) * direction;
      }
      // numeric: true keeps mixed codes in natural order (PRJ-9 before PRJ-10, 999 before 1000).
      return normalize(av).localeCompare(normalize(bv), "tr-TR", { numeric: true }) * direction;
    });
  }

  var escapeHtml = window.AppConfig.escapeHtml;

  function badgeMarkup(value, themeMap) {
    if (!value) return '<span class="status-badge status-badge--neutral">Bilgi yok</span>';
    var theme = themeMap[value] || { label: value, tone: "neutral" };
    return '<span class="status-badge status-badge--' + theme.tone + '">' + escapeHtml(theme.label) + "</span>";
  }

  // The table clamps long text to fit its fixed column widths (projects.css), so every text
  // block carries its full value as a tooltip.
  function clampedText(value, className) {
    var text = escapeHtml(value);
    return '<div class="' + className + '" title="' + text + '">' + text + "</div>";
  }

  function cellMarkup(project, column) {
    if (column.type === "status") return badgeMarkup(project.projeDurum, cfg.statusThemes);
    if (column.type === "criticality") return badgeMarkup(project.projeKritiklik, cfg.criticalityThemes);

    if (column.key === "sunucu") {
      var parts = [];
      if (project.sunucu) parts.push(clampedText(project.sunucu, "cell-title cell-clamp"));
      if (project.websiteUrl) parts.push(clampedText(project.websiteUrl, "cell-secondary cell-clamp"));
      return parts.length ? parts.join("") : '<span class="cell-muted">Bilgi bulunmuyor</span>';
    }

    if (column.key === "projeAdi") {
      // The extra line is only ever shown by CSS when the table card is narrow, where the Proje
      // Kodu / Hizmet Alanı columns are hidden and folded into this cell instead - no separate
      // render path, just a container-query-controlled reveal of markup that's always there.
      return (
        clampedText(project.projeAdi, "cell-title cell-clamp") +
        clampedText("Kod: " + project.projeKodu + " · " + project.projeHizmetAlani, "cell-meta-compact cell-clamp")
      );
    }

    var value = project[column.key];
    if (!value) return '<span class="cell-muted">Bilgi bulunmuyor</span>';
    return clampedText(value, "cell-clamp");
  }

  // Shared between desktop table rows and mobile cards so there is exactly one place that
  // knows what actions a project row exposes (Detaylar / Web Sitesini Aç / Projeyi Sil).
  function projectActionsMarkup(project) {
    var websiteAction = project.websiteUrl
      ? '<a href="' + escapeHtml(project.websiteUrl) + '" target="_blank" rel="noopener">' + cfg.icon("externalLink", 15) + " Web Sitesini Aç</a>"
      : "";

    return (
      '<div class="row-actions">' +
      '<a class="app-button app-button--outline app-button--sm" href="/Proje/Details/' + project.id + '">Detaylar</a>' +
      '<button type="button" class="row-menu-trigger" data-row-menu-trigger aria-haspopup="true" aria-expanded="false" aria-label="Diğer işlemler">' + cfg.icon("moreHorizontal", 16) + "</button>" +
      '<div class="row-menu-panel">' +
      '<a href="/Proje/Details/' + project.id + '">' + cfg.icon("externalLink", 15) + " Detayı Görüntüle</a>" +
      '<button type="button" data-edit-proje="' + project.id + '">' + cfg.icon("edit", 15) + " Projeyi Düzenle</button>" +
      websiteAction +
      '<button type="button" class="row-menu-danger" data-delete-proje="' + project.id + '">' + cfg.icon("trash", 15) + " Projeyi Sil</button>" +
      "</div></div>"
    );
  }

  // Binds the row-menu-trigger/edit/delete handlers inside whichever container was just
  // re-rendered - the table body or the mobile card list both use this.
  function bindRowActionEvents(container) {
    // The panel is the trigger's own sibling. (It used to be looked up by id, but the table row
    // and the mobile card render the same project twice, so on mobile the lookup found the hidden
    // table copy and the "..." button appeared to do nothing.)
    container.querySelectorAll("[data-row-menu-trigger]").forEach(function (trigger) {
      var panel = trigger.nextElementSibling;
      if (panel && panel.classList.contains("row-menu-panel")) window.Dropdown.bind(trigger, panel);
    });

    container.querySelectorAll("[data-delete-proje]").forEach(function (btn) {
      btn.addEventListener("click", function () {
        window.ProjectForm.deleteProje(Number(btn.getAttribute("data-delete-proje")));
      });
    });

    container.querySelectorAll("[data-edit-proje]").forEach(function (btn) {
      btn.addEventListener("click", function () {
        var id = Number(btn.getAttribute("data-edit-proje"));
        if (window.ProjectForm) window.ProjectForm.openEdit(id);
      });
    });
  }

  function renderTableHead() {
    var head = document.querySelector("[data-project-table-head]");
    if (!head) return;

    var cells = cfg.projectColumns.map(function (col) {
      var sortAttr = col.sortable ? ' class="sortable" data-sort-key="' + col.key + '"' : "";
      var indicator = col.sortable ? '<span class="sort-indicator" data-sort-indicator="' + col.key + '">' + cfg.icon("sortNeutral", 12) + "</span>" : "";
      return "<th data-col=\"" + col.key + "\"" + sortAttr + ">" + escapeHtml(col.label) + indicator + "</th>";
    });
    cells.push("<th data-col=\"actions\">İşlemler</th>");
    head.innerHTML = "<tr>" + cells.join("") + "</tr>";

    head.querySelectorAll("th.sortable").forEach(function (th) {
      th.addEventListener("click", function () {
        var key = th.getAttribute("data-sort-key");
        if (state.sort.field === key) {
          state.sort.direction = state.sort.direction === "asc" ? "desc" : "asc";
        } else {
          state.sort.field = key;
          state.sort.direction = "asc";
        }
        applySort();
        state.pagination.page = 1;
        renderAll();
      });
    });
  }

  function updateSortIndicators() {
    document.querySelectorAll("[data-sort-indicator]").forEach(function (el) {
      var key = el.getAttribute("data-sort-indicator");
      var active = state.sort.field === key;
      el.classList.toggle("active", active);
      el.innerHTML = active
        ? cfg.icon(state.sort.direction === "asc" ? "sortAsc" : "sortDesc", 12)
        : cfg.icon("sortNeutral", 12);
    });
  }

  function renderRows(pageItems) {
    var body = document.querySelector("[data-project-table-body]");
    var emptyState = document.querySelector("[data-project-empty-state]");
    var tableWrap = document.querySelector("[data-project-table-wrap]");
    var errorState = document.querySelector("[data-project-error-state]");
    if (!body) return;

    if (state.dataError) {
      if (tableWrap) tableWrap.style.display = "none";
      if (emptyState) emptyState.style.display = "none";
      if (errorState) errorState.style.display = "flex";
      body.innerHTML = "";
      return;
    }

    if (errorState) errorState.style.display = "none";

    if (!pageItems.length) {
      if (tableWrap) tableWrap.style.display = "none";
      if (emptyState) emptyState.style.display = "flex";
      body.innerHTML = "";
      return;
    }

    if (tableWrap) tableWrap.style.display = "block";
    if (emptyState) emptyState.style.display = "none";

    body.innerHTML = pageItems.map(function (project) {
      var cells = cfg.projectColumns.map(function (col) {
        return '<td data-col="' + col.key + '">' + cellMarkup(project, col) + "</td>";
      }).join("");

      return "<tr>" + cells + "<td data-col=\"actions\">" + projectActionsMarkup(project) + "</td></tr>";
    }).join("");

    bindRowActionEvents(body);
  }

  // Card representation of the exact same pageItems used by renderRows - only ever shown by
  // CSS on touch phones, never chosen/detected here in JS.
  function projectCardMarkup(project) {
    var uzmanText = project.yazilimUzmaniAdSoyad
      ? escapeHtml(project.yazilimUzmaniAdSoyad)
      : '<span class="cell-muted">Bilgi bulunmuyor</span>';
    var sunucuText = project.sunucu
      ? escapeHtml(project.sunucu)
      : '<span class="cell-muted">Bilgi bulunmuyor</span>';

    return (
      '<div class="app-card project-card">' +
      '<div class="project-card-header">' +
      '<div class="project-card-title">' + escapeHtml(project.projeAdi) + "</div>" +
      '<div class="project-card-meta">Kod: ' + escapeHtml(project.projeKodu) + " · " + escapeHtml(project.projeHizmetAlani) + "</div>" +
      "</div>" +
      '<div class="project-card-badges">' +
      badgeMarkup(project.projeDurum, cfg.statusThemes) +
      badgeMarkup(project.projeKritiklik, cfg.criticalityThemes) +
      "</div>" +
      '<div class="project-card-detail">' + cfg.icon("user", 14) + "<span>" + uzmanText + "</span></div>" +
      '<div class="project-card-detail">' + cfg.icon("server", 14) + "<span>" + sunucuText + "</span></div>" +
      '<div class="project-card-footer">' + projectActionsMarkup(project) + "</div>" +
      "</div>"
    );
  }

  function renderMobileCards(pageItems) {
    var list = document.querySelector("[data-project-mobile-list]");
    if (!list) return;

    if (state.dataError || !pageItems.length) {
      list.innerHTML = "";
      return;
    }

    list.innerHTML = pageItems.map(projectCardMarkup).join("");
    bindRowActionEvents(list);
  }

  function renderAll() {
    updateSortIndicators();
    window.Filters.render(state);

    var totalItems = state.filteredProjects.length;
    var pageItems;

    if (state.pagination.pageSize === "all") {
      state.pagination.page = 1;
      pageItems = state.filteredProjects;
    } else {
      var totalPages = Math.max(1, Math.ceil(totalItems / state.pagination.pageSize));
      if (state.pagination.page > totalPages) state.pagination.page = totalPages;

      var start = (state.pagination.page - 1) * state.pagination.pageSize;
      pageItems = state.filteredProjects.slice(start, start + state.pagination.pageSize);
    }

    renderRows(pageItems);
    renderMobileCards(pageItems);
    window.Pagination.render(state, totalItems);
  }

  function bindSearchAndFilters() {
    var searchInputs = document.querySelectorAll("[data-project-search]");
    searchInputs.forEach(function (searchInput) {
      searchInput.addEventListener("input", function () {
        state.filters.search = searchInput.value;
        searchInputs.forEach(function (other) {
          if (other !== searchInput) other.value = searchInput.value;
        });
        applyFilters();
        renderAll();
      });
    });

    Object.keys(MULTI_FILTER_FIELDS).forEach(function (key) {
      var select = document.querySelector('[data-project-filter="' + key + '"]');
      if (!select) return;
      select.addEventListener("change", function () {
        if (syncingFilterControls) return;
        state.filters[key] = selectedValues(key, select);
        updateCollapsedSummary(key);
        applyFilters();
        renderAll();
      });
    });

    ["sadeceAktif", "sadeceWebAdresiOlan"].forEach(function (key) {
      var checkbox = document.querySelector('[data-project-filter="' + key + '"]');
      if (!checkbox) return;
      checkbox.addEventListener("change", function () {
        state.filters[key] = checkbox.checked;
        applyFilters();
        renderAll();
      });
    });
  }

  // Accepts repeated keys for the multi-select filters (?durum=Analiz&durum=Test) plus the
  // legacy single values the Home dashboard cards link with (e.g. ?durum=Yayinda).
  function parseQueryParams() {
    if (!window.location.search) return;
    var params = new URLSearchParams(window.location.search);

    var durumlar = [];
    params.getAll("durum").forEach(function (durum) {
      var d = normalize(durum).replace(/\s+/g, "");
      if (d === "yayinda" || d === "yayında") {
        durumlar.push("Yayında");
      } else if (d === "gelistirme" || d === "geliştirme") {
        durumlar.push("Geliştirme");
      } else if (d === "testinceleme" || d === "test_inceleme" || d === "test/inceleme" || d === "test/i̇nceleme") {
        durumlar.push("Test"); // old "Test/İnceleme" dashboard links
      } else if (durum) {
        durumlar.push(durum);
      }
    });
    state.filters.durum = durumlar;

    ["hizmetAlani", "birim", "kritiklik"].forEach(function (key) {
      state.filters[key] = params.getAll(key).filter(Boolean);
    });

    var search = params.get("search");
    if (search) {
      state.filters.search = search;
      document.querySelectorAll("[data-project-search]").forEach(function (input) {
        input.value = search;
      });
    }
  }

  function selectedValues(key, select) {
    if (filterChoices[key]) return filterChoices[key].getValue(true);
    return Array.prototype.map.call(select.selectedOptions, function (o) { return o.value; });
  }

  // Keeps a filter with many selections one line tall: only the first chip stays visible and the
  // rest are summarised as "+N" (every value is still listed/removable in the chip row below).
  function updateCollapsedSummary(key) {
    var instance = filterChoices[key];
    if (!instance) return;
    var outer = instance.containerOuter.element;
    var inner = instance.containerInner.element;
    var count = state.filters[key].length;
    outer.classList.toggle("choices--has-items", count > 0);
    outer.classList.toggle("choices--collapsed", count > 1);
    // Read by the CSS counter (.choices__inner::after { content: attr(data-more) }).
    if (count > 1) {
      inner.setAttribute("data-more", "+" + (count - 1));
    } else {
      inner.removeAttribute("data-more");
    }
  }

  // Options always come from the loaded projects (no hard-coded lists); Durum/Kritiklik keep their
  // enum order from AppConfig, the rest are alphabetical. Also re-applies state.filters to the
  // controls, which is how "Tümünü Temizle" and chip removal reset them.
  function populateFilterSelects() {
    var configs = [
      { key: "hizmetAlani", placeholder: "Tüm Hizmet Alanları" },
      { key: "birim", placeholder: "Tüm Birimler" },
      { key: "durum", placeholder: "Tüm Durumlar", fixedOrder: Object.keys(cfg.statusThemes) },
      { key: "kritiklik", placeholder: "Tüm Seviyeler", fixedOrder: Object.keys(cfg.criticalityThemes) }
    ];

    syncingFilterControls = true;
    try {
      configs.forEach(function (c) {
        var select = document.querySelector('[data-project-filter="' + c.key + '"]');
        if (!select) return;

        var field = MULTI_FILTER_FIELDS[c.key];
        var values = c.fixedOrder
          ? c.fixedOrder.filter(function (v) { return state.projects.some(function (p) { return p[field] === v; }); })
          : distinctValues(state.projects, field);
        var selected = state.filters[c.key];

        if (window.Choices && !filterChoices[c.key]) {
          filterChoices[c.key] = new window.Choices(select, {
            removeItemButton: true,
            shouldSort: false,
            allowHTML: false,
            placeholder: true,
            placeholderValue: c.placeholder,
            searchPlaceholderValue: "Ara...",
            itemSelectText: "",
            noResultsText: "Sonuç bulunamadı",
            noChoicesText: "Seçilecek başka değer yok",
            position: "bottom"
          });
        }

        var instance = filterChoices[c.key];
        if (instance) {
          instance.removeActiveItems();
          instance.setChoices(values.map(function (v) {
            return { value: v, label: v, selected: selected.indexOf(v) !== -1 };
          }), "value", "label", true);
          updateCollapsedSummary(c.key);
        } else {
          select.innerHTML = values.map(function (v) {
            return '<option value="' + escapeHtml(v) + '"' + (selected.indexOf(v) !== -1 ? " selected" : "") + ">" + escapeHtml(v) + "</option>";
          }).join("");
        }
      });
    } finally {
      syncingFilterControls = false;
    }
  }

  function bindRetry() {
    var retryBtn = document.querySelector("[data-retry-load]");
    if (retryBtn) retryBtn.addEventListener("click", function () { window.location.reload(); });
  }

  // Builds the query string an export endpoint needs to reproduce the currently applied
  // filters server-side (see ApplyFilter in ProjeController).
  function buildExportQuery() {
    var f = state.filters;
    var params = new URLSearchParams();
    if (f.search) params.set("search", f.search);
    Object.keys(MULTI_FILTER_FIELDS).forEach(function (key) {
      f[key].forEach(function (value) { params.append(key, value); });
    });
    if (f.sadeceAktif) params.set("sadeceAktif", "true");
    if (f.sadeceWebAdresiOlan) params.set("sadeceWebAdresiOlan", "true");
    return params.toString();
  }

  function bindExportButtons() {
    var excelBtn = document.querySelector("[data-export-excel]");
    var pdfBtn = document.querySelector("[data-export-pdf]");

    if (excelBtn) {
      excelBtn.addEventListener("click", function () {
        window.location.href = "/Proje/ExportToExcel?" + buildExportQuery();
      });
    }

    if (pdfBtn) {
      pdfBtn.addEventListener("click", function () {
        window.location.href = "/Proje/ExportToPdf?" + buildExportQuery();
      });
    }
  }

  // Below desktop width the whole filter-toolbar (search + selects, including the advanced
  // panel above) collapses behind a single "Filtreler" trigger. Purely a visibility toggle -
  // the filter controls themselves, their state and their handlers are untouched.
  function bindFilterToolbarToggle() {
    var toggle = document.querySelector("[data-filter-toolbar-toggle]");
    var toolbar = document.querySelector("[data-filter-toolbar]");
    if (!toggle || !toolbar) return;

    toggle.addEventListener("click", function () {
      var isOpen = toolbar.classList.toggle("open");
      toggle.setAttribute("aria-expanded", String(isOpen));
    });
  }

  function init(initialProjects, dataError) {
    state.projects = Array.isArray(initialProjects) ? initialProjects : [];
    state.dataError = Boolean(dataError);
    state.filteredProjects = state.projects.slice();

    parseQueryParams();
    renderTableHead();
    populateFilterSelects();
    bindSearchAndFilters();
    bindFilterToolbarToggle();
    bindRetry();
    bindExportButtons();
    window.Pagination.bindEvents(state, renderAll);
    window.Filters.bindChipRemoval(state, renderAll, populateFilterSelects);

    applyFilters();
    renderAll();
  }

  window.ProjectsApp = {
    state: state,
    init: init,
    renderAll: renderAll,
    applyFilters: applyFilters,
    multiFilterKeys: Object.keys(MULTI_FILTER_FIELDS)
  };
})(window);
