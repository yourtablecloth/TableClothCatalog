"use strict";

(() => {
  const form = document.querySelector("[data-filters]");
  if (!form) return;
  const search = form.elements.q;
  const category = form.elements.category;
  const entries = [...document.querySelectorAll("[data-entry]")];
  const sections = [...document.querySelectorAll("[data-result-section]")];
  const status = document.querySelector("[data-result-count]");
  const empty = document.querySelector("[data-empty]");
  const normalize = (value) => value.normalize("NFKC").toLocaleLowerCase();
  const indexed = entries.map((entry) => [entry, normalize(entry.dataset.search || "")]);

  function update(writeLocation = true) {
    const terms = normalize(search.value.trim()).split(/\s+/).filter(Boolean);
    let count = 0;
    for (const [entry, haystack] of indexed) {
      const visible = (!category.value || entry.dataset.category === category.value) && terms.every((term) => haystack.includes(term));
      entry.hidden = !visible;
      if (visible) count += 1;
    }
    for (const section of sections) {
      section.hidden = ![...section.querySelectorAll("[data-entry]")].some((entry) => !entry.hidden);
    }
    status.textContent = `전체 ${entries.length}개 중 ${count}개`;
    empty.hidden = count !== 0;
    if (writeLocation) {
      const url = new URL(window.location.href);
      for (const [key, value] of [["q", search.value.trim()], ["category", category.value]]) {
        if (value) url.searchParams.set(key, value);
        else url.searchParams.delete(key);
      }
      try { window.history.replaceState(null, "", url); } catch { /* Search also works when history is unavailable. */ }
    }
  }

  const params = new URLSearchParams(window.location.search);
  search.value = params.get("q") || "";
  const requestedCategory = params.get("category") || "";
  if ([...category.options].some((option) => option.value === requestedCategory)) category.value = requestedCategory;
  form.hidden = false;
  form.addEventListener("submit", (event) => { event.preventDefault(); update(); });
  search.addEventListener("input", () => update());
  category.addEventListener("change", () => update());
  form.addEventListener("reset", () => { window.setTimeout(() => { update(); search.focus(); }, 0); });
  document.querySelectorAll("[data-reset]").forEach((button) => button.addEventListener("click", () => form.reset()));
  update(false);
})();
