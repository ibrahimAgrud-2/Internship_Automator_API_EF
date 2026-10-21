import { apiGet, apiPost, apiPut, apiDelete } from './http'

// ---- Stajyer görevleri ----
export async function getInternTasks(internID) {
  const tasks = await apiGet(`Tasks/AllInternTasks?internID=${encodeURIComponent(internID)}`)

  // Dizi değilse boş liste say
  return Array.isArray(tasks) ? tasks : []
}

// Stajyerin atanmış bir projesi için görev ekler. dueDate "yyyy-MM-ddTHH:mm:ss" biçimindedir.
export function addTask(task) {
  return apiPost('Tasks/Add', task)
}

// Görevi tamamlandı olarak işaretler. Başarıda API true döner.
export function completeTask(taskID) {
  return apiPut(`Tasks/Complete/${encodeURIComponent(taskID)}`)
}

// Görevi siler. Başarıda 204 döner.
export function deleteTask(taskID) {
  return apiDelete(`Tasks/Delete/${encodeURIComponent(taskID)}`)
}

export function updateTask(taskID, task) {
  return apiPut(`Tasks/Update/${encodeURIComponent(taskID)}`, task)
}
