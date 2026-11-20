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

export const addInternToConversation = (conversationID, internID) =>
  apiPost(
    `Conversation/AddIntern?conversationID=${encodeURIComponent(conversationID)}&internID=${encodeURIComponent(internID)}`,
    {}
  )
