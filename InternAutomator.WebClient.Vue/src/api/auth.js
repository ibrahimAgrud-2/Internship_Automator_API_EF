import { apiPost } from './http'

// ---- Personel girişi ----
export function staffLogin(userName, password) {
  return apiPost('User/UserLogin', { userName, password })
}