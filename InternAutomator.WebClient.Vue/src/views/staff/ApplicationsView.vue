<script setup>
import { ref, reactive, computed, watch, onMounted } from 'vue'
import { getAllApplications, updateApplicationStatus, acceptApplication } from '@/api/applications'
import { ApiError, getErrorMessages } from '@/api/http'
import {
  APPLICATION_NEW,
  APPLICATION_REJECTED,
  APPLICATION_APPROVED,
  APPLICATION_STATUS,
  APPLICATION_NOTES_LIMIT,
} from '@/constants'
import { formatDate, normalizeSearchText } from '@/utils/format'
import { useNotice } from '@/composables/useNotice'
import AlertMessages from '@/components/AlertMessages.vue'
import BaseModal from '@/components/BaseModal.vue'

const ACTIONS = {
  approve: {
    status: APPLICATION_APPROVED,
    title: 'Başvuruyu onayla',
    result: 'onaylanacak',
    buttonText: 'Onayla',
    buttonClass: 'btn-success',
    send: (applicationID, _status, note) => acceptApplication(applicationID, note),
    successMessage: 'Başvuru onaylandı. Stajyere giriş bilgileri e-posta ile gönderildi.',
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
    serverErrorMessage: null,
  },
}

const applications = ref([])
const isLoading = ref(true)
const loadErrors = ref([])
const { notice, pushNotice } = useNotice()

const searchText = ref('')
const statusFilter = ref('all')
const classFilter = ref('all')
const sortOrder = ref('newest')
const pageSize = ref(10)
const currentPage = ref(1)

const availableClasses = computed(() =>
  [...new Set(applications.value.map((a) => Number(a.classYear)))]
    .filter((n) => Number.isInteger(n) && n > 0)
    .sort((a, b) => a - b),
)

const filteredApplications = computed(() => {
  const terms = normalizeSearchText(searchText.value).split(' ').filter(Boolean)

  return applications.value
    .filter((application) => {
      if (statusFilter.value !== 'all' && String(application.status) !== statusFilter.value) return false
      if (classFilter.value !== 'all' && String(application.classYear) !== classFilter.value) return false

      const searchable = normalizeSearchText(
        `${getFullName(application)} ${application.personInfo?.email ?? ''} ${application.university ?? ''} ${application.department ?? ''}`,
      )

      return terms.every((word) => searchable.includes(word))
    })
    .sort((a, b) => {
      const dateA = new Date(a.applicationDate).getTime() || 0
      const dateB = new Date(b.applicationDate).getTime() || 0
      return sortOrder.value === 'oldest' ? dateA - dateB : dateB - dateA
    })
})

const totalPages = computed(() => Math.max(1, Math.ceil(filteredApplications.value.length / pageSize.value)))
const visibleApplications = computed(() => filteredApplications.value.slice((currentPage.value - 1) * pageSize.value, currentPage.value * pageSize.value))
const rangeStart = computed(() => (filteredApplications.value.length ? (currentPage.value - 1) * pageSize.value + 1 : 0))
const rangeEnd = computed(() => Math.min(currentPage.value * pageSize.value, filteredApplications.value.length))
const hasFilters = computed(() => searchText.value.trim() || statusFilter.value !== 'all' || classFilter.value !== 'all')

watch([searchText, statusFilter, classFilter, sortOrder, pageSize], () => {
  currentPage.value = 1
})

function clearFilters() {
  searchText.value = ''
  statusFilter.value = 'all'
  classFilter.value = 'all'
  sortOrder.value = 'newest'
}

function getFullName(application) {
  const person = application.personInfo ?? {}
  return `${person.firstName ?? ''} ${person.lastName ?? ''}`.trim()
}

function getStatusInfo(status) {
  return APPLICATION_STATUS[status] ?? { text: 'Bilinmiyor', badge: 'bg-warning text-dark' }
}

function getLinks(application) {
  return [
    { label: 'LinkedIn', url: application.linkedinURL ?? application.linkedInURL, kind: 'linkedin' },
    { label: 'GitHub', url: application.githubURL, kind: 'github' },
  ].filter((link) => link.url)
}

