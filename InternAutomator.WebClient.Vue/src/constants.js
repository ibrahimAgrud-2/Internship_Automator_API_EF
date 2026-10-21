// Sabit değerler: API'daki sayısal durumların ekrandaki karşılıkları.

// ---- Stajyer durumu (API: status 1 = aktif, 0 = ayrıldı) ----
export const INTERN_ACTIVE = 1
export const INTERN_STATUS = {
  1: { text: 'Aktif', badge: 'text-bg-success' },
  0: { text: 'Ayrıldı', badge: 'text-bg-secondary' },
}

// ---- Başvuru durumu ----
export const APPLICATION_NEW = 0
export const APPLICATION_REJECTED = 2
export const APPLICATION_APPROVED = 3
export const APPLICATION_STATUS = {
  0: { text: 'Yeni', badge: 'bg-primary' },
  1: { text: 'İptal', badge: 'bg-secondary' },
  2: { text: 'Reddedildi', badge: 'bg-danger' },
  3: { text: 'Onaylandı', badge: 'bg-success' },
}

// ---- Alan uzunlukları ----
// Veritabanı sütunlarına göre ayarla. Asıl doğrulama API'dadır;
// buradaki sınırlar sadece kullanıcıyı baştan uyarır.
export const MESSAGE_LIMITS = { content: 500 } // API'daki MaxLength(500) ile aynı olmalı
export const PROJECT_LIMITS = { title: 100, technologies: 100, description: 200 }
export const ANNOUNCEMENT_LIMITS = { title: 100, content: 1000 }
export const TASK_LIMITS = { title: 100, description: 200 }
export const APPLICATION_NOTES_LIMIT = 150

// ---- Mesajın referans ettiği içerik (API: referencedContentType) ----
// 0 = referans yok (referencedContentID de 0 gelir), 1 = duyuru, 2 = proje, 3 = görev
export const REFERENCE_NONE = 0
export const REFERENCE_ANNOUNCEMENT = 1
export const REFERENCE_PROJECT = 2
export const REFERENCE_TASK = 3
export const REFERENCE_TYPES = {
  [REFERENCE_ANNOUNCEMENT]: 'Duyuru',
  [REFERENCE_PROJECT]: 'Proje',
  [REFERENCE_TASK]: 'Görev',
}
