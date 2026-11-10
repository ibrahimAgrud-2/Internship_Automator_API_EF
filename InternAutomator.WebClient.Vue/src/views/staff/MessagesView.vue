<script setup>
import { ref, computed, nextTick, onMounted } from 'vue'
import { getAllConversations } from '@/api/conversations'
import { getAllMessages } from '@/api/messages'
import { getErrorMessages } from '@/api/http'
import { REFERENCE_TYPES, REFERENCE_ANNOUNCEMENT, REFERENCE_PROJECT, REFERENCE_TASK } from '@/constants'
import { formatTime, formatDayLabel } from '@/utils/format'
import AlertMessages from '@/components/AlertMessages.vue'
import '@/assets/css/messages.css'

const conversations = ref([])
const isLoading = ref(true)
const loadError = ref([])
const selectedConversationID = ref(null)

const selectedConversation = computed(
  () => conversations.value.find((conversation) => conversation.conversationID === selectedConversationID.value) ?? null,
)

// "stajyerler Ortak Grup" -> "SO"
function getConversationInitials(name) {
  return (name ?? '')
    .trim()
    .split(/\s+/)
    .slice(0, 2)
    .map((word) => word.charAt(0))
    .join('')
    .toLocaleUpperCase('tr-TR')
}

async function loadConversations() {
  isLoading.value = true
  loadError.value = []

  try {
    conversations.value = (await getAllConversations()) ?? []
  } catch (error) {
    loadError.value = getErrorMessages(error)
  } finally {
    isLoading.value = false
  }
}

// ---------- Mesajlar (sağ) ----------
const messages = ref([])
const isMessagesLoading = ref(false)
const messagesError = ref([])
const messageList = ref(null)

// isStaff true ise mesajı personel yazmıştır (sağda), false ise stajyer (solda)
const isFromStaff = (message) => message.isStaff === true
const getSenderLabel = (message) => (isFromStaff(message) ? 'Personel' : message.personFullName || 'Stajyer')

// referencedContentID 0 ise mesaj hiçbir şeyi referans etmiyor: kutu ve başlık hiç gösterilmez
const hasReference = (message) => Number(message.referencedContentID) > 0
const getReferenceTypeLabel = (message) => REFERENCE_TYPES[message.referencedContentType] ?? 'İçerik'

const REFERENCE_CLASSES = {
  [REFERENCE_ANNOUNCEMENT]: 'ref-announcement',
  [REFERENCE_PROJECT]: 'ref-project',
  [REFERENCE_TASK]: 'ref-task',
}
const getReferenceClass = (message) => REFERENCE_CLASSES[message.referencedContentType] ?? ''

// Mesajlar eskiden yeniye gelir; gün değişince ayraç eklenir
const items = computed(() => {
  const result = []
  let lastDay = null
  messages.value.forEach((message, index) => {
    const day = new Date(message.createdDate).toDateString()
    if (day !== lastDay) {
      result.push({ type: 'day', key: `day-${index}`, label: formatDayLabel(message.createdDate) })
      lastDay = day
    }
    result.push({ type: 'message', key: `message-${message.messageID ?? index}-${index}`, message })
  })
  return result
})

let requestId = 0 // eski bir cevap (önceki sohbetin mesajları) yenisinin üstüne yazmasın

async function loadMessages() {
  const conversationID = selectedConversationID.value
  if (conversationID === null) return

  const currentRequest = ++requestId
  messages.value = []
  messagesError.value = []
  isMessagesLoading.value = true

  try {
    const result = (await getAllMessages(conversationID)) ?? []
    if (currentRequest !== requestId) return
    messages.value = result
    await nextTick()
    if (messageList.value) messageList.value.scrollTop = messageList.value.scrollHeight // en yeni mesaj görünsün
  } catch (error) {
    if (currentRequest !== requestId) return
    messagesError.value = getErrorMessages(error)
  } finally {
    if (currentRequest === requestId) isMessagesLoading.value = false
  }
}

function selectConversation(conversationID) {
  if (conversationID === selectedConversationID.value) return
  selectedConversationID.value = conversationID
  loadMessages()
}

onMounted(async () => {
  await loadConversations()
  // İlk açılışta listenin başındaki sohbet (ortak grup) açılır
  if (conversations.value.length > 0) selectConversation(conversations.value[0].conversationID)
})
</script>

