const API_BASE = "https://localhost:7042/api";

// API'dan gelen her hata bu tipte döner; sayfa script'i sadece mesajları gösterir
class ApiError extends Error {
    constructor(status, messages) {
        super(messages.join(" "));
        this.status = status;
        this.messages = messages;
    }
}

async function readErrorMessages(response) {
    if (response.status >= 500) {
        return ["Sunucuda bir hata oluştu. Lütfen daha sonra tekrar deneyin."];
    }

    const text = await response.text();

    try {
        const data = JSON.parse(text);
        if (data && data.errors) return Object.values(data.errors).flat();
        if (data && data.message) return [data.message];
        if (data && data.title) return [data.title];
    } catch {
    }

    return [text || `İstek reddedildi (kod: ${response.status}).`];
}

async function apiPost(path, body) {
    let response;

    try {
        response = await fetch(`${API_BASE}/${path}`, {
            method: "POST",
            headers: { "Content-Type": "Application/json" },
            body: JSON.stringify(body)
        });
    } catch {
        throw new ApiError(0, ["Sunucuya ulaşılamadı. Lütfen tekrar deneyin."]);
    }

    if (!response.ok) {
        throw new ApiError(response.status, await readErrorMessages(response));
    }

    return response.status === 204 ? null : await response.json();
}

// ---- Başvuru işlemleri ----
function submitApplication(application) {
    return apiPost("Application", application);


}


async function apiGet(path) {
    let response;

    try {
        response = await fetch(`${API_BASE}/${path}`);
    } catch {
        throw new ApiError(0, ["Sunucuya ulaşılamadı. Lütfen tekrar deneyin."]);
    }

    if (!response.ok) {
        throw new ApiError(response.status, await readErrorMessages(response));
    }

    return await response.json();
}

// Endpoint tablo boşken 404 döndüğü için burada "boş liste" olarak ele alınıyor
async function getAllApplications() {
    try {
        return await apiGet("Application/All");
    } catch (error) {
        if (error instanceof ApiError && error.status === 404) {
            return [];
        }
        throw error;
    }
}


async function sendJson(method, path, body) {
    let response;

    try {
        response = await fetch(`${API_BASE}/${path}`, {
            method: method,
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(body)
        });
    } catch {
        throw new ApiError(0, ["Sunucuya ulaşılamadı. Lütfen tekrar deneyin."]);
    }

    if (!response.ok) {
        throw new ApiError(response.status, await readErrorMessages(response));
    }

    // Gövde boş olabilir (örn. Ok() veya 204), boşsa null dön
    const text = await response.text();
    return text ? JSON.parse(text) : null;
}

function apiPut(path, body) {
    return sendJson("PUT", path, body);
}

// Başvurunun durumunu (onay/ret) ve değerlendirme notunu günceller
function updateApplicationStatus(applicationID, status, notes) {
    return apiPut("Application/Update/AppStatus", { applicationID, status, notes });
}

// Başvuruyu onaylar; API stajyer hesabını da oluşturur ve giriş bilgilerini döner
function acceptApplication(applicationID, status, notes) {
    return apiPut("Application/Update/AppStatus/accepted", { applicationID, status, notes });
}

// ---- Stajyer girişi ----
function internLogin(email, password) {
    return apiPost("Intern/InternLogin", { email, password });
}
// ---- Stajyer projeleri ----
async function getInternProjects(internID) {
    const projects = await apiGet(
        `OngoingProjects/InternProjects?internID=${encodeURIComponent(internID)}`
    );

    // API boş durumda dizi yerine metin döndürebiliyor; dizi değilse boş liste say
    return Array.isArray(projects) ? projects : [];
}


// ---- Personel girişi ----
function staffLogin(userName, password) {
    return apiPost("User/UserLogin", { userName, password });
}

// Boş tabloda da 200 ve [] döner; 404 gerçek bir hatadır (yol yanlış vb.)
async function getAllInterns() {
    return apiGet("Intern/All");
}
// Tek stajyerin bilgisi (detay sayfası başlığı için). Yoksa API 404 döner.
function getIntern(internID) {
    return apiGet(`Intern/${encodeURIComponent(internID)}`);
}
function separateIntern(internID) {
    return apiPut(`Intern/Separate/${encodeURIComponent(internID)}`);
}
// ---- Stajyer görevleri ----
async function getInternTasks(internID) {
    const tasks = await apiGet(`Tasks/AllInternTasks?internID=${encodeURIComponent(internID)}`);

    // Dizi değilse boş liste say
    return Array.isArray(tasks) ? tasks : [];
}


// ---- Proje kataloğu ----
// Boş tabloda da 200 ve [] döner
async function getAllProjects() {
    const projects = await apiGet("Projects/All");
    return Array.isArray(projects) ? projects : [];
}

// Projenin kaç stajyere atandığı (liste cevabında yok; detay penceresi açılınca tek tek istenir)
async function getProjectAssignedInternCount(projectID) {
    return Number(await apiGet(`Projects/AssignedInternCount?projectID=${encodeURIComponent(projectID)}`));
}

function createProject(project) {
    return apiPost("Projects", project);
}

// Gövdesiz DELETE isteği. Başarıda 204 döner; hata mesajları ApiError ile iletilir.
async function apiDelete(path) {
    let response;

    try {
        response = await fetch(`${API_BASE}/${path}`, { method: "DELETE" });
    } catch {
        throw new ApiError(0, ["Sunucuya ulaşılamadı. Lütfen tekrar deneyin."]);
    }

    if (!response.ok) {
        throw new ApiError(response.status, await readErrorMessages(response));
    }
}

// Projeyi siler. Atanmış proje kuralını API uygular; reddederse 400 ve mesaj döner.
function deleteProject(projectID) {
    return apiDelete(`Projects/Delete/${projectID}`);
}

// Projenin adını, açıklamasını ve teknolojilerini günceller.
// Başarıda gövde boş gelebilir; sendJson bunu null olarak döner.
function updateProject(projectID, project) {
    return apiPut(`Projects/Update/${projectID}`, project);
}
// Stajyere mevcut bir projeyi atar. dueDate boş olabilir (null).
// Başlangıç tarihini API atandığı gün olarak belirler.
function assignProjectToIntern(internID, projectID, dueDate) {
    return apiPost("OngoingProjects/Add", { internID, projectID, dueDate });
}


// Stajyerin atanmış bir projesi için görev ekler. dueDate "yyyy-MM-ddTHH:mm:ss" biçimindedir.
function addTask(task) {
    return apiPost("Tasks/Add", task);
}
// Görevi tamamlandı olarak işaretler. Başarıda API true döner.
function completeTask(taskID) {
    return apiPut(`Tasks/Complete/${encodeURIComponent(taskID)}`);
}