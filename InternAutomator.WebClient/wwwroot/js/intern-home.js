const session = renderInternShell("home");

function formatDate(isoDate) {
    return isoDate ? new Date(isoDate).toLocaleDateString("tr-TR") : "Belirlenmedi";
}

// Giriş yoksa renderInternShell zaten yönlendirdi, sayfayı doldurma
if (session) {
    const fullName = `${session.firstName} ${session.lastName}`.trim();

    document.getElementById("welcomeName").textContent = fullName;
    document.getElementById("startDate").textContent = formatDate(session.startDate);
    document.getElementById("endDate").textContent = formatDate(session.endDate);
}