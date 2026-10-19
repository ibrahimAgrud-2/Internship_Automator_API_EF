const session = renderStaffShell("announcements");

const statusBox = document.getElementById("statusBox");
const announcementList = document.getElementById("announcementList");
const resultCount = document.getElementById("resultCount");


const noticeBox = document.getElementById("noticeBox");

// Yeni duyuru penceresi
const addAnnouncementButton = document.getElementById("addAnnouncementButton");
const modalElement = document.getElementById("announcementModal");
const modal = new bootstrap.Modal(modalElement);
const form = document.getElementById("announcementForm");
const titleInput = document.getElementById("announcementTitleInput");
const contentInput = document.getElementById("announcementContentInput");
const submitButton = document.getElementById("announcementSubmit");
const cancelButton = document.getElementById("announcementCancel");
const errorBox = document.getElementById("announcementError");

titleInput.maxLength = ANNOUNCEMENT_LIMITS.title;
contentInput.maxLength = ANNOUNCEMENT_LIMITS.content;

// Renk sayısı announcements.css içindeki announcement-color-N sınıflarıyla aynı olmalı
const ANNOUNCEMENT_COLOR_COUNT = 5;

// ---------- Kart oluşturucu (veri her zaman textContent ile yazılır) ----------
function createAnnouncementCard(announcement, index) {
    const card = document.createElement("div");
    card.className = `card announcement-card announcement-color-${index % ANNOUNCEMENT_COLOR_COUNT}`;

    // Renkli şerit: tarihin ne olduğunu açıkça yazar
    const band = document.createElement("div");
    band.className = "announcement-band";
    band.textContent = `Yayın tarihi: ${formatDateTime(announcement.createdDate, "-")}`;

    const body = document.createElement("div");
    body.className = "card-body";

    const title = document.createElement("h5");
    title.className = "card-title text-break mb-3";
    title.textContent = announcement.title;

    const content = document.createElement("p");
    content.className = "card-text text-break announcement-content mb-0";
    content.textContent = announcement.content;

    body.append(title, content);

    // Yayınlayan adı API'dan geliyorsa en altta gösterilirloadAnnouncements
    if (announcement.createdByUserName) {
        const publisher = document.createElement("div");
        publisher.className = "small text-muted mt-3";
        publisher.textContent = `Yayınlayan: ${announcement.createdByUserName}`;
        body.appendChild(publisher);
    }

    card.append(band, body);
    return card;
}
// ---------- Sayfa akışı ----------
async function loadAnnouncements() {
    announcementList.replaceChildren();
    resultCount.textContent = "";
    showMessage(statusBox, "info", "Duyurular yükleniyor...");

    try {
        const announcements = await getAllAnnouncements();

        if (announcements.length === 0) {
            showMessage(statusBox, "secondary", "Henüz duyuru yok.");
            return;
        }

        hideMessage(statusBox);
        // Sıralama API'dan geldiği gibi gösterilir (en yeni başta)
        announcements.forEach((item, index) => announcementList.appendChild(createAnnouncementCard(item, index)));        resultCount.textContent = `${announcements.length} duyuru`;
    } catch (error) {
        showError(statusBox, error);
    }
}

// ---------- Yeni duyuru penceresi ----------
function setBusy(isBusy) {
    titleInput.disabled = isBusy;
    contentInput.disabled = isBusy;
    cancelButton.disabled = isBusy;
    submitButton.disabled = isBusy;
}

function openModal() {
    form.reset();
    hideMessage(errorBox);
    setBusy(false);
    modal.show();
}

async function submitAnnouncement(event) {
    event.preventDefault();

    // İstek sürerken ikinci gönderim (Enter'a art arda basma vb.) yapılmasın
    if (submitButton.disabled) return;

    const title = titleInput.value.trim();
    const content = contentInput.value.trim();

    if (!title) {
        showMessage(errorBox, "danger", "Başlık zorunludur.");
        titleInput.focus();
        return;
    }
    if (!content) {
        showMessage(errorBox, "danger", "Duyuru metni zorunludur.");
        contentInput.focus();
        return;
    }

    // Oturumda kullanıcı numarası yoksa istek hiç atılmaz
    if (!session.userID) {
        showMessage(errorBox, "danger", "Oturum bilgisi eksik. Lütfen çıkış yapıp tekrar giriş yapın.");
        return;
    }

    hideMessage(errorBox);
    setBusy(true);

    try {
        // Yayın tarihini API belirler; istemci göndermez
        await createAnnouncement({
            createdByUserID: session.userID,
            title,
            content
        });

        modal.hide();
        showNotice(noticeBox, "success", `"${title}" duyurusu yayınlandı.`);
        await loadAnnouncements();   // ekranda her zaman sunucudaki gerçek hal görünsün
    } catch (error) {
        showError(errorBox, error);
    } finally {
        setBusy(false);
    }
}

addAnnouncementButton.addEventListener("click", openModal);
form.addEventListener("submit", submitAnnouncement);

modalElement.addEventListener("shown.bs.modal", () => {
    titleInput.focus();
});

// Giriş yoksa renderStaffShell zaten yönlendirdi
if (session) {
    loadAnnouncements();
}