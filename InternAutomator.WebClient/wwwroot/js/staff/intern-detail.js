const session = renderStaffShell("interns");

// Stajyer numarası adres çubuğundan gelir: /Staff/intern.html?internID=4
const internID = Number(new URLSearchParams(window.location.search).get("internID"));
const isValidInternID = Number.isInteger(internID) && internID > 0;

const pageStatus = document.getElementById("pageStatus");
const detailSections = document.getElementById("detailSections");

// ---------- Öğe oluşturucular (veri her zaman textContent ile yazılır) ----------

function createItemBox() {
    const box = document.createElement("div");
    box.className = "border rounded p-3";
    return box;
}

function createInfoLine(text) {
    const line = document.createElement("div");
    line.className = "small text-muted";
    line.textContent = text;
    return line;
}

// Projeler 8 renk arasında döner; aynı proje hem solda hem gruplanmış görevlerde aynı rengi alır
const PROJECT_COLOR_COUNT = 8;

function getProjectColorClass(index) {
    return index >= 0 ? `project-color-${index % PROJECT_COLOR_COUNT}` : "project-color-other";
}

function createProjectItem(project, index) {
    const box = createItemBox();
    box.classList.add("project-item", getProjectColorClass(index));

    const title = document.createElement("div");
    title.className = "fw-semibold mb-2";
    title.textContent = project.projectTitle;

    const badges = createTechnologyBadges(project.technologies);
    badges.classList.add("mb-2");

    box.append(
        title,
        badges,
        createInfoLine(
            `Başlangıç: ${formatDate(project.startDate, "-")} · Teslim: ${formatDate(project.dueDate, "-")}`
        )
    );

    // Geri al butonu. Ayrılan stajyerde pasiftir (asıl engel API'dadır).
    const footer = document.createElement("div");
    footer.className = "mt-2 d-flex justify-content-end";

    const button = document.createElement("button");
    button.type = "button";
    button.className = "btn btn-sm btn-outline-danger";
    button.dataset.action = "revoke";
    button.dataset.ongoingProjectId = project.ongoingProjectID;
    button.textContent = "Geri Al";

    if (currentIntern && currentIntern.status !== INTERN_ACTIVE) {
        button.disabled = true;
        button.title = "Ayrılan stajyerden proje geri alınamaz";
    }

    footer.appendChild(button);
    box.appendChild(footer);
    return box;
}
function createTaskStatusBadge(completed) {
    const badge = document.createElement("span");
    badge.className = completed ? "badge text-bg-success" : "badge text-bg-warning";
    badge.textContent = completed ? "Tamamlandı" : "Devam ediyor";
    return badge;
}

// projectTitles: ongoingProjectID -> proje adı (görev cevabında proje adı yok)
function createTaskItem(task, projectTitles) {
    const box = createItemBox();

    const top = document.createElement("div");
    top.className = "d-flex align-items-start justify-content-between gap-2 mb-1";

    const title = document.createElement("div");
    title.className = "fw-semibold text-break";
    title.textContent = task.title;

    top.append(title, createTaskStatusBadge(task.completed));
    box.appendChild(top);

    const projectTitle = projectTitles.get(task.ongoingProjectID);
    if (projectTitle) {
        box.appendChild(createInfoLine(`Proje: ${projectTitle}`));
    }

    if (task.description) {
        const description = document.createElement("div");
        description.className = "small text-break my-2";
        description.textContent = task.description;
        box.appendChild(description);
    }

    let dates = `Atanma: ${formatDate(task.assignedDate, "-")} · Teslim: ${formatDate(task.dueDate, "-")}`;
    if (task.completed) {
        dates += ` · Tamamlanma: ${formatDate(task.completedDate, "-")}`;
    }
    box.appendChild(createInfoLine(dates));

    // İşlem butonları. Ayrılan stajyerde pasiftir (asıl engel API'dadır).
    const footer = document.createElement("div");
    footer.className = "mt-2 d-flex justify-content-end gap-2";

    const canChange = !currentIntern || currentIntern.status === INTERN_ACTIVE;

    function addTaskButton(action, className, text) {
        const button = document.createElement("button");
        button.type = "button";
        button.className = `btn btn-sm ${className}`;
        button.dataset.action = action;
        button.dataset.taskId = task.taskID;
        button.textContent = text;

        if (!canChange) {
            button.disabled = true;
            button.title = "Ayrılan stajyerin görevi değiştirilemez";
        }

        footer.appendChild(button);
    }

    // "Düzenle" ve "Tamamla" sadece tamamlanmamış görevde çıkar
    if (!task.completed) {
        addTaskButton("edit", "btn-outline-secondary", "Düzenle");
        addTaskButton("complete", "btn-outline-success", "Tamamla");
    }
    addTaskButton("delete", "btn-outline-danger", "Sil");

    box.appendChild(footer);
    return box;
}

