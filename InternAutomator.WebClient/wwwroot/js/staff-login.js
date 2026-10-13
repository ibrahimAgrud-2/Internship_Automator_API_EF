const form = document.getElementById("loginForm");
const userNameInput = document.getElementById("userName");
const passwordInput = document.getElementById("password");
const submitButton = form.querySelector("button[type=submit]");
const messageBox = document.getElementById("message");

// Zaten giriş yapılmışsa login'i göstermeden panele geç
if (getSession()?.role === ROLE_STAFF) {
    window.location.replace("/Staff/applications.html");
}

form.addEventListener("submit", async (event) => {
    event.preventDefault();
    hideMessage(messageBox);
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
        showError(messageBox, error);
        submitButton.disabled = false;
    }
});