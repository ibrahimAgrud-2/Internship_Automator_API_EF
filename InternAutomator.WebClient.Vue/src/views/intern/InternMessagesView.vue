<script setup>
import { ref, computed, nextTick, onMounted } from 'vue'
import { getInternConversations } from '@/api/conversations'
import { getAllMessages, addMessage } from '@/api/messages'
import { getErrorMessages } from '@/api/http'
import { getLatestAnnouncements } from '@/api/announcements'
import { getAllProjects } from '@/api/projects'
import { getMyTasks } from '@/api/tasks'
import { MESSAGE_LIMITS, REFERENCE_NONE, REFERENCE_TYPES, REFERENCE_ANNOUNCEMENT, REFERENCE_PROJECT, REFERENCE_TASK } from '@/constants'
import { formatTime, formatDate, formatDayLabel } from '@/utils/format'
import AlertMessages from '@/components/AlertMessages.vue'
import ReferencePickerModal from '@/components/ReferencePickerModal.vue'
import '@/assets/css/messages.css'

// Rota koruması sayesinde bu sayfaya oturum olmadan gelinmez

// ---------- Sohbet listesi (sol) ----------
const conversations = ref([])
const isLoading = ref(true)
const loadError = ref([])
const selectedConversationID = ref(null)

const selectedConversation = computed(
  () => conversations.value.find((conversation) => conversation.conversationID === selectedConversationID.value) ?? null,
)

// Stajyer sadece grup sohbetine ve personelle birebir sohbete dahildir.
// Birebir sohbetin adı stajyerin kendi adıdır; stajyer için "Personel" gösterilir.
const getConversationTitle = (conversation) => (conversation.isGroup ? conversation.conversationName : 'Personel')

