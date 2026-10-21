<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import {
  getAllProjects,
  getProjectAssignedInternCount,
  createProject,
  updateProject,
  deleteProject,
} from '@/api/projects'
import { ApiError, getErrorMessages } from '@/api/http'
import { getSession } from '@/auth/session'
import { PROJECT_LIMITS } from '@/constants'
import { normalizeSearchText } from '@/utils/format'
import { useNotice } from '@/composables/useNotice'
import AlertMessages from '@/components/AlertMessages.vue'
import BaseModal from '@/components/BaseModal.vue'
import ConfirmModal from '@/components/ConfirmModal.vue'
import TechnologyBadges from '@/components/TechnologyBadges.vue'
import '@/assets/css/interns.css' // arama kutusu (.intern-search) burada da kullanılır
import '@/assets/css/projects.css'

const session = getSession()

// Veritabanından açıklama sonunda boşluklarla dolu gelebiliyor; gösterirken kırpılır
const getDescription = (project) => (project.description ?? '').trim()

// ---------- Liste ve arama ----------
const projects = ref([])
const isLoading = ref(true)
const loadErrors = ref([])
const searchText = ref('')
const { notice, pushNotice } = useNotice()

const hasProjects = computed(() => projects.value.length > 0)
const searchWords = computed(() => normalizeSearchText(searchText.value).split(' ').filter(Boolean))

// Yazılan her kelime proje adının içinde geçmeli; sıra önemli değil
const filteredProjects = computed(() =>
  projects.value.filter((project) => {
    const title = normalizeSearchText(project.title)
    return searchWords.value.every((word) => title.includes(word))
  }),
)

const resultCountText = computed(() =>
  searchWords.value.length > 0
    ? `${filteredProjects.value.length} / ${projects.value.length} proje`
    : `${projects.value.length} proje`,
)

async function loadProjects() {
  isLoading.value = true
  loadErrors.value = []
  projects.value = []

  try {
    // Sıralama API'dan geldiği gibi gösterilir
    projects.value = await getAllProjects()
  } catch (error) {
    loadErrors.value = getErrorMessages(error)
  } finally {
    isLoading.value = false
  }
}

// ---------- Detay penceresi ----------
const isDetailsOpen = ref(false)
const detailProject = ref(null)
const detailCount = ref('')
const detailError = ref([])
let detailsRequestId = 0 // eski bir isteğin cevabı yeni açılan pencereyi bozmasın diye

async function openDetails(project) {
  const requestId = ++detailsRequestId

  detailProject.value = project
  detailCount.value = 'Yükleniyor...'
  detailError.value = []
  isDetailsOpen.value = true

  // Atanan stajyer sayısı listede gelmediği için pencere açılınca tek proje için istenir
  try {
    const count = await getProjectAssignedInternCount(project.projectID)
    if (requestId !== detailsRequestId) return

    detailCount.value = String(count)
  } catch (error) {
    if (requestId !== detailsRequestId) return

    detailCount.value = '-'
    detailError.value = getErrorMessages(error)
  }
}

// ---------- Ekleme / düzenleme (aynı form iki iş için) ----------
const isFormOpen = ref(false)
const form = reactive({ title: '', technologies: '', description: '' })
const editingID = ref(null) // null: yeni proje, dolu: düzenleme
const isSaving = ref(false)
const formErrors = ref([])
const titleInput = ref(null)

const isEditing = computed(() => editingID.value !== null)

function openCreate() {
  editingID.value = null
  form.title = ''
  form.technologies = ''
  form.description = ''
  formErrors.value = []
  isSaving.value = false
  isFormOpen.value = true
}

function openEdit(project) {
  editingID.value = project.projectID
  form.title = project.title ?? ''
  form.technologies = (project.technologies ?? '').trim()
  form.description = getDescription(project)
  formErrors.value = []
  isSaving.value = false
  isFormOpen.value = true
}

