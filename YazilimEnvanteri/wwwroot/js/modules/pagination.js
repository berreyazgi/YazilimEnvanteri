(function (window) {
  "use strict";

  var cfg = window.AppConfig;

  function render(state, totalItems) {
    var summary = document.querySelector("[data-pagination-summary]");
    var controls = document.querySelector("[data-pagination-controls]");
    var bar = document.querySelector("[data-pagination-bar]");
    if (!controls || !summary) return;

    if (!totalItems) {
      summary.textContent = "0 proje gösteriliyor";
      controls.innerHTML = "";
      if (bar) bar.style.display = "none";
      return;
    }

    if (bar) bar.style.display = "flex";

    var showingAll = state.pagination.pageSize === "all";
    var page = showingAll ? 1 : state.pagination.page;
    var start = showingAll ? 1 : (page - 1) * state.pagination.pageSize + 1;
    var end = showingAll ? totalItems : Math.min(totalItems, page * state.pagination.pageSize);

    summary.textContent = start + "–" + end + " / " + totalItems + " proje gösteriliyor";

    if (showingAll) {
      controls.innerHTML = "";
      return;
    }

    var pageSize = state.pagination.pageSize;
    var totalPages = Math.max(1, Math.ceil(totalItems / pageSize));

    var buttons = [];
    buttons.push(pageButton("‹", page - 1, page === 1, "Önceki sayfa"));

    var pageNumbers = visiblePageNumbers(page, totalPages);
    pageNumbers.forEach(function (entry) {
      if (entry === "...") {
        buttons.push('<span class="pagination-ellipsis">…</span>');
      } else {
        buttons.push(pageButton(String(entry), entry, false, "Sayfa " + entry, entry === page));
      }
    });

    buttons.push(pageButton("›", page + 1, page === totalPages, "Sonraki sayfa"));
    controls.innerHTML = buttons.join("");
  }

  function pageButton(label, targetPage, disabled, ariaLabel, isActive) {
    return (
      '<button type="button" data-goto-page="' + targetPage + '"' +
      (disabled ? " disabled" : "") +
      (isActive ? ' class="active" aria-current="page"' : "") +
      ' aria-label="' + ariaLabel + '">' + label + "</button>"
    );
  }

  function visiblePageNumbers(current, total) {
    var pages = [];
    for (var i = 1; i <= total; i++) {
      if (i === 1 || i === total || Math.abs(i - current) <= 1) {
        pages.push(i);
      } else if (pages[pages.length - 1] !== "...") {
        pages.push("...");
      }
    }
    return pages;
  }

  function bindEvents(state, onChange) {
    var sizeSelect = document.querySelector("[data-page-size]");
    if (sizeSelect) {
      sizeSelect.innerHTML = cfg.pageSizeOptions.map(function (size) {
        var label = (cfg.pageSizeLabels && cfg.pageSizeLabels[size]) || size;
        return '<option value="' + size + '"' + (size === state.pagination.pageSize ? " selected" : "") + ">" + label + "</option>";
      }).join("");

      sizeSelect.addEventListener("change", function () {
        state.pagination.pageSize = sizeSelect.value === "all" ? "all" : Number(sizeSelect.value);
        state.pagination.page = 1;
        onChange();
      });
    }

    var controls = document.querySelector("[data-pagination-controls]");
    if (controls) {
      controls.addEventListener("click", function (e) {
        var btn = e.target.closest("[data-goto-page]");
        if (!btn || btn.disabled) return;
        state.pagination.page = Number(btn.getAttribute("data-goto-page"));
        onChange();
      });
    }
  }

  window.Pagination = { render: render, bindEvents: bindEvents };
})(window);
