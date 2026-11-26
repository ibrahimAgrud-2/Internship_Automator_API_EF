<script setup>
import { ref, computed, onMounted } from 'vue'
import { getLatestAnnouncements } from '@/api/announcements'
import { getErrorMessages } from '@/api/http'
import { formatDate, formatDateParts, isRecent } from '@/utils/format'
import AlertMessages from '@/components/AlertMessages.vue'
import '@/assets/css/announcements.css'

const announcements = ref([])
const isLoading = ref(true)
const loadErrors = ref([])
const hasAnnouncements = computed(() => announcements.value.length > 0)

async function loadAnnouncements() {
  isLoading.value = true
  loadErrors.value = []
  announcements.value = []

  try {
    announcements.value = await getLatestAnnouncements()
  } catch (error) {
    loadErrors.value = getErrorMessages(error)
  } finally {
    isLoading.value = false
  }
}

onMounted(loadAnnouncements)
</script>

<template>
  <div>
    <h4 class="mb-3">Duyurular</h4>

    <AlertMessages v-if="isLoading" type="info" messages="Duyurular yükleniyor..." />
    <AlertMessages v-else-if="loadErrors.length" :messages="loadErrors" />
    <AlertMessages v-else-if="!hasAnnouncements" type="secondary" messages="Henüz duyuru yok." />

    <div v-else class="announcement-timeline">
      <article v-for="(item, index) in announcements" :key="item.announcementID ?? index" class="announcement-item" :class="{ 'is-recent': isRecent(item.createdDate) }">
        <div class="announcement-date-block" :title="formatDate(item.createdDate, '-')">
          <strong>{{ formatDateParts(item.createdDate).day }}</strong>
          <span>{{ formatDateParts(item.createdDate).month }}</span>
          <small>{{ formatDateParts(item.createdDate).year }}</small>
        </div>
        <div class="announcement-rail" aria-hidden="true"><span class="announcement-dot"></span></div>
        <div class="card announcement-card">
          <div class="card-body">
            <div class="announcement-meta">
              <span class="announcement-date-inline">{{ formatDate(item.createdDate, '-') }}</span>
            </div>
            <h5 class="card-title text-break mb-2">{{ item.title }}<span v-if="isRecent(item.createdDate)" class="announcement-new">Yeni</span></h5>
            <p class="card-text text-break announcement-content mb-0">{{ item.content }}</p>
          </div>
        </div>
      </article>
    </div>
  </div>
</template>
