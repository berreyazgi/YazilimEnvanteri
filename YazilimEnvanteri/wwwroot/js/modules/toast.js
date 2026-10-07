(function (window) {
  "use strict";

  function show(message, tone) {
    var container = document.querySelector("[data-toast-container]");
    if (!container) return;

    var toast = document.createElement("div");
    toast.className = "toast toast--" + (tone || "success");
    toast.textContent = message;
    container.appendChild(toast);

    // Let the browser paint the initial (offscreen) state before animating in.
    requestAnimationFrame(function () {
      toast.classList.add("open");
    });

    window.setTimeout(function () {
      toast.classList.remove("open");
      toast.addEventListener("transitionend", function () {
        toast.remove();
      }, { once: true });
    }, 4000);
  }

  window.Toast = { show: show };
})(window);
