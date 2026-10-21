<script setup>
import { ref, computed, nextTick, onMounted } from 'vue'
import { getConversationMessages, staffAddMessage } from '@/api/messages'
import { getErrorMessages } from '@/api/http'
import { getSession } from '@/auth/session'
import { MESSAGE_LIMITS } from '@/constants'
import { formatTime, formatDayLabel } from '@/utils/format'
import AlertMessages from '@/components/AlertMessages.vue'
import '@/assets/css/messages.css'

const session = getSession()

// Şimdilik tek sohbet var: Stajyerler Ortak Grup. Sohbet listesi endpoint'i gelince bu numara oradan okunacak.
const GROUP_CONVERSATION_ID = 1
const MESSAGE_COUNT = 50

// Stajyer adları kişi numarasına göre sabit bir renkte görünür
const SENDER_COLORS = ['text-success', 'text-primary', 'text-danger', 'text-warning-emphasis', 'text-info-emphasis', 'text-secondary-emphasis']

const messages = ref([])
const isLoading = ref(true)
const loadError = ref([])
const messageList = ref(null)

// ---------- Yardımcılar ----------
// API'da isStaff true ise mesajı personel yazmıştır, false ise stajyer
const isFromStaff = (message) => message.isStaff === true

// Personelin adı gösterilmez (şimdilik stajyer - sistem yazışması); stajyerin adı gösterilir
const getSenderLabel = (message) => (isFromStaff(message) ? 'Personel' : message.personFullName || 'Stajyer')

const getSenderColor = (message) =>
  isFromStaff(message) ? 'text-secondary' : SENDER_COLORS[Math.abs(Number(message.personID) || 0) % SENDER_COLORS.length]

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

// Soldaki sohbet satırında son mesajın önizlemesi
const lastMessage = computed(() => messages.value[messages.value.length - 1] ?? null)
const previewText = computed(() => {
  if (loadError.value.length) return ''
  if (isLoading.value && !lastMessage.value) return ''
  if (!lastMessage.value) return 'Henüz mesaj yok'
  return `${getSenderLabel(lastMessage.value)}: ${lastMessage.value.content}`
})
const previewTime = computed(() => (lastMessage.value ? formatTime(lastMessage.value.createdDate) : ''))

// ---------- Yükleme ----------
let requestId = 0 // eski bir cevap yenisinin üstüne yazmasın

// showLoading false ise (mesaj gönderdikten sonra yenileme) liste silinip yükleniyor yazısı çıkmaz
async function loadMessages(showLoading = true) {
  const currentRequest = ++requestId

  if (showLoading) {
    messages.value = []
    isLoading.value = true
  }
  loadError.value = []

  try {
    const result = await getConversationMessages(GROUP_CONVERSATION_ID, MESSAGE_COUNT)
    if (currentRequest !== requestId) return
    messages.value = result
    await nextTick()
    if (messageList.value) messageList.value.scrollTop = messageList.value.scrollHeight // en yeni mesaj görünsün
  } catch (error) {
    if (currentRequest !== requestId) return
    messages.value = []
    loadError.value = getErrorMessages(error)
  } finally {
    if (currentRequest === requestId) isLoading.value = false
  }
}

onMounted(() => loadMessages())

// ---------- Mesaj gönderme ----------
const draft = ref('')
const isSending = ref(false)
const sendError = ref([])
const messageInput = ref(null)

// Metin boşken ya da istek sürerken Gönder pasif olur
const canSend = computed(() => !isSending.value && draft.value.trim() !== '')

async function sendMessage() {
  // İstek sürerken ikinci gönderim (Enter'a art arda basma vb.) yapılmasın
  if (isSending.value) return

  const content = draft.value.trim()
  if (!content) return

  // Oturumda kullanıcı numarası yoksa istek hiç atılmaz
  if (!session?.userID) {
    sendError.value = ['Oturum bilgisi eksik. Lütfen çıkış yapıp tekrar giriş yapın.']
    return
  }

  sendError.value = []
  isSending.value = true

  try {
    await staffAddMessage({ userID: session.userID, conversationID: GROUP_CONVERSATION_ID, content })
    draft.value = ''
    await loadMessages(false) // ekranda her zaman sunucudaki gerçek hal görünsün
  } catch (error) {
    sendError.value = getErrorMessages(error) // yazılan metin silinmez, tekrar denenebilir
  } finally {
    isSending.value = false
    await nextTick() // alan tekrar aktif olduktan sonra odaklanılabilir
    messageInput.value?.focus()
  }
}

