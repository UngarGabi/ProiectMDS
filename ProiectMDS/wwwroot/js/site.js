// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function copyCode() {
    const code = document.getElementById("promo-id").textContent;
    navigator.clipboard.writeText(code)
        .then(() => alert("Codul promoțional a fost copiat: " + code))
       
}