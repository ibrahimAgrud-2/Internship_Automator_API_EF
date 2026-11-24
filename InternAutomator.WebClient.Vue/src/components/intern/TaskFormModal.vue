<script setup>
import { ref, reactive, watch, computed } from 'vue'
import { addTask, updateTask } from '@/api/tasks'
import { ApiError, getErrorMessages } from '@/api/http'
import { TASK_LIMITS } from '@/constants'
import { getTodayText } from '@/utils/format'
import AlertMessages from '@/components/AlertMessages.vue'
import BaseModal from '@/components/BaseModal.vue'

// Stajyer detay sayfası: "Görevler > Ekle" ve görevdeki "Düzenle" ile açılan görev penceresi.
// task null ise "yeni görev", dolu ise o görevin düzenlenmesidir (aynı form iki iş için).
const props = defineProps({
  modelValue: { type: Boolean, default: false },
  projects: { type: Array, default: () => [] }, // stajyerin atanmış projeleri (görev ongoingProjectID ile bağlanır)
  task: { type: Object, default: null },
})

// saved: kaydedildi, pencere kapandı (üst sayfa bildirim gösterip detayları yeniler)
// stale: düzenlenen görev artık yok (404); üst sayfa listeyi arkada yeniler
const emit = defineEmits(['update:modelValue', 'saved', 'stale'])

const form = reactive({ ongoingProjectID: '', title: '', description: '', dayText: '' })
const original = ref(null) // düzenlenen görevin açılıştaki değerleri (değişiklik karşılaştırması için)
const minDate = ref('')
const isSaving = ref(false)
const errors = ref([])
const projectSelect = ref(null)
const titleInput = ref(null)
const dueDateInput = ref(null)

// Mevcut proje listesindeki teslim tarihini kullan; ek API isteği gerekmez.
const selectedProject = computed(() => props.projects.find((p) => String(p.ongoingProjectID) === form.ongoingProjectID))
function projectDay(value) {
  const day = typeof value === 'string' ? value.slice(0, 10) : ''
  return /^\d{4}-\d{2}-\d{2}$/.test(day) ? day : ''
}
const projectStartDay = computed(() => projectDay(selectedProject.value?.startDate))
const projectDueDay = computed(() => projectDay(selectedProject.value?.dueDate))
const earliestDay = computed(() => [minDate.value, projectStartDay.value].sort().pop())
const outsideProject = (day) => Boolean((projectStartDay.value && day < projectStartDay.value) || (projectDueDay.value && day > projectDueDay.value))
const displayDay = (day) => day.split('-').reverse().join('.')
const calendarOpen = ref(false)
const calendarMonth = ref(new Date())
const monthLabel = computed(() => calendarMonth.value.toLocaleDateString('tr-TR', { month: 'long', year: 'numeric' }))
const calendarDays = computed(() => {
  const year = calendarMonth.value.getFullYear()
  const month = calendarMonth.value.getMonth()
  const offset = (new Date(year, month, 1).getDay() + 6) % 7
  return Array.from({ length: 42 }, (_, i) => {
    const date = new Date(year, month, i - offset + 1)
    const day = `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`
    return { day, number: date.getDate(), outside: date.getMonth() !== month,
      disabled: day < earliestDay.value || outsideProject(day) }
  })
})
function toggleCalendar() {
  if (calendarOpen.value) { calendarOpen.value = false; return }
  const day = form.dayText || earliestDay.value || getTodayText()
  const [year, month] = day.split('-').map(Number)
  calendarMonth.value = new Date(year, month - 1, 1)
  calendarOpen.value = true
}
function moveMonth(delta) {
  calendarMonth.value = new Date(calendarMonth.value.getFullYear(), calendarMonth.value.getMonth() + delta, 1)
}
function chooseDay(day) {
  form.dayText = day
  calendarOpen.value = false
  dueDateInput.value?.focus()
}
watch(() => form.ongoingProjectID, () => { calendarOpen.value = false })

