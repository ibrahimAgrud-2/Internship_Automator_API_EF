// Gösterim yardımcıları: ekrana yazılacak değerleri biçimlendirir. Sunucuyu bilmez.

// "2026-10-06T07:37:06" -> "06.10.2026". Tarih boşsa fallback döner.
export function formatDate(isoDate, fallback = '') {
  return isoDate ? new Date(isoDate).toLocaleDateString('tr-TR') : fallback
}

// Tarih ve saat birlikte: "15.11.2023 09:30"
export function formatDateTime(isoDate, fallback = '') {
  return isoDate
    ? new Date(isoDate).toLocaleString('tr-TR', { dateStyle: 'short', timeStyle: 'short' })
    : fallback
}

export function formatTime(isoDate) {
  return new Date(isoDate).toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' })
}

// "Bugün", "Dün" ya da "19 Ekim 2026"
export function formatDayLabel(isoDate) {
  const day = new Date(isoDate)
  const today = new Date()
  const yesterday = new Date()
  yesterday.setDate(today.getDate() - 1)

  if (day.toDateString() === today.toDateString()) return 'Bugün'
  if (day.toDateString() === yesterday.toDateString()) return 'Dün'
  return day.toLocaleDateString('tr-TR', { dateStyle: 'long' })
}

// Bugünün tarihi "yyyy-MM-dd" olarak (tarayıcının yerel saatiyle; toISOString UTC verir, kullanılmaz)
export function getTodayText() {
  const now = new Date()
  const month = String(now.getMonth() + 1).padStart(2, '0')
  const day = String(now.getDate()).padStart(2, '0')
  return `${now.getFullYear()}-${month}-${day}`
}

// Arama için: küçük harfe çevirir, Türkçe karakterleri sadeleştirir (ı/İ -> i, ç -> c, ş -> s ...),
// böylece "ibrahim", "Ibrahim" ve "İbrahim" aynı şeyi bulur
export function normalizeSearchText(text) {
  return (text ?? '')
    .toLocaleLowerCase('tr-TR')
    .replace(/ı/g, 'i')
    .normalize('NFD')
    .replace(/\p{M}/gu, '')
    .trim()
    .replace(/\s+/g, ' ')
}

// "ibrahim oruc" -> "IO"
export function getInitials(firstName, lastName) {
  const first = (firstName ?? '').trim().charAt(0)
  const last = (lastName ?? '').trim().charAt(0)
  return (first + last).toLocaleUpperCase('tr-TR')
}

// "C#, EF Core, ASP.NET" -> ["C#", "EF Core", "ASP.NET"]
export function splitTechnologies(technologies) {
  return (technologies ?? '')
    .split(',')
    .map((text) => text.trim())
    .filter(Boolean)
}

// Sadece http(s) adresleri tıklanabilir bağlantı yapılır
export function isHttpUrl(url) {
  return /^https?:\/\//i.test(url ?? '')
}