async function submitUpdate(title) {
  const project = projects.value.find((item) => item.projectID === editingID.value)
  const changes = {
    title,
    description: form.description.trim(),
    technologies: form.technologies.trim(),
  }

  // Hiçbir şey değişmediyse sunucuya gitmeye gerek yok
  if (
    project &&
    changes.title === (project.title ?? '').trim() &&
    changes.description === getDescription(project) &&
    changes.technologies === (project.technologies ?? '').trim()
  ) {
    isFormOpen.value = false
    return
  }

  formErrors.value = []
  isSaving.value = true

  try {
    await updateProject(editingID.value, changes)

    isFormOpen.value = false
    pushNotice(`"${title}" projesi güncellendi.`)
    await loadProjects()
  } catch (error) {
    formErrors.value = getErrorMessages(error)

    // Proje artık yoksa ekrandaki liste eskimiş demektir; arkada tazelenir
    if (error instanceof ApiError && error.status === 404) {
      loadProjects()
    }
  } finally {
    isSaving.value = false
  }
}

async function submitForm() {
  // İstek sürerken ikinci gönderim (Enter'a art arda basma vb.) yapılmasın
  if (isSaving.value) return

  const title = form.title.trim()
  if (!title) {
    formErrors.value = ['Proje adı zorunludur.']
    titleInput.value.focus()
    return
  }

  // Düzenleme modunda oluşturan kullanıcı değişmez; sadece alanlar güncellenir
  if (isEditing.value) {
    await submitUpdate(title)
    return
  }

  // Oturumda kullanıcı numarası yoksa istek hiç atılmaz
  if (!session.userID) {
    formErrors.value = ['Oturum bilgisi eksik. Lütfen çıkış yapıp tekrar giriş yapın.']
    return
  }

  formErrors.value = []
  isSaving.value = true

  try {
    await createProject({
      createdByUserID: session.userID,
      title,
      description: form.description.trim(),
      technologies: form.technologies.trim(),
    })

    isFormOpen.value = false
    pushNotice(`"${title}" projesi eklendi.`)
    await loadProjects() // ekranda her zaman sunucudaki gerçek hal görünsün
  } catch (error) {
    formErrors.value = getErrorMessages(error)
  } finally {
    isSaving.value = false
  }
}

// ---------- Silme ----------
const isDeleteOpen = ref(false)
const projectToDelete = ref(null)

function openDelete(project) {
  projectToDelete.value = project
  isDeleteOpen.value = true
}

const deleteAction = () => deleteProject(projectToDelete.value.projectID)

function onDeleted() {
  pushNotice(`"${projectToDelete.value.title}" projesi silindi.`)
  loadProjects()
}

onMounted(loadProjects)
</script>

