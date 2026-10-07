(function (window) {
  "use strict";

  var cfg = window.AppConfig;

  var escapeHtml = window.AppConfig.escapeHtml;

  function openModal(overlay, form, errorBox, resetStepFn) {
    form.reset();
    form.querySelectorAll("select").forEach(function (s) { setSelectValue(s, s.value); });
    errorBox.classList.remove("open");
    errorBox.textContent = "";
    overlay.classList.add("open");
    document.addEventListener("keydown", onEscape);
    if (typeof resetStepFn === "function") {
      resetStepFn();
    }
  }

  function closeModal(overlay) {
    overlay.classList.remove("open");
    document.removeEventListener("keydown", onEscape);
  }

  function onEscape(e) {
    if (e.key === "Escape") {
      document.querySelectorAll(".modal-overlay.open").forEach(closeModal);
    }
  }

  // Empty first option: nothing is preselected, and the selects' "required" then blocks submit.
  var PLACEHOLDER_OPTION = '<option value="">Seçiniz</option>';

  // Same Choices.js widget as the filter toolbar (searchable, touch-friendly list instead of the
  // native picker). Call after the <select> has its options; without the library the native
  // select keeps working.
  function enhanceSelect(select) {
    if (!window.Choices || select._choices) return;
    select._choices = new window.Choices(select, {
      shouldSort: false,
      allowHTML: false,
      searchPlaceholderValue: "Ara...",
      itemSelectText: "",
      noResultsText: "Sonuç bulunamadı",
      position: "bottom"
    });
  }

  function setSelectValue(select, value) {
    if (select._choices) {
      select._choices.setChoiceByValue(String(value == null ? "" : value));
    } else {
      select.value = value;
    }
  }

  // A Choices-wrapped <select> is hidden, so the browser can't show its validation bubble -
  // say which field is missing in the form's error box and open its list instead.
  function reportInvalid(el, errorBox) {
    if (!el._choices) {
      el.reportValidity();
      return;
    }
    var label = el.id && document.querySelector('label[for="' + el.id + '"]');
    errorBox.textContent = (label ? label.textContent.trim() : "Bu alan") + " seçilmelidir.";
    errorBox.classList.add("open");
    el._choices.showDropdown();
  }

  function populateEnumSelect(select, themeMap) {
    var options = Object.keys(themeMap).map(function (key) {
      var theme = themeMap[key];
      return '<option value="' + theme.value + '">' + escapeHtml(theme.label) + "</option>";
    });
    select.innerHTML = PLACEHOLDER_OPTION + options.join("");
    enhanceSelect(select);
  }

  function loadLookup(url) {
    return fetch(url).then(function (res) {
      if (!res.ok) throw new Error("Lookup verisi alınamadı: " + url);
      return res.json();
    });
  }

  function populateLookupSelects(birimSelect, uzmanSelect, teknolojiSelect) {
    return Promise.all([
      loadLookup("/Birim").then(function (birimler) {
        birimSelect.innerHTML = PLACEHOLDER_OPTION + birimler.map(function (b) {
          return '<option value="' + b.id + '">' + escapeHtml(b.birim) + "</option>";
        }).join("");
        enhanceSelect(birimSelect);
      }),
      // Name/username come from the linked Personel record (joined server-side).
      loadLookup("/YazilimUzmani").then(function (uzmanlar) {
        uzmanSelect.innerHTML = PLACEHOLDER_OPTION + uzmanlar.map(function (u) {
          var label = u.adSoyad + (u.kullaniciAdi ? " (" + u.kullaniciAdi + ")" : "");
          return '<option value="' + u.id + '">' + escapeHtml(label) + "</option>";
        }).join("");
        enhanceSelect(uzmanSelect);
      }),
      loadLookup("/Teknoloji").then(function (teknolojiler) {
        teknolojiSelect.innerHTML = PLACEHOLDER_OPTION + teknolojiler.map(function (t) {
          var label = t.backendTeknoloji + " / " + t.frontendTeknoloji + " / " + t.veritabani;
          return '<option value="' + t.id + '">' + escapeHtml(label) + "</option>";
        }).join("");
        enhanceSelect(teknolojiSelect);
      })
    ]);
  }

  // Mirrors Models/Validation/ProjeKoduKurali.cs (the server re-checks and is authoritative):
  // trimmed, digits only.
  var PROJE_KODU_DESEN = /^[0-9]+$/;
  var PROJE_KODU_MESAJ = "Proje kodu yalnızca rakamlardan oluşmalıdır (ör. 100, 9999).";

  function normalizeProjeKodu(value) {
    return String(value == null ? "" : value).trim().toLocaleUpperCase("tr-TR");
  }

  function validateProjeKoduInput(input) {
    if (!input) return;
    var value = normalizeProjeKodu(input.value);
    input.setCustomValidity(value && !PROJE_KODU_DESEN.test(value) ? PROJE_KODU_MESAJ : "");
  }

  function bindProjeKoduInput(form) {
    var input = form.querySelector("[data-proje-kodu-input]");
    if (!input) return;
    input.addEventListener("input", function () { validateProjeKoduInput(input); });
    input.addEventListener("blur", function () {
      input.value = normalizeProjeKodu(input.value);
      validateProjeKoduInput(input);
    });
  }

  // BadRequest(ModelState) from ProjeController returns { Field: [messages] } (or, for problem
  // details, { title, errors: { Field: [...] } }). Show the field messages - e.g. "Bu proje kodu
  // başka bir projede kullanılıyor." - instead of a generic failure.
  function validationErrorsFrom(body) {
    if (!body || typeof body !== "object") return {};
    return body.errors && typeof body.errors === "object" ? body.errors : body;
  }

  function errorMessageFrom(body) {
    var errors = validationErrorsFrom(body);
    var messages = [];
    Object.keys(errors).forEach(function (key) {
      if (Array.isArray(errors[key])) messages = messages.concat(errors[key]);
    });
    if (messages.length) return messages.join(" ");
    return body && body.title ? body.title : "Proje kaydedilemedi.";
  }

  function hasProjeKoduError(body) {
    return Object.keys(validationErrorsFrom(body)).some(function (k) {
      return k.toLowerCase() === "projekodu";
    });
  }

  function buildPayload(form) {
    var data = new FormData(form);
    return {
      id: Number(data.get("id")) || 0,
      projeKodu: normalizeProjeKodu(data.get("projeKodu")),
      projeAdi: data.get("projeAdi") || "",
      projeHizmetAlani: data.get("projeHizmetAlani") || "",
      projeAciklamasi: data.get("projeAciklamasi") || "",
      projeAktifMi: data.get("projeAktifMi") === "true",
      sunucu: data.get("sunucu") || "",
      websiteUrl: data.get("websiteUrl") || "",
      projeDurum: Number(data.get("projeDurum")),
      projeKritiklik: Number(data.get("projeKritiklik")),
      birimId: Number(data.get("birimId")),
      yazilimUzmaniId: Number(data.get("yazilimUzmaniId")),
      teknolojiId: Number(data.get("teknolojiId"))
    };
  }

  // Fills the (shared) create/edit form with an existing project's raw entity values -
  // see ProjeController.GetForEdit, which returns ProjeEntity (not the denormalized list view
  // model) specifically so BirimId/YazilimUzmaniId/TeknolojiId are available to preselect here.
  function populateFormValues(form, proje) {
    form.querySelector('[data-proje-form-id]').value = proje.id;
    form.querySelector('[name="projeKodu"]').value = proje.projeKodu || "";
    form.querySelector('[name="projeAdi"]').value = proje.projeAdi || "";
    form.querySelector('[name="projeHizmetAlani"]').value = proje.projeHizmetAlani || "";
    ["projeDurum", "projeKritiklik", "birimId", "yazilimUzmaniId", "teknolojiId"].forEach(function (name) {
      setSelectValue(form.querySelector('[name="' + name + '"]'), proje[name]);
    });
    form.querySelector('[name="sunucu"]').value = proje.sunucu || "";
    form.querySelector('[name="websiteUrl"]').value = proje.websiteUrl || "";
    // Aktif / Askıda radio pair.
    form.querySelector('[name="projeAktifMi"][value="' + Boolean(proje.projeAktifMi) + '"]').checked = true;
    form.querySelector('[name="projeAciklamasi"]').value = proje.projeAciklamasi || "";
  }

  // Preserves the current search/filter querystring while flagging the reload so site.js can
  // show the right success toast - see ApplyFilter-style state preservation elsewhere in the app.
  function reloadWithFlag(flagName) {
    var url = new URL(window.location.href);
    url.searchParams.set(flagName, "1");
    window.location.href = url.toString();
  }

  function setupSlider(overlay, form) {
    var stepTitles = ["1. Temel Bilgiler", "2. Sorumlu & Teknoloji", "3. Altyapı & Detaylar"];
    var currentStep = 1;
    var totalSteps = 3;

    var track = form.querySelector("[data-modal-slider-track]");
    var slides = form.querySelectorAll(".modal-slide");
    var progressFill = overlay.querySelector("[data-slider-progress-fill]");
    var stepTitle = overlay.querySelector("[data-slider-step-title]");
    var stepCount = overlay.querySelector("[data-slider-step-count]");
    var dots = overlay.querySelectorAll("[data-step-dot]");
    var cancelBtn = overlay.querySelector("[data-modal-cancel-btn]");
    var prevBtn = overlay.querySelector("[data-slider-prev-btn]");
    var nextBtn = overlay.querySelector("[data-slider-next-btn]");
    var submitBtn = overlay.querySelector("[data-create-proje-submit]");
    var sliderBody = overlay.querySelector("[data-modal-body-slider]");

    // Same capability query as the CSS that turns the form into a slider (site.css), so a narrow
    // desktop window keeps the regular two-column form.
    var phoneQuery = window.matchMedia(window.AppConfig.media.phone);
    function isMobileView() {
      return phoneQuery.matches;
    }

    function validateSlideInputs(slideIndex) {
      var slide = slides[slideIndex - 1];
      if (!slide) return true;
      var inputs = slide.querySelectorAll("input, select, textarea");
      for (var i = 0; i < inputs.length; i++) {
        if (!inputs[i].checkValidity()) {
          reportInvalid(inputs[i], overlay.querySelector("[data-create-proje-error]"));
          return false;
        }
      }
      return true;
    }

    function goToStep(targetStep, skipValidation) {
      if (targetStep < 1) targetStep = 1;
      if (targetStep > totalSteps) targetStep = totalSteps;

      if (targetStep > currentStep && !skipValidation) {
        if (!validateSlideInputs(currentStep)) {
          return false;
        }
      }

      currentStep = targetStep;

      if (track) {
        track.setAttribute("data-active-step", String(currentStep));
        track.style.transform = "translateX(-" + ((currentStep - 1) * 100) + "%)";
      }

      slides.forEach(function (slide, idx) {
        if (idx + 1 === currentStep) {
          slide.classList.add("active");
        } else {
          slide.classList.remove("active");
        }
      });

      if (progressFill) {
        progressFill.style.width = ((currentStep / totalSteps) * 100) + "%";
      }
      if (stepTitle) {
        stepTitle.textContent = stepTitles[currentStep - 1] || "";
      }
      if (stepCount) {
        stepCount.textContent = currentStep + " / " + totalSteps;
      }

      dots.forEach(function (dot, idx) {
        if (idx + 1 === currentStep) {
          dot.classList.add("active");
        } else {
          dot.classList.remove("active");
        }
      });

      updateButtonsDisplay();
      if (sliderBody) sliderBody.scrollTop = 0;
      return true;
    }

    function updateButtonsDisplay() {
      var isMobile = isMobileView();
      if (!isMobile) {
        if (cancelBtn) cancelBtn.style.display = "";
        if (prevBtn) prevBtn.style.display = "none";
        if (nextBtn) nextBtn.style.display = "none";
        if (submitBtn) submitBtn.style.display = "";
        return;
      }

      if (currentStep === 1) {
        if (cancelBtn) cancelBtn.style.display = "";
        if (prevBtn) prevBtn.style.display = "none";
        if (nextBtn) nextBtn.style.display = "";
        if (submitBtn) submitBtn.style.display = "none";
      } else if (currentStep === 2) {
        if (cancelBtn) cancelBtn.style.display = "none";
        if (prevBtn) prevBtn.style.display = "";
        if (nextBtn) nextBtn.style.display = "";
        if (submitBtn) submitBtn.style.display = "none";
      } else {
        if (cancelBtn) cancelBtn.style.display = "none";
        if (prevBtn) prevBtn.style.display = "";
        if (nextBtn) nextBtn.style.display = "none";
        if (submitBtn) submitBtn.style.display = "";
      }
    }

    if (nextBtn) {
      nextBtn.addEventListener("click", function () {
        goToStep(currentStep + 1);
      });
    }

    if (prevBtn) {
      prevBtn.addEventListener("click", function () {
        goToStep(currentStep - 1, true);
      });
    }

    dots.forEach(function (dot) {
      dot.addEventListener("click", function () {
        var stepNum = Number(dot.getAttribute("data-step-dot"));
        if (stepNum) goToStep(stepNum, stepNum < currentStep);
      });
    });

    if (sliderBody) {
      // Focusing a dropdown's search box makes the browser scroll this overflow-x:hidden body
      // sideways, shifting the slides out of line - horizontal position belongs to the track only.
      sliderBody.addEventListener("scroll", function () {
        if (sliderBody.scrollLeft) sliderBody.scrollLeft = 0;
      });

      var startX = 0;
      var startY = 0;
      sliderBody.addEventListener("touchstart", function (e) {
        if (!isMobileView()) return;
        startX = e.touches[0].clientX;
        startY = e.touches[0].clientY;
      }, { passive: true });

      sliderBody.addEventListener("touchend", function (e) {
        if (!isMobileView()) return;
        var diffX = e.changedTouches[0].clientX - startX;
        var diffY = e.changedTouches[0].clientY - startY;

        if (Math.abs(diffX) > 40 && Math.abs(diffX) > Math.abs(diffY)) {
          if (diffX < 0) {
            goToStep(currentStep + 1);
          } else {
            goToStep(currentStep - 1, true);
          }
        }
      }, { passive: true });
    }

    window.addEventListener("resize", function () {
      updateButtonsDisplay();
    });

    return {
      reset: function () {
        goToStep(1, true);
      },
      getCurrentStep: function () { return currentStep; },
      goToStep: goToStep
    };
  }

  // Used on the Projelerim page (create button + row "Projeyi Düzenle") and on the project detail
  // page (header "Düzenle"), which has the same modal partial but no create button.
  function init() {
    var openBtn = document.querySelector("[data-open-create-proje]");
    var overlay = document.querySelector("[data-create-proje-overlay]");
    if (!overlay) return;

    var form = overlay.querySelector("[data-create-proje-form]");
    var errorBox = overlay.querySelector("[data-create-proje-error]");
    var submitBtn = overlay.querySelector("[data-create-proje-submit]");
    var titleEl = overlay.querySelector("[data-proje-form-title]");
    var lookupsLoaded = false;

    populateEnumSelect(form.querySelector('[name="projeDurum"]'), cfg.statusThemes);
    populateEnumSelect(form.querySelector('[name="projeKritiklik"]'), cfg.criticalityThemes);

    var slider = setupSlider(overlay, form);
    var projeKoduInput = form.querySelector("[data-proje-kodu-input]");
    bindProjeKoduInput(form);

    function resetForm() {
      if (slider) slider.reset();
      // form.reset() doesn't clear a custom validity message left from the previous attempt.
      validateProjeKoduInput(projeKoduInput);
    }

    function ensureLookupsLoaded() {
      if (lookupsLoaded) return Promise.resolve();
      return populateLookupSelects(
        form.querySelector('[name="birimId"]'),
        form.querySelector('[name="yazilimUzmaniId"]'),
        form.querySelector('[name="teknolojiId"]')
      ).then(function () {
        lookupsLoaded = true;
      });
    }

    if (openBtn) {
      openBtn.addEventListener("click", function () {
        form.dataset.mode = "create";
        if (titleEl) titleEl.textContent = "Yeni Proje Oluştur";

        openModal(overlay, form, errorBox, resetForm);

        ensureLookupsLoaded().catch(function () {
          errorBox.textContent = "Birim, yazılım uzmanı veya teknoloji listeleri yüklenemedi.";
          errorBox.classList.add("open");
        });
      });
    }

    // Triggered from the row menu's "Projeyi Düzenle" action (projects.js bindRowActionEvents) and
    // the detail page's "Düzenle" button (project-detail.js).
    function openEdit(id) {
      form.dataset.mode = "edit";
      if (titleEl) titleEl.textContent = "Projeyi Düzenle";

      openModal(overlay, form, errorBox, resetForm);

      Promise.all([
        ensureLookupsLoaded(),
        loadLookup("/Proje/GetForEdit/" + id)
      ]).then(function (results) {
        populateFormValues(form, results[1]);
        validateProjeKoduInput(projeKoduInput);
      }).catch(function () {
        errorBox.textContent = "Proje bilgileri yüklenemedi.";
        errorBox.classList.add("open");
      });
    }

    overlay.querySelectorAll("[data-close-create-proje]").forEach(function (btn) {
      btn.addEventListener("click", function () { closeModal(overlay); });
    });

    overlay.addEventListener("click", function (e) {
      if (e.target === overlay) closeModal(overlay);
    });

    form.addEventListener("submit", function (e) {
      e.preventDefault();

      if (projeKoduInput) {
        projeKoduInput.value = normalizeProjeKodu(projeKoduInput.value);
        validateProjeKoduInput(projeKoduInput);
      }

      if (!form.checkValidity()) {
        // Find which slide has an invalid input so we can slide to it
        var inputs = form.querySelectorAll("input, select, textarea");
        for (var i = 0; i < inputs.length; i++) {
          if (!inputs[i].checkValidity()) {
            var slideEl = inputs[i].closest("[data-slide]");
            if (slideEl && slider) {
              var stepNum = Number(slideEl.getAttribute("data-slide"));
              slider.goToStep(stepNum, true);
            }
            reportInvalid(inputs[i], errorBox);
            return;
          }
        }
        form.reportValidity();
        return;
      }

      errorBox.classList.remove("open");
      errorBox.textContent = "";
      submitBtn.disabled = true;

      var isEdit = form.dataset.mode === "edit";
      var payload = buildPayload(form);
      var url = isEdit ? "/Proje/Edit/" + payload.id : "/Proje/Create";
      var expectedStatus = isEdit ? 200 : 201;

      fetch(url, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload)
      }).then(function (res) {
        if (res.status === expectedStatus) {
          reloadWithFlag(isEdit ? "updated" : "created");
          return;
        }
        return res.json().catch(function () { return null; }).then(function (body) {
          if (hasProjeKoduError(body) && slider) slider.goToStep(1, true);
          throw new Error(errorMessageFrom(body));
        });
      }).catch(function (err) {
        errorBox.textContent = err.message || "Proje kaydedilemedi.";
        errorBox.classList.add("open");
        submitBtn.disabled = false;
      });
    });

    window.ProjectForm.openEdit = openEdit;
  }

  // Row menu "Projeyi Sil" (list + detail page). Server side this is a soft delete
  // (ProjeService.DeleteAsync). From the detail page there's nothing left to show, so go to the list.
  function deleteProje(id) {
    if (!window.confirm("Bu proje silinecek. Emin misiniz?")) return;
    fetch("/Proje/Delete/" + id, { method: "POST" }).then(function (res) {
      if (!res.ok) throw new Error();
      if (window.location.pathname.toLowerCase().indexOf("/proje/details") === 0) {
        window.location.href = "/Proje?deleted=1";
      } else {
        reloadWithFlag("deleted");
      }
    }).catch(function () {
      if (window.Toast) window.Toast.show("Proje silinemedi.", "danger");
    });
  }

  window.ProjectForm = { init: init, deleteProje: deleteProje };
})(window);
