const session = renderStaffShell("interns");

const statusBox = document.getElementById("statusBox");
const internList = document.getElementById("internList");

// API: status 1 = aktif, 0 = ayrıldı
const INTERN_ACTIVE = 1;
const INTERN_STATUS = {
    1: { text: "Aktif", badge: "text-bg-success" },
    0: { text: "Ayrıldı", badge: "text-bg-secondary" }
};

// ---------- Kart oluşturucular (veri her zaman textContent ile yazılır) ----------

// "ibrahim oruc" -> "İO"
function getInitials(firstName, lastName) {
    const first = (firstName ?? "").trim().charAt(0);
    const last = (lastName ?? "").trim().charAt(0);
    return (first + last).toLocaleUpperCase("tr-TR");
}

function createAvatar(intern) {
    const avatar = document.createElement("div");
    avatar.className = "avatar-circle rounded-circle bg-secondary-subtle text-secondary-emphasis " +
        "d-flex align-items-center justify-content-center fw-semibold";
    avatar.textContent = getInitials(intern.firstName, intern.lastName);
    return avatar;
}

function createStatusBadge(status) {
    const info = INTERN_STATUS[status] ?? { text: "Bilinmiyor", badge: "text-bg-light border" };

    const badge = document.createElement("span");
    badge.className = `badge ${info.badge}`;
    badge.textContent = info.text;
    return badge;
}

function createInfoLine(text) {
    const line = document.createElement("div");
    line.className = "small text-muted";
    line.textContent = text;
    return line;
}

// Şimdilik pasif: işlemler bağlanınca disabled kalkıp click dinleyicisi eklenecek
function createActionButton(text, style) {
    const button = document.createElement("button");
    button.type = "button";
    button.className = `btn btn-sm ${style}`;
    button.textContent = text;
    button.disabled = true;
    button.title = "Yakında";
    return button;
}

function createInternCard(intern) {
    const isActive = intern.status === INTERN_ACTIVE;

    const card = document.createElement("div");
    card.className = "card mb-3" + (isActive ? "" : " opacity-75");
    card.dataset.internId = intern.internID;   // detay sayfası için hazır

    const body = document.createElement("div");
    body.className = "card-body d-flex align-items-center flex-wrap gap-3";

    // Başlık: ad soyad + durum rozeti
    const nameLine = document.createElement("div");
    nameLine.className = "d-flex align-items-center gap-2";

    const name = document.createElement("h5");
    name.className = "mb-0";
    name.textContent = `${intern.firstName} ${intern.lastName}`.trim();
    nameLine.append(name, createStatusBadge(intern.status));

    const info = document.createElement("div");
    info.className = "flex-grow-1";
    info.append(
        nameLine,
        createInfoLine(`${intern.email || "-"} · ${intern.phone || "-"}`),
        createInfoLine(
            `Başlangıç: ${formatDate(intern.startDate, "Belirlenmedi")} · ` +
            `Bitiş: ${formatDate(intern.endDate, "Belirlenmedi")}`
        ),
        createInfoLine(`${intern.activeProjectCount ?? 0} aktif proje`)
    );

    // İşlemler
    const actions = document.createElement("div");
    actions.className = "d-flex gap-2";
    actions.append(
        createActionButton("Projeler", "btn-outline-primary"),
        createActionButton("Görevler", "btn-outline-primary")
    );
    if (isActive) {
        actions.appendChild(createActionButton("Staj Bitir", "btn-outline-danger"));
    }

    body.append(createAvatar(intern), info, actions);
    card.appendChild(body);
    return card;
}

// ---------- Sayfa akışı ----------
async function loadInterns() {
    internList.replaceChildren();
    showMessage(statusBox, "info", "Stajyerler yükleniyor...");

    try {
        const interns = await getAllInterns();

        if (interns.length === 0) {
            showMessage(statusBox, "secondary", "Henüz stajyer yok.");
            return;
        }

        hideMessage(statusBox);
        // Sıralama API'dan geldiği gibi gösterilir
        interns.forEach(intern => internList.appendChild(createInternCard(intern)));
    } catch (error) {
        showError(statusBox, error);
    }
}

// Giriş yoksa renderStaffShell zaten yönlendirdi
if (session) {
    loadInterns();
}