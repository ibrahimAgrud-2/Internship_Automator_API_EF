import { apiGet, apiPut } from './http'


export async function getAllInterns() {
  return apiGet('Intern/All')
}

export function getIntern(internID) {
  return apiGet(`Intern/${encodeURIComponent(internID)}`)
}

export function separateIntern(internID) {
  return apiPut(`Intern/Separate/${encodeURIComponent(internID)}`)
}
