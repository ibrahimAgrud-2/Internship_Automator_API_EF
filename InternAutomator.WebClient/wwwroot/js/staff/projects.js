const session = renderStaffShell("projects");

const statusBox = document.getElementById("statusBox");
const projectList = document.getElementById("projectList");

// Arama
const searchInput = document.getElementById("searchInput");
const searchClear = document.getElementById("searchClear");
const resultCount = document.getElementById("resultCount");

// Detay penceresi
const projectModalElement = document.getElementById("projectModal");
const projectModal = new bootstrap.Modal(projectModalElement);
const projectModalTitle = document.getElementById("projectModalTitle");
const projectModalDescription = document.getElementById("projectModalDescription");
const projectModalTechnologies = document.getElementById("projectModalTechnologies");
const projectModalCreator = document.getElementById("projectModalCreator");
const projectModalCount = document.getElementById("projectModalCount");
const projectModalError = document.getElementById("projectModalError");

const noticeBox = document.getElementById("noticeBox");
const addProjectButton = document.getElementById("addProjectButton");
const createModalElement = document.getElementById("createProjectModal");
const createModal = new bootstrap.Modal(createModalElement);
const createForm = document.getElementById("createProjectForm");
const titleInput = document.getElementById("projectTitleInput");
const technologiesInput = document.getElementById("projectTechnologiesInput");
const descriptionInput = document.getElementById("projectDescriptionInput");
const createSubmit = document.getElementById("createProjectSubmit");
const createCancel = document.getElementById("createProjectCancel");
const createError = document.getElementById("createProjectError");
const createModalTitle = document.getElementById("createProjectTitle");


// Silme penceresi
const deleteModalElement = document.getElementById("deleteProjectModal");
const deleteModal = new bootstrap.Modal(deleteModalElement);
const deleteName = document.getElementById("deleteProjectName");
const deleteError = document.getElementById("deleteProjectError");
const deleteConfirm = document.getElementById("deleteProjectConfirm");
const deleteCancel = document.getElementById("deleteProjectCancel");


// Veritabanındaki sütun uzunluklarıyla aynı olmalı (şimdilik tahmin)
const PROJECT_LIMITS = { title: 100, technologies: 100, description: 200 };
titleInput.maxLength = PROJECT_LIMITS.title;
technologiesInput.maxLength = PROJECT_LIMITS.technologies;
descriptionInput.maxLength = PROJECT_LIMITS.description;

let projectsById = new Map();
let detailsRequestId = 0;   // eski bir isteğin cevabı yeni açılan pencereyi bozmasın diye

// ---------- Yardımcılar ----------
// Veritabanından açıklama sonunda boşluklarla dolu gelebiliyor; gösterirken kırpılır
function getDescription(project) {
    return (project.description ?? "").trim();
}

// ---------- Kart oluşturucular (veri her zaman textContent ile yazılır) ----------
function createProjectCard(project) {
    const card = document.createElement("div");
    card.className = "card project-card";
    card.dataset.projectId = project.projectID;

    const body = document.createElement("div");
    body.className = "card-body";

    const title = document.createElement("h5");
    title.className = "card-title text-break";
    title.textContent = project.title;

    const badges = createTechnologyBadges(project.technologies);
    badges.classList.add("mb-3");

    body.append(title, badges);

    const description = getDescription(project);
    if (description) {
        const text = document.createElement("p");
        text.className = "small text-muted text-break mb-0 project-description";
        text.textContent = description;
        body.appendChild(text);
    }

    const footer = document.createElement("div");
    footer.className = "card-footer bg-transparent border-0 pb-3";

    const detailsButton = document.createElement("button");
    detailsButton.type = "button";
    detailsButton.className = "btn btn-sm btn-outline-primary";
    detailsButton.dataset.action = "details";
    detailsButton.textContent = "Detaylar";
    const deleteButton = document.createElement("button");
    deleteButton.type = "button";
    deleteButton.className = "btn btn-sm btn-outline-danger ms-2";
    deleteButton.dataset.action = "delete";
    deleteButton.textContent = "Sil";

    const editButton = document.createElement("button");
    editButton.type = "button";
    editButton.className = "btn btn-sm btn-outline-secondary ms-2";
    editButton.dataset.action = "edit";
    editButton.textContent = "Düzenle";

    footer.append(detailsButton, editButton, deleteButton);

    card.append(body, footer);
    return card;
}

