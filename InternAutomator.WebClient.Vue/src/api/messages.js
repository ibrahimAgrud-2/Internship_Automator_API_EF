// Mesaj endpoint'leri.
import { apiGet, apiPost } from '@/api/http'

// GET Conversation/AllMessages/{conversationID}
// -> [{ messageID, personFullName, conversationID, content, createdDate, isStaff,
//       referencedContentID, referencedContentType, referencedContentTitle }]
export const getAllMessages = (conversationID) => apiGet(`Conversation/AllMessages/${conversationID}`)

// POST Conversation/AddMessage -> 200 (gövde yok). Tarih API tarafından atanır.
// body: { personID, conversationID, content, isStaff, referencedContentID, referencedContentType }
export const addMessage = (body) => apiPost('Conversation/AddMessage', body)