// Görevler gün bazlıdır; saat sorulmaz. Seçilen günün sonu (23:59:59) gönderilir, böylece
// API'nin "teslim zamanı şu andan sonra olmalı" kuralı bugün seçildiğinde de sağlanır.
// Saat bilgisi eklenmek istenirse sadece bu fonksiyon değişir.
const toTaskDueDate = (dayText) => `${dayText}T23:59:59`

// Pencere her açıldığında formu hazırla
watch(
  () => props.modelValue,
  (isOpen) => {
    calendarOpen.value = false
    if (!isOpen) return

    errors.value = []
    isSaving.value = false
    minDate.value = getTodayText() // sayfa gece yarısını geçerse diye her açılışta yenilenir

    if (props.task) {
      original.value = {
        title: (props.task.title ?? '').trim(),
        description: (props.task.description ?? '').trim(),
        dayText: (props.task.dueDate ?? '').slice(0, 10), // "2026-12-01T23:59:59" -> "2026-12-01"
        dueDate: props.task.dueDate,
      }
      form.ongoingProjectID = String(props.task.ongoingProjectID)
      form.title = original.value.title
      form.description = original.value.description
      form.dayText = original.value.dayText
    } else {
      original.value = null
      // Birden fazla proje varsa kullanıcı bilerek seçsin; tek proje varsa hazır gelir
      form.ongoingProjectID = props.projects.length === 1 ? String(props.projects[0].ongoingProjectID) : ''
      form.title = ''
      form.description = ''
      form.dayText = ''
    }
  },
)

function getProjectTitle(ongoingProjectID) {
  return props.projects.find((project) => project.ongoingProjectID === ongoingProjectID)?.projectTitle ?? '-'
}

function focusFirstField() {
  // Tek proje varsa doğrudan görev adına odaklan
  ;(form.ongoingProjectID ? titleInput.value : projectSelect.value)?.focus()
}

async function submitUpdate(title, dayText) {
  const description = form.description.trim()
  const isDateChanged = dayText !== original.value.dayText

  // Hiçbir şey değişmediyse sunucuya gitmeye gerek yok
  if (title === original.value.title && description === original.value.description && !isDateChanged) {
    emit('update:modelValue', false)
    return
  }

  errors.value = []
  isSaving.value = true

  try {
    await updateTask(props.task.taskID, {
      title,
      description,
      // Tarih değişmediyse eski değer aynen gönderilir; saat bilgisi sessizce değişmesin
      dueDate: isDateChanged ? toTaskDueDate(dayText) : original.value.dueDate,
    })

    emit('update:modelValue', false)
    emit('saved', `"${title}" görevi güncellendi.`)
  } catch (error) {
    errors.value = getErrorMessages(error) // API'nin mesajı olduğu gibi gösterilir

    // Görev artık yoksa ekrandaki liste eskimiş demektir
    if (error instanceof ApiError && error.status === 404) {
      emit('stale')
    }
  } finally {
    isSaving.value = false
  }
}

async function submit() {
  if (isSaving.value) return // çift gönderim koruması

  const ongoingProjectID = Number(form.ongoingProjectID)
  const title = form.title.trim()
  const dayText = form.dayText
  const isEditing = props.task !== null

  // Asıl doğrulama API'dadır; buradakiler kullanıcıyı baştan uyarır.
  // min özelliği elle yazılan tarihi engellemez, bu yüzden tarih burada da kontrol edilir.
  let message = ''
  let invalidField = null

  if (!ongoingProjectID) {
    message = 'Lütfen bir proje seçin.'
    invalidField = projectSelect.value
  } else if (!title) {
    message = 'Görev adı zorunludur.'
    invalidField = titleInput.value
  } else if (!dayText) {
    message = 'Teslim tarihi zorunludur.'
    invalidField = dueDateInput.value
  } else if (dayText < getTodayText() && (!isEditing || dayText !== original.value.dayText)) {
    // Düzenlemede tarih hiç değiştirilmediyse (gecikmiş görev) client engellemez; kararı API verir
    message = 'Teslim tarihi bugünden önce olamaz.'
    invalidField = dueDateInput.value
  } else if (!isEditing && outsideProject(dayText)) {
    message = 'Görev tarihleri projenin tarihleri arasında olmalı.'
    invalidField = dueDateInput.value
  }

  if (message) {
    errors.value = [message]
    invalidField?.focus()
    return
  }

  // Düzenleme modu: aynı doğrulamalardan sonra güncelleme isteği atılır
  if (isEditing) {
    await submitUpdate(title, dayText)
    return
  }

  errors.value = []
  isSaving.value = true

  try {
    await addTask({
      ongoingProjectID,
      title,
      description: form.description.trim() || null,
      dueDate: toTaskDueDate(dayText),
    })

    emit('update:modelValue', false)
    emit('saved', `"${title}" görevi eklendi.`)
  } catch (error) {
    errors.value = getErrorMessages(error) // API'nin mesajı olduğu gibi gösterilir
  } finally {
    isSaving.value = false
  }
}
</script>

