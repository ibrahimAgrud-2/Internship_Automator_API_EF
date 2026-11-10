<script setup>
import { ref, computed, watch } from 'vue'
import { getAllProjects, createProject } from '@/api/projects'
import { assignProjectToIntern } from '@/api/ongoingProjects'
import { getErrorMessages } from '@/api/http'
import { PROJECT_LIMITS } from '@/constants'
import { getTodayText } from '@/utils/format'
import AlertMessages from '@/components/AlertMessages.vue'
import BaseModal from '@/components/BaseModal.vue'
import TechnologyBadges from '@/components/TechnologyBadges.vue'

// Stajyer detay sayfası: "Projeler > Ekle" ile açılan proje atama penceresi.
// Aynı pencerede iki görünüm vardır: "list" (mevcut projelerden seç) ve "create" (yeni proje oluştur).
const props = defineProps({
  modelValue: { type: Boolean, default: false },
  internID: { type: Number, required: true },
  assignedProjectIDs: { type: Array, default: () => [] }, // bu stajyere zaten atanmış projeler listeden çıkarılır
  userID: { type: [Number, String], default: null },
})

// assigned: atama yapıldı, pencere kapandı (üst sayfa bildirim gösterip detayları yeniler)
const emit = defineEmits(['update:modelValue', 'assigned'])

const view = ref('list') // 'list' | 'create'
const catalog = ref([]) // API'dan gelen tüm projeler
const isCatalogLoading = ref(false)
const catalogErrors = ref([])
const selectedProjectID = ref(null)
const dueDate = ref('')
const minDate = ref('')
const isAssigning = ref(false)
const errors = ref([])
let catalogRequestId = 0 // eski bir isteğin cevabı yeni açılan pencereyi bozmasın diye

// Yeni proje formu
const newProject = ref({ title: '', technologies: '', description: '' })
const isCreating = ref(false)
const titleInput = ref(null)

const assignable = computed(() => {
  const assigned = new Set(props.assignedProjectIDs)
  return catalog.value.filter((project) => !assigned.has(project.projectID))
})

const isBusy = computed(() => isAssigning.value || isCreating.value)

// Proje kataloğunu API'dan alır
async function loadCatalog() {
  const requestId = ++catalogRequestId

  catalog.value = []
  catalogErrors.value = []
  isCatalogLoading.value = true

  try {
    const projects = await getAllProjects()
    if (requestId !== catalogRequestId) return

    catalog.value = projects
  } catch (error) {
    if (requestId !== catalogRequestId) return

    catalogErrors.value = getErrorMessages(error)
  } finally {
    if (requestId === catalogRequestId) isCatalogLoading.value = false
  }
}

// Pencere her açıldığında temiz başla
watch(
  () => props.modelValue,
  (isOpen) => {
    if (!isOpen) return

    view.value = 'list'
    selectedProjectID.value = null
    dueDate.value = ''
    minDate.value = getTodayText() // sayfa gece yarısını geçerse diye her açılışta yenilenir
    isAssigning.value = false
    isCreating.value = false
    errors.value = []
    loadCatalog()
  },
)

function showList() {
  view.value = 'list'
  errors.value = []
}

function showCreate() {
  view.value = 'create'
  errors.value = []
  newProject.value = { title: '', technologies: '', description: '' }
  setTimeout(() => titleInput.value?.focus(), 0)
}

function selectProject(projectID) {
  if (isBusy.value) return
  selectedProjectID.value = projectID
}

async function confirmAssign() {
  if (isAssigning.value || selectedProjectID.value === null) return // çift tıklama koruması

  const project = assignable.value.find((item) => item.projectID === selectedProjectID.value)

  // Teslim tarihi zorunludur ve bugünden önce olamaz. min özelliği elle yazılan tarihi engellemez,
  // bu yüzden burada da kontrol edilir (yyyy-MM-dd metinleri tarih sırasıyla karşılaştırılabilir).
  if (!dueDate.value) {
    errors.value = ['Teslim tarihi zorunludur.']
    return
  }
  if (dueDate.value < getTodayText()) {
    errors.value = ['Teslim tarihi bugünden önce olamaz.']
    return
  }

  errors.value = []
  isAssigning.value = true

  try {
    await assignProjectToIntern(props.internID, selectedProjectID.value, dueDate.value)

    emit('update:modelValue', false)
    emit('assigned', project?.title ?? 'Proje')
  } catch (error) {
    errors.value = getErrorMessages(error)
  } finally {
    isAssigning.value = false
  }
}

