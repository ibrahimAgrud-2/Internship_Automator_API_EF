import { apiGet, apiPut } from './http'

// ---- Stajyerler ----
// Boş tabloda da 200 ve [] döner; 404 gerçek bir hatadır (yol yanlış vb.)
export async function getAllInterns() {
  return apiGet('Intern/All')
}

// Tek stajyerin bilgisi (detay sayfası başlığı için). Yoksa API 404 döner.
export function getIntern(internID) {
  return apiGet(`Intern/${encodeURIComponent(internID)}`)
}

// Stajyerin stajını bitirir (status -> ayrıldı). Proje ve görev kayıtları silinmez.
export function separateIntern(internID) {
  return apiPut(`Intern/Separate/${encodeURIComponent(internID)}`)
}
