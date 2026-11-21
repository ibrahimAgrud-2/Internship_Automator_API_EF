// Personel kullanıcı yönetimi API çağrıları.
// Yetkilendirme/JWT ortak HTTP istemcisinden sağlanır.
import { apiGet, apiPost, apiPut, apiDelete } from '@/api/http'

// GET /api/User/All
export const getAllUsers = () => apiGet('User/All')

// POST /api/User -> 200 OK + oluşturulan kullanıcı numarası
export const createUser = (newUser) => apiPost('User', newUser)

// DELETE /api/User/{userID} -> 204 No Content
export const deleteUser = (userID) => apiDelete(`User/${encodeURIComponent(userID)}`)

// PUT /api/User/{userID} -> 200 OK (yanıt gövdesi boş olabilir)
export const updateUser = (userID, userData) =>
  apiPut(`User/${encodeURIComponent(userID)}`, userData)
