<script setup>
import { ref, computed, onMounted } from 'vue'
import { getLatestAnnouncements } from '@/api/announcements'
import { getErrorMessages } from '@/api/http'
import { formatDateTime } from '@/utils/format'
import AlertMessages from '@/components/AlertMessages.vue'
import '@/assets/css/announcements.css'

// Renk sayısı announcements.css içindeki announcement-color-N sınıflarıyla aynı olmalı
const ANNOUNCEMENT_COLOR_COUNT = 5
const colorClass = (index) => `announcement-color-${index % ANNOUNCEMENT_COLOR_COUNT}`

const announcements = ref([])
const isLoading = ref(true)
const loadErrors = ref([])

const hasAnnouncements = computed(() => announcements.value.length > 0)

async function loadAnnouncements() {
  isLoading.value = true
  loadErrors.value = []
  announcements.value = []

  try {
    // Son 15 duyuru, en yeni başta; sıra API'dan geldiği gibi gösterilir
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
    <h4 class="mb-4">Duyurular</h4>

    <!-- Yükleniyor / hata / boş liste: aynı anda yalnızca biri çizilir -->
    <AlertMessages v-if="isLoading" type="info" messages="Duyurular yükleniyor..." />
    <AlertMessages v-else-if="loadErrors.length" :messages="loadErrors" />
    <AlertMessages v-else-if="!hasAnnouncements" type="secondary" messages="Henüz duyuru yok." />

    <div v-else class="d-flex flex-column gap-3">
      <div v-for="(item, index) in announcements" :key="item.announcementID ?? index" class="card announcement-card" :class="colorClass(index)">
        <div class="announcement-band">Yayın tarihi: {{ formatDateTime(item.createdDate, '-') }}</div>

        <div class="card-body">
          <h5 class="card-title text-break mb-3">{{ item.title }}</h5>
          <p class="card-text text-break announcement-content mb-0">{{ item.content }}</p>
        </div>
      </div>
    </div>
  </div>
</template>