<script setup>
import { ref, onMounted } from 'vue'
import { getMyProjects } from '@/api/ongoingProjects'
import { getErrorMessages } from '@/api/http'
import { getSession } from '@/auth/session'
import { formatDate } from '@/utils/format'
import AlertMessages from '@/components/AlertMessages.vue'
import TechnologyBadges from '@/components/TechnologyBadges.vue'

// Rota koruması sayesinde bu sayfaya oturum olmadan gelinmez
const session = getSession()
const fullName = `${session.firstName} ${session.lastName}`.trim()

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
   

    <!-- Staj tarihleri -->
    <div class="row g-3 mb-4">
      <div class="col-sm-6 col-lg-4">

      </div>
    
    </div>

    <h5 class="mb-3">Projelerim</h5>

    <!-- Yükleniyor / hata / boş liste: aynı anda yalnızca biri çizilir -->
    <AlertMessages v-if="isLoading" type="info" messages="Projeler yükleniyor..." />
    <AlertMessages v-else-if="loadErrors.length" :messages="loadErrors" />
    <AlertMessages v-else-if="projects.length === 0" type="secondary" messages="Henüz size atanmış bir proje yok." />

    <div v-else class="row g-3">
      <div v-for="project in projects" :key="project.ongoingProjectID ?? project.projectTitle" class="col-md-6 col-xl-4">
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