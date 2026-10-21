// Oturum bilgisi tek yerde tutulur; sayfalar sessionStorage'a doğrudan dokunmaz.
// NOT: Token olmadığı için bu bilgi sadece arayüz içindir, güvenlik sağlamaz (JWT gelince değişecek).

const SESSION_KEY = 'iaa.session'

export const ROLE_INTERN = 'intern'
export const ROLE_STAFF = 'staff'

export function saveSession(session) {
  sessionStorage.setItem(SESSION_KEY, JSON.stringify(session))
}

export function getSession() {
  const raw = sessionStorage.getItem(SESSION_KEY)
  if (!raw) return null

  try {
    return JSON.parse(raw)
  } catch {
    sessionStorage.removeItem(SESSION_KEY)
    return null
  }
}

export function clearSession() {
  sessionStorage.removeItem(SESSION_KEY)
}
