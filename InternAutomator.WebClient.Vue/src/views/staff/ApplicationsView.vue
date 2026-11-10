<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { getAllApplications, updateApplicationStatus, acceptApplication } from '@/api/applications'
import { ApiError, getErrorMessages } from '@/api/http'
import {
  APPLICATION_NEW,
  APPLICATION_REJECTED,
  APPLICATION_APPROVED,
  APPLICATION_STATUS,
  APPLICATION_NOTES_LIMIT,
} from '@/constants'
import { formatDate, isHttpUrl } from '@/utils/format'
import { useNotice } from '@/composables/useNotice'
import AlertMessages from '@/components/AlertMessages.vue'
import BaseModal from '@/components/BaseModal.vue'

// Buton eylemi -> hangi durum, hangi API fonksiyonu, hangi mesajlar (tek yerde)
const ACTIONS = {
  approve: {
    status: APPLICATION_APPROVED,
    title: 'Başvuruyu onayla',
    result: 'onaylanacak',
    buttonText: 'Onayla',
    buttonClass: 'btn-success',
    send: acceptApplication, // API stajyeri oluşturur ve e-posta gönderir
    successMessage: 'Başvuru onaylandı. Stajyere giriş bilgileri e-posta ile gönderildi.',
    // API e-posta ya da stajyer oluşturma hatasında her şeyi geri alıp 500 döner
    serverErrorMessage: 'Onay tamamlanamadı. İşlem geri alındı, lütfen tekrar deneyin.',
  },
  reject: {
    status: APPLICATION_REJECTED,
    title: 'Başvuruyu reddet',
    result: 'reddedilecek',
    buttonText: 'Reddet',
    buttonClass: 'btn-danger',
    send: updateApplicationStatus,
    successMessage: 'Başvuru reddedildi.',
    serverErrorMessage: null, // genel sunucu hatası mesajı gösterilir
  },
}

// ---------- Liste ----------
const applications = ref([])
const isLoading = ref(true)
const loadErrors = ref([])
const { notice, pushNotice } = useNotice()

// En yeni başvuru en üstte
const sortedApplications = computed(() =>
  [...applications.value].sort((a, b) => new Date(b.applicationDate) - new Date(a.applicationDate)),
)

function getFullName(application) {
  const person = application.personInfo ?? {}
  return `${person.firstName ?? ''} ${person.lastName ?? ''}`.trim()
}

function getStatusInfo(status) {
  return APPLICATION_STATUS[status] ?? { text: 'Bilinmiyor', badge: 'bg-warning text-dark' }
}

// LinkedIn / GitHub: sadece http(s) adresleri tıklanabilir, diğerleri düz metin
function getLinks(application) {
  return [
    { label: 'LinkedIn', url: application.linkedinURL },
    { label: 'GitHub', url: application.githubURL },
  ].filter((link) => link.url)
}

async function loadApplications() {
  isLoading.value = true
  loadErrors.value = []
  applications.value = []

  try {
    applications.value = await getAllApplications()
  } catch (error) {
    loadErrors.value = getErrorMessages(error)
  } finally {
    isLoading.value = false
  }
}

// ---------- Karar penceresi ----------
const isDecisionOpen = ref(false)
const pending = reactive({ actionKey: null, applicationID: null })
const notes = ref('')
const isSaving = ref(false)
const decisionErrors = ref([])
const notesInput = ref(null)

const pendingAction = computed(() => ACTIONS[pending.actionKey] ?? null)
const pendingApplication = computed(() => applications.value.find((a) => a.applicationID === pending.applicationID))
const summaryText = computed(() =>
  pendingAction.value && pendingApplication.value
    ? `${getFullName(pendingApplication.value)} adlı adayın başvurusu ${pendingAction.value.result}. Lütfen bir not girin.`
    : '',
)

function openDecision(actionKey, application) {
  pending.actionKey = actionKey
  pending.applicationID = application.applicationID
  notes.value = ''
  decisionErrors.value = []
  isSaving.value = false
  isDecisionOpen.value = true
}

