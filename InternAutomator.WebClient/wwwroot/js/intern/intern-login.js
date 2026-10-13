const form = document.getElementById("loginForm");
const emailInput = document.getElementById("email");
const passwordInput = document.getElementById("password");
const submitButton = form.querySelector("button[type=submit]");
const messageBox = document.getElementById("message");

// Zaten giriş yapılmışsa login'i göstermeden ana sayfaya geç
if (getSession()?.role === ROLE_INTERN) {
    window.location.replace("/Intern/home.html");
}

form.addEventListener("submit", async (event) => {
    event.preventDefault();
    hideMessage(messageBox);
    submitButton.disabled = true;   // çift tıklamayı engelle

    try {
        const intern = await internLogin(emailInput.value.trim(), passwordInput.value);

        // Şifre hiçbir yerde saklanmaz
        saveSession({
            role: ROLE_INTERN,
            internID: intern.internID,
            firstName: intern.firstName,
            lastName: intern.lastName,
            startDate: intern.startDate,
            endDate: intern.endDate,
            status: intern.status
        });

        window.location.replace("/Intern/home.html");
    } catch (error) {
        passwordInput.value = "";
        showError(messageBox, error);
        submitButton.disabled = false;
    }
});