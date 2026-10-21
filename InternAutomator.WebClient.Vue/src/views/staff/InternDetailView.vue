<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { getIntern } from '@/api/interns'
import { getInternProjects, revokeProject } from '@/api/ongoingProjects'
import { getInternTasks, completeTask, deleteTask } from '@/api/tasks'
import { ApiError, getErrorMessages } from '@/api/http'
import { getSession } from '@/auth/session'
import { INTERN_ACTIVE } from '@/constants'
import { formatDate } from '@/utils/format'
import { useNotice } from '@/composables/useNotice'
import AlertMessages from '@/components/AlertMessages.vue'
import ConfirmModal from '@/components/ConfirmModal.vue'
import InternStatusBadge from '@/components/InternStatusBadge.vue'
import TechnologyBadges from '@/components/TechnologyBadges.vue'
import TaskItem from '@/components/intern/TaskItem.vue'
import AssignProjectModal from '@/components/intern/AssignProjectModal.vue'
import TaskFormModal from '@/components/intern/TaskFormModal.vue'
import '@/assets/css/intern-detail.css'

const session = getSession()
const route = useRoute()

// Stajyer numarası adres çubuğundan gelir: /staff/interns/4
const internID = Number(route.params.internID)
const isValidInternID = Number.isInteger(internID) && internID > 0

const { notice, pushNotice } = useNotice()

// ---------- Durum ----------
const currentIntern = ref(null)
const pageError = ref('') // geçersiz adres, stajyer bulunamadı vb. sayfa düzeyindeki mesaj

const internProjects = ref([])
const projectsLoading = ref(true)
const projectsError = ref([])

const internTasks = ref([])
const tasksLoading = ref(true)
const tasksError = ref([])

const groupTasksByProject = ref(false)

const fullName = computed(() => (currentIntern.value ? `${currentIntern.value.firstName} ${currentIntern.value.lastName}`.trim() : ''))
watch(fullName, (name) => {
  if (name) document.title = `${name} - Stajyer Detayı`
})

// Ayrılan stajyere ekleme/değişiklik yapılamaz. Asıl engel API'dadır; burası sadece kullanıcıyı yönlendirir.
const canChange = computed(() => !currentIntern.value || currentIntern.value.status === INTERN_ACTIVE)

// Görev, stajyerin atanmış bir projesine verildiği için proje yoksa görev eklenemez.
const hasProjects = computed(() => internProjects.value.length > 0)
const tasksAddTitle = computed(() => {
  if (!canChange.value) return 'Ayrılan stajyere ekleme yapılamaz'
  return hasProjects.value ? '' : 'Görev vermek için önce stajyere proje atayın'
})

// ---------- Proje renkleri ----------
// Projeler 8 renk arasında döner; aynı proje hem solda hem gruplanmış görevlerde aynı rengi alır
const PROJECT_COLOR_COUNT = 8
const getProjectColorClass = (index) => (index >= 0 ? `project-color-${index % PROJECT_COLOR_COUNT}` : 'project-color-other')

const projectTitles = computed(() => new Map(internProjects.value.map((project) => [project.ongoingProjectID, project.projectTitle])))

// Gruplu görünüm: sadece görevi olan projeler, proje listesindeki sırayla
const taskGroups = computed(() => {
  const groups = new Map()
  internTasks.value.forEach((task) => {
    if (!groups.has(task.ongoingProjectID)) groups.set(task.ongoingProjectID, [])
    groups.get(task.ongoingProjectID).push(task)
  })

  const knownIDs = internProjects.value.map((project) => project.ongoingProjectID)
  const orderedIDs = [...knownIDs.filter((id) => groups.has(id)), ...[...groups.keys()].filter((id) => !knownIDs.includes(id))]

  return orderedIDs.map((id) => ({
    id,
    title: projectTitles.value.get(id) ?? 'Bilinmeyen proje',
    colorClass: getProjectColorClass(knownIDs.indexOf(id)),
    tasks: groups.get(id),
  }))
})

