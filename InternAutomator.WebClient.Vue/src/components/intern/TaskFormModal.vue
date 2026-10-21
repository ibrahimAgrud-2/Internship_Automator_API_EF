<script setup>
import { ref, reactive, watch } from 'vue'
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
  userID: { type: [Number, String], default: null },
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

// Görevler gün bazlıdır; saat sorulmaz. Seçilen günün sonu (23:59:59) gönderilir, böylece
// API'nin "teslim zamanı şu andan sonra olmalı" kuralı bugün seçildiğinde de sağlanır.
// Saat bilgisi eklenmek istenirse sadece bu fonksiyon değişir.
const toTaskDueDate = (dayText) => `${dayText}T23:59:59`

// Pencere her açıldığında formu hazırla
watch(
  () => props.modelValue,
  (isOpen) => {
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
      lastUpdatedByUserID: props.userID,
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
  } else if (!props.userID) {
    message = 'Oturum bilgisi eksik. Lütfen çıkış yapıp tekrar giriş yapın.'
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
      createdByUserID: props.userID,
      ongoingProjectID,
      title,
      description: form.description.trim(),
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
      <input id="taskDueDate" ref="dueDateInput" v-model="form.dayText" type="date" class="form-control" :min="minDate" :disabled="isSaving" />
    </div>

    <template #footer>
      <button type="button" class="btn btn-secondary" data-bs-dismiss="modal" :disabled="isSaving">Vazgeç</button>
      <button type="submit" class="btn btn-primary" :disabled="isSaving">{{ task ? 'Kaydet' : 'Görevi Ekle' }}</button>
    </template>
  </BaseModal>
</template>
