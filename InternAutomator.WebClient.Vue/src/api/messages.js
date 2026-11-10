// Mesaj endpoint'leri. Mesaj gönderme API'da hazır olunca eklenecek.
import { apiGet } from '@/api/http'

// GET Conversation/AllMessages/{conversationID}
// -> [{ messageID, personFullName, conversationID, content, createdDate, isStaff,
//       referencedContentID, referencedContentType, referencedContentTitle }]
export const getAllMessages = (conversationID) => apiGet(`Conversation/AllMessages/${conversationID}`)