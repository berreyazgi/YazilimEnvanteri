(function (window, document) {
  "use strict";

  // Notion-style editing straight in the Projelerim table: click (or Enter on a focused cell),
  // change the value, Enter / blur saves, Escape cancels. One request per edit (POST
  // /Proje/UpdateField); on success the project object in ProjectsApp.state is updated and the
  // table re-rendered locally, so filters, sort and the current page stay as they are.
  // The server (ProjeController.UpdateField) enforces role + allowlist + validation; the
  // permission check here only decides whether cells look editable.

  var cfg = window.AppConfig;

  function themeOptions(themeMap) {
    return Promise.resolve(Object.keys(themeMap).map(function (key) {
      return { value: String(themeMap[key].value), label: themeMap[key].label };
    }));
  }

  // Lookup lists are fetched once per page, on the first edit that needs them.
  var lookupCache = {};
  function lookupOptions(url, toOption) {
    if (!lookupCache[url]) {
      lookupCache[url] = fetch(url).then(function (res) {
        if (!res.ok) throw new Error();
        return res.json();
      }).then(function (items) { return items.map(toOption); });
      lookupCache[url].catch(function () { delete lookupCache[url]; });
    }
    return lookupCache[url];
  }

  // Table column key -> how that cell is edited. Columns not listed here are read-only.
  var EDITORS = {
    projeKodu: { field: "projeKodu", type: "text", maxLength: 50, required: true, value: function (p) { return p.projeKodu; } },
    projeAdi: { field: "projeAdi", type: "text", maxLength: 200, required: true, value: function (p) { return p.projeAdi; } },
    projeHizmetAlani: { field: "projeHizmetAlani", type: "text", maxLength: 100, required: true, value: function (p) { return p.projeHizmetAlani; } },
    sunucu: { field: "sunucu", type: "text", maxLength: 100, value: function (p) { return p.sunucu; } },
    birim: {
      field: "birimId", type: "select", value: function (p) { return p.birimId; },
      options: function () {
        return lookupOptions("/Birim", function (b) { return { value: String(b.id), label: b.birim }; });
      }
    },
    yazilimUzmaniAdSoyad: {
      field: "yazilimUzmaniId", type: "select", value: function (p) { return p.yazilimUzmaniId; },
      options: function () {
        return lookupOptions("/YazilimUzmani", function (u) {
          return { value: String(u.id), label: u.adSoyad + (u.kullaniciAdi ? " (" + u.kullaniciAdi + ")" : "") };
        });
      }
    },
    projeDurum: {
      field: "projeDurum", type: "select", options: function () { return themeOptions(cfg.statusThemes); },
      value: function (p) { var t = cfg.statusThemes[p.projeDurum]; return t ? t.value : ""; }
    },
    projeKritiklik: {
      field: "projeKritiklik", type: "select", options: function () { return themeOptions(cfg.criticalityThemes); },
      value: function (p) { var t = cfg.criticalityThemes[p.projeKritiklik]; return t ? t.value : ""; }
    }
  };

  var active = null; // { cell, editor, spec, project, original, saving, failedValue }

  function enabled() { return Boolean(cfg.permissions && cfg.permissions.canManageProjects); }
  function isEditable(columnKey) { return enabled() && Object.prototype.hasOwnProperty.call(EDITORS, columnKey); }

  function findProject(id) {
    return window.ProjectsApp.state.projects.filter(function (p) { return p.id === id; })[0];
  }

  function showError(message) {
    var box = active.cell.querySelector(".cell-error");
    if (!box) {
      box = document.createElement("div");
      box.className = "cell-error";
      box.setAttribute("role", "alert");
      active.cell.appendChild(box);
    }
    box.textContent = message;
    active.cell.classList.add("has-error");
  }

  function enterEditMode(cell) {
    if (active && active.cell === cell) return;
    if (active) {
      if (active.saving) return;
      cancelInlineEdit();
    }

    var row = cell.closest("tr[data-project-id]");
    var project = row && findProject(Number(row.getAttribute("data-project-id")));
    var spec = EDITORS[cell.getAttribute("data-col")];
    if (!project || !spec) return;

    var original = spec.value(project);
    var editor;

    if (spec.type === "select") {
      editor = document.createElement("select");
      editor.innerHTML = '<option value="">Yükleniyor…</option>';
      editor.disabled = true;
      spec.options().then(function (options) {
        if (!active || active.editor !== editor) return;
        editor.innerHTML = "";
        options.forEach(function (o) { editor.add(new Option(o.label, o.value)); });
        editor.value = original == null ? "" : String(original);
        editor.disabled = false;
        editor.focus();
      }).catch(function () {
        if (active && active.editor === editor) showError("Seçenekler yüklenemedi.");
      });
      editor.addEventListener("change", function () { saveInlineEdit(); });
    } else {
      editor = document.createElement("input");
      editor.type = "text";
      editor.maxLength = spec.maxLength;
      editor.value = original == null ? "" : original;
      editor.spellcheck = false;
    }

    editor.className = "cell-editor";
    editor.setAttribute("aria-label", cell.getAttribute("data-label") + " düzenle");

    active = { cell: cell, editor: editor, spec: spec, project: project, original: original == null ? "" : String(original), originalHtml: cell.innerHTML };
    cell.classList.add("is-editing");
    cell.innerHTML = "";
    cell.appendChild(editor);
    if (!editor.disabled) {
      editor.focus();
      if (editor.select) editor.select();
    }

    editor.addEventListener("keydown", function (e) {
      if (!active || active.editor !== editor) return;
      if (e.key === "Escape") {
        e.preventDefault();
        var cellToRefocus = active.cell;
        cancelInlineEdit();
        cellToRefocus.focus();
      } else if (e.key === "Enter" && spec.type === "text") {
        e.preventDefault();
        saveInlineEdit();
      }
    });

    // Leaving the editor saves a real change (the Notion behaviour) - but never re-sends a value the
    // server just rejected; that stays on screen with its message until fixed or cancelled.
    editor.addEventListener("blur", function () {
      window.setTimeout(function () {
        if (!active || active.editor !== editor || active.saving) return;
        if (editor.value === active.original) {
          cancelInlineEdit();
        } else if (editor.value !== active.failedValue) {
          saveInlineEdit();
        }
      }, 0);
    });
  }

  function cancelInlineEdit() {
    if (!active) return;
    active.cell.classList.remove("is-editing", "is-saving", "has-error");
    active.cell.innerHTML = active.originalHtml;
    active = null;
  }

  function saveInlineEdit() {
    if (!active || active.saving) return;
    var edit = active;
    var value = edit.editor.value.trim();

    if (value === edit.original) {
      cancelInlineEdit();
      return;
    }
    if (edit.spec.required && !value) {
      edit.failedValue = edit.editor.value;
      showError("Bu alan boş bırakılamaz.");
      edit.editor.focus();
      return;
    }

    edit.saving = true;
    edit.editor.readOnly = true;
    edit.cell.classList.add("is-saving");
    edit.cell.classList.remove("has-error");

    fetch("/Proje/UpdateField", {
      method: "POST",
      headers: Object.assign({ "Content-Type": "application/json" }, cfg.antiforgeryHeaders()),
      body: JSON.stringify({ id: edit.project.id, field: edit.spec.field, value: value })
    }).then(function (res) {
      return res.json().catch(function () { return null; }).then(function (body) {
        if (res.ok && body && body.success) return body;
        if (res.status === 401 || res.status === 403) throw new Error("Bu işlem için yetkiniz yok.");
        throw new Error((body && body.message) || "Değişiklik kaydedilemedi.");
      });
    }).then(function (body) {
      Object.assign(edit.project, body.project);
      active = null;
      window.ProjectsApp.refreshFilterOptions();
      window.ProjectsApp.renderAll();
      flashSaved(edit.project.id, edit.cell.getAttribute("data-col"));
    }).catch(function (err) {
      if (active !== edit) return;
      edit.saving = false;
      edit.failedValue = edit.editor.value;
      edit.editor.readOnly = false;
      edit.cell.classList.remove("is-saving");
      showError(err.message);
      edit.editor.focus();
    });
  }

  function flashSaved(projectId, columnKey) {
    var cell = document.querySelector('[data-project-table-body] tr[data-project-id="' + projectId + '"] td[data-col="' + columnKey + '"]');
    if (!cell) return;
    cell.classList.add("is-saved");
    window.setTimeout(function () { cell.classList.remove("is-saved"); }, 1200);
  }

  function initializeProjectInlineEditing() {
    var body = document.querySelector("[data-project-table-body]");
    if (!body || !enabled()) return;

    body.addEventListener("click", function (e) {
      // Links, the "..." menu, buttons and the editor itself keep their own behaviour.
      if (e.target.closest("a, button, input, select, textarea, .row-menu-panel")) return;
      var cell = e.target.closest("td.cell-editable");
      if (cell) enterEditMode(cell);
    });

    body.addEventListener("keydown", function (e) {
      var cell = e.target;
      if (!cell.classList || !cell.classList.contains("cell-editable")) return;
      if (e.key === "Enter" || e.key === "F2") {
        e.preventDefault();
        enterEditMode(cell);
      }
    });
  }

  window.ProjectInlineEdit = { init: initializeProjectInlineEditing, isEditable: isEditable };
})(window, document);
