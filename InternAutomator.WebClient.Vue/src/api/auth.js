import { apiPost } from './http'

export function login(email, password) {
  return apiPost('Person/Login', { email, password })
}