// Yeni proje oluşturur; liste görünümüne dönünce yeni proje seçili gelir
async function submitNewProject() {
  if (isCreating.value) return

  const title = newProject.value.title.trim()
  if (!title) {
    errors.value = ['Proje adı zorunludur.']
    titleInput.value?.focus()
    return
  }

  // Oturumda kullanıcı numarası yoksa istek hiç atılmaz
  if (!props.userID) {
    errors.value = ['Oturum bilgisi eksik. Lütfen çıkış yapıp tekrar giriş yapın.']
    return
  }

  errors.value = []
  isCreating.value = true

  try {
    const created = await createProject({
      createdByUserID: props.userID,
      title,
      description: newProject.value.description.trim(),
      technologies: newProject.value.technologies.trim(),
    })

    selectedProjectID.value = created?.projectID ?? null
    showList()
    await loadCatalog()
  } catch (error) {
    errors.value = getErrorMessages(error)
  } finally {
    isCreating.value = false
  }
}
</script>

<template>
  <BaseModal
    :model-value="modelValue"
    :title="view === 'list' ? 'Proje Ata' : 'Yeni Proje'"
    dialog-class="modal-dialog-centered modal-lg modal-dialog-scrollable"
    @update:model-value="emit('update:modelValue', $event)"
  >
    <template v-if="view === 'list'" #header-actions>
      <button type="button" class="btn btn-sm btn-outline-primary" :disabled="isBusy" @click="showCreate">Yeni Proje Ekle</button>
    </template>

    <AlertMessages v-if="errors.length" :messages="errors" />

    <!-- Görünüm 1: mevcut projeler -->
    <div v-if="view === 'list'">
      <AlertMessages v-if="isCatalogLoading" type="info" messages="Projeler yükleniyor..." />
      <AlertMessages v-else-if="catalogErrors.length" :messages="catalogErrors" />
      <AlertMessages
        v-else-if="assignable.length === 0"
        type="secondary"
        messages='Atanabilecek proje yok. "Yeni Proje Ekle" ile yeni bir proje oluşturabilirsiniz.'
      />

      <div v-else class="list-group assign-list">
        <button
          v-for="project in assignable"
          :key="project.projectID"
          type="button"
          class="list-group-item list-group-item-action d-flex align-items-center justify-content-between gap-3"
          :class="{ active: project.projectID === selectedProjectID }"
          :aria-pressed="String(project.projectID === selectedProjectID)"
          @click="selectProject(project.projectID)"
        >
          <span class="fw-semibold text-break">{{ project.title }}</span>
          <TechnologyBadges :technologies="project.technologies" class="assign-row-technologies" />
        </button>
      </div>
    </div>

    <!-- Görünüm 2: yeni proje formu -->
    <form v-else id="assignCreateForm" autocomplete="off" novalidate @submit.prevent="submitNewProject">
      <div class="mb-3">
        <label for="assignTitle" class="form-label">Proje adı <span class="text-danger">*</span></label>
        <input id="assignTitle" ref="titleInput" v-model="newProject.title" type="text" class="form-control" :maxlength="PROJECT_LIMITS.title" :disabled="isCreating" />
      </div>
      <div class="mb-3">
        <label for="assignTechnologies" class="form-label">Teknolojiler</label>
        <input id="assignTechnologies" v-model="newProject.technologies" type="text" class="form-control" placeholder="C#, EF Core, ASP.NET" :maxlength="PROJECT_LIMITS.technologies" :disabled="isCreating" />
        <div class="form-text">Virgülle ayırın.</div>
      </div>
      <div>
        <label for="assignDescription" class="form-label">Açıklama</label>
        <textarea id="assignDescription" v-model="newProject.description" class="form-control" rows="3" :maxlength="PROJECT_LIMITS.description" :disabled="isCreating"></textarea>
      </div>
    </form>

    <template #footer>
      <!-- Liste görünümü -->
      <div v-if="view === 'list'" class="w-100 d-flex align-items-center gap-2">
        <label for="assignDueDate" class="small text-muted mb-0">Teslim tarihi <span class="text-danger">*</span></label>
        <input id="assignDueDate" v-model="dueDate" type="date" class="form-control form-control-sm w-auto" :min="minDate" :disabled="isBusy" />
        <button type="button" class="btn btn-secondary ms-auto" data-bs-dismiss="modal" :disabled="isBusy">Vazgeç</button>
        <button type="button" class="btn btn-primary" :disabled="isBusy || selectedProjectID === null" @click="confirmAssign">Projeyi Ata</button>
      </div>

      <!-- Yeni proje görünümü -->
      <div v-else class="w-100 d-flex gap-2">
        <button type="button" class="btn btn-secondary" :disabled="isCreating" @click="showList">Geri</button>
        <button type="submit" form="assignCreateForm" class="btn btn-primary ms-auto" :disabled="isCreating">Projeyi Ekle</button>
      </div>
    </template>
  </BaseModal>
</template>
