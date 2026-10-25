"use strict";
document.querySelectorAll("[data-print]").forEach(button => button.addEventListener("click", () => window.print()));
document.querySelectorAll("form[data-confirm]").forEach(form => {
    form.addEventListener("submit", event => {
        if (!window.confirm(form.dataset.confirm)) event.preventDefault();
    });
});

// Muestra el código que se asignará según el tipo de espacio elegido.
document.querySelectorAll("[data-codigo-destino]").forEach(select => {
    const destino = document.getElementById(select.dataset.codigoDestino);
    if (!destino?.dataset.codigos) return;
    const codigos = JSON.parse(destino.dataset.codigos);
    select.addEventListener("change", () => { if (codigos[select.value]) destino.value = codigos[select.value]; });
});

// Vista previa de la fotografía antes de guardar.
document.querySelectorAll("[data-foto-input]").forEach(input => {
    const preview = input.closest("form")?.querySelector("[data-foto-preview]");
    input.addEventListener("change", () => {
        const archivo = input.files?.[0];
        if (preview && archivo && archivo.type.startsWith("image/")) preview.src = URL.createObjectURL(archivo);
    });
});
