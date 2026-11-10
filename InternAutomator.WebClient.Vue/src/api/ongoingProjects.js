import { apiGet, apiPost, apiDelete } from './http'


export async function getInternProjects(internID) {
  const projects = await apiGet(`OngoingProjects/InternProjects?internID=${encodeURIComponent(internID)}`)

  return Array.isArray(projects) ? projects : []
}

export function assignProjectToIntern(internID, projectID, dueDate) {
  return apiPost('OngoingProjects/Add', { internID, projectID, dueDate })
}

export function revokeProject(ongoingProjectID) {
  return apiDelete(`OngoingProjects/Delete/${encodeURIComponent(ongoingProjectID)}`)
}