// Enter gönderir, Shift+Enter yeni satır ekler
function onEnter(event) {
  if (event.shiftKey || event.isComposing) return
  event.preventDefault()
  sendMessage()
}
</script>

<template>
  <div>
    <h4 class="mb-4">Mesajlar</h4>

    <div class="chat-layout card">
      <!-- Sol: sohbet listesi -->
      <aside class="chat-list-panel border-end">
        <div class="p-3 border-bottom">
          <button type="button" class="btn btn-outline-primary w-100" disabled>Yeni sohbet</button>
        </div>

        <div class="flex-grow-1 overflow-auto">
          <!-- Ortak grup her zaman en üstte -->
          <button type="button" class="chat-list-item active">
            <span class="avatar-circle rounded-circle bg-success text-white d-flex align-items-center justify-content-center fw-semibold">SG</span>
            <span class="chat-list-text">
              <span class="d-flex justify-content-between gap-2">
                <span class="fw-semibold text-truncate">Stajyerler Ortak Grup</span>
                <span class="small text-muted flex-shrink-0">{{ previewTime }}</span>
              </span>
              <span class="chat-preview d-block small text-muted">{{ previewText || ' ' }}</span>
            </span>
          </button>

          <p class="text-muted small text-center p-3 mb-0">Henüz birebir sohbet yok.</p>
        </div>
      </aside>

      <!-- Sağ: seçili sohbet -->
      <section class="chat-panel">
        <div class="d-flex align-items-center gap-3 p-3 border-bottom">
          <span class="avatar-circle rounded-circle bg-success text-white d-flex align-items-center justify-content-center fw-semibold">SG</span>
          <div>
            <div class="fw-semibold">Stajyerler Ortak Grup</div>
            <div class="small text-muted">Tüm stajyerler ve personel</div>
          </div>
        </div>

        <!-- Yükleniyor / boş / hata mesajı -->
        <AlertMessages v-if="isLoading" class="m-3 mb-0" type="info" messages="Mesajlar yükleniyor..." />
        <AlertMessages v-else-if="loadError.length" class="m-3 mb-0" :messages="loadError" />
        <AlertMessages v-else-if="messages.length === 0" class="m-3 mb-0" type="secondary" messages="Henüz mesaj yok." />

        <div ref="messageList" class="chat-messages">
          <template v-for="item in items" :key="item.key">
            <div v-if="item.type === 'day'" class="chat-day">{{ item.label }}</div>
            <div v-else class="chat-bubble" :class="isFromStaff(item.message) ? 'from-staff' : 'from-intern'">
              <div class="chat-sender" :class="getSenderColor(item.message)">{{ getSenderLabel(item.message) }}</div>
              <div class="chat-text text-break">{{ item.message.content }}</div>
              <div class="chat-time">{{ formatTime(item.message.createdDate) }}</div>
            </div>
          </template>
        </div>

        <!-- Gönderme hatası -->
        <AlertMessages v-if="sendError.length" class="m-3 mb-0" :messages="sendError" />

        <!-- Mesaj yazma satırı -->
        <form class="chat-composer border-top" autocomplete="off" novalidate @submit.prevent="sendMessage">
          <div class="dropup">
            <button type="button" class="btn btn-outline-secondary" data-bs-toggle="dropdown" aria-expanded="false" aria-label="Ekle">+</button>
            <ul class="dropdown-menu">
              <li><button type="button" class="dropdown-item" disabled>Dosya seç</button></li>
              <li><hr class="dropdown-divider" /></li>
              <li><h6 class="dropdown-header">Referans ver</h6></li>
              <li><button type="button" class="dropdown-item" disabled>Duyuru</button></li>
              <li><button type="button" class="dropdown-item" disabled>Proje</button></li>
            </ul>
          </div>

          <textarea
            ref="messageInput"
            v-model="draft"
            class="form-control"
            rows="2"
            placeholder="Mesajınızı yazın..."
            aria-label="Mesaj"
            :maxlength="MESSAGE_LIMITS.content"
            :disabled="isSending"
            @keydown.enter="onEnter"
          ></textarea>
          <button type="submit" class="btn btn-outline-primary" :disabled="!canSend">Gönder</button>
        </form>
      </section>
    </div>
  </div>
</template>
