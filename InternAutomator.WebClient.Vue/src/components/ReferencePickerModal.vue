<script setup>
import { ref, watch } from 'vue'
import { getAllAnnouncements } from '@/api/announcements'
import { getAllProjects } from '@/api/projects'
import { getErrorMessages } from '@/api/http'
import { REFERENCE_ANNOUNCEMENT, REFERENCE_PROJECT } from '@/constants'
import BaseModal from './BaseModal.vue'
import AlertMessages from './AlertMessages.vue'

// Mesaja eklenecek duyuru/proje seçtirir. Mevcut listeler kullanılır; yeni endpoint gerekmez.
//   <ReferencePickerModal v-model="open" :type="0" @picked="({ id, type, title }) => ..." />
// type: 0 = duyuru, 1 = proje (API'daki referencedContentType ile aynı)
const props = defineProps({
  modelValue: { type: Boolean, default: false },
  type: { type: Number, default: REFERENCE_ANNOUNCEMENT },
})

const emit = defineEmits(['update:modelValue', 'picked'])

const items = ref([]) // { id, title }
const isLoading = ref(false)
const errors = ref([])
let requestId = 0

const titleOf = () => (props.type === REFERENCE_PROJECT ? 'Proje seç' : 'Duyuru seç')

async function load() {
  const current = ++requestId
  items.value = []
  errors.value = []
  isLoading.value = true

  try {
    if (props.type === REFERENCE_PROJECT) {
      const projects = await getAllProjects()
      if (current === requestId) items.value = projects.map((p) => ({ id: p.projectID, title: p.title }))
    } else {
      const announcements = await getAllAnnouncements()
      if (current === requestId) items.value = announcements.map((a) => ({ id: a.announcementID, title: a.title }))
    }
  } catch (error) {
    if (current === requestId) errors.value = getErrorMessages(error)
  } finally {
    if (current === requestId) isLoading.value = false
  }
}

// Her açılışta liste güncel gelsin
watch(
  () => props.modelValue,
  (isOpen) => {
    if (isOpen) load()
  },
)

function pick(item) {
  emit('picked', { id: item.id, type: props.type, title: item.title })
  emit('update:modelValue', false)
}
</script>

<template>
  <BaseModal :model-value="modelValue" :title="titleOf()" dialog-class="modal-dialog-centered modal-dialog-scrollable" :static-backdrop="false" closable @update:model-value="emit('update:modelValue', $event)">
    <AlertMessages v-if="isLoading" type="info" messages="Yükleniyor..." />
    <AlertMessages v-else-if="errors.length" :messages="errors" />
    <AlertMessages v-else-if="items.length === 0" type="secondary" :messages="type === REFERENCE_PROJECT ? 'Henüz proje yok.' : 'Henüz duyuru yok.'" />

    <div v-else class="list-group">
      <button v-for="item in items" :key="item.id" type="button" class="list-group-item list-group-item-action text-break" @click="pick(item)">
        {{ item.title }}
      </button>
    </div>

    <template #footer>
      <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Vazgeç</button>
    </template>
  </BaseModal>
</template>
