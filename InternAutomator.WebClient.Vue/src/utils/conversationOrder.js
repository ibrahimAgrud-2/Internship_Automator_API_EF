import { getAllMessages } from '@/api/messages'
// Mevcut API sözleşmesini değiştirmeden her sohbetin son mesaj zamanını okur.
// Çağrılar 4'erli gruplara ayrılır; bir sohbetin okunamaması diğerlerini engellemez.
export async function loadConversationRecency(conversations, update) {
  const entries=Array.isArray(conversations)?conversations:[]
  for(let offset=0;offset<entries.length;offset+=4){
    await Promise.all(entries.slice(offset,offset+4).map(async conversation=>{
      try {
        const messages=await getAllMessages(conversation.conversationID)
        const timestamps=(Array.isArray(messages)?messages:[]).map(m=>new Date(m.createdDate).getTime()).filter(Number.isFinite)
        update(conversation.conversationID,timestamps.length?Math.max(...timestamps):0)
      }catch{update(conversation.conversationID,0)}
    }))
  }
}
export function orderConversations(items,lastDates){
  return [...items].sort((a,b)=>{
    if(Boolean(a.isGroup)!==Boolean(b.isGroup))return a.isGroup?-1:1
    const difference=(lastDates[b.conversationID]??0)-(lastDates[a.conversationID]??0)
    return difference || String(a.conversationName??'').localeCompare(String(b.conversationName??''),'tr')
  })
}
