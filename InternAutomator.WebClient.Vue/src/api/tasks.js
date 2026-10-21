import { apiGet, apiPost, apiPut, apiDelete } from './http'

export async function getInternTasks(internID) {
  const tasks = await apiGet(`Tasks/AllInternTasks?internID=${encodeURIComponent(internID)}`)

  return Array.isArray(tasks) ? tasks : []
}

export function addTask(task) {
  return apiPost('Tasks/Add', task)
}

export function completeTask(taskID) {
  return apiPut(`Tasks/Complete/${encodeURIComponent(taskID)}`)
}

export function deleteTask(taskID) {
  return apiDelete(`Tasks/Delete/${encodeURIComponent(taskID)}`)
}

export function updateTask(taskID, task) {
  return apiPut(`Tasks/Update/${encodeURIComponent(taskID)}`, task)
}
