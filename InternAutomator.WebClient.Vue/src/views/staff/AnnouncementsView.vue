<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { getAllAnnouncements, createAnnouncement, updateAnnouncement, deleteAnnouncement } from '@/api/announcements'
import { ApiError, getErrorMessages } from '@/api/http'
import { getSession } from '@/auth/session'
import { ANNOUNCEMENT_LIMITS } from '@/constants'
import { formatDateTime } from '@/utils/format'
import { useNotice } from '@/composables/useNotice'
import AlertMessages from '@/components/AlertMessages.vue'
import BaseModal from '@/components/BaseModal.vue'
import ConfirmModal from '@/components/ConfirmModal.vue'
import '@/assets/css/announcements.css'

const session = getSession()

// Renk sayısı announcements.css içindeki announcement-color-N sınıflarıyla aynı olmalı
const ANNOUNCEMENT_COLOR_COUNT = 5
const colorClass = (index) => `announcement-color-${index % ANNOUNCEMENT_COLOR_COUNT}`

// ---------- Liste ----------
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

// ---------- Yayınlama / düzenleme (aynı form iki iş için) ----------
const isFormOpen = ref(false)
const form = reactive({ title: '', content: '' })
const editingID = ref(null) // null: yeni duyuru, dolu: düzenleme
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

  // Hiçbir şey değişmediyse sunucuya gitmeye gerek yok
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

    // Duyuru artık yoksa ekrandaki liste eskimiş demektir; arkada tazelenir
    if (error instanceof ApiError && error.status === 404) {
      loadAnnouncements()
    }
  } finally {
    isSaving.value = false
  }
}

async function submitForm() {
  // İstek sürerken ikinci gönderim (Enter'a art arda basma vb.) yapılmasın
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

  // Düzenleme modunda yayınlayan ve yayın tarihi değişmez; sadece başlık ve metin güncellenir
  if (isEditing.value) {
    await submitUpdate(title, content)
    return
  }

  // Oturumda kullanıcı numarası yoksa istek hiç atılmaz
  if (!session.userID) {
    formErrors.value = ['Oturum bilgisi eksik. Lütfen çıkış yapıp tekrar giriş yapın.']
    return
  }

  formErrors.value = []
  isSaving.value = true

  try {
    // Yayın tarihini API belirler; istemci göndermez
    await createAnnouncement({ createdByUserID: session.userID, title, content })

    isFormOpen.value = false
    pushNotice(`"${title}" duyurusu yayınlandı.`)
    await loadAnnouncements() // ekranda her zaman sunucudaki gerçek hal görünsün
  } catch (error) {
    formErrors.value = getErrorMessages(error)
  } finally {
    isSaving.value = false
  }
}

// ---------- Silme ----------
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

    <!-- İşlem sonucu bildirimi (duyuru yayınlandı...) -->
    <AlertMessages v-if="notice" :type="notice.type" :messages="notice.text" role="status" />

    <!-- Yükleniyor / hata / boş liste: aynı anda yalnızca biri çizilir -->
    <AlertMessages v-if="isLoading" type="info" messages="Duyurular yükleniyor..." />
    <AlertMessages v-else-if="loadErrors.length" :messages="loadErrors" />
    <AlertMessages v-else-if="!hasAnnouncements" type="secondary" messages="Henüz duyuru yok." />

    <!-- Duyuru listesi (sıralama API'dan geldiği gibi, en yeni başta) -->
    <div v-else class="d-flex flex-column gap-3">
      <div
        v-for="(item, index) in announcements"
        :key="item.announcementID ?? index"
        class="card announcement-card"
        :class="colorClass(index)"
      >
        <!-- Renkli şerit: tarihin ne olduğunu açıkça yazar -->
        <div class="announcement-band">Yayın tarihi: {{ formatDateTime(item.createdDate, '-') }}</div>

        <div class="card-body">
          <h5 class="card-title text-break mb-3">{{ item.title }}</h5>
          <p class="card-text text-break announcement-content mb-0">{{ item.content }}</p>

          <div class="d-flex align-items-center justify-content-between gap-3 mt-3">
            <span class="small text-muted">{{ item.createdByUserName ? 'Yayınlayan: ' + item.createdByUserName : '' }}</span>

            <!-- Kimliği olmayan duyuru düzenlenemez ya da silinemez -->
            <div v-if="item.announcementID" class="d-flex gap-2">
              <button type="button" class="btn btn-sm btn-outline-primary" @click="openEdit(item)">Düzenle</button>
              <button type="button" class="btn btn-sm btn-outline-danger" @click="openDelete(item)">Sil</button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Yayınlama / düzenleme penceresi -->
    <BaseModal
      v-model="isFormOpen"
      :title="isEditing ? 'Duyuruyu Düzenle' : 'Yeni Duyuru'"
      as-form
      @submit="submitForm"
      @shown="titleInput.focus()"
    >
      <div class="mb-3">
        <label for="announcementTitle" class="form-label">Başlık <span class="text-danger">*</span></label>
        <input
          id="announcementTitle"
          ref="titleInput"
          v-model="form.title"
          type="text"
          class="form-control"
          :maxlength="ANNOUNCEMENT_LIMITS.title"
          :disabled="isSaving"
        />
      </div>

      <div class="mb-3">
        <label for="announcementContent" class="form-label">Duyuru metni <span class="text-danger">*</span></label>
        <textarea
          id="announcementContent"
          ref="contentInput"
          v-model="form.content"
          class="form-control"
          rows="6"
          :maxlength="ANNOUNCEMENT_LIMITS.content"
          :disabled="isSaving"
        ></textarea>
        <div class="form-text">Yazdığınız satır boşlukları olduğu gibi gösterilir.</div>
      </div>

      <AlertMessages v-if="formErrors.length" :messages="formErrors" class="mb-0" />

      <template #footer>
        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal" :disabled="isSaving">Vazgeç</button>
        <button type="submit" class="btn btn-primary" :disabled="isSaving">{{ isEditing ? 'Kaydet' : 'Yayınla' }}</button>
      </template>
    </BaseModal>

    <!-- Silme penceresi -->
    <ConfirmModal
      v-model="isDeleteOpen"
      title="Duyuruyu Sil"
      confirm-text="Duyuruyu Sil"
      :action="deleteAction"
      @done="onDeleted"
      @stale="loadAnnouncements"
    >
      <p class="mb-2">
        <strong class="text-break">{{ announcementToDelete?.title }}</strong> duyurusu kalıcı olarak silinecek.
      </p>
      <p class="text-muted small mb-0">Bu işlem geri alınamaz.</p>
    </ConfirmModal>
  </div>
</template>
