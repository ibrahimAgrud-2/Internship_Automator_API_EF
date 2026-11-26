<script setup>
import { ref, computed, watch, nextTick, onBeforeUnmount, useId } from 'vue'
import { formatDayText, getTodayText } from '@/utils/format'

// Projedeki TEK tarih seçici. Tüm tarih alanları bunu kullanır; görünüm ve biçim her yerde aynıdır.
//   <DatePicker v-model="dayText" :min="minDate" :max="maxDate" />
// Değer her zaman "yyyy-MM-dd" (API'ye giden biçim değişmez); ekranda "gg-aa-yyyy" gösterilir.
// Sadece arayüz kısıtıdır (min/max dışındaki günler seçilemez); asıl doğrulama API'dadır.
const props = defineProps({
  modelValue: { type: String, default: '' },
  id: { type: String, default: '' },
  min: { type: String, default: '' },
  max: { type: String, default: '' },
  disabled: { type: Boolean, default: false },
  placeholder: { type: String, default: 'gg-aa-yyyy' },
  size: { type: String, default: '' }, // 'sm' -> küçük alan
  clearable: { type: Boolean, default: true },
  ariaLabel: { type: String, default: 'Tarih seçin' },
})
const emit = defineEmits(['update:modelValue'])

const fallbackId = useId()
const triggerId = computed(() => props.id || `date-${fallbackId}`)
const isOpen = ref(false)
const trigger = ref(null)
const popup = ref(null)
const viewMonth = ref(new Date())
const popupStyle = ref({})
const placeAbove = ref(false)

const WEEKDAYS = ['Pt', 'Sa', 'Ça', 'Pe', 'Cu', 'Ct', 'Pa']
const pad = (n) => String(n).padStart(2, '0')
const toDayText = (date) => `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`

const displayText = computed(() => formatDayText(props.modelValue))
const monthLabel = computed(() => viewMonth.value.toLocaleDateString('tr-TR', { month: 'long', year: 'numeric' }))
const todayText = computed(() => (isOpen.value ? getTodayText() : ''))
const isBlocked = (day) => Boolean((props.min && day < props.min) || (props.max && day > props.max))

const days = computed(() => {
  const year = viewMonth.value.getFullYear()
  const month = viewMonth.value.getMonth()
  const offset = (new Date(year, month, 1).getDay() + 6) % 7 // hafta pazartesi başlar
  return Array.from({ length: 42 }, (_, i) => {
    const date = new Date(year, month, i - offset + 1)
    const day = toDayText(date)
    return { day, number: date.getDate(), outside: date.getMonth() !== month, disabled: isBlocked(day) }
  })
})

// Açılır pencere sayfa düzenini bozmasın diye ekrana sabitlenir; altta yer yoksa üstte açılır
function updatePosition() {
  if (!trigger.value) return
  const rect = trigger.value.getBoundingClientRect()
  const width = 296
  const height = popup.value?.offsetHeight || 330
  const spaceBelow = window.innerHeight - rect.bottom
  placeAbove.value = spaceBelow < height + 12 && rect.top > spaceBelow
  const left = Math.min(Math.max(8, rect.left), window.innerWidth - width - 8)
  popupStyle.value = placeAbove.value
    ? { left: `${left}px`, bottom: `${window.innerHeight - rect.top + 6}px` }
    : { left: `${left}px`, top: `${rect.bottom + 6}px` }
}

function open() {
  if (props.disabled) return
  const start = props.modelValue || (props.min && props.min > getTodayText() ? props.min : getTodayText())
  const [year, month] = start.split('-').map(Number)
  viewMonth.value = new Date(year, month - 1, 1)
  isOpen.value = true
  updatePosition()
  nextTick(updatePosition)
}
function close(refocus = false) {
  isOpen.value = false
  if (refocus) trigger.value?.focus()
}
const toggle = () => (isOpen.value ? close() : open())
const moveMonth = (delta) => {
  viewMonth.value = new Date(viewMonth.value.getFullYear(), viewMonth.value.getMonth() + delta, 1)
}
function choose(day) {
  emit('update:modelValue', day)
  close(true)
}
function chooseToday() {
  const today = getTodayText()
  if (!isBlocked(today)) choose(today)
}

