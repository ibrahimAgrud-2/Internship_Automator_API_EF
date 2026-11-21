// Personel hesapları: yalnızca kullanıcı listeleme.
// Yetkilendirme ve JWT bilgileri ortak HTTP istemcisi tarafından yönetilir.
import { apiGet, apiPost } from '@/api/http'
// GET /api/User/All
// Response: [{ userID, userName, personID, fullName, createdDate, isActive }]
export const getAllUsers = () => apiGet('User/All')
export const createUser = (userData) => apiPost('User', userData)