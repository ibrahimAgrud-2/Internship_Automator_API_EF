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

    <div v-else class="d-flex flex-column gap-3">
      <div v-for="task in sortedTasks" :key="task.taskID" class="card task-card" :class="{ 'task-card-done': task.completed, 'task-card-overdue': !task.completed && getDaysBox(task).label === 'gün gecikti' }">
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
    </div>
  </div>
</template>
