// ui.js: ekranda gösterme işlerinin ortak araçları.
// Sunucuyu (api.js) ve oturumu (auth.js) bilmez, sadece gelen veriyi ekrana yazar.
// Kural: sunucudan ya da kullanıcıdan gelen metin her zaman textContent ile yazılır.

// "2026-10-06T07:37:06" -> "06.10.2026". Tarih boşsa fallback döner.
function formatDate(isoDate, fallback = "") {
    return isoDate ? new Date(isoDate).toLocaleDateString("tr-TR") : fallback;
}

// Bir hatadan kullanıcıya gösterilecek mesaj listesi
function getErrorMessages(error) {
    return error instanceof ApiError
        ? error.messages
        : ["Beklenmeyen bir hata oluştu."];
}

// Mesaj kutusunu doldurur ve gösterir.
// type: "success" | "danger" | "info" | "secondary" ...  messages: metin ya da metin dizisi
function showMessage(box, type, messages) {
    const lines = Array.isArray(messages) ? messages : [messages];

    // Önceki alert-* sınıflarını temizle; kutunun diğer sınıfları (mt-3 gibi) kalır
    [...box.classList]
        .filter(name => name.startsWith("alert"))
        .forEach(name => box.classList.remove(name));
    box.classList.add("alert", `alert-${type}`);

    box.replaceChildren(...lines.map(text => {
        const line = document.createElement("div");
        line.textContent = text;
        return line;
    }));

    box.hidden = false;
}

function hideMessage(box) {
    box.hidden = true;
    box.replaceChildren();
}

// Hata nesnesini kırmızı uyarı olarak gösterir
function showError(box, error) {
    showMessage(box, "danger", getErrorMessages(error));
}

// Birkaç saniye görünüp kendiliğinden kaybolan bildirim
const noticeTimers = new WeakMap();

function showNotice(box, type, text, durationMs = 6000) {
    showMessage(box, type, text);

    clearTimeout(noticeTimers.get(box));
    noticeTimers.set(box, setTimeout(() => hideMessage(box), durationMs));
}

// Tablo hücresi (td)
function createCell(text) {
    const td = document.createElement("td");
    td.textContent = text ?? "";
    return td;
}
// "C#, EF Core, ASP.NET" -> ayrı ayrı etiketler
function createTechnologyBadges(technologies) {
    const wrapper = document.createElement("div");
    wrapper.className = "d-flex flex-wrap gap-1";

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

// ---- Stajyer durumu (API: status 1 = aktif, 0 = ayrıldı) ----
const INTERN_ACTIVE = 1;
const INTERN_STATUS = {
    1: { text: "Aktif", badge: "text-bg-success" },
    0: { text: "Ayrıldı", badge: "text-bg-secondary" }
};

function createInternStatusBadge(status) {
    const info = INTERN_STATUS[status] ?? { text: "Bilinmiyor", badge: "text-bg-light border" };

    const badge = document.createElement("span");
    badge.className = `badge ${info.badge}`;
    badge.textContent = info.text;
    return badge;
}