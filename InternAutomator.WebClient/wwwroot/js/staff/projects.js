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
    footer.appendChild(detailsButton);

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

// ---------- Olay dinleyicileri ----------
projectList.addEventListener("click", (event) => {
    const button = event.target.closest("button[data-action=details]");
    if (!button) return;

    openDetails(Number(button.closest(".project-card").dataset.projectId));
});

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

// Giriş yoksa renderStaffShell zaten yönlendirdi
if (session) {
    loadProjects();
}