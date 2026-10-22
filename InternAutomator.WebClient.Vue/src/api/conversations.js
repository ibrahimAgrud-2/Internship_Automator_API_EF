import { apiGet, apiPost } from './http'

// ---- Sohbetler ----
// Tüm sohbetleri döner: [{ conversationID, internID, conversationName, isGroup }]. Boşsa [].
// Ortak grupta internID null gelir.
export async function getAllConversations() {
  const conversations = await apiGet('Conversation/GetAll')
  return Array.isArray(conversations) ? conversations : []
}

// Yeni sohbet açar. Stajyerle zaten birebir sohbet varsa API yeni açmaz, mevcut sohbeti aynı biçimde döner.
// Cevap: { conversationID, internID, conversationName, isGroup }
export function createConversation({ internID, conversationName, isGroup }) {
  return apiPost('Conversation', { internID, conversationName, isGroup })
}