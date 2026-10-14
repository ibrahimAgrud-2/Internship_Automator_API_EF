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

function createProjectItem(project) {
    const box = createItemBox();

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

    return box;
}

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
    items.forEach(item => list.appendChild(createItem(item)));
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
// Ekle işlemleri henüz yazılmadığı için butonlar şimdilik her durumda pasif kalır.
function updateAddButtons(intern) {
    const isActive = intern.status === INTERN_ACTIVE;
    const title = isActive ? "Yakında" : "Ayrılan stajyere ekleme yapılamaz";

    ["projectsAdd", "tasksAdd"].forEach(id => {
        const button = document.getElementById(id);
        button.disabled = true;   // işlem bağlanınca: button.disabled = !isActive
        button.title = title;
    });
}

// ---------- Sayfa akışı ----------
function showPageError(message) {
    detailSections.hidden = true;
    showMessage(pageStatus, "danger", message);
}

async function loadIntern() {
    try {
        const intern = await getIntern(internID);
        renderInternSummary(intern);
        updateAddButtons(intern);
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
    const [projectsResult, tasksResult] = await Promise.allSettled([
        getInternProjects(internID),
        getInternTasks(internID)
    ]);

    renderSection({
        result: projectsResult,
        statusBox: projectsStatus,
        list: document.getElementById("projectsList"),
        countBadge: document.getElementById("projectsCount"),
        emptyText: "Bu stajyere henüz proje atanmamış.",
        createItem: createProjectItem
    });

    // Görevlerde proje adını göstermek için proje listesinden eşleştirme tablosu
    const projectTitles = new Map(
        projectsResult.status === "fulfilled"
            ? projectsResult.value.map(project => [project.ongoingProjectID, project.projectTitle])
            : []
    );

    renderSection({
        result: tasksResult,
        statusBox: tasksStatus,
        list: document.getElementById("tasksList"),
        countBadge: document.getElementById("tasksCount"),
        emptyText: "Bu stajyere henüz görev verilmemiş.",
        createItem: task => createTaskItem(task, projectTitles)
    });
}

async function loadPage() {
    // Önce stajyerin var olduğundan emin ol; yoksa proje/görev istekleri hiç atılmaz
    const intern = await loadIntern();
    if (intern) {
        await loadDetails();
    }
}

// Giriş yoksa renderStaffShell zaten yönlendirdi
if (session) {
    if (isValidInternID) {
        loadPage();
    } else {
        showPageError("Geçersiz stajyer numarası. Lütfen listeden bir stajyer seçin.");
    }
}