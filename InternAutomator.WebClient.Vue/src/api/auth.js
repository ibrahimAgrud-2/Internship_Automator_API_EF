import { apiPost } from './http'

export function login(email, password) {
  return apiPost('Person/Login', { email, password })
}

export function changePassword(oldPassword, newPassword) {
  return apiPost('Person/ChangePassword', {
    oldPassword,
    newPassword
  })
}
