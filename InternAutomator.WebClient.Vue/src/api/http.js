// API ile konuşmanın tek kapısı. API adresini sadece bu dosya bilir.
// (Eski client'taki core/api.js'in modül hâli: aynı mantık, import/export ile.)

export const API_BASE_URL = 'https://localhost:7042/api'

// API'dan gelen her hata bu tipte döner; sayfa sadece mesajları gösterir
export class ApiError extends Error {
  constructor(status, messages) {
    super(messages.join(' '))
    this.status = status
    this.messages = messages
  }
}

// Bir hatadan kullanıcıya gösterilecek mesaj listesi
export function getErrorMessages(error) {
  return error instanceof ApiError ? error.messages : ['Beklenmeyen bir hata oluştu.']
}

async function readErrorMessages(response) {
  if (response.status >= 500) {
    return ['Sunucuda bir hata oluştu. Lütfen daha sonra tekrar deneyin.']
  }

  const text = await response.text()

  try {
    const data = JSON.parse(text)
    if (data && data.errors) return Object.values(data.errors).flat()
    if (data && data.message) return [data.message]
    if (data && data.title) return [data.title]
  } catch {
    // JSON değilse düz metindir; aşağıda olduğu gibi gösterilir
  }

  return [text || `İstek reddedildi (kod: ${response.status}).`]
}

// Tüm istekler buradan geçer; GET, POST, PUT, DELETE için ortak
async function request(method, path, body) {
  const options = { method }

  if (body !== undefined) {
    options.headers = { 'Content-Type': 'application/json' }
    options.body = JSON.stringify(body)
  }

  let response

  try {
    response = await fetch(`${API_BASE_URL}/${path}`, options)
  } catch {
    throw new ApiError(0, ['Sunucuya ulaşılamadı. Lütfen tekrar deneyin.'])
  }

  if (!response.ok) {
    throw new ApiError(response.status, await readErrorMessages(response))
  }

  // Gövde boş olabilir (Ok(), 204): null döner. JSON değilse düz metin döner.
  const text = await response.text()
  if (!text) return null

  try {
    return JSON.parse(text)
  } catch {
    return text
  }
}

export const apiGet = (path) => request('GET', path)
export const apiPost = (path, body) => request('POST', path, body)
export const apiPut = (path, body) => request('PUT', path, body)
export const apiDelete = (path) => request('DELETE', path)