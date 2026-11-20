// Sohbetler (Conversation) endpoint'leri.
import { apiGet, apiPost } from '@/api/http'

// Personel: tüm sohbetler
// GET Conversation/All -> [{ conversationID, conversationName, isGroup }]
export const getAllConversations = () => apiGet('Conversation/All')

// Stajyer: sadece dahil olduğu sohbetler (internID token'dan okunur)
// GET ConversationIntern/AllConversation -> [{ conversationID, internID, conversationName, isGroup }]
export const getInternConversations = () => apiGet('ConversationIntern/AllConversation')

// Personel: seçtiği stajyerle birebir sohbet oluşturur.
// Mevcutsa mevcut sohbetin ID'sini döndürür.
// POST Conversation/CreatePrivateConversation?internID=15
// Body: { conversationName: 'Stajyer Adı Soyadı', isGroup: false }
// Başarı: mevcut veya yeni sohbetin ID'si ("15" string veya 15 number).
export const createPrivateConversation = (internID, conversation) =>
  apiPost(
    `Conversation/CreatePrivateConversation?internID=${encodeURIComponent(internID)}`,
    conversation
  )

// Personel: yeni grup oluşturur. Üye listesi isteğe bağlıdır.
// POST Conversation/CreateGroupConversation
// Body: { conversationName: 'Yazılım Ekibi', interIDs: [4, 9] }
// Boş grup: interIDs: []
// Başarı: oluşturulan grubun ID'si ("21" veya 21).
export const createGroupConversation = (groupConversation) =>
  apiPost('Conversation/CreateGroupConversation', groupConversation)

// Personel: var olan bir sohbete/gruba tek stajyer ekler.
// POST Conversation/AddIntern?conversationID=21&internID=7
// Endpoint'in parametreleri basit int olduğu için query string kullanılır.
// Başarı: 200 OK (response body boş olabilir).
export const addInternToConversation = (conversationID, internID) =>
  apiPost(
    `Conversation/AddIntern?conversationID=${encodeURIComponent(conversationID)}&internID=${encodeURIComponent(internID)}`,
    {}
  )

// Personel: belirli bir grup sohbetine kayıtlı stajyerleri getirir.
// GET ConversationIntern/AllConversationIntern/{conversationID}
// Response: [{ internID, conversationID, firstName, lastName }]
export const getAllConversationInterns = (conversationID) =>
  apiGet(`ConversationIntern/AllConversationIntern/${encodeURIComponent(conversationID)}`)

// Personel: stajyeri gruptan çıkarır.
// POST Conversation/RemoveIntern/{conversationID}/Intern/{internID}
// Başarı: 200 OK (response body boş olabilir).
export const removeInternFromConversation = (conversationID, internID) =>
  apiPost(
    `Conversation/RemoveIntern/${encodeURIComponent(conversationID)}/Intern/${encodeURIComponent(internID)}`,
    {}
  )