<template>
  <div>
    <div class="chat-layout card">
      <!-- Sol: sohbet listesi -->
      <aside class="chat-list-panel border-end">
        <div class="p-3 border-bottom d-grid gap-2">
          <button type="button" class="btn btn-outline-primary" disabled>Yeni sohbet</button>
          <button type="button" class="btn btn-outline-primary" disabled>Grup ekle</button>
        </div>

        <div class="flex-grow-1 overflow-auto">
          <AlertMessages v-if="isLoading" class="m-3" type="info" messages="Sohbetler yükleniyor..." />
          <AlertMessages v-else-if="loadError.length" class="m-3" :messages="loadError" />
          <p v-else-if="conversations.length === 0" class="text-muted small text-center p-3 mb-0">Henüz sohbet yok.</p>

          <button
            v-for="conversation in conversations"
            :key="conversation.conversationID"
            type="button"
            class="chat-list-item"
            :class="{ active: conversation.conversationID === selectedConversationID }"
            @click="selectConversation(conversation.conversationID)"
          >
            <span
              class="avatar-circle rounded-circle text-white d-flex align-items-center justify-content-center fw-semibold"
              :class="conversation.isGroup ? 'bg-success' : 'bg-primary'"
            >
              {{ getConversationInitials(conversation.conversationName) }}
            </span>
            <span class="chat-list-text">
              <span class="fw-semibold text-truncate d-block">{{ conversation.conversationName }}</span>
              <span class="chat-preview d-block small text-muted">{{ conversation.isGroup ? 'Grup sohbeti' : 'Birebir sohbet' }}</span>
            </span>
          </button>
        </div>
      </aside>

      <!-- Sağ: seçili sohbet -->
      <section class="chat-panel">
    <div v-if="selectedConversation" class="d-flex align-items-center gap-3 px-3 py-2 border-bottom">
            <span
            class="avatar-circle rounded-circle text-white d-flex align-items-center justify-content-center fw-semibold"
            :class="selectedConversation.isGroup ? 'bg-success' : 'bg-primary'"
          >
            {{ getConversationInitials(selectedConversation.conversationName) }}
          </span>
          <div class="fw-semibold text-break">{{ selectedConversation.conversationName }}</div>
        </div>

        <template v-if="selectedConversation">
          <!-- Yükleniyor / hata mesajı -->
          <AlertMessages v-if="isMessagesLoading" class="m-3 mb-0" type="info" messages="Mesajlar yükleniyor..." />
          <AlertMessages v-else-if="messagesError.length" class="m-3 mb-0" :messages="messagesError" />

          <div ref="messageList" class="chat-messages">
            <template v-for="item in items" :key="item.key">
              <div v-if="item.type === 'day'" class="chat-day">{{ item.label }}</div>
              <div v-else class="chat-bubble" :class="isFromStaff(item.message) ? 'from-staff' : 'from-intern'">
                <div class="chat-sender" :class="isFromStaff(item.message) ? 'text-secondary' : 'text-primary'">{{ getSenderLabel(item.message) }}</div>
                <div v-if="hasReference(item.message)" class="chat-reference text-break" :class="getReferenceClass(item.message)">
                  <span class="chat-reference-type">{{ getReferenceTypeLabel(item.message) }}</span>
                  {{ item.message.referencedContentTitle }}
                </div>
                <div class="chat-text text-break">{{ item.message.content }}</div>
                <div class="chat-time">{{ formatTime(item.message.createdDate) }}</div>
              </div>
            </template>
          </div>
        </template>

        <div v-else class="d-flex flex-grow-1 align-items-center justify-content-center text-muted p-4 text-center">
          {{ isLoading ? 'Sohbetler yükleniyor...' : 'Soldan bir sohbet seçin.' }}
        </div>

        <!-- Mesaj yazma satırı (pasif) -->
        <form class="chat-composer border-top" autocomplete="off" novalidate @submit.prevent>
          <div class="dropup">
            <button type="button" class="btn btn-outline-secondary" aria-label="Ekle" disabled>+</button>
          </div>
          <textarea class="form-control" rows="2" placeholder="Mesajınızı yazın..." aria-label="Mesaj" disabled></textarea>
          <button type="submit" class="btn btn-outline-primary" disabled>Gönder</button>
        </form>
      </section>
    </div>
  </div>
</template>