<template>
  <BaseModal
    :model-value="modelValue"
    :title="task ? 'Görevi Düzenle' : 'Görev Ekle'"
    as-form
    @update:model-value="emit('update:modelValue', $event)"
    @submit="submit"
    @shown="focusFirstField"
  >
    <AlertMessages v-if="errors.length" :messages="errors" />

    <div class="mb-3">
      <label for="taskProject" class="form-label">Proje <span class="text-danger">*</span></label>
      <!-- Düzenlemede görevin projesi değiştirilemez; liste sadece görevin projesini gösterir (pasif) -->
      <select id="taskProject" ref="projectSelect" v-model="form.ongoingProjectID" class="form-select" :disabled="isSaving || task !== null">
        <template v-if="task">
          <option :value="String(task.ongoingProjectID)">{{ getProjectTitle(task.ongoingProjectID) }}</option>
        </template>
        <template v-else>
          <option v-if="projects.length !== 1" value="">Proje seçin</option>
          <option v-for="project in projects" :key="project.ongoingProjectID" :value="String(project.ongoingProjectID)">
            {{ project.projectTitle }}
          </option>
        </template>
      </select>
    </div>

    <div class="mb-3">
      <label for="taskTitle" class="form-label">Görev adı <span class="text-danger">*</span></label>
      <input id="taskTitle" ref="titleInput" v-model="form.title" type="text" class="form-control" :maxlength="TASK_LIMITS.title" :disabled="isSaving" />
    </div>

    <div class="mb-3">
      <label for="taskDescription" class="form-label">Açıklama</label>
      <textarea id="taskDescription" v-model="form.description" class="form-control" rows="3" :maxlength="TASK_LIMITS.description" :disabled="isSaving"></textarea>
    </div>

    <div>
      <label for="taskDueDate" class="form-label">Teslim tarihi <span class="text-danger">*</span></label>
      <input v-if="task" id="taskDueDate" ref="dueDateInput" v-model="form.dayText" type="date" class="form-control" :min="minDate" :disabled="isSaving" />
      <div v-else class="task-calendar-wrap" @keydown.esc.stop.prevent="calendarOpen = false; dueDateInput?.focus()" @focusout="!$event.currentTarget.contains($event.relatedTarget) && (calendarOpen = false)">
        <button id="taskDueDate" ref="dueDateInput" type="button" class="form-control task-calendar-trigger" :disabled="isSaving" :aria-expanded="calendarOpen" @click="toggleCalendar">
          <span :class="{ 'text-muted': !form.dayText }">{{ form.dayText ? displayDay(form.dayText) : 'gg.aa.yyyy' }}</span>
          <svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" aria-hidden="true"><rect x="3" y="5" width="18" height="16" rx="2"/><path d="M3 10h18M8 3v4M16 3v4"/></svg>
        </button>
        <div v-if="calendarOpen" class="task-calendar" role="group" aria-label="Teslim tarihi seçin">
          <div class="task-calendar-header">
            <button type="button" class="task-calendar-nav" aria-label="Önceki ay" @click="moveMonth(-1)">‹</button>
            <strong>{{ monthLabel }}</strong>
            <button type="button" class="task-calendar-nav" aria-label="Sonraki ay" @click="moveMonth(1)">›</button>
          </div>
          <div class="task-calendar-grid">
            <span v-for="weekday in ['Pt', 'Sa', 'Ça', 'Pe', 'Cu', 'Ct', 'Pa']" :key="weekday" class="task-calendar-weekday">{{ weekday }}</span>
            <button v-for="day in calendarDays" :key="day.day" type="button" class="task-calendar-day" :class="{ outside: day.outside, selected: day.day === form.dayText }" :disabled="day.disabled || isSaving" :aria-label="displayDay(day.day)" :aria-pressed="day.day === form.dayText" @click="chooseDay(day.day)">{{ day.number }}</button>
          </div>
          <div class="task-calendar-footer">
            <div class="task-calendar-hint">
              <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" aria-hidden="true"><circle cx="12" cy="12" r="9"/><path d="M12 7v6M12 16v1"/></svg>
              <span>Görev tarihleri projenin tarihleri arasında olmalı.<template v-if="projectStartDay && projectDueDay"><br>{{ displayDay(projectStartDay) }} – {{ displayDay(projectDueDay) }}</template><template v-else-if="projectDueDay"><br>Son tarih: {{ displayDay(projectDueDay) }}</template><template v-else-if="projectStartDay"><br>Başlangıç: {{ displayDay(projectStartDay) }}</template></span>
            </div>
            <button type="button" class="btn btn-sm btn-outline-secondary" @click="chooseDay('')">Clear</button>
          </div>
        </div>
      </div>
    </div>

    <template #footer>
      <button type="button" class="btn btn-secondary" data-bs-dismiss="modal" :disabled="isSaving">Vazgeç</button>
      <button type="submit" class="btn btn-primary" :disabled="isSaving">{{ task ? 'Kaydet' : 'Görevi Ekle' }}</button>
    </template>
  </BaseModal>
