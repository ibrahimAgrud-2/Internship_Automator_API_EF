// Sohbetler (Conversation) endpoint'leri.
import { apiGet } from '@/api/http'

// Personel: tüm sohbetler
// GET Conversation/All -> [{ conversationID, conversationName, isGroup }]
export const getAllConversations = () => apiGet('Conversation/All')

// Stajyer: sadece dahil olduğu sohbetler (internID token'dan okunur)
// GET ConversationIntern/AllConversation -> [{ conversationID, internID, conversationName, isGroup }]
export const getInternConversations = () => apiGet('ConversationIntern/AllConversation')