<script setup>
import { ref, computed, nextTick, onMounted } from 'vue'
import {
  getAllConversations,
  createPrivateConversation,
  createGroupConversation,
  addInternToConversation,
  getAllConversationInterns,
  removeInternFromConversation,
} from '@/api/conversations'
import { getAllInterns } from '@/api/interns'
import { getAllMessages, addMessage } from '@/api/messages'
import { getAllProjects } from '@/api/projects'
import { getInternTasks } from '@/api/tasks'
import { getLatestAnnouncements } from '@/api/announcements'
import { getErrorMessages } from '@/api/http'
import { loadConversationRecency, orderConversations } from '@/utils/conversationOrder'
import {
  INTERN_ACTIVE,
  MESSAGE_LIMITS,
  REFERENCE_NONE,
  REFERENCE_TYPES,
  REFERENCE_ANNOUNCEMENT,
  REFERENCE_PROJECT,
  REFERENCE_TASK,
} from '@/constants'
import { formatTime, formatDayLabel, splitTechnologies } from '@/utils/format'
import AlertMessages from '@/components/AlertMessages.vue'
import BaseModal from '@/components/BaseModal.vue'
import ReferencePickerModal from '@/components/ReferencePickerModal.vue'
import '@/assets/css/messages.css'

const conversations = ref([])
const isLoading = ref(true)
const loadError = ref([])
const selectedConversationID = ref(null)
const lastMessageDates = ref({})
const sortedConversations = computed(() =>
  orderConversations(conversations.value, lastMessageDates.value)
)

async function refreshChatOrder(items) {
  await loadConversationRecency(items, (id, time) => {
    lastMessageDates.value = { ...lastMessageDates.value, [id]: time }
  })
}

// ---------- Grup bilgileri / üyeler / yeni üye ekleme (yalnızca personel) ----------
// Tek modal içinde iki görünüm kullanılır: "members" ve "add".
// İkinci Bootstrap modal açılmadığı için üst üste modal sorunu yaşanmaz.
const isGroupInfoOpen = ref(false)
const groupInfoView = ref('members')
const groupInfoConversation = ref(null)
const groupMembers = ref([])
const isGroupMembersLoading = ref(false)
const groupMembersErrors = ref([])
let groupMembersRequestID = 0

// Mevcut üyeler API yanıtından gelir; frontend sahte üyelik oluşturmaz.
const existingGroupMemberIDs = computed(() =>
  new Set(groupMembers.value.map((member) => Number(member.internID)))
)

const memberList = ref([])
const memberSearch = ref('')
const selectedMemberID = ref(null)
const isMemberListLoading = ref(false)
const isAddingMember = ref(false)
const memberLoadErrors = ref([])
const memberSaveErrors = ref([])
const memberSuccess = ref('')

// Üye çıkarma: onay aynı üye satırında gösterilir, ikinci modal açılmaz.
const pendingRemovalInternID = ref(null)
const isRemovingMember = ref(false)
const removeMemberErrors = ref([])

function requestRemoveMember(member) {
  if (!groupInfoConversation.value?.isGroup || !isGroupInfoOpen.value) return
  if (isRemovingMember.value || isGroupMembersLoading.value || groupMembersErrors.value.length) return
  const internID = Number(member?.internID)
  if (!Number.isSafeInteger(internID) || internID <= 0) return
  if (!existingGroupMemberIDs.value.has(internID)) return
  pendingRemovalInternID.value = internID
  removeMemberErrors.value = []
  memberSuccess.value = ''
}

function cancelRemoveMember() {
  if (isRemovingMember.value) return
  pendingRemovalInternID.value = null
  removeMemberErrors.value = []
}

async function confirmRemoveMember() {
  if (!groupInfoConversation.value?.isGroup) return
  if (isRemovingMember.value || isGroupMembersLoading.value || !isGroupInfoOpen.value) return
  const conversationID = Number(groupInfoConversation.value?.conversationID)
  const internID = Number(pendingRemovalInternID.value)
  if (!Number.isSafeInteger(conversationID) || conversationID <= 0) return
  if (!Number.isSafeInteger(internID) || internID <= 0) return
  if (!existingGroupMemberIDs.value.has(internID)) return

  isRemovingMember.value = true
  removeMemberErrors.value = []
  memberSuccess.value = ''
  try {
    await removeInternFromConversation(conversationID, internID)
    // Üye yalnızca backend yanıtıyla yenilenen listeden kaybolur.
    pendingRemovalInternID.value = null
    const refreshed = await loadGroupMembers(conversationID)
    if (!refreshed) {
      memberSuccess.value = 'Çıkarma isteği başarılı; üye listesi yenilenemedi. Yeniden Dene ile listeyi güncelleyin.'
    } else if (existingGroupMemberIDs.value.has(internID)) {
      memberSuccess.value = 'API başarılı yanıt verdi ancak stajyer hâlâ üye listesinde. Sunucu kaydını kontrol edin.'
    } else {
      memberSuccess.value = 'Stajyer gruptan çıkarıldı.'
    }
  } catch (error) {
    removeMemberErrors.value = getErrorMessages(error)
  } finally {
    isRemovingMember.value = false
  }
}

