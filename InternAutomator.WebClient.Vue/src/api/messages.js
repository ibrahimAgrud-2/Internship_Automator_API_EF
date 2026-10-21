import { apiGet, apiPost } from './http'

// ---- Mesajlar ----
// Sohbetin son mesajlarını eskiden yeniye sıralı döner (API sıralar). Boşsa [].
export async function getConversationMessages(conversationID, messageCount) {
  const messages = await apiGet(`Conversation/GetAllMessages/${conversationID}/${messageCount}`)
  return Array.isArray(messages) ? messages : []
}

// Personel olarak sohbete mesaj yazar. Başarıda API true döner.
// Referans varsa message içine { referencedContentID, referencedContentType } eklenir; yoksa bu alanlar hiç gönderilmez.
// userID şimdilik oturumdan gelir; API bunu kişi kaydına çevirir. JWT eklenince token'dan alınacak.
export function staffAddMessage(message) {
  return apiPost('Conversation/StaffAddMessage', message)
}