// Stajyerin atanmış projeleri; "Proje Ata" penceresi bunları listeden eler
let internProjects = [];
let currentIntern = null;
let internTasks = [];
let lastTasksResult = null;      // son görev cevabı; gruplama değişince API'ya gitmeden yeniden çizilir
let groupTasksByProject = false;

// ---------- Bölüm çizimi ----------
// result: Promise.allSettled sonucu. Her bölüm birbirinden bağımsız hata gösterir.
function renderSection({ result, statusBox, list, countBadge, emptyText, createItem }) {
    list.replaceChildren();

    if (result.status === "rejected") {
        countBadge.textContent = "!";
        showError(statusBox, result.reason);
        return;
    }

    const items = result.value;
    countBadge.textContent = items.length;

    if (items.length === 0) {
        showMessage(statusBox, "secondary", emptyText);
        return;
    }

    hideMessage(statusBox);
    items.forEach((item, index) => list.appendChild(createItem(item, index)));
}

// ---------- Stajyer özeti ----------
function renderInternSummary(intern) {
    const fullName = `${intern.firstName} ${intern.lastName}`.trim();

    document.getElementById("internName").textContent = fullName;
    document.title = `${fullName} - Stajyer Detayı`;

    document.getElementById("internStatusBadge").replaceChildren(createInternStatusBadge(intern.status));

    document.getElementById("internContact").textContent =
        `${intern.email || "-"} · ${intern.phone || "-"}`;
    document.getElementById("internDates").textContent =
        `Başlangıç: ${formatDate(intern.startDate, "Belirlenmedi")} · ` +
        `Bitiş: ${formatDate(intern.endDate, "Belirlenmedi")}`;
}

// Ayrılan stajyere ekleme yapılamaz. Asıl engel API'dadır; burası sadece kullanıcıyı yönlendirir.
// Görev, stajyerin atanmış bir projesine verildiği için proje yoksa görev eklenemez.
function updateAddButtons() {
    if (!currentIntern) return;

    const isActive = currentIntern.status === INTERN_ACTIVE;
    const inactiveTitle = "Ayrılan stajyere ekleme yapılamaz";

    const projectsAdd = document.getElementById("projectsAdd");
    projectsAdd.disabled = !isActive;
    projectsAdd.title = isActive ? "" : inactiveTitle;

    const tasksAdd = document.getElementById("tasksAdd");
    const hasProjects = internProjects.length > 0;
    tasksAdd.disabled = !isActive || !hasProjects;
    tasksAdd.title = !isActive ? inactiveTitle : (hasProjects ? "" : "Görev vermek için önce stajyere proje atayın");
}

// ---------- Sayfa akışı ----------
function showPageError(message) {
    detailSections.hidden = true;
    showMessage(pageStatus, "danger", message);
}

async function loadIntern() {
    try {
        const intern = await getIntern(internID);
        currentIntern = intern;
        renderInternSummary(intern);
        updateAddButtons();
        return intern;
    } catch (error) {
        if (error instanceof ApiError && error.status === 404) {
            showPageError("Stajyer bulunamadı. Lütfen listeden bir stajyer seçin.");
        } else {
            showPageError(getErrorMessages(error).join(" "));
        }
        return null;
    }
}

