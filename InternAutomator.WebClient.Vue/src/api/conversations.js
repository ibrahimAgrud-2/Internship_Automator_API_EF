// Sohbetler (Conversation) endpoint'leri. Şimdilik sadece personelin tüm sohbetleri görmesi.
import { apiGet } from '@/api/http'

// GET Conversation/All -> [{ conversationID, conversationName, isGroup }]
export const getAllConversations = () => apiGet('Conversation/All')