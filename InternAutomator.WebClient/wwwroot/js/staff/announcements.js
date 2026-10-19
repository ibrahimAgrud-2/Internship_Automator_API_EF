const session = renderStaffShell("announcements");

const statusBox = document.getElementById("statusBox");
const announcementList = document.getElementById("announcementList");
const resultCount = document.getElementById("resultCount");
const noticeBox = document.getElementById("noticeBox");

const addAnnouncementButton = document.getElementById("addAnnouncementButton");
const modalElement = document.getElementById("announcementModal");
const modal = new bootstrap.Modal(modalElement);
const modalTitle = document.getElementById("announcementModalTitle");
const form = document.getElementById("announcementForm");
const titleInput = document.getElementById("announcementTitleInput");
const contentInput = document.getElementById("announcementContentInput");
const submitButton = document.getElementById("announcementSubmit");
const cancelButton = document.getElementById("announcementCancel");
const errorBox = document.getElementById("announcementError");


const deleteModalElement = document.getElementById("deleteAnnouncementModal");
const deleteModal = new bootstrap.Modal(deleteModalElement);
const deleteName = document.getElementById("deleteAnnouncementName");
const deleteError = document.getElementById("deleteAnnouncementError");
const deleteConfirm = document.getElementById("deleteAnnouncementConfirm");
const deleteCancel = document.getElementById("deleteAnnouncementCancel");

titleInput.maxLength = ANNOUNCEMENT_LIMITS.title;
contentInput.maxLength = ANNOUNCEMENT_LIMITS.content;

let announcementsById = new Map();

const ANNOUNCEMENT_COLOR_COUNT = 5;
function createAnnouncementCard(announcement, index) {
    const card = document.createElement("div");
    card.className = `card announcement-card announcement-color-${index % ANNOUNCEMENT_COLOR_COUNT}`;

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

    const footer = document.createElement("div");
    footer.className = "d-flex align-items-center justify-content-between gap-3 mt-3";

    const publisher = document.createElement("span");
    publisher.className = "small text-muted";
    if (announcement.createdByUserName) {
        publisher.textContent = `Yayınlayan: ${announcement.createdByUserName}`;
    }
    footer.appendChild(publisher);

    if (announcement.announcementID) {
        const actions = document.createElement("div");
        actions.className = "d-flex gap-2";

        const editButton = document.createElement("button");
        editButton.type = "button";
        editButton.className = "btn btn-sm btn-outline-primary";
        editButton.textContent = "Düzenle";
        editButton.dataset.action = "edit";
        editButton.dataset.announcementId = announcement.announcementID;

        const deleteButton = document.createElement("button");
        deleteButton.type = "button";
        deleteButton.className = "btn btn-sm btn-outline-danger";
        deleteButton.textContent = "Sil";
        deleteButton.dataset.action = "delete";
        deleteButton.dataset.announcementId = announcement.announcementID;

        actions.append(editButton, deleteButton);
        footer.appendChild(actions);
    }

    body.appendChild(footer);

    card.append(band, body);
    return card;
}

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
        announcementsById = new Map(announcements.map(item => [item.announcementID, item]));
        // Sıralama API'dan geldiği gibi gösterilir (en yeni başta)
        announcements.forEach((item, index) => announcementList.appendChild(createAnnouncementCard(item, index)));
        resultCount.textContent = `${announcements.length} duyuru`;
    } catch (error) {
        showError(statusBox, error);
    }
}


let editingAnnouncementID = null;

function setBusy(isBusy) {
    titleInput.disabled = isBusy;
    contentInput.disabled = isBusy;
    cancelButton.disabled = isBusy;
    submitButton.disabled = isBusy;
}

function openModal() {
    editingAnnouncementID = null;
    modalTitle.textContent = "Yeni Duyuru";
    submitButton.textContent = "Yayınla";
    form.reset();
    hideMessage(errorBox);
    setBusy(false);
    modal.show();
}

function openEditModal(announcementID) {
    const announcement = announcementsById.get(announcementID);
    if (!announcement) return;

    editingAnnouncementID = announcementID;
    modalTitle.textContent = "Duyuruyu Düzenle";
    submitButton.textContent = "Kaydet";
    form.reset();
    hideMessage(errorBox);
    setBusy(false);

    titleInput.value = (announcement.title ?? "").trim();
    contentInput.value = (announcement.content ?? "").trim();

    modal.show();
}

async function submitUpdate(title, content) {
    const announcement = announcementsById.get(editingAnnouncementID);

    // Hiçbir şey değişmediyse sunucuya gitmeye gerek yok
    if (announcement
        && title === (announcement.title ?? "").trim()
        && content === (announcement.content ?? "").trim()) {
        modal.hide();
        return;
    }

    hideMessage(errorBox);
    setBusy(true);

    try {
        // Güncelleme tarihini API belirler; istemci göndermez
        await updateAnnouncement(editingAnnouncementID, { title, content });

        modal.hide();
        showNotice(noticeBox, "success", `"${title}" duyurusu güncellendi.`);
        await loadAnnouncements();
    } catch (error) {
        showError(errorBox, error);

        // Duyuru artık yoksa ekrandaki liste eskimiş demektir; arkada tazelenir
        if (error instanceof ApiError && error.status === 404) {
            loadAnnouncements();
        }
    } finally {
        setBusy(false);
    }
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

    // Düzenleme modunda yayınlayan ve yayın tarihi değişmez; sadece başlık ve metin güncellenir
    if (editingAnnouncementID !== null) {
        await submitUpdate(title, content);
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

// ---------- Silme penceresi ----------
let announcementToDeleteID = null;

function openDeleteModal(announcementID) {
    const announcement = announcementsById.get(announcementID);
    if (!announcement) return;

    announcementToDeleteID = announcementID;
    deleteName.textContent = announcement.title;
    hideMessage(deleteError);
    deleteConfirm.disabled = false;
    deleteCancel.disabled = false;

    deleteModal.show();
}

async function confirmDelete() {
    if (deleteConfirm.disabled || announcementToDeleteID === null) return;   // çift tıklama koruması

    const title = announcementsById.get(announcementToDeleteID)?.title ?? "";
    hideMessage(deleteError);
    deleteConfirm.disabled = true;
    deleteCancel.disabled = true;

    try {
        await deleteAnnouncement(announcementToDeleteID);

        deleteModal.hide();
        showNotice(noticeBox, "success", `"${title}" duyurusu silindi.`);
        await loadAnnouncements();
    } catch (error) {
        showError(deleteError, error);

        // Duyuru zaten yoksa ekrandaki liste eskimiş demektir; arkada tazelenir
        if (error instanceof ApiError && error.status === 404) {
            loadAnnouncements();
        }
    } finally {
        deleteConfirm.disabled = false;
        deleteCancel.disabled = false;
    }
}

announcementList.addEventListener("click", (event) => {
    const button = event.target.closest("button[data-action]");
    if (!button) return;

    const announcementID = Number(button.dataset.announcementId);

    if (button.dataset.action === "edit") {
        openEditModal(announcementID);
    } else if (button.dataset.action === "delete") {
        openDeleteModal(announcementID);
    }
});

deleteConfirm.addEventListener("click", confirmDelete);

// Giriş yoksa renderStaffShell zaten yönlendirdi
if (session) {
    loadAnnouncements();
}