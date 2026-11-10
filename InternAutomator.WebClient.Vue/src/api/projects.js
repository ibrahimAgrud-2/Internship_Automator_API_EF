import { apiGet, apiPost, apiPut, apiDelete } from './http'

// ---- Proje kataloğu ----
// Boş tabloda da 200 ve [] döner
export async function getAllProjects() {
  const projects = await apiGet('Projects/All')
  return Array.isArray(projects) ? projects : []
}

// Projenin kaç stajyere atandığı (liste cevabında yok; detay penceresi açılınca tek tek istenir)
export async function getProjectAssignedInternCount(projectID) {
  return Number(await apiGet(`Projects/AssignedInternCount?projectID=${encodeURIComponent(projectID)}`))
}

export function createProject(project) {
  return apiPost('Projects', project)
}

// Projeyi siler. Atanmış proje kuralını API uygular; reddederse 400 ve mesaj döner.
export function deleteProject(projectID) {
  return apiDelete(`Projects/Delete/${projectID}`)
}

// Projenin adını, açıklamasını ve teknolojilerini günceller.
export function updateProject(projectID, project) {
  return apiPut(`Projects/Update/${projectID}`, project)
}