// ---------- Sayfa akışı ----------
async function loadProjects() {
    projectList.replaceChildren();
    resultCount.textContent = "";
    showMessage(statusBox, "info", "Projeler yükleniyor...");

    try {
        const projects = await getAllProjects();

        projectsById = new Map(projects.map(project => [project.projectID, project]));

        if (projects.length === 0) {
            searchInput.disabled = true;
            showMessage(statusBox, "secondary", "Henüz proje yok.");
            return;
        }

        hideMessage(statusBox);
        // Sıralama API'dan geldiği gibi gösterilir
        projects.forEach(project => projectList.appendChild(createProjectCard(project)));

        searchInput.disabled = false;
        applySearch();
    } catch (error) {
        searchInput.disabled = true;
        showError(statusBox, error);
    }
}

// ---------- Arama (proje adına göre) ----------
function applySearch() {
    const words = normalizeSearchText(searchInput.value).split(" ").filter(Boolean);
    const total = projectsById.size;
    let visible = 0;

    projectList.querySelectorAll(".project-card").forEach(card => {
        const project = projectsById.get(Number(card.dataset.projectId));
        const title = project ? normalizeSearchText(project.title) : "";
        const isMatch = words.every(word => title.includes(word));

        card.hidden = !isMatch;
        if (isMatch) visible++;
    });

    searchClear.disabled = searchInput.value === "";
    resultCount.textContent = words.length > 0 ? `${visible} / ${total} proje` : `${total} proje`;

    if (visible === 0 && total > 0) {
        showMessage(statusBox, "secondary", "Aramanızla eşleşen proje bulunamadı.");
    } else if (total > 0) {
        hideMessage(statusBox);
    }
}

// ---------- Detay penceresi ----------
async function openDetails(projectID) {
    const project = projectsById.get(projectID);
    if (!project) return;

    const requestId = ++detailsRequestId;

    projectModalTitle.textContent = project.title;
    projectModalDescription.textContent = getDescription(project) || "Açıklama yok.";
    projectModalTechnologies.replaceChildren(createTechnologyBadges(project.technologies));
    projectModalCreator.textContent = project.userName || "-";
    projectModalCount.textContent = "Yükleniyor...";
    hideMessage(projectModalError);

    projectModal.show();

    // Atanan stajyer sayısı listede gelmediği için pencere açılınca tek proje için istenir
    try {
        const count = await getProjectAssignedInternCount(projectID);
        if (requestId !== detailsRequestId) return;

        projectModalCount.textContent = String(count);
    } catch (error) {
        if (requestId !== detailsRequestId) return;

        projectModalCount.textContent = "-";
        showError(projectModalError, error);
    }
}

// ---------- Yeni proje penceresi ----------
function setCreateBusy(isBusy) {
    titleInput.disabled = isBusy;
    technologiesInput.disabled = isBusy;
    descriptionInput.disabled = isBusy;
    createCancel.disabled = isBusy;
    createSubmit.disabled = isBusy;
}

// Aynı form hem ekleme hem düzenleme için kullanılır.
// editingProjectID null ise "yeni proje", dolu ise "düzenleme" modudur.
let editingProjectID = null;

function openCreateModal() {
    editingProjectID = null;
    createModalTitle.textContent = "Yeni Proje";
    createSubmit.textContent = "Projeyi Ekle";
    createForm.reset();
    hideMessage(createError);
    setCreateBusy(false);

    createModal.show();
}

function openEditModal(projectID) {
    const project = projectsById.get(projectID);
    if (!project) return;

    editingProjectID = projectID;
    createModalTitle.textContent = "Projeyi Düzenle";
    createSubmit.textContent = "Kaydet";
    createForm.reset();
    hideMessage(createError);
    setCreateBusy(false);

    titleInput.value = project.title ?? "";
    technologiesInput.value = (project.technologies ?? "").trim();
    descriptionInput.value = getDescription(project);

    createModal.show();
}

