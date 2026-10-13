const session = requireLogin(ROLE_INTERN, "/Intern/login.html");

function formatDate(isoDate) {
    return isoDate ? new Date(isoDate).toLocaleDateString("tr-TR") : "Belirlenmedi";
}

// Giriş yoksa requireLogin zaten yönlendirdi, sayfayı doldurma
if (session) {
    const fullName = `${session.firstName} ${session.lastName}`.trim();

    document.getElementById("userName").textContent = fullName;
    document.getElementById("welcomeName").textContent = fullName;
    document.getElementById("startDate").textContent = formatDate(session.startDate);
    document.getElementById("endDate").textContent = formatDate(session.endDate);

    document.getElementById("logoutButton").addEventListener("click", () => {
        clearSession();
        window.location.replace("/Intern/login.html");
    });
}