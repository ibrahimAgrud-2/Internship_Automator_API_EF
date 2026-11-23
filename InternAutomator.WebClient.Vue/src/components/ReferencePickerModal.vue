<script setup>
import { computed } from 'vue'
import BaseModal from './BaseModal.vue'
import AlertMessages from './AlertMessages.vue'

// Mesaja eklenecek içeriği (duyuru / proje / görev) seçtirir. Sadece görüntüdür: veriyi çekmez.
// Sayfa veriyi çekip bölümlere ayırır, bu pencere başlıklı bölümler halinde gösterir.
// sections: [{ title?: 'Bölüm başlığı', items: [{ id, title, subtitle?, badge?, muted?, chips?: [] }] }]
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
  <BaseModal
    :model-value="modelValue"
    :title="title"
    dialog-class="modal-dialog-centered modal-dialog-scrollable"
    :static-backdrop="false"
    closable
    @update:model-value="emit('update:modelValue', $event)"
  >
    <AlertMessages v-if="isLoading" type="info" messages="Yükleniyor..." />
    <AlertMessages v-else-if="errors.length" :messages="errors" />
    <AlertMessages v-else-if="visibleSections.length === 0" type="secondary" :messages="emptyText" />

    <template v-else>
      <div v-for="(section, index) in visibleSections" :key="index" :class="{ 'mt-4': index > 0 }">
        <div v-if="section.title" class="reference-section-title">{{ section.title }}</div>

        <div class="reference-picker-grid">
          <button
            v-for="item in section.items"
            :key="item.id"
            type="button"
            class="reference-card"
            :class="{ 'is-muted': item.muted }"
            @click="pick(item)"
          >
            <div class="reference-card-main">
              <div class="reference-card-header">
                <strong class="reference-card-title">{{ item.title }}</strong>
                <span v-if="item.badge" class="badge flex-shrink-0" :class="'text-bg-' + item.badge.tone">{{ item.badge.text }}</span>
              </div>

              <p v-if="item.subtitle" class="reference-card-subtitle">{{ item.subtitle }}</p>

              <div v-if="item.chips?.length" class="reference-card-chips">
                <span v-for="(chip, chipIndex) in item.chips" :key="`${item.id}-${chipIndex}`" class="reference-chip">
                  {{ chip }}
                </span>
              </div>
            </div>

            <span class="reference-card-arrow" aria-hidden="true">→</span>
          </button>
        </div>
      </div>
    </template>

    <template #footer>
      <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Vazgeç</button>
    </template>
  </BaseModal>
</template>

<style scoped>
.reference-section-title{
  margin-bottom:.85rem;
  color:var(--text-muted);
  font-size:.76rem;
  font-weight:750;
  letter-spacing:.08em;
  text-transform:uppercase;
}
.reference-picker-grid{
  display:grid;
  gap:.8rem;
}
.reference-card{
  width:100%;
  display:flex;
  align-items:flex-start;
  justify-content:space-between;
  gap:.9rem;
  padding:1rem 1.05rem;
  border:1px solid var(--border-soft);
  border-radius:14px;
  background:#fff;
  text-align:left;
  color:var(--text-strong);
  transition:border-color .16s ease, box-shadow .16s ease, transform .16s ease, background-color .16s ease;
}
.reference-card:hover{
  border-color:var(--brand-primary-border);
  background:var(--surface-hover);
  box-shadow:var(--shadow-md);
  transform:translateY(-1px);
}
.reference-card:focus-visible{
  outline:3px solid rgba(152,43,44,.16);
  outline-offset:2px;
}
.reference-card.is-muted{
  opacity:.82;
}
.reference-card-main{
  min-width:0;
  flex:1;
}
.reference-card-header{
  display:flex;
  align-items:flex-start;
  justify-content:space-between;
  gap:.75rem;
}
.reference-card-title{
  display:block;
  font-size:1rem;
  font-weight:700;
  line-height:1.35;
}
.reference-card-subtitle{
  margin:.42rem 0 0;
  color:var(--text-muted);
  font-size:.88rem;
  line-height:1.45;
}
.reference-card-chips{
  display:flex;
  flex-wrap:wrap;
  gap:.45rem;
  margin-top:.75rem;
}
.reference-chip{
  display:inline-flex;
  align-items:center;
  min-height:28px;
  padding:.26rem .62rem;
  border:1px solid var(--brand-primary-border);
  border-radius:999px;
  background:var(--brand-primary-softer);
  color:var(--brand-primary);
  font-size:.76rem;
  font-weight:650;
  line-height:1.2;
}
.reference-card-arrow{
  flex-shrink:0;
  color:var(--text-faint);
  font-size:1rem;
  line-height:1.4;
  margin-top:.1rem;
}
@media (max-width: 575.98px){
  .reference-card{
    padding:.92rem;
  }
  .reference-card-title{
    font-size:.96rem;
  }
}
</style>
