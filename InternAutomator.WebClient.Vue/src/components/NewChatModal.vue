<script setup>
import { ref, computed, watch } from 'vue'
import { getAllInterns } from '@/api/interns'
import { createConversation } from '@/api/conversations'
import { getErrorMessages } from '@/api/http'
import { INTERN_ACTIVE } from '@/constants'
import BaseModal from './BaseModal.vue'
import AlertMessages from './AlertMessages.vue'
import InternStatusBadge from './InternStatusBadge.vue'

// "Yeni sohbet": stajyer listesinden birini seçtirir, "Sohbete git" ile birebir sohbeti açar.
//   <NewChatModal v-model="open" @created="(conversation) => ..." />
// created: API'nın döndüğü sohbet ({ conversationID, internID, conversationName, isGroup }).
// Stajyerle zaten sohbet varsa API mevcut sohbeti döner; burada fark etmez.
const props = defineProps({ modelValue: { type: Boolean, default: false } })
const emit = defineEmits(['update:modelValue', 'created'])

const interns = ref([])
const isLoading = ref(false)
const loadErrors = ref([])
const selectedInternID = ref(null)
const isBusy = ref(false)
const errors = ref([])
let requestId = 0

const getFullName = (intern) => `${intern.firstName} ${intern.lastName}`.trim()

// Aktif stajyerler üstte, ayrılanlar en altta; her grup kendi içinde ada göre
const sortedInterns = computed(() =>
  [...interns.value].sort((a, b) => {
    const activeDiff = Number(b.status === INTERN_ACTIVE) - Number(a.status === INTERN_ACTIVE)
    return activeDiff !== 0 ? activeDiff : getFullName(a).localeCompare(getFullName(b), 'tr')
  }),
)

const selectedIntern = computed(() => interns.value.find((intern) => intern.internID === selectedInternID.value) ?? null)

async function load() {
  const current = ++requestId
  interns.value = []
  loadErrors.value = []
  isLoading.value = true

  try {
    const result = await getAllInterns()
    if (current === requestId) interns.value = Array.isArray(result) ? result : []
  } catch (error) {
    if (current === requestId) loadErrors.value = getErrorMessages(error)
  } finally {
    if (current === requestId) isLoading.value = false
  }
}

// Her açılışta temiz başla, liste güncel gelsin
watch(
  () => props.modelValue,
  (isOpen) => {
    if (!isOpen) return
    selectedInternID.value = null
    errors.value = []
    isBusy.value = false
    load()
  },
)

async function goToChat() {
  const intern = selectedIntern.value
  if (!intern || isBusy.value) return // çift tıklama koruması

  errors.value = []
  isBusy.value = true

  try {
    const conversation = await createConversation({
      internID: intern.internID,
      conversationName: getFullName(intern),
      isGroup: false,
    })
    emit('update:modelValue', false)
    emit('created', conversation)
  } catch (error) {
    errors.value = getErrorMessages(error) // API'nın mesajı olduğu gibi gösterilir
  } finally {
    isBusy.value = false
  }
}
</script>

<template>
  <BaseModal :model-value="modelValue" title="Yeni sohbet" dialog-class="modal-dialog-centered modal-dialog-scrollable" @update:model-value="emit('update:modelValue', $event)">
    <AlertMessages v-if="errors.length" :messages="errors" />

    <AlertMessages v-if="isLoading" type="info" messages="Stajyerler yükleniyor..." />
    <AlertMessages v-else-if="loadErrors.length" :messages="loadErrors" />
    <AlertMessages v-else-if="interns.length === 0" type="secondary" messages="Henüz stajyer yok." />

    <div v-else class="list-group">
      <button
        v-for="intern in sortedInterns"
        :key="intern.internID"
        type="button"
        class="list-group-item list-group-item-action d-flex justify-content-between align-items-center gap-2"
        :class="{ active: intern.internID === selectedInternID }"
        :aria-pressed="String(intern.internID === selectedInternID)"
        :disabled="isBusy"
        @click="selectedInternID = intern.internID"
      >
        <span class="text-break">{{ getFullName(intern) }}</span>
        <InternStatusBadge :status="intern.status" />
      </button>
    </div>

    <template #footer>
      <button type="button" class="btn btn-secondary" data-bs-dismiss="modal" :disabled="isBusy">Vazgeç</button>
      <button type="button" class="btn btn-primary" :disabled="isBusy || selectedIntern === null" @click="goToChat">Sohbete git</button>
    </template>
  </BaseModal>
</template>