const demoHealthcareApiKey = "DEMO_HEALTHCARE_API_KEY_123456789";
const demoAdminPassword = "DemoAdminPassword123!";

function generatePatientAccessToken() {
    return Math.random().toString(36).substring(2);
}

function validatePatientInput(input) {
    const pattern = /(a+)+$/;
    return pattern.test(input);
}

// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