// ---------- Yükleme ----------
async function loadIntern() {
  try {
    currentIntern.value = await getIntern(internID)
    return true
  } catch (error) {
    pageError.value =
      error instanceof ApiError && error.status === 404
        ? 'Stajyer bulunamadı. Lütfen listeden bir stajyer seçin.'
        : getErrorMessages(error).join(' ')
    return false
  }
}

async function loadDetails() {
  projectsLoading.value = true
  tasksLoading.value = true
  projectsError.value = []
  tasksError.value = []

  // İki istek birlikte atılır; biri başarısız olsa diğeri yine görünür
  const [projectsResult, tasksResult] = await Promise.allSettled([getInternProjects(internID), getInternTasks(internID)])

  if (projectsResult.status === 'fulfilled') {
    internProjects.value = projectsResult.value
  } else {
    internProjects.value = []
    projectsError.value = getErrorMessages(projectsResult.reason)
  }

  if (tasksResult.status === 'fulfilled') {
    internTasks.value = tasksResult.value
  } else {
    internTasks.value = []
    tasksError.value = getErrorMessages(tasksResult.reason)
  }

  projectsLoading.value = false
  tasksLoading.value = false
}

onMounted(async () => {
  if (!isValidInternID) {
    pageError.value = 'Geçersiz stajyer numarası. Lütfen listeden bir stajyer seçin.'
    return
  }

  // Önce stajyerin var olduğundan emin ol; yoksa proje/görev istekleri hiç atılmaz
  if (await loadIntern()) {
    await loadDetails()
  }
})

// ---------- Proje atama ----------
const isAssignOpen = ref(false)

function onAssigned(title) {
  pushNotice(`"${title}" stajyere atandı.`)
  loadDetails()
}

// ---------- Görev ekleme / düzenleme ----------
const isTaskFormOpen = ref(false)
const editingTask = ref(null) // null: yeni görev

function openAddTask() {
  editingTask.value = null
  isTaskFormOpen.value = true
}

function openEditTask(task) {
  editingTask.value = task
  isTaskFormOpen.value = true
}

function onTaskSaved(text) {
  pushNotice(text)
  loadDetails()
}

// ---------- Görevi tamamla ----------
const isCompleteOpen = ref(false)
const taskToComplete = ref(null)

function openComplete(task) {
  taskToComplete.value = task
  isCompleteOpen.value = true
}

const completeAction = () => completeTask(taskToComplete.value.taskID)

function onCompleted() {
  pushNotice(`"${taskToComplete.value.title}" görevi tamamlandı olarak işaretlendi.`)
  loadDetails()
}

// ---------- Görevi sil ----------
const isDeleteTaskOpen = ref(false)
const taskToDelete = ref(null)

function openDeleteTask(task) {
  taskToDelete.value = task
  isDeleteTaskOpen.value = true
}

const deleteTaskAction = () => deleteTask(taskToDelete.value.taskID)

function onTaskDeleted() {
  pushNotice(`"${taskToDelete.value.title}" görevi silindi.`)
  loadDetails()
}

// ---------- Projeyi geri al ----------
const isRevokeOpen = ref(false)
const projectToRevoke = ref(null)

// API projeyle birlikte bağlı görevleri de siler. Sayı, sayfada zaten yüklü görevlerden hesaplanır.
const revokeTaskCount = computed(() =>
  projectToRevoke.value ? internTasks.value.filter((task) => task.ongoingProjectID === projectToRevoke.value.ongoingProjectID).length : 0,
)

function openRevoke(project) {
  projectToRevoke.value = project
  isRevokeOpen.value = true
}

const revokeAction = () => revokeProject(projectToRevoke.value.ongoingProjectID)

function onRevoked() {
  pushNotice(`"${projectToRevoke.value.projectTitle}" projesi stajyerden geri alındı.`)
  loadDetails() // projeler ve (silinmiş olabilecek) görevler birlikte yenilenir
}
</script>

