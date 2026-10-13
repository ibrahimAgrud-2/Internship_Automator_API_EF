const form = document.getElementById("loginForm");
const userNameInput = document.getElementById("userName");
const passwordInput = document.getElementById("password");
const submitButton = form.querySelector("button[type=submit]");
const messageBox = document.getElementById("message");

// Zaten giriş yapılmışsa login'i göstermeden panele geç
if (getSession()?.role === ROLE_STAFF) {
    window.location.replace("/Staff/applications.html");
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
        const user = await staffLogin(userNameInput.value.trim(), passwordInput.value);

        // Şifre hiçbir yerde saklanmaz
        saveSession({
            role: ROLE_STAFF,
            userID: user.userID,
            firstName: user.firstName,
            lastName: user.lastName,
            permissionType: user.permissionType,
            isActive: user.isActive
        });

        window.location.replace("/Staff/applications.html");
    } catch (error) {
        passwordInput.value = "";
        showError(error instanceof ApiError ? error.messages : ["Beklenmeyen bir hata oluştu."]);
        submitButton.disabled = false;
    }
});