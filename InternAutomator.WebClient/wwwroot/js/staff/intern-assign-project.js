// Stajyer detay sayfası: "Projeler > Ekle" ile açılan proje atama penceresi.
// intern-detail.js içindeki internID, internProjects ve loadDetails() burada kullanılır.

const assignAddButton = document.getElementById("projectsAdd");
const assignModalElement = document.getElementById("assignProjectModal");
const assignModal = new bootstrap.Modal(assignModalElement);
const assignTitle = document.getElementById("assignProjectTitle");
const assignError = document.getElementById("assignError");
const assignNoticeBox = document.getElementById("noticeBox");

// Liste görünümü
const assignListView = document.getElementById("assignListView");
const assignStatus = document.getElementById("assignStatus");
const assignList = document.getElementById("assignList");
const assignListFooter = document.getElementById("assignListFooter");
const assignDueDate = document.getElementById("assignDueDate");
const assignConfirm = document.getElementById("assignConfirm");
const assignCancel = document.getElementById("assignCancel");
const assignNewProject = document.getElementById("assignNewProject");

// Yeni proje görünümü
const assignCreateForm = document.getElementById("assignCreateForm");
const assignCreateFooter = document.getElementById("assignCreateFooter");
const assignTitleInput = document.getElementById("assignTitleInput");
const assignTechnologiesInput = document.getElementById("assignTechnologiesInput");
const assignDescriptionInput = document.getElementById("assignDescriptionInput");
const assignBack = document.getElementById("assignBack");
const assignCreateSubmit = document.getElementById("assignCreateSubmit");

assignTitleInput.maxLength = PROJECT_LIMITS.title;
assignTechnologiesInput.maxLength = PROJECT_LIMITS.technologies;
assignDescriptionInput.maxLength = PROJECT_LIMITS.description;

let assignableProjects = [];     // pencere açılınca API'dan gelen, henüz atanmamış projeler
let selectedProjectID = null;
let assignRequestId = 0;         // eski bir isteğin cevabı yeni açılan pencereyi bozmasın diye
let isAssigning = false;

// ---------- Görünüm değiştirme ----------
function showListView() {
    assignTitle.textContent = "Proje Ata";
    assignListView.hidden = false;
    assignListFooter.hidden = false;
    assignNewProject.hidden = false;
    assignCreateForm.hidden = true;
    assignCreateFooter.hidden = true;
    hideMessage(assignError);
}

function showCreateView() {
    assignTitle.textContent = "Yeni Proje";
    assignListView.hidden = true;
    assignListFooter.hidden = true;
    assignNewProject.hidden = true;
    assignCreateForm.hidden = false;
    assignCreateFooter.hidden = false;
    hideMessage(assignError);

    assignCreateForm.reset();
    assignTitleInput.focus();
}

function setAssignBusy(isBusy) {
    isAssigning = isBusy;
    assignConfirm.disabled = isBusy || selectedProjectID === null;
    assignCancel.disabled = isBusy;
    assignNewProject.disabled = isBusy;
    assignDueDate.disabled = isBusy;
}

// ---------- Liste ----------
function createAssignRow(project) {
    const row = document.createElement("button");
    row.type = "button";
    row.className = "list-group-item list-group-item-action d-flex align-items-center justify-content-between gap-3";
    row.dataset.projectId = project.projectID;
    row.setAttribute("aria-pressed", "false");

    const title = document.createElement("span");
    title.className = "fw-semibold text-break";
    title.textContent = project.title;

    const badges = createTechnologyBadges(project.technologies);
    badges.classList.add("assign-row-technologies");

    row.append(title, badges);
    return row;
}

function markSelectedRow() {
    assignList.querySelectorAll(".list-group-item").forEach(row => {
        const isSelected = Number(row.dataset.projectId) === selectedProjectID;
        row.classList.toggle("active", isSelected);
        row.setAttribute("aria-pressed", String(isSelected));
    });

    assignConfirm.disabled = isAssigning || selectedProjectID === null;
}

