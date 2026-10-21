import { apiGet } from './http'

// API en yeni duyuruyu başa koyarak sıralı döner; olduğu gibi gösterilir.
export async function getAllAnnouncements() {
  const announcements = await apiGet('Announcement/All')

  // Duyuru yoksa API 200 + [] döner; beklenmeyen biçimde de boş liste say
  return Array.isArray(announcements) ? announcements : []
}