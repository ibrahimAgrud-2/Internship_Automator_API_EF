<script setup>
import { ref, onMounted } from 'vue'
import { getMyProjects } from '@/api/ongoingProjects'
import { getErrorMessages } from '@/api/http'
import { formatDate } from '@/utils/format'
import AlertMessages from '@/components/AlertMessages.vue'
import TechnologyBadges from '@/components/TechnologyBadges.vue'

const projects = ref([])
const isLoading = ref(true)
const loadErrors = ref([])

async function loadProjects() {
  isLoading.value = true
  loadErrors.value = []
  projects.value = []

  try {
    projects.value = await getMyProjects()
  } catch (error) {
    loadErrors.value = getErrorMessages(error)
  } finally {
    isLoading.value = false
  }
}

onMounted(loadProjects)
</script>

<template>
  <div>
   

    <h5 class="mb-3">Projelerim</h5>

    <!-- Yükleniyor / hata / boş liste: aynı anda yalnızca biri çizilir -->
    <AlertMessages v-if="isLoading" type="info" messages="Projeler yükleniyor..." />
    <AlertMessages v-else-if="loadErrors.length" :messages="loadErrors" />
    <AlertMessages v-else-if="projects.length === 0" type="secondary" messages="Henüz size atanmış bir proje yok." />

    <div v-else class="intern-project-grid">
      <div v-for="project in projects" :key="project.ongoingProjectID ?? project.projectTitle" class="intern-project-item">
        <div class="card h-100">
          <div class="card-body">
            <h5 class="card-title">{{ project.projectTitle }}</h5>
            <TechnologyBadges :technologies="project.technologies" class="mb-3" />
            <div class="small text-muted">Atanma tarihi: {{ formatDate(project.startDate, '-') }}</div>
            <div class="small text-muted">Teslim tarihi: {{ formatDate(project.dueDate, '-') }}</div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
<style scoped>
.intern-project-grid{display:grid;grid-template-columns:repeat(auto-fill,minmax(220px,1fr));gap:14px}.intern-project-item{min-width:0}.intern-project-item .card{border:1px solid #eae4e4;border-radius:13px;box-shadow:0 6px 18px rgba(0,0,0,.06)}.intern-project-item .card-title{font-size:1rem;font-weight:700}
@media(max-width:500px){.intern-project-grid{grid-template-columns:1fr}}
</style>
