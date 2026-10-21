import { apiGet, apiPost, apiDelete } from './http'

// ---- Stajyere atanmış projeler ----
export async function getInternProjects(internID) {
  const projects = await apiGet(`OngoingProjects/InternProjects?internID=${encodeURIComponent(internID)}`)

  // API boş durumda dizi yerine metin döndürebiliyor; dizi değilse boş liste say
  return Array.isArray(projects) ? projects : []
}

// Stajyere mevcut bir projeyi atar. Başlangıç tarihini API atandığı gün olarak belirler.
export function assignProjectToIntern(internID, projectID, dueDate) {
  return apiPost('OngoingProjects/Add', { internID, projectID, dueDate })
}

// Atanmış projeyi stajyerden geri alır (atama kaydı silinir; API bağlı görevleri de siler).
export function revokeProject(ongoingProjectID) {
  return apiDelete(`OngoingProjects/Delete/${encodeURIComponent(ongoingProjectID)}`)
}
