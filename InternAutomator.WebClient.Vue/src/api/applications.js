import { apiGet, apiGetBlob, apiPost, apiPut, ApiError } from './http'

// ---- Başvurular ----
// Başvuru + PDF özgeçmiş aynı multipart/form-data isteğinde gönderilir.
export function submitApplication(application, cvFile) {
  const data = new FormData()
  const person = application.personInfo

  data.append('PersonInfo.FirstName', person.firstName)
  data.append('PersonInfo.LastName', person.lastName)
  data.append('PersonInfo.Email', person.email)
  data.append('PersonInfo.Phone', person.phone)

  if (person.address) data.append('PersonInfo.Address', person.address)

  data.append('University', application.university)
  data.append('Department', application.department)
  data.append('ClassYear', String(application.classYear))

  if (application.linkedinURL) data.append('LinkedinURL', application.linkedinURL)
  if (application.githubURL) data.append('GithubURL', application.githubURL)

  data.append('CV', cvFile, cvFile.name)
  return apiPost('Application/Apply', data)
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

export function acceptApplication(applicationID, status, notes) {
  return apiPut('Application/Update/AppStatus/accepted', { applicationID, status, notes })
}

// Personel yetkisi gerektiren PDF yanıtı (JWT ortak http.js üzerinden eklenir).
export function getApplicationCV(applicationID) {
  return apiGetBlob(`Application/${applicationID}/CV`)
}
