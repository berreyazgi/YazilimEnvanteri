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
      if (f.durum && p.projeDurum !== f.durum) return false;
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

  function metrics() {
    var list = state.projects;
    return {
      toplam: list.length,
      yayinda: list.filter(function (p) { return p.projeDurum === "Yayında"; }).length,
      gelistirmede: list.filter(function (p) { return p.projeDurum === "Geliştirme"; }).length,
      testInceleme: list.filter(function (p) { return p.projeDurum === "Test" || p.projeDurum === "İnceleme"; }).length
    };
  }

  function renderMetrics() {
    var m = metrics();
    setText("[data-metric='toplam']", m.toplam);
    setText("[data-metric='yayinda']", m.yayinda);
    setText("[data-metric='gelistirmede']", m.gelistirmede);
    setText("[data-metric='testInceleme']", m.testInceleme);
  }

  function setText(selector, value) {
    var el = document.querySelector(selector);
    if (el) el.textContent = value;
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
      return '<div class="cell-title">' + escapeHtml(project.projeAdi) + "</div>";
    }

    var value = project[column.key];
    if (!value) return '<span class="cell-muted">Bilgi bulunmuyor</span>';
    return escapeHtml(value);
  }

  function renderTableHead() {
    var head = document.querySelector("[data-project-table-head]");
    if (!head) return;

    var cells = cfg.projectColumns.map(function (col) {
      var sortAttr = col.sortable ? ' class="sortable" data-sort-key="' + col.key + '"' : "";
      var indicator = col.sortable ? '<span class="sort-indicator" data-sort-indicator="' + col.key + '">' + cfg.icon("sortNeutral", 12) + "</span>" : "";
      return "<th" + sortAttr + ">" + escapeHtml(col.label) + indicator + "</th>";
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
        return "<td>" + cellMarkup(project, col) + "</td>";
      }).join("");

      var websiteAction = project.websiteUrl
        ? '<a href="' + escapeHtml(project.websiteUrl) + '" target="_blank" rel="noopener">' + cfg.icon("externalLink", 15) + " Web Sitesini Aç</a>"
        : "";

      return (
        "<tr>" + cells +
        '<td><div class="row-actions">' +
        '<a class="app-button app-button--outline app-button--sm" href="/Home/ProjeDetay/' + project.id + '">Detaylar</a>' +
        '<button type="button" class="row-menu-trigger" data-row-menu-trigger="' + project.id + '" aria-haspopup="true" aria-expanded="false" aria-label="Diğer işlemler">' + cfg.icon("moreHorizontal", 16) + "</button>" +
        '<div class="row-menu-panel" id="row-menu-' + project.id + '">' +
        '<a href="/Home/ProjeDetay/' + project.id + '">' + cfg.icon("externalLink", 15) + " Detayı Görüntüle</a>" +
        websiteAction +
        '<button type="button" data-copy-info="' + project.id + '">' + cfg.icon("copy", 15) + " Bilgileri Kopyala</button>" +
        "</div></div></td></tr>"
      );
    }).join("");

    body.querySelectorAll("[data-row-menu-trigger]").forEach(function (trigger) {
      var panel = document.getElementById("row-menu-" + trigger.getAttribute("data-row-menu-trigger"));
      if (panel) window.Dropdown.bind(trigger, panel);
    });

    body.querySelectorAll("[data-copy-info]").forEach(function (btn) {
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

  function renderAll() {
    renderMetrics();
    updateSortIndicators();
    window.Filters.renderChips(state, cfg);

    var totalItems = state.filteredProjects.length;
    var totalPages = Math.max(1, Math.ceil(totalItems / state.pagination.pageSize));
    if (state.pagination.page > totalPages) state.pagination.page = totalPages;

    var start = (state.pagination.page - 1) * state.pagination.pageSize;
    var pageItems = state.filteredProjects.slice(start, start + state.pagination.pageSize);

    renderRows(pageItems);
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

      var options = ['<option value="">' + c.placeholder + "</option>"]
        .concat(values.map(function (v) { return '<option value="' + escapeHtml(v) + '">' + escapeHtml(v) + "</option>"; }));

      select.innerHTML = options.join("");
    });
  }

  function bindAdvancedFilterToggle() {
    var toggle = document.querySelector("[data-advanced-filter-toggle]");
    var panel = document.querySelector("[data-advanced-filter-panel]");
    if (!toggle || !panel) return;

    toggle.addEventListener("click", function () {
      var isOpen = panel.style.display !== "none";
      panel.style.display = isOpen ? "none" : "grid";
      toggle.setAttribute("aria-expanded", String(!isOpen));
    });
  }

  function bindRetry() {
    var retryBtn = document.querySelector("[data-retry-load]");
    if (retryBtn) retryBtn.addEventListener("click", function () { window.location.reload(); });
  }

  function init(initialProjects, dataError) {
    state.projects = Array.isArray(initialProjects) ? initialProjects : [];
    state.dataError = Boolean(dataError);
    state.filteredProjects = state.projects.slice();

    renderTableHead();
    populateFilterSelects();
    bindSearchAndFilters();
    bindAdvancedFilterToggle();
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
