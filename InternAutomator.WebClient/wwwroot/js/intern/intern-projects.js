const session = renderInternShell("projects");

const statusBox = document.getElementById("statusBox");
const projectList = document.getElementById("projectList");

// ---------- Kart oluşturucular (veri her zaman textContent ile yazılır) ----------

// "C#, EF Core, ASP.NET" -> ayrı ayrı etiketler
function createBadgesWithSpacing(technologies) {
    const badges = createTechnologyBadges(technologies);
    badges.classList.add("mb-3");
    return badges;
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
        createBadgesWithSpacing(project.technologies),
        createInfoLine("Atanma tarihi", formatDate(project.startDate, "-")),
        createInfoLine("Teslim tarihi", formatDate(project.dueDate, "-"))
    );

    card.appendChild(body);
    col.appendChild(card);
    return col;
}

// ---------- Sayfa akışı ----------
async function loadProjects() {
    projectList.replaceChildren();
    showMessage(statusBox, "info", "Projeler yükleniyor...");

    try {
        const projects = await getInternProjects(session.internID);

        if (projects.length === 0) {
            showMessage(statusBox, "secondary", "Henüz size atanmış bir proje yok.");
            return;
        }

        hideMessage(statusBox);
        projects.forEach(project => projectList.appendChild(createProjectCard(project)));
    } catch (error) {
        showError(statusBox, error);
    }
}

// Giriş yoksa renderInternShell zaten yönlendirdi
if (session) {
    loadProjects();
}