<script setup>
import { ref, computed, onMounted } from 'vue'
import { getLatestAnnouncements } from '@/api/announcements'
import { getErrorMessages } from '@/api/http'
import { formatDateParts, isRecent } from '@/utils/format'
import AlertMessages from '@/components/AlertMessages.vue'
import '@/assets/css/announcements.css'
 
// Yalnızca son 2 gün içinde yayınlanan duyurular "Yeni" sayılır
const NEW_DAYS = 2
 
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
 
    <div v-else class="announcement-zigzag">
      <article
        v-for="(item, index) in announcements"
        :key="item.announcementID ?? index"
        class="zz-item"
        :class="[index % 2 === 0 ? 'is-right' : 'is-left', { 'is-recent': isRecent(item.createdDate, NEW_DAYS) }]"
      >
        <div class="zz-card">
          <h5 class="zz-title text-break">{{ item.title }}<span v-if="isRecent(item.createdDate, NEW_DAYS)" class="announcement-new">Yeni</span></h5>
          <p class="zz-content text-break mb-0">{{ item.content }}</p>
        </div>
        <div class="zz-tab" aria-hidden="true">
          <span class="zz-day">{{ formatDateParts(item.createdDate).day }}</span>
          <span class="zz-month">{{ formatDateParts(item.createdDate).month }} {{ formatDateParts(item.createdDate).year }}</span>
        </div>
      </article>
    </div>
  </div>
</template>
 