(function (window) {
  "use strict";

  var cfg = window.AppConfig;

  function escapeHtml(value) {
    return String(value == null ? "" : value).replace(/[&<>"']/g, function (ch) {
      return { "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[ch];
    });
  }

  function openModal(overlay, form, errorBox, resetStepFn) {
    form.reset();
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

  function populateEnumSelect(select, themeMap) {
    var options = Object.keys(themeMap).map(function (key) {
      var theme = themeMap[key];
      return '<option value="' + theme.value + '">' + escapeHtml(theme.label) + "</option>";
    });
    select.innerHTML = options.join("");
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
        birimSelect.innerHTML = birimler.map(function (b) {
          return '<option value="' + b.id + '">' + escapeHtml(b.birim) + "</option>";
        }).join("");
      }),
      loadLookup("/YazilimUzmani").then(function (uzmanlar) {
        uzmanSelect.innerHTML = uzmanlar.map(function (u) {
          var label = u.ad + " " + u.soyad + " (" + u.kullanıcıAdi + ")";
          return '<option value="' + u.id + '">' + escapeHtml(label) + "</option>";
        }).join("");
      }),
      loadLookup("/Teknoloji").then(function (teknolojiler) {
        teknolojiSelect.innerHTML = teknolojiler.map(function (t) {
          var label = t.backendTeknoloji + " / " + t.frontendTeknoloji + " / " + t.veritabani;
          return '<option value="' + t.id + '">' + escapeHtml(label) + "</option>";
        }).join("");
      })
    ]);
  }

  function buildPayload(form) {
    var data = new FormData(form);
    return {
      projeKodu: Number(data.get("projeKodu")),
      projeAdi: data.get("projeAdi") || "",
      projeHizmetAlani: data.get("projeHizmetAlani") || "",
      projeAciklamasi: data.get("projeAciklamasi") || "",
      projeAktifMi: data.get("projeAktifMi") === "on",
      sunucu: data.get("sunucu") || "",
      websiteUrl: data.get("websiteUrl") || "",
      projeDurum: Number(data.get("projeDurum")),
      projeKritiklik: Number(data.get("projeKritiklik")),
      birimId: Number(data.get("birimId")),
      yazilimUzmaniId: Number(data.get("yazilimUzmaniId")),
      teknolojiId: Number(data.get("teknolojiId"))
    };
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

    function isMobileView() {
      return window.innerWidth <= 767;
    }

    function validateSlideInputs(slideIndex) {
      var slide = slides[slideIndex - 1];
      if (!slide) return true;
      var inputs = slide.querySelectorAll("input, select, textarea");
      for (var i = 0; i < inputs.length; i++) {
        if (!inputs[i].checkValidity()) {
          inputs[i].reportValidity();
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

  function init() {
    var openBtn = document.querySelector("[data-open-create-proje]");
    var overlay = document.querySelector("[data-create-proje-overlay]");
    if (!openBtn || !overlay) return;

    var form = overlay.querySelector("[data-create-proje-form]");
    var errorBox = overlay.querySelector("[data-create-proje-error]");
    var submitBtn = overlay.querySelector("[data-create-proje-submit]");
    var lookupsLoaded = false;

    populateEnumSelect(form.querySelector('[name="projeDurum"]'), cfg.statusThemes);
    populateEnumSelect(form.querySelector('[name="projeKritiklik"]'), cfg.criticalityThemes);

    var slider = setupSlider(overlay, form);

    openBtn.addEventListener("click", function () {
      openModal(overlay, form, errorBox, function () {
        if (slider) slider.reset();
      });

      if (!lookupsLoaded) {
        populateLookupSelects(
          form.querySelector('[name="birimId"]'),
          form.querySelector('[name="yazilimUzmaniId"]'),
          form.querySelector('[name="teknolojiId"]')
        ).then(function () {
          lookupsLoaded = true;
        }).catch(function () {
          errorBox.textContent = "Birim, yazılım uzmanı veya teknoloji listeleri yüklenemedi.";
          errorBox.classList.add("open");
        });
      }
    });

    overlay.querySelectorAll("[data-close-create-proje]").forEach(function (btn) {
      btn.addEventListener("click", function () { closeModal(overlay); });
    });

    overlay.addEventListener("click", function (e) {
      if (e.target === overlay) closeModal(overlay);
    });

    form.addEventListener("submit", function (e) {
      e.preventDefault();

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
            inputs[i].reportValidity();
            return;
          }
        }
        form.reportValidity();
        return;
      }

      errorBox.classList.remove("open");
      errorBox.textContent = "";
      submitBtn.disabled = true;

      fetch("/Proje/Create", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(buildPayload(form))
      }).then(function (res) {
        if (res.status === 201) {
          window.location.reload();
          return;
        }
        return res.json().catch(function () { return null; }).then(function (body) {
          throw new Error(body && body.title ? body.title : "Proje kaydedilemedi.");
        });
      }).catch(function (err) {
        errorBox.textContent = err.message || "Proje kaydedilemedi.";
        errorBox.classList.add("open");
        submitBtn.disabled = false;
      });
    });
  }

  window.ProjectForm = { init: init };
})(window);
