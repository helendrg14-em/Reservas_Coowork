"use strict";
document.querySelectorAll("[data-print]").forEach(button => button.addEventListener("click", () => window.print()));
document.querySelectorAll("form[data-confirm]").forEach(form => {
    form.addEventListener("submit", event => {
        if (!window.confirm(form.dataset.confirm)) event.preventDefault();
    });
});

// evita que un doble clic envie el formulario dos veces
document.querySelectorAll('form[method="post" i]').forEach(form => {
    form.addEventListener("submit", event => {
        if (event.defaultPrevented) return;
        if (form.dataset.enviando) { event.preventDefault(); return; }
        form.dataset.enviando = "1";
        setTimeout(() => form.querySelectorAll("button[type=submit]").forEach(b => b.disabled = true), 0);
    });
});
window.addEventListener("pageshow", () => document.querySelectorAll("form[data-enviando]").forEach(form => {
    delete form.dataset.enviando;
    form.querySelectorAll("button[type=submit]").forEach(b => b.disabled = false);
}));

document.querySelectorAll("[data-filtrar-select]").forEach(input => {
    const select = document.getElementById(input.dataset.filtrarSelect);
    if (!select) return;
    const normalizar = texto => texto.normalize("NFD").replace(/[\u0300-\u036f]/g, "").toLowerCase();
    input.addEventListener("input", () => {
        const termino = normalizar(input.value.trim());
        const visibles = [];
        Array.from(select.options).forEach(option => {
            if (option.value === "0" || option.value === "") return;
            option.hidden = termino !== "" && !normalizar(option.text).includes(termino);
            if (!option.hidden) visibles.push(option);
        });
        if (termino !== "" && visibles.length === 1) select.value = visibles[0].value;
        else if (select.selectedOptions[0]?.hidden) select.value = "0";
    });
});

document.querySelectorAll("[data-codigo-destino]").forEach(select => {
    const destino = document.getElementById(select.dataset.codigoDestino);
    if (!destino?.dataset.codigos) return;
    const codigos = JSON.parse(destino.dataset.codigos);
    select.addEventListener("change", () => { if (codigos[select.value]) destino.value = codigos[select.value]; });
});

document.querySelectorAll("[data-foto-input]").forEach(input => {
    const preview = input.closest("form")?.querySelector("[data-foto-preview]");
    input.addEventListener("change", () => {
        const archivo = input.files?.[0];
        if (preview && archivo && archivo.type.startsWith("image/")) preview.src = URL.createObjectURL(archivo);
    });
});
