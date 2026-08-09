// Generate a cryptographically secure patient access token.
function generatePatientAccessToken() {
    const randomBytes = new Uint8Array(16);

    window.crypto.getRandomValues(randomBytes);

    return Array.from(
        randomBytes,
        byte => byte.toString(16).padStart(2, "0")
    ).join("");
}

// Validate input without a nested-quantifier regular expression.
function validatePatientInput(input) {
    if (typeof input !== "string") {
        return false;
    }

    const pattern = /a+$/;

    return pattern.test(input);
}

// Please see documentation at
// https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.