// Stajyer arayüzünün ortak iskeleti. Menü TEK yerde tanımlıdır.
// Bir sayfa hazır olunca ilgili satıra sadece adresini (href) yazman yeterli.
const INTERN_MENU = [
    { key: "home", text: "Ana sayfa", href: "/Intern/home.html" },
    { key: "announcements", text: "Duyurular", href: null },
    { key: "courses", text: "Kurslarım", href: null },
    { key: "projects", text: "Projelerim", href: "/Intern/projects.html" }, 
    { key: "messages", text: "Mesaj", href: null },
    { key: "faq", text: "SSS", href: null }
];

function createMenuItem(item, activeKey) {
    const li = document.createElement("li");
    li.className = "nav-item";

    if (item.href) {
        const link = document.createElement("a");
        link.className = "nav-link" + (item.key === activeKey ? " active" : "");
        link.href = item.href;
        link.textContent = item.text;
        li.appendChild(link);
        return li;
    }

    // Henüz hazır olmayan bölüm: tıklanamaz, soluk gösterilir
    const placeholder = document.createElement("span");
    placeholder.className = "nav-link disabled d-flex justify-content-between align-items-center";
    placeholder.setAttribute("aria-disabled", "true");

    const label = document.createElement("span");
    label.textContent = item.text;

    const badge = document.createElement("span");
    badge.className = "badge text-bg-light border";
    badge.textContent = "Yakında";

    placeholder.append(label, badge);
    li.appendChild(placeholder);
    return li;
}

function renderHeader(session) {
    const header = document.getElementById("shellHeader");
    header.className = "navbar bg-light border-bottom px-3";

    // Sabit şablon (içinde veri yok); ad aşağıda textContent ile yazılır
    header.innerHTML = `
        <div class="d-flex align-items-center">
            <button id="sidebarToggle" type="button" class="btn btn-outline-secondary btn-sm me-3"
                    aria-label="Menüyü aç/kapat">&#9776;</button>
            <span class="navbar-brand mb-0 h1">Staj Otomasyonu</span>
        </div>
        <div>
            <span id="shellUserName" class="me-3"></span>
            <button id="shellLogout" type="button" class="btn btn-outline-secondary btn-sm">Çıkış</button>
        </div>`;

    document.getElementById("shellUserName").textContent =
        `${session.firstName} ${session.lastName}`.trim();

    document.getElementById("shellLogout").addEventListener("click", () => {
        clearSession();
        window.location.replace("/Intern/login.html");
    });
}

function renderSidebar(activeKey) {
    const sidebar = document.getElementById("shellSidebar");
    sidebar.className = "shell-sidebar border-end bg-white";

    const list = document.createElement("ul");
    list.className = "nav nav-pills flex-column p-3 gap-1";
    INTERN_MENU.forEach(item => list.appendChild(createMenuItem(item, activeKey)));

    sidebar.replaceChildren(list);
}

function initSidebarToggle() {
    // Dar ekranlarda menü kapalı başlar
    if (window.innerWidth < 768) {
        document.body.classList.add("sidebar-collapsed");
    }

    document.getElementById("sidebarToggle").addEventListener("click", () => {
        document.body.classList.toggle("sidebar-collapsed");
    });
}

// Her stajyer sayfası bunu çağırır. Giriş yoksa login'e yönlendirir ve null döner.
function renderInternShell(activeKey) {
    const session = requireLogin(ROLE_INTERN, "/Intern/login.html");
    if (!session) return null;

    renderHeader(session);
    renderSidebar(activeKey);
    initSidebarToggle();

    return session;
}