const filteredMembers = computed(() => {
  const words = memberSearch.value.toLocaleLowerCase('tr-TR').trim().split(/\s+/).filter(Boolean)
  return [...memberList.value]
    .filter((intern) => !existingGroupMemberIDs.value.has(Number(intern.internID)))
    .sort((a, b) => {
      const activeA = a.isActive === INTERN_ACTIVE
      const activeB = b.isActive === INTERN_ACTIVE
      if (activeA !== activeB) return Number(activeB) - Number(activeA)
      return `${a.firstName ?? ''} ${a.lastName ?? ''}`.localeCompare(
        `${b.firstName ?? ''} ${b.lastName ?? ''}`, 'tr'
      )
    })
    .filter((intern) => {
      const searchText = `${intern.firstName ?? ''} ${intern.lastName ?? ''} ${intern.email ?? ''}`
        .toLocaleLowerCase('tr-TR')
      return words.every((word) => searchText.includes(word))
    })
})

const selectedMember = computed(() =>
  filteredMembers.value.find((intern) => Number(intern.internID) === Number(selectedMemberID.value)) ?? null
)

async function loadGroupMembers(conversationID) {
  const currentRequest = ++groupMembersRequestID
  isGroupMembersLoading.value = true
  groupMembersErrors.value = []
  try {
    const result = await getAllConversationInterns(conversationID)
    if (!Array.isArray(result)) {
      throw new Error('Grup üyeleri beklenen biçimde alınamadı.')
    }
    // Eski bir isteğin yanıtı farklı bir gruba yazılmasın.
    if (currentRequest !== groupMembersRequestID) return false
    if (Number(groupInfoConversation.value?.conversationID) !== conversationID) return false
    const uniqueMembers = new Map()
    for (const member of result) {
      const id = Number(member.internID)
      if (Number.isSafeInteger(id) && id > 0) uniqueMembers.set(id, member)
    }
    groupMembers.value = [...uniqueMembers.values()]
    return true
  } catch (error) {
    if (currentRequest === groupMembersRequestID) {
      groupMembersErrors.value = getErrorMessages(error)
    }
    return false
  } finally {
    if (currentRequest === groupMembersRequestID) isGroupMembersLoading.value = false
  }
}

async function openConversationInfo() {
  const conversation = selectedConversation.value
  if (!conversation) return
  const conversationID = Number(conversation.conversationID)
  if (!Number.isSafeInteger(conversationID) || conversationID <= 0) return

  groupInfoConversation.value = {
    conversationID,
    conversationName: conversation.conversationName,
    isGroup: conversation.isGroup === true,
  }
  groupInfoView.value = 'members'
  groupMembers.value = []
  memberSuccess.value = ''
  memberSaveErrors.value = []
  pendingRemovalInternID.value = null
  removeMemberErrors.value = []
  isGroupInfoOpen.value = true
  await loadGroupMembers(conversationID)
}

async function openMemberPicker() {
  if (isGroupMembersLoading.value || groupMembersErrors.value.length || isAddingMember.value || isRemovingMember.value) return
  if (!groupInfoConversation.value?.isGroup) return

  // Bir üye başka yerden eklenmişse eski liste üzerinden seçim yaptırma.
  const refreshed = await loadGroupMembers(Number(groupInfoConversation.value.conversationID))
  if (!refreshed || !groupInfoConversation.value?.isGroup || !isGroupInfoOpen.value) return

  pendingRemovalInternID.value = null
  removeMemberErrors.value = []
  groupInfoView.value = 'add'
  memberSearch.value = ''
  selectedMemberID.value = null
  memberList.value = []
  memberLoadErrors.value = []
  memberSaveErrors.value = []
  isMemberListLoading.value = true

  try {
    const interns = await getAllInterns()
    if (!Array.isArray(interns)) throw new Error('Stajyer listesi beklenen biçimde alınamadı.')
    // Aktif ve pasif stajyerler listelenir; aynı ID yalnızca bir kez gösterilir.
    const uniqueInterns = new Map()
    for (const intern of interns) {
      const id = Number(intern.internID)
      if (Number.isSafeInteger(id) && id > 0) uniqueInterns.set(id, intern)
    }
    memberList.value = [...uniqueInterns.values()]
  } catch (error) {
    memberLoadErrors.value = getErrorMessages(error)
  } finally {
    isMemberListLoading.value = false
  }
}

async function submitAddMember() {
  if (!groupInfoConversation.value?.isGroup || isAddingMember.value || isMemberListLoading.value || !selectedMember.value) return
  const conversationID = Number(groupInfoConversation.value?.conversationID)
  const internID = Number(selectedMember.value.internID)
  if (!Number.isSafeInteger(conversationID) || conversationID <= 0) return
  if (!Number.isSafeInteger(internID) || internID <= 0) return
  if (existingGroupMemberIDs.value.has(internID)) return

  isAddingMember.value = true
  memberSaveErrors.value = []
  memberSuccess.value = ''

  try {
    // API başarılıysa (body boş olabilir), güncel üye listesi tekrar sunucudan alınır.
    await addInternToConversation(conversationID, internID)
    const refreshed = await loadGroupMembers(conversationID)
    if (!refreshed) {
      // AddIntern başarılı oldu, ancak liste güncellenemedi. Tekrar ekleme önermiyoruz.
      groupInfoView.value = 'members'
      memberSuccess.value = 'Üyelik isteği başarılı. Üye listesi yenilenemedi; yeniden deneyin.'
      return
    }
    groupInfoView.value = 'members'
    memberSuccess.value = 'Üyelik işlemi başarılı. Grup üyeleri güncellendi.'
  } catch (error) {
    memberSaveErrors.value = getErrorMessages(error)
  } finally {
    isAddingMember.value = false
  }
}