<template>
  <div>
    <div class="d-flex flex-wrap align-items-center justify-content-between gap-3 mb-4">
      <h4 class="mb-0">Projeler</h4>

      <div class="d-flex flex-wrap align-items-center gap-3">
        <span v-if="hasProjects" class="small text-muted">{{ resultCountText }}</span>
        <div class="input-group intern-search">
          <input
            v-model="searchText"
            type="search"
            class="form-control"
            placeholder="Proje adı ara..."
            aria-label="Proje ara"
            autocomplete="off"
            :disabled="!hasProjects"
            @keydown.esc="searchText = ''"
          />
          <button type="button" class="btn btn-outline-secondary" :disabled="searchText === ''" @click="searchText = ''">Temizle</button>
        </div>
        <button type="button" class="btn btn-outline-primary" @click="openCreate">Proje Ekle</button>
      </div>
    </div>

    <AlertMessages v-if="notice" :type="notice.type" :messages="notice.text" role="status" />

    <!-- Yükleniyor / hata / boş liste / arama sonucu yok: aynı anda yalnızca biri çizilir -->
    <AlertMessages v-if="isLoading" type="info" messages="Projeler yükleniyor..." />
    <AlertMessages v-else-if="loadErrors.length" :messages="loadErrors" />
    <AlertMessages v-else-if="!hasProjects" type="secondary" messages="Henüz proje yok." />
    <AlertMessages v-else-if="filteredProjects.length === 0" type="secondary" messages="Aramanızla eşleşen proje bulunamadı." />

    <div class="project-grid">
      <div v-for="project in filteredProjects" :key="project.projectID" class="card project-card">
        <div class="card-body">
          <h5 class="card-title text-break">{{ project.title }}</h5>
          <TechnologyBadges :technologies="project.technologies" class="mb-3" />
          <p v-if="getDescription(project)" class="small text-muted text-break mb-0 project-description">{{ getDescription(project) }}</p>
        </div>

        <div class="card-footer bg-transparent border-0 pb-3">
          <button type="button" class="btn btn-sm btn-outline-primary" @click="openDetails(project)">Detaylar</button>
          <button type="button" class="btn btn-sm btn-outline-secondary ms-2" @click="openEdit(project)">Düzenle</button>
          <button type="button" class="btn btn-sm btn-outline-danger ms-2" @click="openDelete(project)">Sil</button>
        </div>
      </div>
    </div>

    <!-- Proje detay penceresi -->
    <BaseModal v-model="isDetailsOpen" :title="detailProject?.title ?? ''" :static-backdrop="false" closable>
      <p class="text-break" style="white-space: pre-line">{{ detailProject ? getDescription(detailProject) || 'Açıklama yok.' : '' }}</p>
      <TechnologyBadges :technologies="detailProject?.technologies" class="mb-3" />

      <div class="small"><span class="text-muted">Oluşturan: </span><span>{{ detailProject?.userName || '-' }}</span></div>
      <div class="small"><span class="text-muted">Atanan stajyer sayısı: </span><span>{{ detailCount }}</span></div>

      <AlertMessages v-if="detailError.length" :messages="detailError" class="mt-3" />

      <template #footer>
        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Kapat</button>
      </template>
    </BaseModal>

    <!-- Ekleme / düzenleme penceresi -->
    <BaseModal
      v-model="isFormOpen"
      :title="isEditing ? 'Projeyi Düzenle' : 'Yeni Proje'"
      as-form
      @submit="submitForm"
      @shown="titleInput.focus()"
    >
      <AlertMessages v-if="formErrors.length" :messages="formErrors" />

      <div class="mb-3">
        <label for="projectTitle" class="form-label">Proje adı <span class="text-danger">*</span></label>
        <input
          id="projectTitle"
          ref="titleInput"
          v-model="form.title"
          type="text"
          class="form-control"
          :maxlength="PROJECT_LIMITS.title"
          :disabled="isSaving"
        />
      </div>

      <div class="mb-3">
        <label for="projectTechnologies" class="form-label">Teknolojiler</label>
        <input
          id="projectTechnologies"
          v-model="form.technologies"
          type="text"
          class="form-control"
          placeholder="C#, EF Core, ASP.NET"
          :maxlength="PROJECT_LIMITS.technologies"
          :disabled="isSaving"
        />
        <div class="form-text">Virgülle ayırın.</div>
      </div>

      <div class="mb-0">
        <label for="projectDescription" class="form-label">Açıklama</label>
        <textarea
          id="projectDescription"
          v-model="form.description"
          class="form-control"
          rows="3"
          :maxlength="PROJECT_LIMITS.description"
          :disabled="isSaving"
        ></textarea>
      </div>

      <template #footer>
        <button type="button" class="btn btn-outline-secondary" data-bs-dismiss="modal" :disabled="isSaving">Vazgeç</button>
        <button type="submit" class="btn btn-primary" :disabled="isSaving">{{ isEditing ? 'Kaydet' : 'Projeyi Ekle' }}</button>
      </template>
    </BaseModal>

    <!-- Silme penceresi -->
    <ConfirmModal
      v-model="isDeleteOpen"
      title="Projeyi Sil"
      confirm-text="Projeyi Sil"
      :action="deleteAction"
      @done="onDeleted"
      @stale="loadProjects"
    >
      <p class="mb-2">
        <strong class="text-break">{{ projectToDelete?.title }}</strong> projesi kalıcı olarak silinecek.
      </p>
      <p class="text-muted small mb-0">Bu işlem geri alınamaz.</p>
    </ConfirmModal>
  </div>
</template>
