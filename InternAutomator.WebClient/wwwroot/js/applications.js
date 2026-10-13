// ---------- Sabitler ----------
// Durum numarası -> ekranda gösterilecek metin ve rozet rengi
// Giriş yoksa personel login sayfasına yönlendirir
const session = renderStaffShell("applications");
const STATUS = {
    0: { text: "Yeni", badge: "bg-primary" },
    1: { text: "İptal", badge: "bg-secondary" },
    2: { text: "Reddedildi", badge: "bg-danger" },
    3: { text: "Onaylandı", badge: "bg-success" }
};
const STATUS_NEW = 0;
const STATUS_REJECTED = 2;
const STATUS_APPROVED = 3;

// Buton eylemi -> hangi durum, hangi API fonksiyonu, hangi mesajlar (tek yerde)
const ACTIONS = {
    approve: {
        status: STATUS_APPROVED,
        title: "Başvuruyu onayla",
        result: "onaylanacak",
        buttonText: "Onayla",
        buttonClass: "btn-success",
        send: acceptApplication,   // API stajyeri oluşturur ve e-posta gönderir
        successMessage: "Başvuru onaylandı. Stajyere giriş bilgileri e-posta ile gönderildi.",
        // API e-posta ya da stajyer oluşturma hatasında her şeyi geri alıp 500 döner
        serverErrorMessage: "Onay tamamlanamadı. İşlem geri alındı, lütfen tekrar deneyin."
    },
    reject: {
        status: STATUS_REJECTED,
        title: "Başvuruyu reddet",
        result: "reddedilecek",
        buttonText: "Reddet",
        buttonClass: "btn-danger",
        send: updateApplicationStatus,
        successMessage: "Başvuru reddedildi.",
        serverErrorMessage: null   // genel sunucu hatası mesajı gösterilir
    }
};

// ---------- Sayfa elemanları ----------
const noticeBox = document.getElementById("noticeBox");
const statusBox = document.getElementById("statusBox");
const tableWrapper = document.getElementById("tableWrapper");
const tableBody = document.getElementById("applicationsBody");

const decisionModalElement = document.getElementById("decisionModal");
const decisionModal = new bootstrap.Modal(decisionModalElement);
const decisionForm = document.getElementById("decisionForm");
const decisionTitle = document.getElementById("decisionTitle");
const decisionSummary = document.getElementById("decisionSummary");
const decisionNotes = document.getElementById("decisionNotes");
const decisionError = document.getElementById("decisionError");
const decisionSubmit = document.getElementById("decisionSubmit");

// ---------- Durum ----------
let applicationsById = new Map();   // listedeki başvurular (id -> başvuru)
let pendingDecision = null;         // modalda açık olan karar: { action, applicationID }
let noticeTimer = null;

// ---------- Ortak yardımcılar ----------
function getErrorMessages(error) {
    return error instanceof ApiError
        ? error.messages
        : ["Beklenmeyen bir hata oluştu."];
}

function fillMessageBox(box, messages) {
    box.replaceChildren();

    messages.forEach(text => {
        const line = document.createElement("div");
        line.textContent = text;
        box.appendChild(line);
    });

    box.hidden = false;
}

// ---------- Sayfa üstündeki bildirim (birkaç saniye sonra kaybolur) ----------
function showNotice(type, text) {
    noticeBox.className = `alert alert-${type}`;
    noticeBox.textContent = text;
    noticeBox.hidden = false;

    clearTimeout(noticeTimer);
    noticeTimer = setTimeout(() => {
        noticeBox.hidden = true;
    }, 6000);
}

// ---------- Liste üstündeki mesaj kutusu ----------
function showStatus(type, messages) {
    statusBox.className = `alert alert-${type}`;
    fillMessageBox(statusBox, messages);
}

function hideStatus() {
    statusBox.hidden = true;
}

// ---------- Hücre oluşturucular (veri her zaman textContent ile yazılır) ----------
function createCell(text) {
    const td = document.createElement("td");
    td.textContent = text ?? "";
    return td;
}

function formatDate(isoDate) {
    if (!isoDate) return "";
    return new Date(isoDate).toLocaleDateString("tr-TR");
}

function getFullName(application) {
    const person = application.personInfo ?? {};
    return `${person.firstName ?? ""} ${person.lastName ?? ""}`.trim();
}

function createLinkItem(label, url) {
    if (!url) return null;

    // Sadece http(s) adresleri tıklanabilir yapılır
    if (/^https?:\/\//i.test(url)) {
        const a = document.createElement("a");
        a.href = url;
        a.textContent = label;
        a.target = "_blank";
        a.rel = "noopener noreferrer";
        return a;
    }

    const span = document.createElement("span");
    span.textContent = `${label}: ${url}`;
    return span;
}

function createLinksCell(application) {
    const td = document.createElement("td");

    [
        createLinkItem("LinkedIn", application.linkedinURL),
        createLinkItem("GitHub", application.githubURL)
    ]
        .filter(Boolean)
        .forEach(item => {
            const line = document.createElement("div");
            line.appendChild(item);
            td.appendChild(line);
        });

    return td;
}