const selectedConversation = computed(
  () => conversations.value.find((conversation) => Number(conversation.conversationID) === selectedConversationID.value) ?? null,
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
  // API'den gelen numara bazen string olabilir.
  const id = Number(conversationID)
  if (!Number.isSafeInteger(id) || id <= 0) return

  if (selectedConversationID.value === id) {
    // Özellikle yeniden açılan mevcut bir sohbet için mesajları tazele.
    void loadMessages(false)
    return
  }

  selectedConversationID.value = id
  memberSuccess.value = ''
  // Önceki sohbetten seçici / ipucu / referans taşınmasın.
  ++pickerRequestId
  isPickerOpen.value = false
  isTaskRuleVisible.value = false
  draft.value = ''
  reference.value = null
  sendError.value = []
  void loadMessages()
}

// ---------- Referans seçme (+ menüsü) ----------
// Mesaja eklenecek içerik: { id, type, title } ya da null. Gönderilene kadar yazma satırının üstünde durur.
const reference = ref(null)

// Görev referansları yalnızca özel sohbetlerde kullanılabilir.
const isTaskReferenceDisabled = computed(() => !selectedConversation.value || selectedConversation.value.isGroup)
const isTaskRuleVisible = ref(false)

const isPickerOpen = ref(false)
const pickerType = ref(REFERENCE_ANNOUNCEMENT)
const pickerSections = ref([])
const isPickerLoading = ref(false)
const pickerErrors = ref([])
let pickerRequestId = 0

