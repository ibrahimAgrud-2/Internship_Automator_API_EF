<script setup>
import { ref, watch, onMounted, onBeforeUnmount, useId } from 'vue'
import { Modal } from 'bootstrap'

// Bootstrap modal'ı Vue'ya bağlar. Açık/kapalı durumu v-model ile yönetilir:
//   <BaseModal v-model="isOpen" title="Başlık"> ...gövde... <template #footer> ...butonlar... </template> </BaseModal>
//
// asForm: pencere bir <form> olur, Enter ya da submit butonu 'submit' olayını tetikler.
// staticBackdrop: dışarı tıklayınca / Esc ile kapanmaz (kayıt sürerken yanlışlıkla kapanmasın diye).
const props = defineProps({
  modelValue: { type: Boolean, default: false },
  title: { type: String, default: '' },
  dialogClass: { type: String, default: 'modal-dialog-centered' },
  headerClass: { type: String, default: '' },
  asForm: { type: Boolean, default: false },
  staticBackdrop: { type: Boolean, default: true },
  closable: { type: Boolean, default: false },
})

const emit = defineEmits(['update:modelValue', 'submit', 'shown'])

const titleId = useId()
const element = ref(null)
let modal = null

onMounted(() => {
  modal = new Modal(element.value, {
    backdrop: props.staticBackdrop ? 'static' : true,
    keyboard: !props.staticBackdrop,
  })

  element.value.addEventListener('shown.bs.modal', () => emit('shown'))

  // Kullanıcı pencereyi kapattığında (Vazgeç, X, Esc) üstteki durumu da kapalıya çek
  element.value.addEventListener('hidden.bs.modal', () => emit('update:modelValue', false))

  if (props.modelValue) modal.show()
})

watch(
  () => props.modelValue,
  (isOpen) => {
    if (!modal) return
    if (isOpen) modal.show()
    else modal.hide()
  },
)

onBeforeUnmount(() => {
  // Pencere açıkken sayfa değişirse (tarayıcı geri tuşu) Bootstrap'in bıraktığı perdeyi temizle
  if (props.modelValue) {
    document.querySelectorAll('.modal-backdrop').forEach((node) => node.remove())
    document.body.classList.remove('modal-open')
    document.body.style.removeProperty('overflow')
    document.body.style.removeProperty('padding-right')
  }
  modal?.dispose()
})
</script>

<template>
  <div ref="element" class="modal fade" tabindex="-1" :aria-labelledby="titleId" aria-hidden="true">
    <div class="modal-dialog" :class="dialogClass">
      <component
        :is="asForm ? 'form' : 'div'"
        class="modal-content"
        v-bind="asForm ? { novalidate: true, autocomplete: 'off' } : {}"
        @submit.prevent="emit('submit')"
      >
        <div class="modal-header" :class="headerClass">
          <h5 :id="titleId" class="modal-title text-break" :class="{ 'me-auto': $slots['header-actions'] }">{{ title }}</h5>
          <slot name="header-actions" />
          <button v-if="closable" type="button" class="btn-close" aria-label="Kapat" data-bs-dismiss="modal"></button>
        </div>

        <div class="modal-body">
          <slot />
        </div>

        <div v-if="$slots.footer" class="modal-footer">
          <slot name="footer" />
        </div>
      </component>
    </div>
  </div>
</template>
