const session = renderInternShell("projects");

const statusBox = document.getElementById("statusBox");
const projectList = document.getElementById("projectList");

// ---------- Mesaj kutusu ----------
function showStatus(type, messages) {
    statusBox.className = `alert alert-${type}`;
    statusBox.replaceChildren();

    messages.forEach(text => {
        const line = document.createElement("div");
        line.textContent = text;
        statusBox.appendChild(line);
    });

    statusBox.hidden = false;
}

function hideStatus() {
    statusBox.hidden = true;
}

// ---------- Kart oluşturucular (veri her zaman textContent ile yazılır) ----------
function formatDate(isoDate) {
    return isoDate ? new Date(isoDate).toLocaleDateString("tr-TR") : "-";
}

// "C#, EF Core, ASP.NET" -> ayrı ayrı etiketler
function createTechnologyBadges(technologies) {
    const wrapper = document.createElement("div");
    wrapper.className = "d-flex flex-wrap gap-1 mb-3";

    (technologies ?? "")
        .split(",")
        .map(text => text.trim())
        .filter(Boolean)
        .forEach(text => {
            const badge = document.createElement("span");
            badge.className = "badge text-bg-secondary";
            badge.textContent = text;
            wrapper.appendChild(badge);
        });

    return wrapper;
}

function createInfoLine(label, value) {
    const line = document.createElement("div");
    line.className = "small text-muted";
    line.textContent = `${label}: ${value}`;
    return line;
}

function createProjectCard(project) {
    const col = document.createElement("div");
    col.className = "col-md-6 col-xl-4";

    const card = document.createElement("div");
    card.className = "card h-100";

    const body = document.createElement("div");
    body.className = "card-body";

    const title = document.createElement("h5");
    title.className = "card-title";
    title.textContent = project.projectTitle;

    body.append(
        title,
        createTechnologyBadges(project.technologies),
        createInfoLine("Atanma tarihi", formatDate(project.startDate)),
        createInfoLine("Teslim tarihi", formatDate(project.dueDate))
    );

    card.appendChild(body);
    col.appendChild(card);
    return col;
}

// ---------- Sayfa akışı ----------
async function loadProjects() {
    projectList.replaceChildren();
    showStatus("info", ["Projeler yükleniyor..."]);

    try {
        const projects = await getInternProjects(session.internID);

        if (projects.length === 0) {
            showStatus("secondary", ["Henüz size atanmış bir proje yok."]);
            return;
        }

        hideStatus();
        projects.forEach(project => projectList.appendChild(createProjectCard(project)));
    } catch (error) {
        const messages = error instanceof ApiError
            ? error.messages
            : ["Beklenmeyen bir hata oluştu."];
        showStatus("danger", messages);
    }
}

// Giriş yoksa renderInternShell zaten yönlendirdi
if (session) {
    loadProjects();
}