const PICKER_TITLES = {
  [REFERENCE_ANNOUNCEMENT]: 'Duyuru seç',
  [REFERENCE_PROJECT]: 'Proje seç',
  [REFERENCE_TASK]: 'Görev seç',
}
const PICKER_EMPTY_TEXTS = {
  [REFERENCE_ANNOUNCEMENT]: 'Henüz duyuru yok.',
  [REFERENCE_PROJECT]: 'Henüz proje yok.',
  [REFERENCE_TASK]: 'Bu stajyere henüz görev verilmemiş.',
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

// Projeler en çok alınandan en aza doğru gösterilir. Kartta sadece başlık ve kullanılan
// teknolojiler görünür; kaç kişinin aldığı bilgisi gösterilmez.
async function loadProjectSections() {
  const projects = await getAllProjects()
  return [
    {
      items: [...projects]
        .sort((a, b) => (Number(b.takenByInternCount) || 0) - (Number(a.takenByInternCount) || 0))
        .map((project) => ({
          id: project.projectID,
          title: project.title,
          chips: splitTechnologies(project.technologies),
          subtitle: splitTechnologies(project.technologies).length === 0 ? 'Teknoloji bilgisi belirtilmemiş.' : '',
        })),
    },
  ]
}

// Özel sohbetteki tek stajyeri bulup yalnızca ona atanmış görevleri gösterir.
async function loadTaskSections() {
  const conversation = selectedConversation.value
  if (!conversation || conversation.isGroup) return []

  const conversationID = Number(conversation.conversationID)
  const members = await getAllConversationInterns(conversationID)
  if (!Array.isArray(members)) {
    throw new Error('Sohbetin stajyer bilgileri alınamadı.')
  }

  const internIDs = [...new Set(
    members
      .map((member) => Number(member.internID))
      .filter((id) => Number.isSafeInteger(id) && id > 0)
  )]
  if (internIDs.length !== 1) {
    throw new Error('Özel sohbete ait stajyer belirlenemedi.')
  }

  const tasks = await getInternTasks(internIDs[0])
  if (!Array.isArray(tasks)) {
    throw new Error('Görev listesi beklenen biçimde alınamadı.')
  }

  return [{
    items: tasks.map((task) => ({
      id: task.taskID,
      title: task.title,
      subtitle: task.description,
    })),
  }]
}

const PICKER_LOADERS = {
  [REFERENCE_ANNOUNCEMENT]: loadAnnouncementSections,
  [REFERENCE_PROJECT]: loadProjectSections,
  [REFERENCE_TASK]: loadTaskSections,
}
async function openPicker(type) {
  // Gruptan programatik olarak da görev seçicisi açılamasın.
  if (type === REFERENCE_TASK && isTaskReferenceDisabled.value) return
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
// ---------- Yeni özel sohbet (yalnızca personel) ----------
const isNewChatOpen = ref(false)
const availableInterns = ref([])
const selectedInternID = ref(null)
const isInternsLoading = ref(false)
const isCreatingChat = ref(false)
const newChatErrors = ref([])
const newChatSearchText = ref('')
const appliedNewChatSearch = ref('')

// Liste pencere açılır açılmaz görünür; Ara butonu filtreyi uygular.
const filteredChatInterns = computed(() => {
  const words = appliedNewChatSearch.value
    .toLocaleLowerCase('tr-TR')
    .split(/\s+/)
    .filter(Boolean)

  return availableInterns.value.filter((intern) => {
    const searchableText = [
      intern.firstName,
      intern.lastName,
      intern.email,
    ].filter(Boolean).join(' ').toLocaleLowerCase('tr-TR')
    return words.every((word) => searchableText.includes(word))
  })
})

function searchNewChat() {
  appliedNewChatSearch.value = newChatSearchText.value.trim()
}

function clearNewChatSearch() {
  newChatSearchText.value = ''
  appliedNewChatSearch.value = ''
}

const selectedIntern = computed(() =>
  availableInterns.value.find(
    (intern) => Number(intern.internID) === Number(selectedInternID.value)
  ) ?? null
)

async function openNewChat() {
  if (isInternsLoading.value || isCreatingChat.value) return

  selectedInternID.value = null
  availableInterns.value = []
  newChatErrors.value = []
  clearNewChatSearch()
  isNewChatOpen.value = true
  isInternsLoading.value = true

  try {
    const interns = await getAllInterns()
    if (!Array.isArray(interns)) {
      throw new Error('Stajyer listesi beklenen biçimde alınamadı.')
    }

    // Personel, stajı bitmiş stajyerlerle de birebir sohbet başlatabilir.
    availableInterns.value = [...interns]
      .sort((a, b) =>
        `${a.firstName ?? ''} ${a.lastName ?? ''}`.localeCompare(
          `${b.firstName ?? ''} ${b.lastName ?? ''}`,
          'tr'
        )
      )
  } catch (error) {
    newChatErrors.value = getErrorMessages(error)
  } finally {
    isInternsLoading.value = false
  }
}

// API şu an başarılı istekte doğrudan "15" veya 15 döndürüyor.
function parseConversationID(response) {
  const value = response && typeof response === 'object'
    ? response.conversationID
    : response
  const cleaned = typeof value === 'string'
    ? value.trim().replace(/^"(.*)"$/, '$1')
    : value
  const id = Number(cleaned)

  if (!Number.isSafeInteger(id) || id <= 0) {
    throw new Error('API geçerli bir sohbet ID döndürmedi.')
  }
  return id
}

async function submitNewChat() {
  if (isCreatingChat.value || isInternsLoading.value || !selectedIntern.value) return

  const intern = selectedIntern.value
  isCreatingChat.value = true
  newChatErrors.value = []

  try {
    const conversationName = `${intern.firstName ?? ''} ${intern.lastName ?? ''}`.trim()
    const response = await createPrivateConversation(Number(intern.internID), {
      conversationName,
      isGroup: false,
    })
    const conversationID = parseConversationID(response)

    // API, mevcut sohbetin ID'sini veya yeni oluşturulan sohbetin ID'sini döndürür.
    // Önce sunucudaki gerçek sohbet listesini yeniden alıyoruz.
    const serverConversations = await getAllConversations()
    if (!Array.isArray(serverConversations)) {
      throw new Error('Sunucudan sohbet listesi alınamadı. Sohbet açılmadı.')
    }

    // İsim eşleştirmesi yapma: aynı isimli iki stajyer olabilir.
    // API'dan gelen ID sunucu listesinde yoksa frontend'de sahte sohbet oluşturma.
    const serverConversation = serverConversations.find(
      (item) => Number(item.conversationID) === conversationID && item.isGroup === false
    )
    conversations.value = serverConversations
    void refreshChatOrder(serverConversations)

    if (!serverConversation) {
      throw new Error(
        `API ${conversationID} numaralı sohbeti döndürdü fakat bu sohbet Conversation/All listesinde bulunamadı. ` +
        'Lütfen CreatePrivateConversation ve Conversation/All endpointlerini kontrol edin.'
      )
    }

    // Hem mevcut hem yeni sohbet için sadece sunucuda doğrulanmış ID'yi aç.
    isNewChatOpen.value = false
    selectConversation(conversationID)
  } catch (error) {
    newChatErrors.value = getErrorMessages(error)
  } finally {
    isCreatingChat.value = false
  }
}


// ---------- Yeni grup oluşturma (yalnızca personel) ----------
const isNewGroupOpen = ref(false)
const groupName = ref('')
const groupInterns = ref([])
const groupSearchText = ref('')
const selectedGroupInternIDs = ref([])
const isGroupInternsLoading = ref(false)
const isCreatingGroup = ref(false)
const groupErrors = ref([])

const filteredGroupInterns = computed(() => {
  const words = groupSearchText.value.toLocaleLowerCase('tr-TR').trim().split(/\s+/).filter(Boolean)
  return groupInterns.value.filter((intern) => {
    const text = `${intern.firstName ?? ''} ${intern.lastName ?? ''} ${intern.email ?? ''}`
      .toLocaleLowerCase('tr-TR')
    return words.every((word) => text.includes(word))
  })
})

async function openNewGroup() {
  if (isCreatingGroup.value) return

  groupName.value = ''
  groupSearchText.value = ''
  selectedGroupInternIDs.value = []
  groupInterns.value = []
  groupErrors.value = []
  isNewGroupOpen.value = true  // Liste otomatik açılır, ikinci tıklama gerekmez.
  isGroupInternsLoading.value = true

  try {
    const interns = await getAllInterns()
    if (!Array.isArray(interns)) throw new Error('Stajyer listesi beklenen biçimde alınamadı.')
    // Aktif/pasif ayrımı yok. Aynı ID kazara iki kez listelenirse tek kez göster.
    const uniqueInterns = new Map()
    for (const intern of interns) {
      const id = Number(intern.internID)
      if (Number.isSafeInteger(id) && id > 0) uniqueInterns.set(id, intern)
    }
    groupInterns.value = [...uniqueInterns.values()].sort((a, b) =>
      `${a.firstName ?? ''} ${a.lastName ?? ''}`.localeCompare(
        `${b.firstName ?? ''} ${b.lastName ?? ''}`, 'tr'
      )
    )
  } catch (error) {
    // Liste alınamazsa yine de boş grup oluşturulabilir.
    groupErrors.value = getErrorMessages(error)
  } finally {
    isGroupInternsLoading.value = false
  }
}

async function submitNewGroup() {
  if (isCreatingGroup.value || !groupName.value.trim()) return

  const name = groupName.value.trim()
  const allowedIDs = new Set(groupInterns.value.map((intern) => Number(intern.internID)))
  // JSON array, backend HashSet<int> olarak karşılar. Sadece listede bulunan ID'ler.
  const interIDs = [...new Set(selectedGroupInternIDs.value.map(Number))]
    .filter((id) => Number.isSafeInteger(id) && id > 0 && allowedIDs.has(id))

  isCreatingGroup.value = true
  groupErrors.value = []

  try {
    const response = await createGroupConversation({
      conversationName: name,
      interIDs,
    })
    const groupID = parseConversationID(response)

    // Önce veritabanı kaydını, sohbet listesi endpoint'inden doğrula.
    // Yalnızca frontend'de görünen geçici bir grup ASLA ekleme.
    const serverConversations = await getAllConversations()
    if (!Array.isArray(serverConversations)) {
      throw new Error('Grup işlemi sonrası sohbet listesi sunucudan alınamadı.')
    }
    conversations.value = serverConversations
    void refreshChatOrder(serverConversations)

    const createdGroup = serverConversations.find(
      (chat) => Number(chat.conversationID) === groupID && chat.isGroup === true
    )
    if (!createdGroup) {
      throw new Error(
        `API ${groupID} numaralı grubu döndürdü ancak Conversation/All listesinde doğrulanamadı. ` +
        'Oluşturma isteğini tekrar göndermeden önce veritabanını kontrol edin.'
      )
    }

    isNewGroupOpen.value = false
    selectConversation(groupID)
  } catch (error) {
    groupErrors.value = getErrorMessages(error)
  } finally {
    isCreatingGroup.value = false
  }
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
          <button
            type="button"
            class="btn btn-outline-primary"
            :disabled="isInternsLoading || isCreatingChat"
            @click="openNewChat"
          >
            Yeni sohbet
          </button>
          <button
            type="button"
            class="btn btn-outline-primary"
            :disabled="isCreatingGroup"
            @click="openNewGroup"
          >
            Yeni grup
          </button>
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
            :class="{ active: Number(conversation.conversationID) === selectedConversationID }"
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
          <button
            type="button"
            class="btn btn-link text-decoration-none text-body d-flex align-items-center gap-3 p-0 text-start flex-grow-1"
            :aria-label="selectedConversation.isGroup ? 'Grup bilgilerini görüntüle' : 'Sohbet bilgilerini görüntüle'"
            :title="selectedConversation.isGroup ? 'Grup bilgileri' : 'Sohbet bilgileri'"
            @click="openConversationInfo"
          >
            <span
              class="avatar-circle rounded-circle text-white d-flex align-items-center justify-content-center fw-semibold"
              :class="selectedConversation.isGroup ? 'bg-success' : 'bg-primary'"
            >
              {{ getConversationInitials(selectedConversation.conversationName) }}
            </span>
            <span class="fw-semibold text-break">{{ selectedConversation.conversationName }}</span>
          </button>
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
              <li v-if="!isTaskReferenceDisabled">
                <button
                  type="button"
                  class="dropdown-item"
                  @click="openPicker(REFERENCE_TASK)"
                >
                  Görev
                </button>
              </li>
              <li
                v-else
                class="task-reference-rule-item"
                @mouseenter="isTaskRuleVisible = true"
                @mouseleave="isTaskRuleVisible = false"
              >
                <!-- Gerçek disabled yerine aria-disabled kullanılır: açıklama odak ve tıklamada da açılır. -->
                <button
                  type="button"
                  class="dropdown-item task-reference-disabled"
                  aria-disabled="true"
                  aria-describedby="task-reference-rule"
                  @focus="isTaskRuleVisible = true"
                  @blur="isTaskRuleVisible = false"
                  @click.stop.prevent="isTaskRuleVisible = true"
                >
                  Görev <span class="task-reference-info-icon" aria-hidden="true">ⓘ</span>
                </button>
                <div
                  v-show="isTaskRuleVisible"
                  id="task-reference-rule"
                  class="task-reference-rule-tooltip"
                  role="tooltip"
                >
                  Görevler kişiye özeldir. Görevlere yalnızca birebir sohbetlerde referans verebilirsiniz.
                </div>
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

    <!-- Yeni sohbet: personel stajyer seçer, API sohbeti oluşturur veya mevcut ID'yi döndürür. -->
    <BaseModal
      v-model="isNewChatOpen"
      title="Yeni Sohbet"
      as-form
      @submit="submitNewChat"
    >
      <label for="newChatSearch" class="form-label">
        Sohbet başlatmak istediğiniz stajyeri seçin
      </label>

      <div class="input-group mb-3">
        <input
          id="newChatSearch"
          v-model="newChatSearchText"
          type="search"
          class="form-control"
          placeholder="Ad, soyad veya e-posta ara..."
          autocomplete="off"
          :disabled="isInternsLoading || isCreatingChat"
          @keydown.enter.prevent="searchNewChat"
        />
        <button
          type="button"
          class="btn btn-outline-primary"
          :disabled="isInternsLoading || isCreatingChat"
          @click="searchNewChat"
        >
          Ara
        </button>
        <button
          type="button"
          class="btn btn-outline-secondary"
          :disabled="isInternsLoading || isCreatingChat || (!newChatSearchText && !appliedNewChatSearch)"
          @click="clearNewChatSearch"
        >
          Temizle
        </button>
      </div>

      <AlertMessages
        v-if="isInternsLoading"
        type="info"
        messages="Stajyerler yükleniyor..."
      />
      <AlertMessages
        v-else-if="newChatErrors.length"
        :messages="newChatErrors"
      />
      <p
        v-else-if="availableInterns.length === 0"
        class="text-muted small mb-0"
      >
        Henüz stajyer bulunamadı.
      </p>
      <template v-else>
        <p class="small text-muted mb-2">
          {{ filteredChatInterns.length }} stajyer gösteriliyor
        </p>
        <div
          class="chat-member-picker"
          role="radiogroup"
          aria-label="Sohbet başlatılacak stajyer"
          style="max-height: 320px; overflow-y: auto"
        >
          <label
            v-for="intern in filteredChatInterns"
            :key="intern.internID"
            class="chat-member-row"
            :class="{ selected: selectedInternID === Number(intern.internID) }"
          >
            <input
              v-model="selectedInternID"
              type="radio"
              name="newChatIntern"
              :value="Number(intern.internID)"
              :disabled="isCreatingChat"
            />
            <span>
              <strong>{{ intern.firstName }} {{ intern.lastName }}</strong>
              <small v-if="intern.email" class="d-block text-muted">{{ intern.email }}</small>
            </span>
            <span class="badge" :class="intern.isActive === INTERN_ACTIVE ? 'text-bg-success' : 'text-bg-secondary'">
              {{ intern.isActive === INTERN_ACTIVE ? 'Aktif' : 'Ayrıldı' }}
            </span>
          </label>
          <p v-if="filteredChatInterns.length === 0" class="text-muted small text-center py-3 mb-0">
            Aramanızla eşleşen stajyer bulunamadı.
          </p>
        </div>
      </template>

      <template #footer>
        <button
          type="button"
          class="btn btn-secondary"
          data-bs-dismiss="modal"
          :disabled="isCreatingChat"
        >
          Vazgeç
        </button>
        <button
          type="submit"
          class="btn btn-primary"
          :disabled="!selectedIntern || isInternsLoading || isCreatingChat"
        >
          {{ isCreatingChat ? 'İşleniyor...' : 'Sohbeti Aç' }}
        </button>
      </template>
    </BaseModal>


    <!-- Yeni grup: ad + isteğe bağlı çoklu stajyer seçimi. -->
    <BaseModal
      v-model="isNewGroupOpen"
      title="Yeni Grup Oluştur"
      dialog-class="modal-dialog-scrollable"
      as-form
      @submit="submitNewGroup"
    >
      <label for="groupConversationName" class="form-label fw-semibold">Grup adı</label>
      <input
        id="groupConversationName"
        v-model="groupName"
        type="text"
        class="form-control mb-3"
        placeholder="Örneğin: Yazılım Ekibi"
        autocomplete="off"
        :disabled="isCreatingGroup"
        required
      />

      <div class="d-flex justify-content-between align-items-center gap-2 mb-2">
        <label for="groupInternSearch" class="form-label fw-semibold mb-0">
          Üyeler (isteğe bağlı)
        </label>
        <span class="small text-muted">{{ selectedGroupInternIDs.length }} seçili</span>
      </div>
      <div class="input-group mb-2">
        <input
          id="groupInternSearch"
          v-model="groupSearchText"
          type="search"
          class="form-control"
          placeholder="Ad, soyad veya e-posta ara..."
          aria-label="Grup üyesi ara"
          :disabled="isCreatingGroup || isGroupInternsLoading"
          @keydown.enter.prevent
        />
        <button
          type="button"
          class="btn btn-outline-secondary"
          :disabled="!groupSearchText || isCreatingGroup || isGroupInternsLoading"
          @click="groupSearchText = ''"
        >
          Temizle
        </button>
      </div>

      <AlertMessages
        v-if="isGroupInternsLoading"
        type="info"
        messages="Stajyerler yükleniyor..."
      />
      <template v-else>
        <p v-if="groupInterns.length === 0" class="text-muted small mt-2">
          Listelenecek stajyer yok. Üye seçmeden de grup oluşturabilirsiniz.
        </p>
        <div
          v-else
          class="chat-member-picker"
          style="max-height: 320px; overflow-y: auto"
          aria-label="Gruba eklenecek stajyerler"
        >
          <label
            v-for="intern in filteredGroupInterns"
            :key="intern.internID"
            class="chat-member-row"
            :class="{ selected: selectedGroupInternIDs.includes(Number(intern.internID)) }"
          >
            <input
              v-model="selectedGroupInternIDs"
              type="checkbox"
              :value="Number(intern.internID)"
              :disabled="isCreatingGroup"
            />
            <span class="flex-grow-1">
              <strong>{{ intern.firstName }} {{ intern.lastName }}</strong>
              <small v-if="intern.email" class="d-block text-muted">{{ intern.email }}</small>
            </span>
            <span
              class="badge"
              :class="intern.isActive === INTERN_ACTIVE ? 'text-bg-success' : 'text-bg-secondary'"
            >
              {{ intern.isActive === INTERN_ACTIVE ? 'Aktif' : 'Ayrıldı' }}
            </span>
          </label>
          <p v-if="filteredGroupInterns.length === 0" class="text-muted small text-center py-3 mb-0">
            Aramanızla eşleşen stajyer bulunamadı.
          </p>
        </div>
      </template>

      <p class="small text-muted mt-2 mb-0">
        Üye seçmezseniz grup boş oluşturulur. Daha sonra üyeler eklenebilir.
      </p>
      <AlertMessages v-if="groupErrors.length" class="mt-3" :messages="groupErrors" />

      <template #footer>
        <button
          type="button"
          class="btn btn-secondary"
          data-bs-dismiss="modal"
          :disabled="isCreatingGroup"
        >
          Vazgeç
        </button>
        <button
          type="submit"
          class="btn btn-primary"
          :disabled="!groupName.trim() || isCreatingGroup"
        >
          {{ isCreatingGroup ? 'Oluşturuluyor...' : 'Grubu Oluştur' }}
        </button>
      </template>
    </BaseModal>

    <!-- Grup ve özel sohbet başlığına/avatarına tıklandığında aynı bilgi penceresi açılır. -->
    <BaseModal
      v-model="isGroupInfoOpen"
      :title="groupInfoView === 'add' ? 'Gruba Üye Ekle' : (groupInfoConversation?.isGroup ? 'Grup Bilgileri' : 'Sohbet Bilgileri')"
      dialog-class="modal-dialog-scrollable"
    >
      <template v-if="groupInfoView === 'members'">
        <div class="d-flex align-items-center gap-3 mb-3">
          <span
            class="avatar-circle rounded-circle text-white d-flex align-items-center justify-content-center fw-semibold"
            :class="groupInfoConversation?.isGroup ? 'bg-success' : 'bg-primary'"
          >
            {{ getConversationInitials(groupInfoConversation?.conversationName) }}
          </span>
          <div>
            <div class="fw-semibold text-break">{{ groupInfoConversation?.conversationName }}</div>
            <div class="small text-muted">{{ groupInfoConversation?.isGroup ? 'Grup sohbeti' : 'Özel sohbet' }}</div>
          </div>
        </div>

        <div class="d-flex justify-content-between align-items-center gap-2 mb-2">
          <span class="fw-semibold">{{ groupInfoConversation?.isGroup ? 'Üyeler' : 'Stajyer' }} ({{ groupMembers.length }})</span>
          <button
            v-if="groupInfoConversation?.isGroup"
            type="button"
            class="btn btn-sm btn-outline-primary"
            :disabled="isGroupMembersLoading || groupMembersErrors.length > 0 || isAddingMember || isRemovingMember"
            @click="openMemberPicker"
          >
            + Yeni Üye Ekle
          </button>
        </div>

        <AlertMessages v-if="memberSuccess" class="mb-3" type="success" :messages="memberSuccess" role="status" />
        <AlertMessages v-if="isGroupMembersLoading" type="info" messages="Grup üyeleri yükleniyor..." />
        <template v-else-if="groupMembersErrors.length">
          <AlertMessages :messages="groupMembersErrors" />
          <button
            type="button"
            class="btn btn-outline-secondary btn-sm mt-2"
            @click="loadGroupMembers(groupInfoConversation.conversationID)"
          >
            Yeniden Dene
          </button>
        </template>
        <p v-else-if="groupMembers.length === 0" class="text-muted small mb-0 py-3 text-center">
          {{ groupInfoConversation?.isGroup ? 'Bu grupta henüz stajyer bulunmuyor. Yeni üye ekleyebilirsiniz.' : 'Bu sohbete ait stajyer bilgisi bulunamadı.' }}
        </p>
        <div v-else class="chat-member-picker" style="max-height: 350px; overflow-y: auto" :aria-label="groupInfoConversation?.isGroup ? 'Grubun üyeleri' : 'Özel sohbetin stajyeri'">
          <div
            v-for="member in groupMembers"
            :key="member.internID"
            class="chat-member-row flex-wrap"
          >
            <div class="d-flex align-items-center gap-3 w-100">
              <span
                class="avatar-circle rounded-circle bg-primary text-white d-flex align-items-center justify-content-center fw-semibold"
                style="width: 36px; height: 36px; flex-shrink: 0"
              >
                {{ getConversationInitials(`${member.firstName ?? ''} ${member.lastName ?? ''}`) }}
              </span>
              <span class="flex-grow-1 text-break">
                <span class="fw-medium d-block">{{ `${member.firstName ?? ''} ${member.lastName ?? ''}`.trim() || `Stajyer #${member.internID}` }}</span>
                <small v-if="member.email" class="text-muted d-block">{{ member.email }}</small>
              </span>
              <button
                v-if="groupInfoConversation?.isGroup"
                type="button"
                class="btn btn-sm btn-outline-secondary d-inline-flex align-items-center justify-content-center"
                style="width: 32px; height: 32px; flex-shrink: 0"
                :aria-label="`${member.firstName ?? ''} ${member.lastName ?? ''} adlı stajyeri gruptan çıkar`"
                :aria-expanded="pendingRemovalInternID === Number(member.internID)"
                :aria-controls="`remove-member-confirm-${member.internID}`"
                title="Gruptan çıkar"
                :disabled="isRemovingMember || isGroupMembersLoading"
                @click.stop="requestRemoveMember(member)"
              >
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
                  <path d="M3 6h18" />
                  <path d="M8 6V4h8v2" />
                  <path d="M19 6l-1 14H6L5 6" />
                  <path d="M10 10v6" />
                  <path d="M14 10v6" />
                </svg>
              </button>
            </div>
            <!-- İkinci modal yerine, ilgili üyenin altında açılan küçük onay alanı. -->
            <div
              v-if="groupInfoConversation?.isGroup && pendingRemovalInternID === Number(member.internID)"
              :id="`remove-member-confirm-${member.internID}`"
              class="w-100 d-flex justify-content-end mt-2"
            >
              <div class="border rounded bg-body-tertiary p-2 shadow-sm" style="width: min(100%, 310px)" role="group" aria-label="Üyeyi çıkarma onayı">
                <p class="small mb-2">Bu stajyeri gruptan çıkarmak istiyor musunuz?</p>
                <AlertMessages v-if="removeMemberErrors.length" :messages="removeMemberErrors" class="mb-2" />
                <div class="d-flex justify-content-end gap-2">
                  <button
                    type="button"
                    class="btn btn-sm btn-outline-secondary"
                    :disabled="isRemovingMember"
                    @click="cancelRemoveMember"
                  >Vazgeç</button>
                  <button
                    type="button"
                    class="btn btn-sm btn-primary"
                    :disabled="isRemovingMember"
                    @click="confirmRemoveMember"
                  >{{ isRemovingMember ? 'Çıkarılıyor...' : 'Evet, çıkar' }}</button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </template>

      <template v-else>
        <p class="small text-muted mb-3">
          {{ groupInfoConversation?.conversationName }} grubuna eklenecek stajyeri seçin.
        </p>
        <AlertMessages v-if="isMemberListLoading" type="info" messages="Stajyerler yükleniyor..." />
        <AlertMessages v-else-if="memberLoadErrors.length" :messages="memberLoadErrors" />
        <template v-else>
          <input
            v-model="memberSearch"
            type="search"
            class="form-control mb-3"
            placeholder="Ad, soyad veya e-posta ara..."
            aria-label="Gruba eklenecek stajyeri ara"
            :disabled="isAddingMember"
            @keydown.enter.prevent
          />
          <div class="chat-member-picker" role="radiogroup" aria-label="Gruba eklenecek stajyer" style="max-height: 350px; overflow-y: auto">
            <label
              v-for="intern in filteredMembers"
              :key="intern.internID"
              class="chat-member-row"
              :class="{ selected: selectedMemberID === Number(intern.internID) }"
            >
              <input
                v-model="selectedMemberID"
                type="radio"
                name="member"
                :value="Number(intern.internID)"
                :disabled="isAddingMember"
              />
              <span class="flex-grow-1">
                <strong>{{ intern.firstName }} {{ intern.lastName }}</strong>
                <small v-if="intern.email" class="d-block text-muted">{{ intern.email }}</small>
              </span>
              <span
                class="badge"
                :class="intern.isActive === INTERN_ACTIVE ? 'text-bg-success' : 'text-bg-secondary'"
              >
                {{ intern.isActive === INTERN_ACTIVE ? 'Aktif' : 'Ayrıldı' }}
              </span>
            </label>
            <p v-if="filteredMembers.length === 0" class="text-muted text-center py-3 mb-0">
              Eklenecek stajyer bulunamadı.
            </p>
          </div>
        </template>
        <AlertMessages v-if="memberSaveErrors.length" class="mt-3" :messages="memberSaveErrors" />
      </template>

      <template #footer>
        <template v-if="groupInfoView === 'members'">
          <button type="button" class="btn btn-secondary" data-bs-dismiss="modal" :disabled="isRemovingMember">Kapat</button>
        </template>
        <template v-else>
          <button
            type="button"
            class="btn btn-outline-secondary"
            :disabled="isAddingMember"
            @click="groupInfoView = 'members'"
          >
            Geri
          </button>
          <button
            type="button"
            class="btn btn-primary"
            :disabled="!selectedMember || isMemberListLoading || isAddingMember"
            @click="submitAddMember"
          >
            {{ isAddingMember ? 'Ekleniyor...' : 'Gruba Ekle' }}
          </button>
        </template>
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

<style scoped>
.task-reference-rule-item {
  position: relative;
}

.task-reference-disabled {
  color: var(--bs-secondary-color, #6c757d);
  cursor: help;
}

.task-reference-disabled:hover,
.task-reference-disabled:focus {
  color: var(--bs-secondary-color, #6c757d);
  background-color: var(--bs-tertiary-bg, #f8f9fa);
}

.task-reference-info-icon {
  margin-left: 0.3rem;
  font-size: 0.9em;
}

.task-reference-rule-tooltip {
  position: absolute;
  bottom: calc(100% + 6px);
  left: 0.5rem;
  z-index: 1100;
  width: min(260px, 75vw);
  padding: 0.6rem 0.7rem;
  border-radius: 0.4rem;
  background: #252b33;
  color: #fff;
  font-size: 0.8rem;
  line-height: 1.35;
  white-space: normal;
  pointer-events: none;
  box-shadow: 0 5px 15px rgb(0 0 0 / 18%);
}
</style>