// API bazı bağlantıları protokolsüz (github.com/kullanici gibi) gönderebilir.
// Sadece geçerli http(s) adreslerini bağlantıya dönüştür; diğerlerini ikonlu fakat pasif göster.
function getLinkHref(value) {
  const raw = String(value ?? '').trim()
  if (!raw) return null
  const candidate = /^https?:\/\//i.test(raw)
    ? raw
    : /^(?:[a-z0-9-]+\.)+[a-z]{2,}(?::\d+)?(?:[/?#][^\s]*)?$/i.test(raw)
      ? `https://${raw}`
      : null

  if (!candidate) return null
  try {
    const url = new URL(candidate)
    return url.protocol === 'https:' || url.protocol === 'http:' ? url.href : null
  } catch {
    return null
  }
}

async function loadApplications() {
  isLoading.value = true
  loadErrors.value = []
  applications.value = []

  try {
    applications.value = await getAllApplications()
    currentPage.value = 1
  } catch (error) {
    loadErrors.value = getErrorMessages(error)
  } finally {
    isLoading.value = false
  }
}

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
    ? `${getFullName(pendingApplication.value)} adlı adayın başvurusu ${pendingAction.value.result}. ${pending.actionKey === 'reject' ? 'Lütfen bir not girin.' : 'İsterseniz bir not ekleyebilirsiniz.'}`
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
  if (pending.actionKey === 'reject' && !text) {
    decisionErrors.value = ['Ret işlemi için not alanı zorunludur.']
    return
  }

  const action = pendingAction.value
  decisionErrors.value = []
  isSaving.value = true

  try {
    await action.send(pending.applicationID, action.status, text)
    isDecisionOpen.value = false
    pushNotice(action.successMessage)
    await loadApplications()
  } catch (error) {
    const useCustomMessage = error instanceof ApiError && error.status >= 500 && action.serverErrorMessage
    decisionErrors.value = useCustomMessage ? [action.serverErrorMessage] : getErrorMessages(error)

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

    <AlertMessages v-if="notice" :type="notice.type" :messages="notice.text" role="status" />
    <AlertMessages v-if="isLoading" type="info" messages="Başvurular yükleniyor..." />
    <AlertMessages v-else-if="loadErrors.length" :messages="loadErrors" />
    <AlertMessages v-else-if="applications.length === 0" type="secondary" messages="Henüz başvuru yok." />

    <section v-else class="applications-card">
      <div class="applications-filters">
        <label class="applications-field applications-search">
          <span>Ara</span>
          <input v-model="searchText" type="search" class="form-control" placeholder="Ad, e-posta, üniversite veya bölüm" autocomplete="off" />
        </label>
        <label class="applications-field">
          <span>Durum</span>
          <select v-model="statusFilter" class="form-select">
            <option value="all">Tüm durumlar</option>
            <option value="0">Yeni</option>
            <option value="1">İptal</option>
            <option value="2">Reddedildi</option>
            <option value="3">Onaylandı</option>
          </select>
        </label>
        <label class="applications-field">
          <span>Sınıf</span>
          <select v-model="classFilter" class="form-select">
            <option value="all">Tüm sınıflar</option>
            <option v-for="year in availableClasses" :key="year" :value="String(year)">{{ year }}. sınıf</option>
          </select>
        </label>
        <label class="applications-field">
          <span>Sıralama</span>
          <select v-model="sortOrder" class="form-select">
            <option value="newest">En yeni önce</option>
            <option value="oldest">En eski önce</option>
          </select>
        </label>
      </div>

      <div class="applications-filter-footer">
        <span>{{ filteredApplications.length }} / {{ applications.length }} başvuru</span>
        <button v-if="hasFilters" type="button" class="btn btn-sm btn-link" @click="clearFilters">Filtreleri temizle</button>
      </div>

      <div v-if="filteredApplications.length === 0" class="text-center text-muted py-5">Filtrelere uygun başvuru bulunamadı.</div>

      <div v-else class="table-responsive">
        <table class="table table-hover align-middle applications-table">
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
            <tr v-for="application in visibleApplications" :key="application.applicationID">
              <td>{{ getFullName(application) }}</td>
              <td>{{ application.personInfo?.email }}</td>
              <td>{{ application.university }}</td>
              <td>{{ application.department }}</td>
              <td>{{ application.classYear }}</td>
              <td>{{ formatDate(application.applicationDate) }}</td>
              <td>
                <div class="application-links">
                  <a
                    v-for="link in getLinks(application)"
                    :key="link.kind"
                    :href="getLinkHref(link.url) || undefined"
                    :target="getLinkHref(link.url) ? '_blank' : undefined"
                    :rel="getLinkHref(link.url) ? 'noopener noreferrer' : undefined"
                    :aria-disabled="!getLinkHref(link.url)"
                    :title="getLinkHref(link.url) ? `${link.label} profilini aç` : 'Geçerli bir bağlantı adresi girilmemiş'"
                    class="application-link-pill"
                    :class="[`is-${link.kind}`, { 'is-disabled': !getLinkHref(link.url) }]"
                  >
                    <span class="application-link-icon" aria-hidden="true">
                      <svg v-if="link.kind === 'linkedin'" width="16" height="16" viewBox="0 0 24 24" fill="currentColor">
                        <path d="M6.94 8.5H3.56V20h3.38V8.5ZM5.25 3A2 2 0 1 0 5.3 7a2 2 0 0 0-.05-4ZM20.44 12.76c0-3.06-1.63-4.48-3.8-4.48a3.29 3.29 0 0 0-2.97 1.63V8.5h-3.38c.04.94 0 11.5 0 11.5h3.38v-6.42c0-.34.02-.68.13-.93.27-.68.9-1.38 1.95-1.38 1.38 0 1.93 1.04 1.93 2.58V20H21v-7.24Z" />
                      </svg>
                      <svg v-else width="16" height="16" viewBox="0 0 24 24" fill="currentColor">
                        <path d="M12 .5C5.65.5.5 5.68.5 12.07c0 5.1 3.3 9.42 7.88 10.95.58.1.79-.25.79-.57v-2.2c-3.2.7-3.88-1.38-3.88-1.38-.52-1.34-1.28-1.7-1.28-1.7-1.04-.72.08-.7.08-.7 1.15.08 1.76 1.19 1.76 1.19 1.02 1.77 2.68 1.26 3.33.96.1-.75.4-1.26.72-1.55-2.56-.3-5.25-1.3-5.25-5.76 0-1.27.45-2.3 1.18-3.12-.12-.3-.51-1.52.11-3.17 0 0 .96-.31 3.14 1.19a10.84 10.84 0 0 1 5.72 0c2.17-1.5 3.13-1.19 3.13-1.19.63 1.65.24 2.87.12 3.17.74.82 1.18 1.85 1.18 3.12 0 4.47-2.69 5.46-5.26 5.75.42.36.79 1.06.79 2.14v3.17c0 .32.2.68.8.57 4.57-1.53 7.86-5.85 7.86-10.95C23.5 5.68 18.35.5 12 .5Z" />
                      </svg>
                    </span>
                    <span>{{ link.label }}</span>
                  </a>
                  <span v-if="!getLinks(application).length" class="text-muted">—</span>
                </div>
              </td>
              <td>
                <span class="badge application-status-pill" :class="getStatusInfo(application.status).badge">
                  {{ getStatusInfo(application.status).text }}
                </span>
              </td>
              <td>
                <template v-if="application.status === APPLICATION_NEW">
                  <button type="button" class="btn btn-sm btn-success me-1" @click="openDecision('approve', application)">Onayla</button>
                  <button type="button" class="btn btn-sm btn-danger me-1" @click="openDecision('reject', application)">Reddet</button>
                </template>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div v-if="filteredApplications.length" class="applications-pagination">
        <span>{{ rangeStart }}–{{ rangeEnd }} / {{ filteredApplications.length }} başvuru</span>
        <div class="d-flex align-items-center gap-2">
          <label class="small text-muted" for="appPageSize">Sayfa başına</label>
          <select id="appPageSize" v-model.number="pageSize" class="form-select form-select-sm" style="width: 75px">
            <option :value="10">10</option>
            <option :value="20">20</option>
            <option :value="50">50</option>
          </select>
          <button type="button" class="btn btn-sm app-page-btn" :disabled="currentPage === 1" @click="currentPage--">‹</button>
          <span>{{ currentPage }} / {{ totalPages }}</span>
          <button type="button" class="btn btn-sm app-page-btn" :disabled="currentPage === totalPages" @click="currentPage++">›</button>
        </div>
      </div>
    </section>

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
      <div class="form-text">
        {{ pending.actionKey === 'reject' ? 'Zorunlu' : 'İsteğe bağlı' }}, en fazla {{ APPLICATION_NOTES_LIMIT }} karakter.
      </div>
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

<style scoped>
.applications-card{background:#fff;border:1px solid #e9e2e2;border-radius:14px;overflow:hidden;box-shadow:0 9px 26px rgba(0,0,0,.075)}
.applications-filters{display:grid;grid-template-columns:minmax(200px,2fr) repeat(3,minmax(120px,1fr));gap:12px;padding:19px 20px 10px}
.applications-field{display:flex;flex-direction:column;gap:6px;min-width:0}
.applications-field span{font-size:.82rem;font-weight:650;color:#333}
.applications-field .form-select,.applications-field .form-control{font-size:.88rem;min-height:42px}
.applications-filter-footer{display:flex;align-items:center;justify-content:space-between;gap:10px;padding:4px 20px 15px;font-size:.84rem;color:#666}
.applications-filter-footer .btn-link{color:#822328;text-decoration:none}
.applications-table{font-size:.86rem;min-width:1070px;margin:0}
.applications-table thead th{background:#faf8f8;padding:14px 12px;white-space:nowrap;border-color:#ebdfdf}
.applications-table td{padding:14px 12px;vertical-align:middle}
.applications-table tbody tr:last-child td{border-bottom:0}
.application-status-pill{display:inline-flex;align-items:center;justify-content:center;min-width:88px;padding:.46rem .72rem;font-size:.76rem}
.application-links{display:flex;flex-wrap:wrap;gap:6px}
.application-link-pill{display:inline-flex;align-items:center;gap:6px;padding:6px 10px;border-radius:999px;border:1px solid #e6d8d8;background:#fbf7f7;color:#4a4a4a;text-decoration:none;font-size:.76rem;font-weight:600}
.application-link-pill:hover{border-color:#822328;color:#822328;background:#f9eeee}
.application-link-pill.is-linkedin{color:#0a66c2;border-color:#cfe0f6;background:#f5f9ff}
.application-link-pill.is-linkedin:hover{color:#084f97;border-color:#0a66c2}
.application-link-pill.is-github{color:#333}
.application-link-pill.is-disabled{opacity:.65;cursor:default}
.application-link-icon{display:inline-flex;align-items:center;justify-content:center;line-height:1}
.applications-pagination{display:flex;justify-content:space-between;align-items:center;gap:15px;flex-wrap:wrap;padding:15px 20px;border-top:1px solid #eee;font-size:.84rem;color:#555}
.app-page-btn{border:1px solid #c8b3b5;color:#822328;background:#fff}
.app-page-btn:hover:not(:disabled){background:#822328;color:#fff;border-color:#822328}
.app-page-btn:disabled{opacity:.55;color:#9b8d8f}
@media(max-width:1100px){.applications-filters{grid-template-columns:repeat(2,minmax(0,1fr))}}
@media(max-width:600px){.applications-filters{grid-template-columns:1fr;padding:15px}.applications-filter-footer{padding:5px 15px 14px}.applications-pagination{padding:15px}}
</style>
