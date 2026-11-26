<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { getAllAnnouncements, createAnnouncement, updateAnnouncement, deleteAnnouncement } from '@/api/announcements'
import { ApiError, getErrorMessages } from '@/api/http'
import { ANNOUNCEMENT_LIMITS } from '@/constants'
import { formatDateParts, isRecent, normalizeSearchText } from '@/utils/format'
import { useNotice } from '@/composables/useNotice'
import AlertMessages from '@/components/AlertMessages.vue'
import BaseModal from '@/components/BaseModal.vue'
import ConfirmModal from '@/components/ConfirmModal.vue'
import '@/assets/css/announcements.css'
 
// Yalnızca son 2 gün içinde yayınlanan duyurular "Yeni" sayılır
const NEW_DAYS = 2
 
const announcements = ref([])
const isLoading = ref(true)
const loadErrors = ref([])
const { notice, pushNotice } = useNotice()
const hasAnnouncements = computed(() => announcements.value.length > 0)
 
// Arama: yüklenen duyurular içinde başlık ve metne göre süzer (Türkçe karakterlere duyarsız)
const searchText = ref('')
const filteredAnnouncements = computed(() => {
  const query = normalizeSearchText(searchText.value)
  if (!query) return announcements.value
  return announcements.value.filter((item) => normalizeSearchText(`${item.title} ${item.content}`).includes(query))
})
 
async function loadAnnouncements() {
  isLoading.value = true
  loadErrors.value = []
  announcements.value = []
  try {
    announcements.value = await getAllAnnouncements()
  } catch (error) {
    loadErrors.value = getErrorMessages(error)
  } finally {
    isLoading.value = false
  }
}
 
const isFormOpen = ref(false)
const form = reactive({ title: '', content: '' })
const editingID = ref(null)
const isSaving = ref(false)
const formErrors = ref([])
const titleInput = ref(null)
const contentInput = ref(null)
const isEditing = computed(() => editingID.value !== null)
 
function openCreate() {
  editingID.value = null
  form.title = ''
  form.content = ''
  formErrors.value = []
  isSaving.value = false
  isFormOpen.value = true
}
 
function openEdit(item) {
  editingID.value = item.announcementID
  form.title = (item.title ?? '').trim()
  form.content = (item.content ?? '').trim()
  formErrors.value = []
  isSaving.value = false
  isFormOpen.value = true
}
 
async function submitUpdate(title, content) {
  const current = announcements.value.find((item) => item.announcementID === editingID.value)
  if (current && title === (current.title ?? '').trim() && content === (current.content ?? '').trim()) {
    isFormOpen.value = false
    return
  }
 
  formErrors.value = []
  isSaving.value = true
  try {
    await updateAnnouncement(editingID.value, { title, content })
    isFormOpen.value = false
    pushNotice(`"${title}" duyurusu güncellendi.`)
    await loadAnnouncements()
  } catch (error) {
    formErrors.value = getErrorMessages(error)
    if (error instanceof ApiError && error.status === 404) loadAnnouncements()
  } finally {
    isSaving.value = false
  }
}
 
async function submitForm() {
  if (isSaving.value) return
 
  const title = form.title.trim()
  const content = form.content.trim()
 
  if (!title) {
    formErrors.value = ['Başlık zorunludur.']
    titleInput.value.focus()
    return
  }
  if (!content) {
    formErrors.value = ['Duyuru metni zorunludur.']
    contentInput.value.focus()
    return
  }
 
  if (isEditing.value) {
    await submitUpdate(title, content)
    return
  }
 
  formErrors.value = []
  isSaving.value = true
  try {
    await createAnnouncement({ title, content })
    isFormOpen.value = false
    pushNotice(`"${title}" duyurusu yayınlandı.`)
    await loadAnnouncements()
  } catch (error) {
    formErrors.value = getErrorMessages(error)
  } finally {
    isSaving.value = false
  }
}
 
const isDeleteOpen = ref(false)
const announcementToDelete = ref(null)
function openDelete(item) {
  announcementToDelete.value = item
  isDeleteOpen.value = true
}
const deleteAction = () => deleteAnnouncement(announcementToDelete.value.announcementID)
function onDeleted() {
  pushNotice(`"${announcementToDelete.value.title}" duyurusu silindi.`)
  loadAnnouncements()
}
 
onMounted(loadAnnouncements)
</script>
 
