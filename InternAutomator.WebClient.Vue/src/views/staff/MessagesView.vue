<script setup>

import { getAllProjects } from '@/api/projects'
import { getAllInterns } from '@/api/interns'
import BaseModal from '@/components/BaseModal.vue'
import { ref, computed, nextTick, onMounted } from 'vue'
import { getAllConversations } from '@/api/conversations'
import { getAllMessages, addMessage } from '@/api/messages'
import { loadConversationRecency, orderConversations } from '@/utils/conversationOrder'
import { getErrorMessages } from '@/api/http'
import { getLatestAnnouncements } from '@/api/announcements'
import { MESSAGE_LIMITS, REFERENCE_NONE, REFERENCE_TYPES, REFERENCE_ANNOUNCEMENT, REFERENCE_PROJECT, REFERENCE_TASK } from '@/constants'
import { formatTime, formatDayLabel } from '@/utils/format'
import AlertMessages from '@/components/AlertMessages.vue'
import ReferencePickerModal from '@/components/ReferencePickerModal.vue'
import '@/assets/css/messages.css'


const conversations = ref([])
const isLoading = ref(true)
const loadError = ref([])
const selectedConversationID = ref(null)
const lastMessageDates=ref({})
const sortedConversations=computed(()=>orderConversations(conversations.value,lastMessageDates.value))
async function refreshChatOrder(items){
  await loadConversationRecency(items,(id,time)=>{
    lastMessageDates.value={...lastMessageDates.value,[id]:time}
  })
}

// Grup üyesi seçimi hazırdır; kayıt endpoint'i henüz API sözleşmesinde yoktur.
const isMemberPickerOpen=ref(false)
const memberList=ref([])
const memberSearch=ref('')
const selectedMemberID=ref(null)
const isMemberListLoading=ref(false)
const memberLoadErrors=ref([])
const filteredMembers = computed(() => {
  const query = memberSearch.value.toLocaleLowerCase('tr-TR')
  return [...memberList.value]
    .sort((a, b) => {
      const activeA = a.isActive === true || a.isActive === 1
      const activeB = b.isActive === true || b.isActive === 1
      const activeCompare = Number(activeB) - Number(activeA)
      if (activeCompare !== 0) return activeCompare
      const nameA = `${a.firstName ?? ''} ${a.lastName ?? ''}`.trim().toLocaleLowerCase('tr-TR')
      const nameB = `${b.firstName ?? ''} ${b.lastName ?? ''}`.trim().toLocaleLowerCase('tr-TR')
      return nameA.localeCompare(nameB, 'tr')
    })
    .filter((i) => `${i.firstName ?? ''} ${i.lastName ?? ''} ${i.email ?? ''}`.toLocaleLowerCase('tr-TR').includes(query))
})
async function openMemberPicker(){
  if(!selectedConversation.value?.isGroup)return
  memberSearch.value='';selectedMemberID.value=null;memberLoadErrors.value=[]
  isMemberPickerOpen.value=true;isMemberListLoading.value=true
  try{const list=await getAllInterns();memberList.value=Array.isArray(list)?list:[]}
  catch(error){memberLoadErrors.value=getErrorMessages(error)}
  finally{isMemberListLoading.value=false}
}
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
    void refreshChatOrder(conversations.value)
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
    const dates=(Array.isArray(result)?result:[]).map(m=>new Date(m.createdDate).getTime()).filter(Number.isFinite)
    lastMessageDates.value={...lastMessageDates.value,[conversationID]:dates.length?Math.max(...dates):0}
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
  // Yarım yazılan metin ve referans başka sohbete gitmesin
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

const PICKER_TITLES = { [REFERENCE_ANNOUNCEMENT]: 'Duyuru seç', [REFERENCE_PROJECT]: 'Proje seç' }
const PICKER_EMPTY_TEXTS = { [REFERENCE_ANNOUNCEMENT]: 'Henüz duyuru yok.', [REFERENCE_PROJECT]: 'Henüz proje yok.' }
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

// Görev buraya eklenecek: her tür kendi bölümlerini döndürür
const PICKER_LOADERS = {
  [REFERENCE_ANNOUNCEMENT]: loadAnnouncementSections,
  [REFERENCE_PROJECT]: loadProjectSections,
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

          <template v-for="(conversation, index) in sortedConversations" :key="conversation.conversationID">
          <div v-if="index === 0 || (sortedConversations[index-1]?.isGroup && !conversation.isGroup)" class="chat-list-section-title">{{ conversation.isGroup ? 'Gruplar' : 'Sohbetler' }}</div>
          <button
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
          </template>
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
          <div class="fw-semibold text-break flex-grow-1">{{ selectedConversation.conversationName }}</div>
          <button v-if="selectedConversation.isGroup" type="button" class="btn btn-sm btn-outline-primary" @click="openMemberPicker">+ Üye Ekle</button>
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
              <li><button type="button" class="dropdown-item" disabled>Görev</button></li>
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

    <!-- Üye seçimi: grup üyeliği kaydetme endpoint'i henüz sağlanmadı. -->
    <BaseModal v-model="isMemberPickerOpen" title="Gruba Üye Ekle" dialog-class="modal-dialog-scrollable">
      <p class="small text-muted">{{ selectedConversation?.conversationName }} grubuna eklenecek stajyeri seçin.</p>
      <AlertMessages v-if="isMemberListLoading" type="info" messages="Stajyerler yükleniyor..." />
      <AlertMessages v-else-if="memberLoadErrors.length" :messages="memberLoadErrors" />
      <template v-else>
        <input v-model="memberSearch" type="search" class="form-control mb-3" placeholder="Stajyer ara..." aria-label="Stajyer ara" />
        <div class="chat-member-picker" role="radiogroup" aria-label="Gruba eklenecek stajyer">
          <label v-for="person in filteredMembers" :key="person.internID" class="chat-member-row" :class="{selected:selectedMemberID===person.internID}">
            <input v-model="selectedMemberID" type="radio" name="member" :value="person.internID" />
            <span><strong>{{person.firstName}} {{person.lastName}}</strong><small class="d-block text-muted">{{person.email}}</small></span>
            <span class="badge" :class="person.isActive ? 'text-bg-success' : 'text-bg-secondary'">{{person.isActive?'Aktif':'Ayrıldı'}}</span>
          </label>
          <div v-if="filteredMembers.length===0" class="text-muted text-center py-3">Stajyer bulunamadı.</div>
        </div>
        <div v-if="selectedMemberID !== null" class="alert alert-warning mt-3 mb-0 small">
          Seçim yapıldı. Ancak grup üyeliğini kaydedecek API endpoint'i henüz mevcut değil; işlem tamamlanamaz.
        </div>
      </template>
      <template #footer>
        <button type="button" class="btn btn-outline-secondary" data-bs-dismiss="modal">Kapat</button>
        <button type="button" class="btn btn-primary" disabled title="Gruba üye ekleme endpoint'i gerekli">Gruba Ekle</button>
      </template>
    </BaseModal>

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
