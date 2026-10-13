// Ortak iskelet: üst çubuk + sol menü. Menüler TEK yerde tanımlıdır.
// Bir sayfa hazır olunca ilgili satıra sadece adresini (href) yazman yeterli.
const INTERN_MENU = [
    { key: "home", text: "Ana sayfa", href: "/Intern/home.html" },
    { key: "announcements", text: "Duyurular", href: null },
    { key: "courses", text: "Kurslarım", href: null },
    { key: "projects", text: "Projelerim", href: "/Intern/projects.html" },
    { key: "messages", text: "Mesaj", href: null },
    { key: "faq", text: "SSS", href: null }
];

const STAFF_MENU = [
    { key: "applications", text: "Başvurular", href: "/Staff/applications.html" },
    { key: "interns", text: "Stajyerler", href: null },
    { key: "projects", text: "Projeler", href: null },
    { key: "announcements", text: "Duyurular", href: null },
    { key: "courses", text: "Kurslar", href: null },
    { key: "messages", text: "Mesajlar", href: null }
];

// Rol -> giriş sayfası ve menü
const SHELL_CONFIG = {
    [ROLE_INTERN]: { loginPage: "/Intern/login.html", menu: INTERN_MENU },
    [ROLE_STAFF]: { loginPage: "/Staff/login.html", menu: STAFF_MENU }
};

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

function renderHeader(session, loginPage) {
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
        window.location.replace(loginPage);
    });
}

function renderSidebar(menu, activeKey) {
    const sidebar = document.getElementById("shellSidebar");
    sidebar.className = "shell-sidebar border-end bg-white";

    const list = document.createElement("ul");
    list.className = "nav nav-pills flex-column p-3 gap-1";
    menu.forEach(item => list.appendChild(createMenuItem(item, activeKey)));

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

// Role göre iskeleti çizer. Giriş yoksa (ya da rol uymuyorsa) login'e yönlendirir ve null döner.
function renderShell(role, activeKey) {
    const config = SHELL_CONFIG[role];

    const session = requireLogin(role, config.loginPage);
    if (!session) return null;

    renderHeader(session, config.loginPage);
    renderSidebar(config.menu, activeKey);
    initSidebarToggle();

    return session;
}

// Sayfaların kullandığı kısa yollar
function renderInternShell(activeKey) {
    return renderShell(ROLE_INTERN, activeKey);
}

function renderStaffShell(activeKey) {
    return renderShell(ROLE_STAFF, activeKey);
}