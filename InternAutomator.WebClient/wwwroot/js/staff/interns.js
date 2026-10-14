const session = renderStaffShell("interns");

const statusBox = document.getElementById("statusBox");
const noticeBox = document.getElementById("noticeBox");
const internList = document.getElementById("internList");

// Staj bitirme penceresi
const separateModalElement = document.getElementById("separateModal");
const separateModal = new bootstrap.Modal(separateModalElement);
const separateForm = document.getElementById("separateForm");
const separateSummary = document.getElementById("separateSummary");
const separateExpected = document.getElementById("separateExpected");
const separateConfirm = document.getElementById("separateConfirm");
const separateSubmit = document.getElementById("separateSubmit");
const separateCancel = document.getElementById("separateCancel");
const separateError = document.getElementById("separateError");

let internsById = new Map();
let pendingIntern = null;   // penceresi açık olan stajyer

// API: status 1 = aktif, 0 = ayrıldı
const INTERN_ACTIVE = 1;
const INTERN_STATUS = {
    1: { text: "Aktif", badge: "text-bg-success" },
    0: { text: "Ayrıldı", badge: "text-bg-secondary" }
};

// ---------- Kart oluşturucular (veri her zaman textContent ile yazılır) ----------

// "ibrahim oruc" -> "IO"
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

// action verilmezse buton şimdilik pasiftir ("Yakında")
function createActionButton(text, style, action) {
    const button = document.createElement("button");
    button.type = "button";
    button.className = `btn btn-sm ${style}`;
    button.textContent = text;

    if (action) {
        button.dataset.action = action;
    } else {
        button.disabled = true;
        button.title = "Yakında";
    }
    return button;
}

function getFullName(intern) {
    return `${intern.firstName} ${intern.lastName}`.trim();
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
    name.textContent = getFullName(intern);
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
        actions.appendChild(createActionButton("Staj Bitir", "btn-outline-danger", "separate"));
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

        internsById = new Map(interns.map(intern => [intern.internID, intern]));

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

// ---------- Staj bitirme penceresi ----------
// Büyük/küçük harf ve fazla boşluk farkı önemsenmez
function normalizeName(text) {
    return (text ?? "").trim().replace(/\s+/g, " ").toLocaleLowerCase("tr-TR");
}

function isConfirmationValid() {
    return pendingIntern !== null &&
        normalizeName(separateConfirm.value) === normalizeName(getFullName(pendingIntern));
}

function updateSeparateSubmit() {
    separateSubmit.disabled = !isConfirmationValid();
}

function setSeparateBusy(isBusy) {
    separateConfirm.disabled = isBusy;
    separateCancel.disabled = isBusy;
    separateSubmit.disabled = isBusy || !isConfirmationValid();
}

function openSeparateModal(internID) {
    const intern = internsById.get(internID);
    if (!intern || intern.status !== INTERN_ACTIVE) return;

    pendingIntern = intern;

    separateSummary.textContent = `${getFullName(intern)} adlı stajyerin stajı bitirilecek.`;
    separateExpected.textContent = getFullName(intern);
    separateConfirm.value = "";
    hideMessage(separateError);
    setSeparateBusy(false);

    separateModal.show();
}

async function submitSeparate(event) {
    event.preventDefault();
    if (!isConfirmationValid()) return;   // Enter ile gönderimi de engeller

    const intern = pendingIntern;

    hideMessage(separateError);
    setSeparateBusy(true);

    try {
        await separateIntern(intern.internID);

        separateModal.hide();
        showNotice(noticeBox, "success", `${getFullName(intern)} adlı stajyerin stajı sona erdi.`);
        await loadInterns();   // ekranda her zaman sunucudaki gerçek hal görünsün
    } catch (error) {
        showError(separateError, error);

        // İstek sunucuya ulaştıysa liste eskimiş olabilir (örn. stajyer zaten ayrılmış)
        if (error instanceof ApiError && error.status > 0) {
            loadInterns();
        }
    } finally {
        setSeparateBusy(false);
    }
}

// ---------- Olay dinleyicileri ----------
// Kartlar her yüklemede yeniden çizildiği için tek bir dinleyici listede durur
internList.addEventListener("click", (event) => {
    const button = event.target.closest("button[data-action]");
    if (!button) return;

    const internID = Number(button.closest(".card").dataset.internId);

    if (button.dataset.action === "separate") {
        openSeparateModal(internID);
    }
});

separateConfirm.addEventListener("input", updateSeparateSubmit);
separateForm.addEventListener("submit", submitSeparate);

separateModalElement.addEventListener("hidden.bs.modal", () => {
    pendingIntern = null;
});

// Giriş yoksa renderStaffShell zaten yönlendirdi
if (session) {
    loadInterns();
}