// "stajyerler Ortak Grup" -> "SO"
function getConversationInitials(conversation) {
  return getConversationTitle(conversation)
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
    conversations.value = (await getInternConversations()) ?? []  } catch (error) {
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
const getBubbleClass = (message) => (message.isStaff ? 'from-staff' : 'from-intern')
const getSenderLabel = (message) => (message.isStaff ? 'Personel' : message.personFullName || 'Stajyer')

// referencedContentID 0 ise mesaj hiçbir şeyi referans etmiyor: kutu ve başlık hiç gösterilmez
const hasReference = (message) => Number(message.referencedContentID) > 0
const getReferenceTypeLabel = (message) => REFERENCE_TYPES[message.referencedContentType] ?? 'İçerik'

const REFERENCE_CLASSES = {
  [REFERENCE_ANNOUNCEMENT]: 'ref-announcement',
  [REFERENCE_PROJECT]: 'ref-project',
  [REFERENCE_TASK]: 'ref-task',
}
const getReferenceClassByType = (type) => REFERENCE_CLASSES[type] ?? ''
const getReferenceClass = (message) => getReferenceClassByType(message.referencedContentType)

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

// showLoading false ise (mesaj gönderdikten sonra yenileme) liste silinip yükleniyor yazısı çıkmaz
async function loadMessages(showLoading = true) {
  const conversationID = selectedConversationID.value
  if (conversationID === null) return

  const currentRequest = ++requestId

  if (showLoading) {
    messages.value = []
    isMessagesLoading.value = true
  }
  messagesError.value = []

  try {
    const result = (await getAllMessages(conversationID)) ?? []
    if (currentRequest !== requestId) return
    messages.value = result
    await nextTick()
    if (messageList.value) messageList.value.scrollTop = messageList.value.scrollHeight // en yeni mesaj görünsün
  } catch (error) {
    if (currentRequest !== requestId) return
    messages.value = []
    messagesError.value = getErrorMessages(error)
  } finally {
    if (currentRequest === requestId) isMessagesLoading.value = false
  }
}

function selectConversation(conversationID) {
  if (conversationID === selectedConversationID.value) return
  selectedConversationID.value = conversationID
  // Yarım yazılan metin başka sohbete gitmesin
  draft.value = ''
  reference.value = null
  sendError.value = []
  loadMessages()
}

// ---------- Referans seçme (+ menüsü) ----------
// Mesaja eklenecek içerik: { id, type, title } ya da null. Gönderilene kadar yazma satırının üstünde durur.
const reference = ref(null)

const isPickerOpen = ref(false)
const pickerType = ref(REFERENCE_ANNOUNCEMENT)
const pickerSections = ref([])
const isPickerLoading = ref(false)
const pickerErrors = ref([])
let pickerRequestId = 0

const PICKER_TITLES = { [REFERENCE_ANNOUNCEMENT]: 'Duyuru seç', [REFERENCE_PROJECT]: 'Proje seç', [REFERENCE_TASK]: 'Görev seç' }
const PICKER_EMPTY_TEXTS = {
  [REFERENCE_ANNOUNCEMENT]: 'Henüz duyuru yok.',
  [REFERENCE_PROJECT]: 'Henüz proje yok.',
  [REFERENCE_TASK]: 'Henüz görevin yok.',
}

// Duyurular tek bölüm: API'nın verdiği son duyurular, yeniden eskiye
async function loadAnnouncementSections() {
  const announcements = await getLatestAnnouncements()
  return [
    {
      items: announcements.map((announcement) => ({
        id: announcement.announcementID,
        title: announcement.title,
        subtitle: formatDayLabel(announcement.createdDate),
      })),
    },
  ]
}

// Projeler tek bölüm: API en çok stajyerin çalıştığından en aza doğru sıralı verir, sıra olduğu gibi korunur.
// Çalışan stajyeri olan projeler renkli, hiç alınmayanlar gri görünür.
async function loadProjectSections() {
  const projects = await getAllProjects()
  return [
    {
      items: projects.map((project) => {
        const count = Number(project.takenByInternCount) || 0
        return {
          id: project.projectID,
          title: project.title,
          subtitle: project.technologies,
          badge: { text: `${count} stajyer`, tone: count > 0 ? 'primary' : 'secondary' },
          muted: count === 0,
        }
      }),
    },
  ]
}

// Görevler: sadece bu stajyerin görevleri. Bekleyenler üstte, tamamlananlar ayrı bölümde ve soluk.
// Her bölümün kendi içindeki sıra API'dan geldiği gibi kalır.
async function loadTaskSections() {
  const tasks = await getMyTasks()
    const toItem = (task) => ({
    id: task.taskID,
    title: task.title,
    subtitle: `Son tarih: ${formatDate(task.dueDate)}`,
    muted: task.completed,
  })
  return [
    { title: 'Yapılacaklar', items: tasks.filter((task) => !task.completed).map(toItem) },
    { title: 'Tamamlananlar', items: tasks.filter((task) => task.completed).map(toItem) },
  ]
}

const PICKER_LOADERS = {
  [REFERENCE_ANNOUNCEMENT]: loadAnnouncementSections,
  [REFERENCE_PROJECT]: loadProjectSections,
  [REFERENCE_TASK]: loadTaskSections,
}

async function openPicker(type) {
  const current = ++pickerRequestId
  pickerType.value = type
  pickerSections.value = []
  pickerErrors.value = []
  isPickerLoading.value = true
  isPickerOpen.value = true // her açılışta liste güncel gelir

  try {
    const sections = await PICKER_LOADERS[type]()
    if (current === pickerRequestId) pickerSections.value = sections
  } catch (error) {
    if (current === pickerRequestId) pickerErrors.value = getErrorMessages(error)
  } finally {
    if (current === pickerRequestId) isPickerLoading.value = false
  }
}

function onReferencePicked(item) {
  reference.value = { id: item.id, type: pickerType.value, title: item.title }
}

// ---------- Mesaj gönderme ----------
const draft = ref('')
const isSending = ref(false)
const sendError = ref([])
const messageInput = ref(null)

// Metin boşken ya da istek sürerken Gönder pasif olur
const canSend = computed(() => !isSending.value && draft.value.trim() !== '')

async function sendMessage() {
  // İstek sürerken ikinci gönderim (Enter'a art arda basma vb.) yapılmasın
  if (isSending.value || selectedConversationID.value === null) return

  const content = draft.value.trim()
  if (!content) return

  sendError.value = []
  isSending.value = true

  try {
    // Referans yoksa 0 / 0 gönderilir (API'da 0 = referans yok). Tarihi API atar.
    await addMessage({
      conversationID: selectedConversationID.value,
      content,
      referencedContentID: reference.value ? reference.value.id : 0,
      referencedContentType: reference.value ? reference.value.type : REFERENCE_NONE,
    })
    draft.value = ''
    reference.value = null
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

onMounted(async () => {
  await loadConversations()
  // İlk açılışta listenin başındaki sohbet açılır
  if (conversations.value.length > 0) selectConversation(conversations.value[0].conversationID)
})
</script>

<template>
  <div>
    <div class="chat-layout card">
      <!-- Sol: stajyerin dahil olduğu sohbetler (sohbet ekleme / grup oluşturma yok) -->
      <aside class="chat-list-panel border-end">
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
              {{ getConversationInitials(conversation) }}
            </span>
            <span class="chat-list-text">
              <span class="fw-semibold text-truncate d-block">{{ getConversationTitle(conversation) }}</span>
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
            {{ getConversationInitials(selectedConversation) }}
          </span>
          <div class="fw-semibold text-break">{{ getConversationTitle(selectedConversation) }}</div>
        </div>

        <template v-if="selectedConversation">
          <!-- Yükleniyor / hata mesajı -->
          <AlertMessages v-if="isMessagesLoading" class="m-3 mb-0" type="info" messages="Mesajlar yükleniyor..." />
          <AlertMessages v-else-if="messagesError.length" class="m-3 mb-0" :messages="messagesError" />

          <div ref="messageList" class="chat-messages">
            <template v-for="item in items" :key="item.key">
              <div v-if="item.type === 'day'" class="chat-day">{{ item.label }}</div>
              <div v-else class="chat-bubble" :class="getBubbleClass(item.message)">
                              <div class="chat-sender" :class="item.message.isStaff ? 'text-secondary' : 'text-primary'">{{ getSenderLabel(item.message) }}</div>
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

        <!-- Gönderme hatası -->
        <AlertMessages v-if="sendError.length" class="m-3 mb-0" :messages="sendError" />

        <!-- Seçilen referans: gönderilene kadar burada durur, istenirse kaldırılır -->
        <div v-if="reference" class="d-flex align-items-center gap-2 px-3 pt-2">
          <div class="chat-reference text-break flex-grow-1" :class="getReferenceClassByType(reference.type)">
            <span class="chat-reference-type">{{ REFERENCE_TYPES[reference.type] }}</span>
            {{ reference.title }}
          </div>
          <button type="button" class="btn-close" aria-label="Referansı kaldır" :disabled="isSending" @click="reference = null"></button>
        </div>

        <!-- Mesaj yazma satırı -->
        <form class="chat-composer border-top" autocomplete="off" novalidate @submit.prevent="sendMessage">
          <div class="dropup">
            <button
              type="button"
              class="btn btn-outline-secondary"
              data-bs-toggle="dropdown"
              aria-expanded="false"
              aria-label="Ekle"
              :disabled="selectedConversationID === null || isSending"
            >
              +
            </button>
            <ul class="dropdown-menu">
              <li><h6 class="dropdown-header">Referans ver</h6></li>
              <li><button type="button" class="dropdown-item" @click="openPicker(REFERENCE_ANNOUNCEMENT)">Duyuru</button></li>
              <li><button type="button" class="dropdown-item" @click="openPicker(REFERENCE_PROJECT)">Proje</button></li>
              <!-- Görev sadece personelle birebir sohbette verilir; grupta pasif -->
              <li>
                <button type="button" class="dropdown-item" :disabled="selectedConversation?.isGroup" @click="openPicker(REFERENCE_TASK)">Görev</button>
              </li>
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
            :disabled="selectedConversationID === null || isSending"
            @keydown.enter="onEnter"
          ></textarea>
          <button type="submit" class="btn btn-outline-primary" :disabled="!canSend">Gönder</button>
        </form>
      </section>
    </div>

    <ReferencePickerModal
      v-model="isPickerOpen"
      :title="PICKER_TITLES[pickerType]"
      :sections="pickerSections"
      :is-loading="isPickerLoading"
      :errors="pickerErrors"
      :empty-text="PICKER_EMPTY_TEXTS[pickerType]"
      @picked="onReferencePicked"
    />
  </div>
</template>