function onFocusOut(event) {
  if (!event.currentTarget.contains(event.relatedTarget)) close()
}
// Pencere içindeki kaydırma ya da boyut değişiminde konum güncellenir
watch(isOpen, (value) => {
  const method = value ? 'addEventListener' : 'removeEventListener'
  window[method]('scroll', updatePosition, true)
  window[method]('resize', updatePosition)
})
watch(() => props.disabled, (value) => value && close())
onBeforeUnmount(() => {
  window.removeEventListener('scroll', updatePosition, true)
  window.removeEventListener('resize', updatePosition)
})

defineExpose({ focus: () => trigger.value?.focus() })
</script>

<template>
  <div class="dp" :class="{ 'dp-sm': size === 'sm' }" @focusout="onFocusOut" @keydown.esc.stop.prevent="close(true)">
    <button
      :id="triggerId"
      ref="trigger"
      type="button"
      class="form-control dp-trigger"
      :class="{ 'form-control-sm': size === 'sm', 'is-open': isOpen }"
      :disabled="disabled"
      :aria-expanded="isOpen"
      aria-haspopup="dialog"
      @click="toggle"
      @keydown.down.prevent="open"
    >
      <span :class="displayText ? 'dp-value' : 'dp-placeholder'">{{ displayText || placeholder }}</span>
      <svg class="dp-icon" width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" aria-hidden="true"><rect x="3" y="5" width="18" height="16" rx="3" /><path d="M3 10h18M8 3v4M16 3v4" /></svg>
    </button>

    <Transition name="dp-pop">
      <div v-if="isOpen" ref="popup" class="dp-popup" :class="{ 'is-above': placeAbove }" :style="popupStyle" role="dialog" :aria-label="ariaLabel">
        <div class="dp-header">
          <button type="button" class="dp-nav" aria-label="Önceki ay" @click="moveMonth(-1)">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" aria-hidden="true"><path d="m15 18-6-6 6-6" /></svg>
          </button>
          <strong class="dp-month">{{ monthLabel }}</strong>
          <button type="button" class="dp-nav" aria-label="Sonraki ay" @click="moveMonth(1)">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" aria-hidden="true"><path d="m9 18 6-6-6-6" /></svg>
          </button>
        </div>

        <div class="dp-grid">
          <span v-for="weekday in WEEKDAYS" :key="weekday" class="dp-weekday">{{ weekday }}</span>
          <button
            v-for="item in days"
            :key="item.day"
            type="button"
            class="dp-day"
            :class="{ 'is-outside': item.outside, 'is-selected': item.day === modelValue, 'is-today': item.day === todayText }"
            :disabled="item.disabled"
            :aria-label="formatDayText(item.day)"
            :aria-pressed="item.day === modelValue"
            @click="choose(item.day)"
          >{{ item.number }}</button>
        </div>

        <!-- İsteğe bağlı açıklama (örn. projenin tarih aralığı) -->
        <div v-if="$slots.hint" class="dp-hint"><slot name="hint" /></div>

        <div class="dp-footer">
          <button type="button" class="dp-link" :disabled="isBlocked(getTodayText())" @click="chooseToday">Bugün</button>
          <button v-if="clearable && modelValue" type="button" class="dp-link dp-link-muted" @click="choose('')">Temizle</button>
        </div>
      </div>
    </Transition>
  </div>
</template>

