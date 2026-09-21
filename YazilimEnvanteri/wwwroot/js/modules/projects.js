(function (window) {
  "use strict";

  var cfg = window.AppConfig;

  var state = {
    projects: [],
    filteredProjects: [],
    dataError: false,

    filters: {
      search: "",
      hizmetAlani: "",
      birim: "",
      durum: "",
      kritiklik: "",
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
      if (f.hizmetAlani && p.projeHizmetAlani !== f.hizmetAlani) return false;
      if (f.birim && p.birim !== f.birim) return false;

      if (f.durum) {
        var dNorm = normalize(f.durum);
        if (dNorm === "testinceleme" || dNorm === "test_inceleme" || dNorm === "test / inceleme" || dNorm === "test / i̇nceleme") {
          if (p.projeDurum !== "Test" && p.projeDurum !== "İnceleme") return false;
        } else if (normalize(p.projeDurum) !== dNorm && p.projeDurum !== f.durum) {
          return false;
        }
      }

      if (f.kritiklik && p.projeKritiklik !== f.kritiklik) return false;
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

    state.filteredProjects.sort(function (a, b) {
      var av = a[field];
      var bv = b[field];
      if (typeof av === "number" && typeof bv === "number") {
        return (av - bv) * direction;
      }
      return normalize(av).localeCompare(normalize(bv), "tr-TR") * direction;
    });
  }

  function escapeHtml(value) {
    return String(value == null ? "" : value).replace(/[&<>"']/g, function (ch) {
      return { "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[ch];
    });
  }

  function badgeMarkup(value, themeMap) {
    if (!value) return '<span class="status-badge status-badge--neutral">Bilgi yok</span>';
    var theme = themeMap[value] || { label: value, tone: "neutral" };
    return '<span class="status-badge status-badge--' + theme.tone + '">' + escapeHtml(theme.label) + "</span>";
  }

  function cellMarkup(project, column) {
    if (column.type === "status") return badgeMarkup(project.projeDurum, cfg.statusThemes);
    if (column.type === "criticality") return badgeMarkup(project.projeKritiklik, cfg.criticalityThemes);

    if (column.key === "sunucu") {
      var parts = [];
      if (project.sunucu) parts.push('<div class="cell-title">' + escapeHtml(project.sunucu) + "</div>");
      if (project.websiteUrl) {
        parts.push('<div class="cell-secondary">' + escapeHtml(project.websiteUrl) + "</div>");
      }
      return parts.length ? parts.join("") : '<span class="cell-muted">Bilgi bulunmuyor</span>';
    }

    if (column.key === "projeAdi") {
      // The extra line is only ever shown by CSS at tablet widths, where the Proje Kodu /
      // Hizmet Alanı columns are hidden and folded into this cell instead - no separate
      // tablet render path, just a breakpoint-controlled reveal of markup that's always there.
      return (
        '<div class="cell-title">' + escapeHtml(project.projeAdi) + "</div>" +
        '<div class="cell-meta-compact">Kod: ' + escapeHtml(project.projeKodu) + " · " + escapeHtml(project.projeHizmetAlani) + "</div>"
      );
    }

    var value = project[column.key];
    if (!value) return '<span class="cell-muted">Bilgi bulunmuyor</span>';
    return escapeHtml(value);
  }

  // Shared between desktop table rows and mobile cards so there is exactly one place that
  // knows what actions a project row exposes (Detaylar / Web Sitesini Aç / Bilgileri Kopyala).
  function projectActionsMarkup(project) {
    var websiteAction = project.websiteUrl
      ? '<a href="' + escapeHtml(project.websiteUrl) + '" target="_blank" rel="noopener">' + cfg.icon("externalLink", 15) + " Web Sitesini Aç</a>"
      : "";

    return (
      '<div class="row-actions">' +
      '<a class="app-button app-button--outline app-button--sm" href="/Proje/Details/' + project.id + '">Detaylar</a>' +
      '<button type="button" class="row-menu-trigger" data-row-menu-trigger="' + project.id + '" aria-haspopup="true" aria-expanded="false" aria-label="Diğer işlemler">' + cfg.icon("moreHorizontal", 16) + "</button>" +
      '<div class="row-menu-panel" id="row-menu-' + project.id + '">' +
      '<a href="/Proje/Details/' + project.id + '">' + cfg.icon("externalLink", 15) + " Detayı Görüntüle</a>" +
      websiteAction +
      '<button type="button" data-copy-info="' + project.id + '">' + cfg.icon("copy", 15) + " Bilgileri Kopyala</button>" +
      "</div></div>"
    );
  }

  // Binds the row-menu-trigger/copy-info handlers inside whichever container was just
  // re-rendered - the table body or the mobile card list both use this.
  function bindRowActionEvents(container) {
    container.querySelectorAll("[data-row-menu-trigger]").forEach(function (trigger) {
      var panel = document.getElementById("row-menu-" + trigger.getAttribute("data-row-menu-trigger"));
      if (panel) window.Dropdown.bind(trigger, panel);
    });

    container.querySelectorAll("[data-copy-info]").forEach(function (btn) {
      btn.addEventListener("click", function () {
        var id = Number(btn.getAttribute("data-copy-info"));
        var project = state.projects.find(function (p) { return p.id === id; });
        if (!project || !navigator.clipboard) return;
        var text = [project.projeAdi, project.projeKodu, project.projeHizmetAlani, project.sunucu, project.websiteUrl]
          .filter(Boolean).join(" • ");
        navigator.clipboard.writeText(text);
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
    cells.push("<th>İşlemler</th>");
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

      return "<tr>" + cells + "<td>" + projectActionsMarkup(project) + "</td></tr>";
    }).join("");

    bindRowActionEvents(body);
  }

  // Card representation of the exact same pageItems used by renderRows - only ever shown by
  // CSS at mobile widths, never chosen/detected here in JS.
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
    window.Filters.renderChips(state, cfg);

    var totalItems = state.filteredProjects.length;
    var totalPages = Math.max(1, Math.ceil(totalItems / state.pagination.pageSize));
    if (state.pagination.page > totalPages) state.pagination.page = totalPages;

    var start = (state.pagination.page - 1) * state.pagination.pageSize;
    var pageItems = state.filteredProjects.slice(start, start + state.pagination.pageSize);

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

    ["hizmetAlani", "birim", "durum", "kritiklik"].forEach(function (key) {
      var select = document.querySelector('[data-project-filter="' + key + '"]');
      if (!select) return;
      select.addEventListener("change", function () {
        state.filters[key] = select.value;
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

  function parseQueryParams() {
    if (!window.location.search) return;
    var params = new URLSearchParams(window.location.search);

    var durum = params.get("durum");
    if (durum) {
      var d = normalize(durum);
      if (d === "yayinda" || d === "yayında") {
        state.filters.durum = "Yayında";
      } else if (d === "gelistirme" || d === "geliştirme") {
        state.filters.durum = "Geliştirme";
      } else if (d === "testinceleme" || d === "test_inceleme" || d === "test / inceleme" || d === "test / i̇nceleme") {
        state.filters.durum = "Test / İnceleme";
      } else {
        state.filters.durum = durum;
      }
    }

    var hizmet = params.get("hizmetAlani");
    if (hizmet) state.filters.hizmetAlani = hizmet;

    var birim = params.get("birim");
    if (birim) state.filters.birim = birim;

    var kritiklik = params.get("kritiklik");
    if (kritiklik) state.filters.kritiklik = kritiklik;

    var search = params.get("search");
    if (search) {
      state.filters.search = search;
      document.querySelectorAll("[data-project-search]").forEach(function (input) {
        input.value = search;
      });
    }
  }

  function populateFilterSelects() {
    var configs = [
      { key: "hizmetAlani", field: "projeHizmetAlani", placeholder: "Tüm Hizmet Alanları" },
      { key: "birim", field: "birim", placeholder: "Tüm Birimler" },
      { key: "durum", field: "projeDurum", placeholder: "Tüm Durumlar", fixedOrder: Object.keys(cfg.statusThemes) },
      { key: "kritiklik", field: "projeKritiklik", placeholder: "Tüm Kritiklik Seviyeleri", fixedOrder: Object.keys(cfg.criticalityThemes) }
    ];

    configs.forEach(function (c) {
      var select = document.querySelector('[data-project-filter="' + c.key + '"]');
      if (!select) return;

      var values = c.fixedOrder
        ? c.fixedOrder.filter(function (v) { return state.projects.some(function (p) { return p[c.field] === v; }); })
        : distinctValues(state.projects, c.field);

      if (c.key === "durum") {
        if (state.projects.some(function (p) { return p.projeDurum === "Test" || p.projeDurum === "İnceleme"; })) {
          if (values.indexOf("Test / İnceleme") === -1) {
            values.push("Test / İnceleme");
          }
        }
      }

      var options = ['<option value="">' + c.placeholder + "</option>"]
        .concat(values.map(function (v) { return '<option value="' + escapeHtml(v) + '">' + escapeHtml(v) + "</option>"; }));

      select.innerHTML = options.join("");
      if (state.filters[c.key]) {
        select.value = state.filters[c.key];
      }
    });
  }

  function bindAdvancedFilterToggle() {
    var toggle = document.querySelector("[data-advanced-filter-toggle]");
    var panel = document.querySelector("[data-advanced-filter-panel]");
    if (!toggle || !panel) return;

    toggle.addEventListener("click", function () {
      var isOpen = panel.classList.toggle("open");
      toggle.setAttribute("aria-expanded", String(isOpen));
    });
  }

  function bindRetry() {
    var retryBtn = document.querySelector("[data-retry-load]");
    if (retryBtn) retryBtn.addEventListener("click", function () { window.location.reload(); });
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
    bindAdvancedFilterToggle();
    bindFilterToolbarToggle();
    bindRetry();
    window.Pagination.bindEvents(state, renderAll);
    window.Filters.bindClearAll(state, renderAll, populateFilterSelects);

    applyFilters();
    renderAll();
  }

  window.ProjectsApp = {
    state: state,
    init: init,
    renderAll: renderAll,
    applyFilters: applyFilters
  };
})(window);
