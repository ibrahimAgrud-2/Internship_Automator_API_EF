import { apiGet, apiPost, apiPut, ApiError } from './http'

// ---- Başvurular ----
export function submitApplication(application) {
  return apiPost('Application', application)
}

export async function getAllApplications() {
  try {
    return await apiGet('Application/All')
  } catch (error) {
    if (error instanceof ApiError && error.status === 404) {
      return []
    }
    throw error
  }
}

export function updateApplicationStatus(applicationID, status, notes) {
  return apiPut('Application/Update/AppStatus', { applicationID, status, notes })
}



export function acceptApplication(applicationID, note = '') {
  const query = new URLSearchParams({
    applicationID: String(applicationID)
  })

  if (note.trim()) {
    query.set('note', note.trim())
  }

  return apiPut(`Application/Update/AppStatus/accept?${query.toString()}`)
}
