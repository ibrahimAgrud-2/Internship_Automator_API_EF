import { apiPost } from './http'

export function staffLogin(userName, password) {
  return apiPost('User/UserLogin', { userName, password })
}

export function internLogin(email, password) {
  return apiPost('Intern/InternLogin', { email, password })
}
