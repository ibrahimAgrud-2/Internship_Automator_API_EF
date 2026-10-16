const session = renderStaffShell("announcements");

const statusBox = document.getElementById("statusBox");
const announcementList = document.getElementById("announcementList");
const resultCount = document.getElementById("resultCount");

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

// Giriş yoksa renderStaffShell zaten yönlendirdi
if (session) {
    loadAnnouncements();
}