function createStatusCell(status) {
    const td = document.createElement("td");
    const info = STATUS[status] ?? { text: "Bilinmiyor", badge: "bg-warning text-dark" };

    const badge = document.createElement("span");
    badge.className = `badge ${info.badge}`;
    badge.textContent = info.text;

    td.appendChild(badge);
    return td;
}

function createActionButton(actionKey, applicationId) {
    const action = ACTIONS[actionKey];

    const button = document.createElement("button");
    button.type = "button";
    button.className = `btn btn-sm ${action.buttonClass} me-1`;
    button.textContent = action.buttonText;
    button.dataset.action = actionKey;
    button.dataset.applicationId = applicationId;
    return button;
}

function createActionsCell(application) {
    const td = document.createElement("td");

    // Onay/ret butonları sadece "Yeni" başvurularda görünür
    if (application.status === STATUS_NEW) {
        td.appendChild(createActionButton("approve", application.applicationID));
        td.appendChild(createActionButton("reject", application.applicationID));
    }

    return td;
}

// ---------- Satır ve tablo ----------
function createRow(application) {
    const person = application.personInfo ?? {};
    const tr = document.createElement("tr");

    tr.appendChild(createCell(getFullName(application)));
    tr.appendChild(createCell(person.email));
    tr.appendChild(createCell(application.university));
    tr.appendChild(createCell(application.department));
    tr.appendChild(createCell(application.classYear));
    tr.appendChild(createCell(formatDate(application.applicationDate)));
    tr.appendChild(createLinksCell(application));
    tr.appendChild(createStatusCell(application.status));
    tr.appendChild(createActionsCell(application));

    return tr;
}

function renderApplications(applications) {
    tableBody.replaceChildren();
    applications.forEach(application => tableBody.appendChild(createRow(application)));
}

// ---------- Listeyi yükleme ----------
async function loadApplications() {
    tableWrapper.hidden = true;
    showStatus("info", ["Başvurular yükleniyor..."]);

    try {
        const applications = await getAllApplications();

        applicationsById = new Map(applications.map(a => [a.applicationID, a]));

        if (applications.length === 0) {
            showStatus("secondary", ["Henüz başvuru yok."]);
            return;
        }

        // En yeni başvuru en üstte
        const sorted = [...applications].sort(
            (a, b) => new Date(b.applicationDate) - new Date(a.applicationDate)
        );

        renderApplications(sorted);
        hideStatus();
        tableWrapper.hidden = false;
    } catch (error) {
        showStatus("danger", getErrorMessages(error));
    }
}

// ---------- Karar penceresi (modal) ----------
function showDecisionError(messages) {
    fillMessageBox(decisionError, messages);
}

function hideDecisionError() {
    decisionError.hidden = true;
}

function setDecisionBusy(isBusy) {
    decisionSubmit.disabled = isBusy;
    decisionNotes.disabled = isBusy;
}

function openDecisionModal(actionKey, applicationID) {
    const action = ACTIONS[actionKey];
    const application = applicationsById.get(applicationID);
    if (!action || !application) return;

    pendingDecision = { action, applicationID };

    decisionTitle.textContent = action.title;
    decisionSummary.textContent =
        `${getFullName(application)} adlı adayın başvurusu ${action.result}. Lütfen bir not girin.`;
    decisionNotes.value = "";
    hideDecisionError();
    setDecisionBusy(false);

    decisionSubmit.className = `btn ${action.buttonClass}`;
    decisionSubmit.textContent = action.buttonText;

    decisionModal.show();
}

async function submitDecision(event) {
    event.preventDefault();
    if (!pendingDecision) return;

    const notes = decisionNotes.value.trim();
    if (!notes) {
        showDecisionError(["Not alanı zorunludur."]);
        return;
    }

    const { action, applicationID } = pendingDecision;

    hideDecisionError();
    setDecisionBusy(true);

    try {
        await action.send(applicationID, action.status, notes);

        decisionModal.hide();
        showNotice("success", action.successMessage);
        await loadApplications();   // ekranda her zaman sunucudaki gerçek hal görünsün
    } catch (error) {
        const useCustomMessage =
            error instanceof ApiError && error.status >= 500 && action.serverErrorMessage;

        showDecisionError(useCustomMessage ? [action.serverErrorMessage] : getErrorMessages(error));

        // İstek sunucuya ulaşıp başarısız olduysa liste eskimiş olabilir (geri alma yapılmış olabilir)
        if (error instanceof ApiError && error.status > 0) {
            loadApplications();
        }
    } finally {
        setDecisionBusy(false);
    }
}

// ---------- Olay dinleyicileri ----------
// Satırlar her yüklemede yeniden çizildiği için tek bir dinleyici tablo gövdesinde durur
tableBody.addEventListener("click", (event) => {
    const button = event.target.closest("button[data-action]");
    if (!button) return;

    openDecisionModal(button.dataset.action, Number(button.dataset.applicationId));
});

decisionForm.addEventListener("submit", submitDecision);

decisionModalElement.addEventListener("hidden.bs.modal", () => {
    pendingDecision = null;
});

// Giriş yoksa renderStaffShell zaten yönlendirdi
if (session) {
    loadApplications();
}