async function submitUpdate(title) {
    const project = projectsById.get(editingProjectID);
    const changes = {
        title,
        description: descriptionInput.value.trim(),
        technologies: technologiesInput.value.trim()
    };

    // Hiçbir şey değişmediyse sunucuya gitmeye gerek yok
    if (project
        && changes.title === (project.title ?? "").trim()
        && changes.description === getDescription(project)
        && changes.technologies === (project.technologies ?? "").trim()) {
        createModal.hide();
        return;
    }

    hideMessage(createError);
    setCreateBusy(true);

    try {
        await updateProject(editingProjectID, changes);

        createModal.hide();
        showNotice(noticeBox, "success", `"${title}" projesi güncellendi.`);
        await loadProjects();
    } catch (error) {
        showError(createError, error);

        // Proje artık yoksa ekrandaki liste eskimiş demektir; arkada tazelenir
        if (error instanceof ApiError && error.status === 404) {
            loadProjects();
        }
    } finally {
        setCreateBusy(false);
    }
}

async function submitCreate(event) {
    event.preventDefault();

    // İstek sürerken ikinci gönderim (Enter'a art arda basma vb.) yapılmasın
    if (createSubmit.disabled) return;

    const title = titleInput.value.trim();
    if (!title) {
        showMessage(createError, "danger", "Proje adı zorunludur.");
        titleInput.focus();
        return;
    }

    // Düzenleme modunda oluşturan kullanıcı değişmez; sadece alanlar güncellenir
    if (editingProjectID !== null) {
        await submitUpdate(title);
        return;
    }

    // Oturumda kullanıcı numarası yoksa istek hiç atılmaz
    if (!session.userID) {
        showMessage(createError, "danger", "Oturum bilgisi eksik. Lütfen çıkış yapıp tekrar giriş yapın.");
        return;
    }

    hideMessage(createError);
    setCreateBusy(true);

    try {
        await createProject({
            createdByUserID: session.userID,
            title,
            description: descriptionInput.value.trim(),
            technologies: technologiesInput.value.trim()
        });

        createModal.hide();
        showNotice(noticeBox, "success", `"${title}" projesi eklendi.`);
        await loadProjects();   // ekranda her zaman sunucudaki gerçek hal görünsün
    } catch (error) {
        showError(createError, error);
    } finally {
        setCreateBusy(false);
    }
}
// ---------- Silme penceresi ----------
let projectToDeleteID = null;

function openDeleteModal(projectID) {
    const project = projectsById.get(projectID);
    if (!project) return;

    projectToDeleteID = projectID;
    deleteName.textContent = project.title;
    hideMessage(deleteError);
    deleteConfirm.disabled = false;
    deleteCancel.disabled = false;

    deleteModal.show();
}

async function confirmDelete() {
    if (deleteConfirm.disabled || projectToDeleteID === null) return;   // çift tıklama koruması

    const title = projectsById.get(projectToDeleteID)?.title ?? "";
    hideMessage(deleteError);
    deleteConfirm.disabled = true;
    deleteCancel.disabled = true;

    try {
        await deleteProject(projectToDeleteID);

        deleteModal.hide();
        showNotice(noticeBox, "success", `"${title}" projesi silindi.`);
        await loadProjects();
    } catch (error) {
        showError(deleteError, error);

        // Proje zaten yoksa ekrandaki liste eskimiş demektir; arkada tazelenir
        if (error instanceof ApiError && error.status === 404) {
            loadProjects();
        }
    } finally {
        deleteConfirm.disabled = false;
        deleteCancel.disabled = false;
    }
}
projectList.addEventListener("click", (event) => {
    const button = event.target.closest("button[data-action]");
    if (!button) return;

    const projectID = Number(button.closest(".project-card").dataset.projectId);

    if (button.dataset.action === "details") openDetails(projectID);
    if (button.dataset.action === "edit") openEditModal(projectID);
    if (button.dataset.action === "delete") openDeleteModal(projectID);
});

deleteConfirm.addEventListener("click", confirmDelete);

searchInput.addEventListener("input", applySearch);

searchInput.addEventListener("keydown", (event) => {
    if (event.key === "Escape") {
        searchInput.value = "";
        applySearch();
    }
});

searchClear.addEventListener("click", () => {
    searchInput.value = "";
    applySearch();
    searchInput.focus();
});


addProjectButton.addEventListener("click", openCreateModal);
createForm.addEventListener("submit", submitCreate);

createModalElement.addEventListener("shown.bs.modal", () => {
    titleInput.focus();
});

// Giriş yoksa renderStaffShell zaten yönlendirdi
if (session) {
    loadProjects();
}