<style scoped>
.dp { position: relative; }
.dp-trigger { display: flex; align-items: center; justify-content: space-between; gap: 10px; min-width: 150px; text-align: left; cursor: pointer; }
.dp-trigger.is-open { border-color: var(--brand-primary); box-shadow: 0 0 0 .22rem var(--brand-primary-ring); }
.dp-trigger:disabled { cursor: not-allowed; }
.dp-value { font-variant-numeric: tabular-nums; letter-spacing: .02em; color: var(--text-strong); }
.dp-placeholder { color: var(--text-faint); letter-spacing: .02em; }
.dp-icon { flex-shrink: 0; color: var(--text-muted); transition: color var(--dur-fast) ease; }
.dp-trigger:hover:not(:disabled) .dp-icon, .dp-trigger.is-open .dp-icon { color: var(--brand-primary); }
.dp-sm .dp-trigger { min-width: 132px; gap: 8px; }

.dp-popup { position: fixed; z-index: 2000; width: 296px; padding: 12px; border: 1px solid var(--border-soft); border-radius: var(--radius); background: var(--surface); box-shadow: var(--shadow-lg); transform-origin: top left; }
.dp-popup.is-above { transform-origin: bottom left; }
.dp-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 8px; }
.dp-month { font-size: .92rem; font-weight: 650; color: var(--text-strong); text-transform: capitalize; }
.dp-nav { display: grid; place-items: center; width: 30px; height: 30px; border: 0; border-radius: 8px; background: transparent; color: var(--text-muted); transition: background-color var(--dur-fast) ease, color var(--dur-fast) ease; }
.dp-nav:hover { background: var(--brand-primary-softer); color: var(--brand-primary); }

.dp-grid { display: grid; grid-template-columns: repeat(7, minmax(0, 1fr)); gap: 2px; text-align: center; }
.dp-weekday { padding: 4px 0 6px; font-size: .7rem; font-weight: 600; letter-spacing: .03em; color: var(--text-faint); }
.dp-day { height: 34px; border: 0; border-radius: 9px; background: transparent; color: var(--text-body); font-size: .84rem; font-variant-numeric: tabular-nums; transition: background-color var(--dur-fast) ease, color var(--dur-fast) ease, box-shadow var(--dur-fast) ease; }
.dp-day:not(:disabled):hover { background: var(--brand-primary-soft); color: var(--brand-primary); }
.dp-day.is-outside { color: var(--text-faint); }
.dp-day.is-today { box-shadow: inset 0 0 0 1px var(--brand-primary-border); font-weight: 650; }
.dp-day.is-selected, .dp-day.is-selected:hover { background: var(--brand-primary); color: #fff; font-weight: 650; box-shadow: 0 3px 10px rgba(var(--brand-primary-rgb), .28); }
.dp-day:disabled { color: var(--border-strong); background: transparent; cursor: not-allowed; }

.dp-hint { margin-top: 10px; padding: 8px 10px; border-radius: 9px; background: var(--surface-muted); font-size: .74rem; line-height: 1.4; color: var(--text-muted); }
.dp-footer { display: flex; justify-content: space-between; gap: 8px; margin-top: 10px; padding-top: 8px; border-top: 1px solid var(--border-soft); }
.dp-link { border: 0; background: transparent; padding: 4px 8px; border-radius: 7px; font-size: .8rem; font-weight: 600; color: var(--brand-primary); transition: background-color var(--dur-fast) ease; }
.dp-link:hover:not(:disabled) { background: var(--brand-primary-softer); }
.dp-link:disabled { color: var(--text-faint); cursor: not-allowed; }
.dp-link-muted { color: var(--text-muted); }

.dp-pop-enter-active { transition: opacity var(--dur) var(--ease-out), scale var(--dur) var(--ease-out), translate var(--dur) var(--ease-out); }
.dp-pop-leave-active { transition: opacity var(--dur-fast) ease; }
.dp-pop-enter-from { opacity: 0; scale: .97; translate: 0 -4px; }
.dp-pop-enter-from.is-above { translate: 0 4px; }
.dp-pop-leave-to { opacity: 0; }
</style>
