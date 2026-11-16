// Sohbetler (Conversation) endpoint'leri.
import { apiGet } from '@/api/http'

// Personel: tüm sohbetler
// GET Conversation/All -> [{ conversationID, conversationName, isGroup }]
export const getAllConversations = () => apiGet('Conversation/All')

// Stajyer: sadece dahil olduğu sohbetler
// GET ConversationIntern/AllConversation/{internID} -> [{ conversationID, internID, conversationName, isGroup }]
export const getInternConversations = (internID) => apiGet(`ConversationIntern/AllConversation/${internID}`)    