function renderAssignList() {
    assignList.replaceChildren();

    if (assignableProjects.length === 0) {
        showMessage(assignStatus, "secondary",
            "Atanabilecek proje yok. \"Yeni Proje Ekle\" ile yeni bir proje oluşturabilirsiniz.");
        return;
    }

    hideMessage(assignStatus);
    assignableProjects.forEach(project => assignList.appendChild(createAssignRow(project)));
    markSelectedRow();
}

// Proje kataloğunu API'dan alır; bu stajyere zaten atanmış olanlar listeden çıkarılır
async function loadAssignableProjects() {
    const requestId = ++assignRequestId;

    assignList.replaceChildren();
    showMessage(assignStatus, "info", "Projeler yükleniyor...");

    try {
        const projects = await getAllProjects();
        if (requestId !== assignRequestId) return;

        const assignedIDs = new Set(internProjects.map(project => project.projectID));
        assignableProjects = projects.filter(project => !assignedIDs.has(project.projectID));

        renderAssignList();
    } catch (error) {
        if (requestId !== assignRequestId) return;

        assignableProjects = [];
        showError(assignStatus, error);
    }
}

function openAssignModal() {
    selectedProjectID = null;
    assignDueDate.value = "";
    setAssignBusy(false);
    showListView();

    assignModal.show();
    loadAssignableProjects();
}

// ---------- Atama ----------
async function confirmAssign() {
    if (isAssigning || selectedProjectID === null) return;   // çift tıklama koruması

    const project = assignableProjects.find(item => item.projectID === selectedProjectID);
    hideMessage(assignError);
    setAssignBusy(true);

    try {
        await assignProjectToIntern(internID, selectedProjectID, assignDueDate.value || null);

        assignModal.hide();
        showNotice(assignNoticeBox, "success", `"${project?.title ?? "Proje"}" stajyere atandı.`);
        await loadDetails();
    } catch (error) {
        showError(assignError, error);
    } finally {
        setAssignBusy(false);
    }
}

// ---------- Yeni proje (pencere içinden) ----------
async function submitNewProject(event) {
    event.preventDefault();
    if (assignCreateSubmit.disabled) return;

    const title = assignTitleInput.value.trim();
    if (!title) {
        showMessage(assignError, "danger", "Proje adı zorunludur.");
        assignTitleInput.focus();
        return;
    }

    // Oturumda kullanıcı numarası yoksa istek hiç atılmaz
    if (!session.userID) {
        showMessage(assignError, "danger", "Oturum bilgisi eksik. Lütfen çıkış yapıp tekrar giriş yapın.");
        return;
    }

    hideMessage(assignError);
    assignCreateSubmit.disabled = true;
    assignBack.disabled = true;

    try {
        const created = await createProject({
            createdByUserID: session.userID,
            title,
            description: assignDescriptionInput.value.trim(),
            technologies: assignTechnologiesInput.value.trim()
        });

        // Yeni proje listede seçili gelir; kullanıcı sadece "Projeyi Ata"ya basar
        selectedProjectID = created?.projectID ?? null;
        showListView();
        await loadAssignableProjects();
        markSelectedRow();
    } catch (error) {
        showError(assignError, error);
    } finally {
        assignCreateSubmit.disabled = false;
        assignBack.disabled = false;
    }
}

// ---------- Olay dinleyicileri ----------
assignAddButton.addEventListener("click", openAssignModal);
assignNewProject.addEventListener("click", showCreateView);
assignBack.addEventListener("click", showListView);
assignConfirm.addEventListener("click", confirmAssign);
assignCreateForm.addEventListener("submit", submitNewProject);

assignList.addEventListener("click", (event) => {
    const row = event.target.closest(".list-group-item");
    if (!row || isAssigning) return;

    selectedProjectID = Number(row.dataset.projectId);
    markSelectedRow();
});