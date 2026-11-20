<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { getAllAnnouncements, createAnnouncement, updateAnnouncement, deleteAnnouncement } from '@/api/announcements'
import { ApiError, getErrorMessages } from '@/api/http'
import { ANNOUNCEMENT_LIMITS } from '@/constants'
import { formatDate } from '@/utils/format'
import { useNotice } from '@/composables/useNotice'
import AlertMessages from '@/components/AlertMessages.vue'
import BaseModal from '@/components/BaseModal.vue'
import ConfirmModal from '@/components/ConfirmModal.vue'
import '@/assets/css/announcements.css'

const announcements = ref([])
const isLoading = ref(true)
const loadErrors = ref([])
const { notice, pushNotice } = useNotice()
const hasAnnouncements = computed(() => announcements.value.length > 0)

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
    <div class="d-flex flex-wrap align-items-center justify-content-between gap-3 mb-4">
      <h4 class="mb-0">Duyurular</h4>
      <div class="d-flex flex-wrap align-items-center gap-3">
        <span v-if="hasAnnouncements" class="small text-muted">{{ announcements.length }} duyuru</span>
        <button type="button" class="btn btn-outline-primary" @click="openCreate">Duyuru Yayınla</button>
      </div>
    </div>

    <AlertMessages v-if="notice" :type="notice.type" :messages="notice.text" role="status" />
    <AlertMessages v-if="isLoading" type="info" messages="Duyurular yükleniyor..." />
    <AlertMessages v-else-if="loadErrors.length" :messages="loadErrors" />
    <AlertMessages v-else-if="!hasAnnouncements" type="secondary" messages="Henüz duyuru yok." />

    <div v-else class="d-flex flex-column gap-3">
      <div v-for="(item, index) in announcements" :key="item.announcementID ?? index" class="card announcement-card">
        <div class="announcement-layout">
          <div class="announcement-date">{{ formatDate(item.createdDate, '-') }}</div>
          <div class="card-body">
            <h5 class="card-title text-break mb-3">{{ item.title }}</h5>
            <p class="card-text text-break announcement-content mb-0">{{ item.content }}</p>

            <div class="d-flex align-items-center justify-content-between gap-3 mt-3">
              <span></span>
              <div v-if="item.announcementID" class="d-flex gap-2">
                <button type="button" class="btn btn-sm btn-outline-primary" @click="openEdit(item)">Düzenle</button>
                <button type="button" class="btn btn-sm btn-outline-danger" @click="openDelete(item)">Sil</button>
              </div>
            </div>
          </div>
        </div>
      </div>
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
