<script setup>
import { computed } from 'vue'
import BaseModal from './BaseModal.vue'
import AlertMessages from './AlertMessages.vue'

// Mesaja eklenecek içeriği (duyuru / proje / görev) seçtirir. Sadece görüntüdür: veriyi çekmez.
// Sayfa veriyi çekip bölümlere ayırır, bu pencere başlıklı bölümler halinde gösterir.
//   <ReferencePickerModal v-model="open" title="Duyuru seç" :sections="sections" :is-loading="loading" :errors="errors" empty-text="..." @picked="onPicked" />
// sections: [{ title?: 'Bölüm başlığı', items: [{ id, title, subtitle? }] }]  (boş bölümler gösterilmez)
// picked olayı seçilen satırı olduğu gibi gönderir: { id, title, subtitle? }
const props = defineProps({
  modelValue: { type: Boolean, default: false },
  title: { type: String, default: 'Seç' },
  sections: { type: Array, default: () => [] },
  isLoading: { type: Boolean, default: false },
  errors: { type: Array, default: () => [] },
  emptyText: { type: String, default: 'Liste boş.' },
})

const emit = defineEmits(['update:modelValue', 'picked'])

const visibleSections = computed(() => props.sections.filter((section) => section.items.length > 0))

function pick(item) {
  emit('picked', item)
  emit('update:modelValue', false)
}
</script>

<template>
  <BaseModal :model-value="modelValue" :title="title" dialog-class="modal-dialog-centered modal-dialog-scrollable" :static-backdrop="false" closable @update:model-value="emit('update:modelValue', $event)">
    <AlertMessages v-if="isLoading" type="info" messages="Yükleniyor..." />
    <AlertMessages v-else-if="errors.length" :messages="errors" />
    <AlertMessages v-else-if="visibleSections.length === 0" type="secondary" :messages="emptyText" />

    <template v-else>
      <div v-for="(section, index) in visibleSections" :key="index" :class="{ 'mt-3': index > 0 }">
        <h6 v-if="section.title" class="text-muted text-uppercase small mb-2">{{ section.title }}</h6>
        <div class="list-group">
          <button v-for="item in section.items" :key="item.id" type="button" class="list-group-item list-group-item-action text-break" @click="pick(item)">
            {{ item.title }}
            <span v-if="item.subtitle" class="d-block small text-muted">{{ item.subtitle }}</span>
          </button>
        </div>
      </div>
    </template>

    <template #footer>
      <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Vazgeç</button>
    </template>
  </BaseModal>
</template>