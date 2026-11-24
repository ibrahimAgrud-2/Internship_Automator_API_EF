<script setup>
import { ref, computed, onMounted } from 'vue'
import { getMyTasks } from '@/api/tasks'
import { getErrorMessages } from '@/api/http'
import { formatDate, formatDateTime } from '@/utils/format'
import AlertMessages from '@/components/AlertMessages.vue'
import '@/assets/css/intern-tasks.css'



const tasks = ref([])
const isLoading = ref(true)
const loadErrors = ref([])

const hasTasks = computed(() => tasks.value.length > 0)

// Tamamlanan görevler en alta iner. İki grubun kendi içindeki sıra API'dan geldiği gibi kalır
// (bitiş tarihi en yakın olan başta). Görevi personel teslim aldığında tamamlandı yapar.
const sortedTasks = computed(() => [
  ...tasks.value.filter((task) => !task.completed),
  ...tasks.value.filter((task) => task.completed),
])

// ---------- Özet şeridi: sayılar aynı zamanda filtre görevi görür ----------
const filter = ref('all') // all | active | overdue | done

const isOverdue = (task) => !task.completed && getDaysLeft(task.dueDate) < 0
const stats = computed(() => ({
  all: tasks.value.length,
  active: tasks.value.filter((t) => !t.completed && !isOverdue(t)).length,
  overdue: tasks.value.filter(isOverdue).length,
  done: tasks.value.filter((t) => t.completed).length,
}))
const STAT_TILES = [
  { key: 'all', label: 'Tümü' },
  { key: 'active', label: 'Devam eden' },
  { key: 'overdue', label: 'Geciken' },
  { key: 'done', label: 'Tamamlanan' },
]
const visibleTasks = computed(() =>
  sortedTasks.value.filter((task) => {
    if (filter.value === 'active') return !task.completed && !isOverdue(task)
    if (filter.value === 'overdue') return isOverdue(task)
    if (filter.value === 'done') return task.completed
    return true
  }),
)

async function loadTasks() {
  isLoading.value = true
  loadErrors.value = []
  tasks.value = []

  try {
    tasks.value = await getMyTasks()
  } catch (error) {
    loadErrors.value = getErrorMessages(error)
  } finally {
    isLoading.value = false
  }
}

// ---------- Kalan gün ve aciliyet rengi ----------
const MS_PER_DAY = 24 * 60 * 60 * 1000

// Takvim günü olarak kalan gün: bugün 0, yarın 1, geçmişse negatif (saat değil, gün sayılır)
function getDaysLeft(dueDate) {
  const due = new Date(dueDate)
  const today = new Date()
  const dueDay = new Date(due.getFullYear(), due.getMonth(), due.getDate())
  const todayDay = new Date(today.getFullYear(), today.getMonth(), today.getDate())
  return Math.round((dueDay - todayDay) / MS_PER_DAY)
}

// En fazla kalan gün -> renk. 10 gün ve üstünde renk yok.
const URGENCY_LEVELS = [
  { maxDaysLeft: 1, tone: 'red' }, // 2 günden az (bugün, yarın, gecikmiş)
  { maxDaysLeft: 4, tone: 'orange' }, // 5 günden az
  { maxDaysLeft: 9, tone: 'yellow' }, // 10 günden az
]

// Sol kutunun rengi ve yazısı. Renk tek başına anlam taşımaz: kutuda gün sayısı da yazar.
function getDaysBox(task) {
  if (task.completed) return { tone: 'done', top: '✓', label: 'Tamamlandı', isWord: false }

  const daysLeft = getDaysLeft(task.dueDate)
  const level = URGENCY_LEVELS.find((item) => daysLeft <= item.maxDaysLeft)
  const tone = level ? level.tone : 'none'

  if (daysLeft < 0) return { tone, top: String(-daysLeft), label: 'gün gecikti', isWord: false }
  if (daysLeft === 0) return { tone, top: 'Bugün', label: 'son gün', isWord: true }
  return { tone, top: String(daysLeft), label: 'gün kaldı', isWord: false }
}

onMounted(loadTasks)
</script>

<template>
  <div>
    <h4 class="mb-4">Görevlerim</h4>

    <!-- Yükleniyor / hata / boş liste: aynı anda yalnızca biri çizilir -->
    <AlertMessages v-if="isLoading" type="info" messages="Görevler yükleniyor..." />
    <AlertMessages v-else-if="loadErrors.length" :messages="loadErrors" />
    <AlertMessages v-else-if="!hasTasks" type="secondary" messages="Henüz size verilmiş bir görev yok." />

    <template v-else>
    <div class="task-stats" role="group" aria-label="Görevleri filtrele">
      <button
        v-for="tile in STAT_TILES"
        :key="tile.key"
        type="button"
        class="task-stat"
        :class="[`task-stat-${tile.key}`, { active: filter === tile.key }]"
        :aria-pressed="filter === tile.key"
        @click="filter = tile.key"
      >
        <strong>{{ stats[tile.key] }}</strong>
        <span>{{ tile.label }}</span>
      </button>
    </div>

    <AlertMessages v-if="!visibleTasks.length" type="secondary" messages="Bu filtreye uyan görev yok." />

    <TransitionGroup v-else name="task-list" tag="div" class="d-flex flex-column gap-3 task-list">
      <div v-for="task in visibleTasks" :key="task.taskID" class="card task-card" :class="{ 'task-card-done': task.completed, 'task-card-overdue': !task.completed && getDaysBox(task).label === 'gün gecikti' }">
        <div class="d-flex">
          <!-- Sol: kalan gün kutusu -->
          <div class="task-days" :class="`task-days-${getDaysBox(task).tone}`">
            <span class="task-days-number" :class="{ 'task-days-number-word': getDaysBox(task).isWord }">{{ getDaysBox(task).top }}</span>
            <span class="task-days-label">{{ getDaysBox(task).label }}</span>
          </div>

          <!-- Sağ: görev bilgisi -->
          <div class="card-body task-body">
            <h5 class="card-title text-break">{{ task.title }}</h5>
            <p v-if="task.description" class="card-text text-break task-description">{{ task.description }}</p>

            <div class="small text-muted">
              Verilme: {{ formatDate(task.assignedDate, '-') }} · Son tarih: {{ formatDate(task.dueDate, '-') }}
              <template v-if="task.completed"> · Tamamlanma: {{ formatDateTime(task.completedDate, '-') }}</template>
            </div>
          </div>
        </div>
      </div>
    </TransitionGroup>
    </template>
  </div>
</template>