async function loadDetails() {
    const projectsStatus = document.getElementById("projectsStatus");
    const tasksStatus = document.getElementById("tasksStatus");

    showMessage(projectsStatus, "info", "Projeler yükleniyor...");
    showMessage(tasksStatus, "info", "Görevler yükleniyor...");

    // İki istek birlikte atılır; biri başarısız olsa diğeri yine görünür
    const [projectsResult, tasksResultFromApi] = await Promise.allSettled([
        getInternProjects(internID),
        getInternTasks(internID)
    ]);

    internProjects = projectsResult.status === "fulfilled" ? projectsResult.value : [];
    updateAddButtons();

    renderSection({
        result: projectsResult,
        statusBox: projectsStatus,
        list: document.getElementById("projectsList"),
        countBadge: document.getElementById("projectsCount"),
        emptyText: "Bu stajyere henüz proje atanmamış.",
        createItem: createProjectItem
    });

    lastTasksResult = tasksResultFromApi;
    internTasks = tasksResultFromApi.status === "fulfilled" ? tasksResultFromApi.value : [];
    renderTasks();
}

// Görevleri düz liste ya da projeye göre gruplanmış çizer
function renderTasks() {
    const result = lastTasksResult;
    if (!result) return;

    const list = document.getElementById("tasksList");
    const statusBox = document.getElementById("tasksStatus");
    const projectTitles = new Map(internProjects.map(project => [project.ongoingProjectID, project.projectTitle]));
    const groupButton = document.getElementById("tasksGroup");
    const hasTasks = result.status === "fulfilled" && result.value.length > 0;

    // Gruplama düğmesi: görev yoksa anlamsız olduğu için pasif
    groupButton.disabled = !hasTasks;
    groupButton.setAttribute("aria-pressed", String(groupTasksByProject));
    groupButton.classList.toggle("btn-secondary", groupTasksByProject);
    groupButton.classList.toggle("btn-outline-secondary", !groupTasksByProject);
    document.getElementById("projectsList").classList.toggle("color-coded", groupTasksByProject);

    if (!groupTasksByProject || !hasTasks) {
        renderSection({
            result,
            statusBox,
            list,
            countBadge: document.getElementById("tasksCount"),
            emptyText: "Bu stajyere henüz görev verilmemiş.",
            createItem: task => createTaskItem(task, projectTitles)
        });
        return;
    }

    // Gruplu görünüm: sadece görevi olan projeler, proje listesindeki sırayla
    list.replaceChildren();
    hideMessage(statusBox);
    document.getElementById("tasksCount").textContent = result.value.length;

    const groups = new Map();
    result.value.forEach(task => {
        if (!groups.has(task.ongoingProjectID)) groups.set(task.ongoingProjectID, []);
        groups.get(task.ongoingProjectID).push(task);
    });

    const orderedIDs = [
        ...internProjects.map(project => project.ongoingProjectID).filter(id => groups.has(id)),
        ...[...groups.keys()].filter(id => !internProjects.some(project => project.ongoingProjectID === id))
    ];

    orderedIDs.forEach(ongoingProjectID => {
        const tasks = groups.get(ongoingProjectID);
        const colorIndex = internProjects.findIndex(project => project.ongoingProjectID === ongoingProjectID);

        const group = document.createElement("div");
        group.className = `task-group ${getProjectColorClass(colorIndex)}`;

        const header = document.createElement("div");
        header.className = "d-flex align-items-center gap-2 mb-2";

        const dot = document.createElement("span");
        dot.className = "task-group-dot";

        const name = document.createElement("span");
        name.className = "fw-semibold text-break";
        name.textContent = projectTitles.get(ongoingProjectID) ?? "Bilinmeyen proje";

        const count = document.createElement("span");
        count.className = "badge text-bg-secondary";
        count.textContent = tasks.length;

        header.append(dot, name, count);

        const items = document.createElement("div");
        items.className = "d-flex flex-column gap-3";
        tasks.forEach(task => items.appendChild(createTaskItem(task, projectTitles)));

        group.append(header, items);
        list.appendChild(group);
    });
}

async function loadPage() {
    // Önce stajyerin var olduğundan emin ol; yoksa proje/görev istekleri hiç atılmaz
    const intern = await loadIntern();
    if (intern) {
        await loadDetails();
    }
}

// "Projeye göre" düğmesi: açık/kapalı geçiş yapar, API'ya gitmeden mevcut görevleri yeniden çizer
document.getElementById("tasksGroup").addEventListener("click", () => {
    groupTasksByProject = !groupTasksByProject;
    renderTasks();
});

// Giriş yoksa renderStaffShell zaten yönlendirdi
if (session) {
    if (isValidInternID) {
        loadPage();
    } else {
        showPageError("Geçersiz stajyer numarası. Lütfen listeden bir stajyer seçin.");
    }
}