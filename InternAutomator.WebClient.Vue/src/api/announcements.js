import { apiGet, apiPost, apiPut, apiDelete } from './http'

// ---- Duyurular ----
// API en yeni duyuruyu başa koyarak sıralı döner; olduğu gibi gösterilir.
export async function getAllAnnouncements() {
  const announcements = await apiGet('Announcement/All')

  // Duyuru yoksa API 200 + [] döner; beklenmeyen biçimde de boş liste say
  return Array.isArray(announcements) ? announcements : []
}

// Başarıda 201 döner; yayın tarihini API belirler
export function createAnnouncement(announcement) {
  return apiPost('Announcement/Add', announcement)
}

// Duyurunun başlığını ve metnini günceller. Başarıda API true döner.
export function updateAnnouncement(announcementID, announcement) {
  return apiPut(`Announcement/Update/${encodeURIComponent(announcementID)}`, announcement)
}

// Duyuruyu siler. Başarıda 204 döner; kayıt yoksa 404 ve mesaj döner.
export function deleteAnnouncement(announcementID) {
  return apiDelete(`Announcement/Delete/${announcementID}`)
}
