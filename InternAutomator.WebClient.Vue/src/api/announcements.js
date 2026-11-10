import { apiGet, apiPost, apiPut, apiDelete } from './http'


export async function getAllAnnouncements() {
  const announcements = await apiGet('Announcement/All')

  return Array.isArray(announcements) ? announcements : []
}

export function createAnnouncement(announcement) {
  return apiPost('Announcement/Add', announcement)
}

export function updateAnnouncement(announcementID, announcement) {
  return apiPut(`Announcement/Update/${encodeURIComponent(announcementID)}`, announcement)
}

export function deleteAnnouncement(announcementID) {
  return apiDelete(`Announcement/Delete/${announcementID}`)
}

// En yeni duyurular, yeniden eskiye. Mesaja referans verirken kullanılır (GET Announcement/All/{count}).
export async function getLatestAnnouncements(count = 15) {
  const announcements = await apiGet(`Announcement/All/${count}`)
  return Array.isArray(announcements) ? announcements : []
}