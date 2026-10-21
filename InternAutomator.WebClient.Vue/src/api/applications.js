import { apiGet, apiPost, apiPut, ApiError } from './http'

// ---- Başvurular ----
export function submitApplication(application) {
  return apiPost('Application', application)
}

// Endpoint tablo boşken 404 döndüğü için burada "boş liste" olarak ele alınıyor
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

// Başvurunun durumunu (onay/ret) ve değerlendirme notunu günceller
export function updateApplicationStatus(applicationID, status, notes) {
  return apiPut('Application/Update/AppStatus', { applicationID, status, notes })
}

// Başvuruyu onaylar; API stajyer hesabını da oluşturur ve giriş bilgilerini e-postayla gönderir
export function acceptApplication(applicationID, status, notes) {
  return apiPut('Application/Update/AppStatus/accepted', { applicationID, status, notes })
}