async function submitDecision() {
  if (isSaving.value || !pendingAction.value) return

  const text = notes.value.trim()
  if (!text) {
    decisionErrors.value = ['Not alanı zorunludur.']
    return
  }

  const action = pendingAction.value

  decisionErrors.value = []
  isSaving.value = true

  try {
    await action.send(pending.applicationID, action.status, text)

    isDecisionOpen.value = false
    pushNotice(action.successMessage)
    await loadApplications() // ekranda her zaman sunucudaki gerçek hal görünsün
  } catch (error) {
    const useCustomMessage = error instanceof ApiError && error.status >= 500 && action.serverErrorMessage

    decisionErrors.value = useCustomMessage ? [action.serverErrorMessage] : getErrorMessages(error)

    // İstek sunucuya ulaşıp başarısız olduysa liste eskimiş olabilir (geri alma yapılmış olabilir)
    if (error instanceof ApiError && error.status > 0) {
      loadApplications()
    }
  } finally {
    isSaving.value = false
  }
}

onMounted(loadApplications)
</script>

<template>
  <div>
    <h4 class="mb-4">Staj Başvuruları</h4>

    <!-- İşlem sonucu bildirimi (onaylandı, reddedildi...) -->
    <AlertMessages v-if="notice" :type="notice.type" :messages="notice.text" role="status" />

    <!-- Yükleniyor / hata / boş liste: aynı anda yalnızca biri çizilir -->
    <AlertMessages v-if="isLoading" type="info" messages="Başvurular yükleniyor..." />
    <AlertMessages v-else-if="loadErrors.length" :messages="loadErrors" />
    <AlertMessages v-else-if="applications.length === 0" type="secondary" messages="Henüz başvuru yok." />

    <div v-else class="table-responsive">
      <table class="table table-striped table-hover align-middle">
        <thead>
          <tr>
            <th>Ad Soyad</th>
            <th>E-posta</th>
            <th>Üniversite</th>
            <th>Bölüm</th>
            <th>Sınıf</th>
            <th>Başvuru Tarihi</th>
            <th>Bağlantılar</th>
            <th>Durum</th>
            <th>İşlem</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="application in sortedApplications" :key="application.applicationID">
            <td>{{ getFullName(application) }}</td>
            <td>{{ application.personInfo?.email }}</td>
            <td>{{ application.university }}</td>
            <td>{{ application.department }}</td>
            <td>{{ application.classYear }}</td>
            <td>{{ formatDate(application.applicationDate) }}</td>
            <td>
              <div v-for="link in getLinks(application)" :key="link.label">
                <a v-if="isHttpUrl(link.url)" :href="link.url" target="_blank" rel="noopener noreferrer">{{ link.label }}</a>
                <span v-else>{{ link.label }}: {{ link.url }}</span>
              </div>
            </td>
            <td>
              <span class="badge" :class="getStatusInfo(application.status).badge">{{ getStatusInfo(application.status).text }}</span>
            </td>
            <td>
              <!-- Onay/ret butonları sadece "Yeni" başvurularda görünür -->
              <template v-if="application.status === APPLICATION_NEW">
                <button type="button" class="btn btn-sm btn-success me-1" @click="openDecision('approve', application)">Onayla</button>
                <button type="button" class="btn btn-sm btn-danger me-1" @click="openDecision('reject', application)">Reddet</button>
              </template>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Karar penceresi -->
    <BaseModal
      v-model="isDecisionOpen"
      :title="pendingAction?.title ?? ''"
      dialog-class=""
      as-form
      closable
      @submit="submitDecision"
      @shown="notesInput.focus()"
    >
      <p>{{ summaryText }}</p>

      <label for="decisionNotes" class="form-label">Not</label>
      <textarea
        id="decisionNotes"
        ref="notesInput"
        v-model="notes"
        class="form-control"
        rows="3"
        :maxlength="APPLICATION_NOTES_LIMIT"
        :disabled="isSaving"
      ></textarea>
      <div class="form-text">Zorunlu, en fazla {{ APPLICATION_NOTES_LIMIT }} karakter.</div>

      <AlertMessages v-if="decisionErrors.length" :messages="decisionErrors" class="mt-3" />

      <template #footer>
        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal" :disabled="isSaving">Vazgeç</button>
        <button type="submit" class="btn" :class="pendingAction?.buttonClass ?? 'btn-primary'" :disabled="isSaving">
          {{ pendingAction?.buttonText }}
        </button>
      </template>
    </BaseModal>
  </div>
</template>
