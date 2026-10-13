const form = document.getElementById("loginForm");
const emailInput = document.getElementById("email");
const passwordInput = document.getElementById("password");
const submitButton = form.querySelector("button[type=submit]");
const messageBox = document.getElementById("message");

// Zaten giriş yapılmışsa login'i göstermeden ana sayfaya geç
if (getSession()?.role === ROLE_INTERN) {
    window.location.replace("/Intern/home.html");
}

function showError(messages) {
    messageBox.replaceChildren();

    const alertDiv = document.createElement("div");
    alertDiv.className = "alert alert-danger";

    // textContent: sunucudan gelen metin HTML olarak yorumlanmasın
    messages.forEach(text => {
        const line = document.createElement("div");
        line.textContent = text;
        alertDiv.appendChild(line);
    });

    messageBox.appendChild(alertDiv);
}

form.addEventListener("submit", async (event) => {
    event.preventDefault();
    messageBox.replaceChildren();
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
        showError(error instanceof ApiError ? error.messages : ["Beklenmeyen bir hata oluştu."]);
        submitButton.disabled = false;
    }
});