</template>

<style scoped>
.task-calendar-wrap { position: relative; }
.task-calendar-trigger { display: flex; align-items: center; justify-content: space-between; text-align: left; }
.task-calendar { position: absolute; bottom: calc(100% + .4rem); left: 0; z-index: 10; width: 340px; max-width: 100%; max-height: min(360px, 65vh); overflow-y: auto; padding: .8rem; border: 1px solid var(--bs-border-color, #ddd); border-radius: .8rem; background: var(--bs-body-bg, #fff); box-shadow: 0 4px 14px #00000014; }
.task-calendar-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: .5rem; text-transform: capitalize; }
.task-calendar-nav { border: 0; background: transparent; color: inherit; font-size: 1.5rem; padding: 0 .5rem; }
.task-calendar-grid { display: grid; grid-template-columns: repeat(7, minmax(0, 1fr)); gap: .2rem; text-align: center; }
.task-calendar-weekday { font-size: .75rem; color: var(--bs-secondary-color, #777); padding: .25rem 0; }
.task-calendar-day { border: 1px solid transparent; border-radius: .3rem; background: transparent; color: inherit; min-height: 30px; font-size: .85rem; }
.task-calendar-day.outside { opacity: .5; }
.task-calendar-day:disabled { background: var(--bs-secondary-bg, #eee); color: var(--bs-secondary-color, #777); opacity: .55; cursor: not-allowed; }
.task-calendar-day:not(:disabled):hover, .task-calendar-day.selected { border-color: var(--bs-primary, #0d6efd); background: var(--bs-tertiary-bg, #f4f4f4); }
.task-calendar-footer { display: flex; align-items: center; justify-content: space-between; gap: .5rem; margin-top: .6rem; padding-top: .6rem; border-top: 1px solid var(--bs-border-color, #ddd); }
.task-calendar-hint { display: flex; align-items: flex-start; gap: .3rem; font-size: .72rem; line-height: 1.35; color: var(--bs-secondary-color, #777); }
.task-calendar-hint svg { flex-shrink: 0; }
</style>
