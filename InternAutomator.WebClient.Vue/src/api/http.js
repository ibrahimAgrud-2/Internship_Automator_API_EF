import { getSession, clearSession } from '@/auth/session'

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

async function request(method, path, body, responseType = 'json') {
  const options = { method }
  const headers = {}

  // Herkese açık endpoint'lere eski/sona ermiş token gönderme.
  const isPublicEndpoint = path === 'Person/Login' || path === 'Application/Apply'
  if (!isPublicEndpoint) {
    const token = getSession()?.token
    if (typeof token === 'string' && token.trim()) {
      headers.Authorization = `Bearer ${token.trim()}`
    }
  }

  if (body !== undefined) {
    if (body instanceof FormData) {
      // Multipart boundary'yi tarayıcı kendisi ekler; Content-Type elle verilmez.
      options.body = body
    } else {
      headers['Content-Type'] = 'application/json'
      options.body = JSON.stringify(body)
    }
  }

  if (Object.keys(headers).length > 0) {
    options.headers = headers
  }

  let response

  try {
    response = await fetch(`${API_BASE_URL}/${path}`, options)
  } catch {
    throw new ApiError(0, ['Sunucuya ulaşılamadı. Lütfen tekrar deneyin.'])
  }

  if (!response.ok) {
    // Oturum açıkken korumalı bir endpoint 401 döndürürse token artık geçerli değildir.
    // Giriş / başvuru gibi herkese açık sayfalardaki hataları eskisi gibi göster.
    if (response.status === 401 && !isPublicEndpoint && getSession()?.role) {
      clearSession()
      // Tam sayfa yönlendirmesi eski ekranı ve bekleyen işlemleri de sıfırlar.
      // replace sayesinde Geri tuşu kullanıcıyı korumalı ekrana döndürmez.
      window.location.replace(`${import.meta.env.BASE_URL}login`)
      throw new ApiError(401, ['Oturumunuzun süresi doldu. Lütfen yeniden giriş yapın.'])
    }

    throw new ApiError(response.status, await readErrorMessages(response))
  }

  // PDF gibi ikili yanıtlarda JSON/text dönüşümü yapılmaz.
  if (responseType === 'blob') return response.blob()

  const text = await response.text()
  if (!text) return null

  try {
    return JSON.parse(text)
  } catch {
    return text
  }
}

export const apiGet = (path) => request('GET', path)
export const apiGetBlob = (path) => request('GET', path, undefined, 'blob')
export const apiPost = (path, body) => request('POST', path, body)
export const apiPut = (path, body) => request('PUT', path, body)
export const apiDelete = (path) => request('DELETE', path)