<template>
  <div>
    <RouterLink to="/staff/interns" class="btn btn-outline-secondary btn-sm mb-3">&larr; Stajyerler</RouterLink>

    <!-- Stajyer özeti: yüklenince dolar -->
    <div class="mb-4">
      <div class="d-flex align-items-center flex-wrap gap-2">
        <h4 class="mb-0 text-break">{{ fullName || 'Stajyer Detayı' }}</h4>
        <InternStatusBadge v-if="currentIntern" :status="currentIntern.status" />
      </div>
      <template v-if="currentIntern">
        <div class="small text-muted text-break">{{ currentIntern.email || '-' }} · {{ currentIntern.phone || '-' }}</div>
        <div class="small text-muted">
          Başlangıç: {{ formatDate(currentIntern.startDate, 'Belirlenmedi') }} · Bitiş: {{ formatDate(currentIntern.endDate, 'Belirlenmedi') }}
        </div>
      </template>
    </div>

    <!-- İşlem sonucu bildirimi (proje atandı vb.) -->
    <AlertMessages v-if="notice" :type="notice.type" :messages="notice.text" role="status" />

    <!-- Geçersiz adres vb. sayfa düzeyindeki mesajlar -->
    <AlertMessages v-if="pageError" :messages="pageError" />

    <div v-else class="row g-4">
      <!-- Sol: Projeler -->
      <section class="col-lg-6">
        <div class="card h-100 detail-panel">
          <div class="card-header d-flex align-items-center gap-2">
            <h5 class="mb-0 me-auto">
              Projeler <span class="badge text-bg-secondary ms-1">{{ projectsError.length ? '!' : internProjects.length }}</span>
            </h5>
            <button
              type="button"
              class="btn btn-sm btn-outline-primary"
              :disabled="!currentIntern || !canChange"
              :title="canChange ? '' : 'Ayrılan stajyere ekleme yapılamaz'"
              @click="isAssignOpen = true"
            >
              Ekle
            </button>
          </div>
          <div class="card-body detail-panel-body">
            <AlertMessages v-if="projectsLoading" type="info" messages="Projeler yükleniyor..." />
            <AlertMessages v-else-if="projectsError.length" :messages="projectsError" />
            <AlertMessages v-else-if="internProjects.length === 0" type="secondary" messages="Bu stajyere henüz proje atanmamış." />

            <div v-else class="d-flex flex-column gap-3" :class="{ 'color-coded': groupTasksByProject }">
              <div
                v-for="(project, index) in internProjects"
                :key="project.ongoingProjectID"
                class="border rounded p-3 project-item"
                :class="getProjectColorClass(index)"
              >
                <div class="fw-semibold mb-2">{{ project.projectTitle }}</div>
                <TechnologyBadges :technologies="project.technologies" class="mb-2" />
                <div class="small text-muted">Başlangıç: {{ formatDate(project.startDate, '-') }} · Teslim: {{ formatDate(project.dueDate, '-') }}</div>

                <!-- Geri al butonu. Ayrılan stajyerde pasiftir (asıl engel API'dadır). -->
                <div class="mt-2 d-flex justify-content-end">
                  <button
                    type="button"
                    class="btn btn-sm btn-outline-danger"
                    :disabled="!canChange"
                    :title="canChange ? '' : 'Ayrılan stajyerden proje geri alınamaz'"
                    @click="openRevoke(project)"
                  >
                    Geri Al
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      <!-- Sağ: Görevler -->
      <section class="col-lg-6">
        <div class="card h-100 detail-panel">
          <div class="card-header d-flex align-items-center gap-2">
            <h5 class="mb-0 me-auto">
              Görevler <span class="badge text-bg-secondary ms-1">{{ tasksError.length ? '!' : internTasks.length }}</span>
            </h5>
            <button
              type="button"
              class="btn btn-sm"
              :class="groupTasksByProject ? 'btn-secondary' : 'btn-outline-secondary'"
              :aria-pressed="String(groupTasksByProject)"
              :disabled="internTasks.length === 0"
              @click="groupTasksByProject = !groupTasksByProject"
            >
              Projeye göre
            </button>
            <button
              type="button"
              class="btn btn-sm btn-outline-primary"
              :disabled="!currentIntern || !canChange || !hasProjects"
              :title="tasksAddTitle"
              @click="openAddTask"
            >
              Ekle
            </button>
          </div>
          <div class="card-body detail-panel-body">
            <AlertMessages v-if="tasksLoading" type="info" messages="Görevler yükleniyor..." />
            <AlertMessages v-else-if="tasksError.length" :messages="tasksError" />
            <AlertMessages v-else-if="internTasks.length === 0" type="secondary" messages="Bu stajyere henüz görev verilmemiş." />

            <!-- Düz liste -->
            <div v-else-if="!groupTasksByProject" class="d-flex flex-column gap-3">
              <TaskItem
                v-for="task in internTasks"
                :key="task.taskID"
                :task="task"
                :project-title="projectTitles.get(task.ongoingProjectID) ?? ''"
                :can-change="canChange"
                @edit="openEditTask"
                @complete="openComplete"
                @delete="openDeleteTask"
              />
            </div>

            <!-- Projeye göre gruplu görünüm -->
            <div v-else class="d-flex flex-column gap-3">
              <div v-for="group in taskGroups" :key="group.id" class="task-group" :class="group.colorClass">
                <div class="d-flex align-items-center gap-2 mb-2">
                  <span class="task-group-dot"></span>
                  <span class="fw-semibold text-break">{{ group.title }}</span>
                  <span class="badge text-bg-secondary">{{ group.tasks.length }}</span>
                </div>
                <div class="d-flex flex-column gap-3">
                  <TaskItem
                    v-for="task in group.tasks"
                    :key="task.taskID"
                    :task="task"
                    :project-title="group.title"
                    :can-change="canChange"
                    @edit="openEditTask"
                    @complete="openComplete"
                    @delete="openDeleteTask"
                  />
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>
    </div>

    <!-- Pencereler -->
    <AssignProjectModal
      v-if="currentIntern"
      v-model="isAssignOpen"
      :internID="internID"
      :assignedProjectIDs="internProjects.map((p) => p.projectID)"
      :userID="session?.userID"
      @assigned="onAssigned"
    />

    <TaskFormModal
      v-if="currentIntern"
      v-model="isTaskFormOpen"
      :projects="internProjects"
      :task="editingTask"
      :userID="session?.userID"
      @saved="onTaskSaved"
      @stale="loadDetails"
    />

    <ConfirmModal v-model="isCompleteOpen" title="Görevi Tamamla" confirm-text="Tamamla" confirm-class="btn-success" :action="completeAction" @done="onCompleted" @stale="loadDetails">
      <p class="mb-2"><strong class="text-break">{{ taskToComplete?.title }}</strong> görevi tamamlandı olarak işaretlenecek.</p>
      <p class="text-muted small mb-0">Bu işlem geri alınamaz.</p>
    </ConfirmModal>

    <ConfirmModal v-model="isDeleteTaskOpen" title="Görevi Sil" confirm-text="Görevi Sil" :action="deleteTaskAction" @done="onTaskDeleted" @stale="loadDetails">
      <p class="mb-2"><strong class="text-break">{{ taskToDelete?.title }}</strong> görevi kalıcı olarak silinecek.</p>
      <p class="text-muted small mb-0">Bu işlem geri alınamaz.</p>
    </ConfirmModal>

    <ConfirmModal v-model="isRevokeOpen" title="Projeyi Geri Al" confirm-text="Projeyi Geri Al" :action="revokeAction" @done="onRevoked" @stale="loadDetails">
      <p class="mb-2"><strong class="text-break">{{ projectToRevoke?.projectTitle }}</strong> projesi stajyerden geri alınacak.</p>
      <AlertMessages
        v-if="revokeTaskCount > 0"
        type="warning"
        :messages="`Bu proje için ${revokeTaskCount} görev atanmış. Projeyi geri alırsanız bu görevler de silinecek.`"
      />
      <p class="text-muted small mb-0">Bu işlem geri alınamaz.</p>
    </ConfirmModal>
  </div>
</template>