<template>
  <div>
    <div class="page-head">
      <h4 class="mb-0">Duyurular</h4>
      <div class="d-flex flex-wrap align-items-center gap-3">
        <span v-if="hasAnnouncements" class="small text-muted">{{ filteredAnnouncements.length }} duyuru</span>
        <label class="ann-search">
          <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><circle cx="11" cy="11" r="7"/><path d="m21 21-4.35-4.35"/></svg>
          <input v-model="searchText" type="search" placeholder="Duyuru ara" aria-label="Duyuru ara" autocomplete="off" />
        </label>
        <button type="button" class="ann-btn ann-btn--primary" @click="openCreate">Duyuru Yayınla</button>
      </div>
    </div>
 
    <AlertMessages v-if="notice" :type="notice.type" :messages="notice.text" role="status" />
    <AlertMessages v-if="isLoading" type="info" messages="Duyurular yükleniyor..." />
    <AlertMessages v-else-if="loadErrors.length" :messages="loadErrors" />
    <AlertMessages v-else-if="!hasAnnouncements" type="secondary" messages="Henüz duyuru yok." />
 
    <AlertMessages v-else-if="!filteredAnnouncements.length" type="secondary" messages="Aramaya uygun duyuru bulunamadı." />
 
    <div v-else class="announcement-zigzag">
      <article
        v-for="(item, index) in filteredAnnouncements"
        :key="item.announcementID ?? index"
        class="zz-item"
        :class="[index % 2 === 0 ? 'is-right' : 'is-left', { 'is-recent': isRecent(item.createdDate, NEW_DAYS) }]"
      >
        <div class="zz-card">
          <h5 class="zz-title text-break">{{ item.title }}<span v-if="isRecent(item.createdDate, NEW_DAYS)" class="announcement-new">Yeni</span></h5>
          <p class="zz-content text-break mb-0">{{ item.content }}</p>
          <div v-if="item.announcementID" class="zz-actions">
            <button type="button" class="ann-icon-btn" title="Düzenle" aria-label="Düzenle" @click="openEdit(item)">
              <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M17 3a2.83 2.83 0 1 1 4 4L7.5 20.5 2 22l1.5-5.5L17 3z"/></svg>
            </button>
            <button type="button" class="ann-icon-btn" title="Sil" aria-label="Sil" @click="openDelete(item)">
              <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><polyline points="3 6 5 6 21 6"/><path d="M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6"/><path d="M10 11v6"/><path d="M14 11v6"/><path d="M9 6V4a1 1 0 0 1 1-1h4a1 1 0 0 1 1 1v2"/></svg>
            </button>
          </div>
        </div>
        <time class="zz-tab" :datetime="item.createdDate">
          <span class="zz-day">{{ formatDateParts(item.createdDate).day }}</span>
          <span class="zz-month">{{ formatDateParts(item.createdDate).month }} {{ formatDateParts(item.createdDate).year }}</span>
        </time>
      </article>
    </div>
 
    <BaseModal v-model="isFormOpen" :title="isEditing ? 'Duyuruyu Düzenle' : 'Yeni Duyuru'" as-form @submit="submitForm" @shown="titleInput.focus()">
      <div class="mb-3">
        <label for="announcementTitle" class="form-label">Başlık <span class="text-danger">*</span></label>
        <input id="announcementTitle" ref="titleInput" v-model="form.title" type="text" class="form-control" :maxlength="ANNOUNCEMENT_LIMITS.title" :disabled="isSaving" />
      </div>
      <div class="mb-3">
        <label for="announcementContent" class="form-label">Duyuru metni <span class="text-danger">*</span></label>
        <textarea id="announcementContent" ref="contentInput" v-model="form.content" class="form-control" rows="6" :maxlength="ANNOUNCEMENT_LIMITS.content" :disabled="isSaving"></textarea>
        <div class="form-text">Yazdığınız satır boşlukları olduğu gibi gösterilir.</div>
      </div>
      <AlertMessages v-if="formErrors.length" :messages="formErrors" class="mb-0" />
      <template #footer>
        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal" :disabled="isSaving">Vazgeç</button>
        <button type="submit" class="btn btn-primary" :disabled="isSaving">{{ isEditing ? 'Kaydet' : 'Yayınla' }}</button>
      </template>
    </BaseModal>
 
    <ConfirmModal v-model="isDeleteOpen" title="Duyuruyu Sil" confirm-text="Duyuruyu Sil" :action="deleteAction" @done="onDeleted" @stale="loadAnnouncements">
      <p class="mb-2"><strong class="text-break">{{ announcementToDelete?.title }}</strong> duyurusu kalıcı olarak silinecek.</p>
      <p class="mb-0 text-muted small">Bu işlem geri alınamaz.</p>
    </ConfirmModal>
  </div>
</template>
 