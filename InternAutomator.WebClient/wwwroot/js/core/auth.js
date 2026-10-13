// Oturum bilgisi tek yerde tutulur; sayfalar sessionStorage'a doğrudan dokunmaz.
// NOT: Token olmadığı için bu bilgi sadece arayüz içindir, güvenlik sağlamaz.
const SESSION_KEY = "iaa.session";
const ROLE_INTERN = "intern";
const ROLE_STAFF = "staff";
function saveSession(session) {
    sessionStorage.setItem(SESSION_KEY, JSON.stringify(session));
}

function getSession() {
    const raw = sessionStorage.getItem(SESSION_KEY);
    if (!raw) return null;

    try {
        return JSON.parse(raw);
    } catch {
        sessionStorage.removeItem(SESSION_KEY);
        return null;
    }
}

function clearSession() {
    sessionStorage.removeItem(SESSION_KEY);
}

// Giriş yoksa (ya da rol uymuyorsa) login sayfasına yönlendirir, aksi halde oturumu döner
function requireLogin(role, loginPage) {
    const session = getSession();

    if (!session || session.role !== role) {
        window.location.replace(loginPage);
        return null;
    }

    return session;
}