const session = renderInternShell("home");

// Giriş yoksa renderInternShell zaten yönlendirdi, sayfayı doldurma
if (session) {
    const fullName = `${session.firstName} ${session.lastName}`.trim();

    document.getElementById("welcomeName").textContent = fullName;
    document.getElementById("startDate").textContent = formatDate(session.startDate, "Belirlenmedi");
    document.getElementById("endDate").textContent = formatDate(session.endDate, "Belirlenmedi");
}