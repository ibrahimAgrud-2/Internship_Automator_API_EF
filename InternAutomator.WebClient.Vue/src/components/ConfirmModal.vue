<script setup>
import { ref, watch } from 'vue'
import { ApiError, getErrorMessages } from '@/api/http'
import BaseModal from './BaseModal.vue'
import AlertMessages from './AlertMessages.vue'

// Onay penceresi (sil, tamamla, geri al...). İşlemi `action` yapar; hata olursa pencerede gösterir.
//   <ConfirmModal v-model="open" title="Görevi Sil" confirm-text="Görevi Sil" :action="() => deleteTask(id)"
//                 @done="..." @stale="...">gövde</ConfirmModal>
//
// done:  işlem başarılı oldu, pencere kapandı (üst sayfa bildirim gösterip listeyi yeniler)
// stale: kayıt zaten yoktu (404); liste eskimiş demektir, üst sayfa listeyi arkada yeniler
const props = defineProps({
  modelValue: { type: Boolean, default: false },
  title: { type: String, required: true },
  confirmText: { type: String, required: true },
  confirmClass: { type: String, default: 'btn-danger' },
  action: { type: Function, required: true },
})

const emit = defineEmits(['update:modelValue', 'done', 'stale'])

const isBusy = ref(false)
const errors = ref([])

// Her açılışta temiz başla
watch(
  () => props.modelValue,
  (isOpen) => {
    if (isOpen) {
      errors.value = []
      isBusy.value = false
    }
  },
)

async function confirm() {
  if (isBusy.value) return // çift tıklama koruması

  errors.value = []
  isBusy.value = true

  try {
    await props.action()
    emit('update:modelValue', false)
    emit('done')
  } catch (error) {
    errors.value = getErrorMessages(error) // API'nin mesajı olduğu gibi gösterilir

    if (error instanceof ApiError && error.status === 404) {
      emit('stale')
    }
  } finally {
    isBusy.value = false
  }
}
</script>

<template>
  <BaseModal :model-value="modelValue" :title="title" @update:model-value="emit('update:modelValue', $event)">
    <AlertMessages v-if="errors.length" :messages="errors" />

    <slot />

    <template #footer>
      <button type="button" class="btn btn-secondary" data-bs-dismiss="modal" :disabled="isBusy">Vazgeç</button>
      <button type="button" class="btn" :class="confirmClass" :disabled="isBusy" @click="confirm">{{ confirmText }}</button>
    </template>
  </BaseModal>
</template>
