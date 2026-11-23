<script setup>
import { ref, reactive, computed, watch, onMounted, onBeforeUnmount, nextTick } from 'vue'
import { useRouter } from 'vue-router'
import { clearSession } from '@/auth/session'
import { getAllApplications, getApplicationCV, updateApplicationStatus, acceptApplication } from '@/api/applications'
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

const router = useRouter()
const applications = ref([])
const cvLoadingApplicationID = ref(null)
const cvErrors = ref([])
// C# JSON serileştirme varsayılanı hasCv; API hasCV döndürürse de destekle.
function hasCV(application) {
  return (application?.hasCv ?? application?.hasCV) === true
}
const missingHasCV = computed(() =>
  applications.value.some((application) => typeof (application.hasCv ?? application.hasCV) !== 'boolean'),
)
const isLoading = ref(true)
const loadErrors = ref([])
const { notice, pushNotice } = useNotice()

const searchText = ref('')
const statusFilter = ref(String(APPLICATION_NEW))
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
        `${getFullName(application)} ${getApplicantEmail(application)} ${application.university ?? ''} ${application.department ?? ''}`,
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
const hasFilters = computed(() => searchText.value.trim() || statusFilter.value !== String(APPLICATION_NEW) || classFilter.value !== 'all')

watch([searchText, statusFilter, classFilter, sortOrder, pageSize], () => {
  currentPage.value = 1
  closeLinksMenu()
})

function clearFilters() {
  searchText.value = ''
  statusFilter.value = String(APPLICATION_NEW)
  classFilter.value = 'all'
  sortOrder.value = 'newest'
}

// Yeni Application/All yanıtı başvuranın adını ve e-postasını doğrudan döndürür.
// Eski yanıttaki personInfo için geri dönük uyumluluk korunur.
function getFullName(application) {
  const fullName = String(application.applicantFullName ?? '').trim()
  if (fullName) return fullName
  const person = application.personInfo ?? {}
  return `${person.firstName ?? ''} ${person.lastName ?? ''}`.trim()
}

function getApplicantEmail(application) {
  return String(application?.applicantEmail ?? application?.personInfo?.email ?? '').trim()
}

function getStatusInfo(status) {
  return APPLICATION_STATUS[status] ?? { text: 'Bilinmiyor', badge: 'bg-warning text-dark' }
}

// Başvuru iletişim menüsü her satır için ayrı bir API çağrısı yapmaz.
// Teleport ile body içine taşındığı için yatay kaydırmalı tabloda kesilmez.
const openLinksApplicationID = ref(null)
const linksTrigger = ref(null)
const linksMenu = ref(null)
const linksMenuPosition = reactive({ top: 0, left: 0 })

const selectedLinksApplication = computed(() =>
  applications.value.find((application) => application.applicationID === openLinksApplicationID.value) ?? null,
)

function getGmailComposeUrl(application) {
  const email = getApplicantEmail(application)
  if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) return null
  return `https://mail.google.com/mail/?view=cm&fs=1&to=${encodeURIComponent(email)}`
}

function closeLinksMenu(restoreFocus = false) {
  const trigger = linksTrigger.value
  openLinksApplicationID.value = null
  linksTrigger.value = null
  if (restoreFocus) nextTick(() => trigger?.focus())
}

async function toggleLinksMenu(event, application) {
  if (openLinksApplicationID.value === application.applicationID) {
    closeLinksMenu()
    return
  }

  linksTrigger.value = event.currentTarget
  openLinksApplicationID.value = application.applicationID
  await nextTick()

  // Menü genişliği/yüksekliği ölçülerek ekran dışına taşması önlenir.
  const triggerRect = linksTrigger.value?.getBoundingClientRect()
  if (!triggerRect || !linksMenu.value) return

  const menuWidth = linksMenu.value.offsetWidth
  const menuHeight = linksMenu.value.offsetHeight
  const viewportWidth = window.innerWidth
  const viewportHeight = window.innerHeight
  const spaceBelow = viewportHeight - triggerRect.bottom
  const openBelow = spaceBelow >= menuHeight + 8 || spaceBelow >= triggerRect.top

  linksMenuPosition.left = Math.max(
    8,
    Math.min(triggerRect.right - menuWidth, viewportWidth - menuWidth - 8),
  )
  linksMenuPosition.top = Math.max(
    8,
    Math.min(
      openBelow ? triggerRect.bottom + 8 : triggerRect.top - menuHeight - 8,
      viewportHeight - menuHeight - 8,
    ),
  )
}

function handleLinksOutsidePointer(event) {
  if (openLinksApplicationID.value === null) return
  if (linksMenu.value?.contains(event.target) || linksTrigger.value?.contains(event.target)) return
  closeLinksMenu()
}

function handleLinksKeyDown(event) {
  if (event.key === 'Escape' && openLinksApplicationID.value !== null) {
    event.preventDefault()
    closeLinksMenu(true)
  }
}

function handleLinksScroll(event) {
  // Dar ekranlarda menünün kendi içinde kaydırılması, menüyü kapatmaz.
  if (linksMenu.value?.contains(event.target)) return
  if (openLinksApplicationID.value !== null) closeLinksMenu()
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

// Tıklama sırasında (ilk await'ten ÖNCE) boş sekme açarak popup engelini aş.
// Burada özel bir PDF görüntüleyici oluşturmuyoruz; yanıt gelince tarayıcının
// yerleşik PDF görüntüleyicisine doğrudan blob URL'si ile geçiyoruz.
function createCVViewerTab() {
  const tab = window.open('', '_blank')
  if (!tab) return null

  tab.document.title = 'CV yükleniyor...'
  tab.document.body.textContent = 'CV yükleniyor...'
  try { tab.opener = null } catch { /* bazı tarayıcılar izin vermeyebilir */ }
  return tab
}

function displayCVInTab(tab, blob) {
  if (tab.closed) return

  const pdf = blob.type === 'application/pdf'
    ? blob
    : new Blob([blob], { type: 'application/pdf' })
  const blobURL = URL.createObjectURL(pdf)

  try {
    // PDF'yi yeni bir iframe içine değil, sekmenin doğrudan kendisine aç.
    tab.location.replace(blobURL)
    tab.focus()
  } catch (error) {
    URL.revokeObjectURL(blobURL)
    throw error
  }

  // Tarayıcının kendi PDF indirme/yazdırma düğmeleri sekme açıkken
  // blob'a erişmeye devam edebilsin. Sekme kapandığında belleği temizle.
  const timer = window.setInterval(() => {
    if (tab.closed) {
      window.clearInterval(timer)
      URL.revokeObjectURL(blobURL)
    }
  }, 2000)
}

async function viewApplicationCV(application) {
  if (cvLoadingApplicationID.value !== null || !hasCV(application)) return
  cvErrors.value = []
  const applicationID = Number(application.applicationID)
  const tab = createCVViewerTab()
  if (!tab) {
    cvErrors.value = ['Tarayıcı yeni sekmeyi engelledi. Açılır pencerelere izin verip tekrar deneyin.']
    return
  }

  cvLoadingApplicationID.value = applicationID
  try {
    const blob = await getApplicationCV(applicationID)
    if (tab.closed) {
      cvErrors.value = ['Görüntüleme sekmesi kapatıldı. CV için tekrar deneyebilirsiniz.']
      return
    }
    displayCVInTab(tab, blob)
    closeLinksMenu()
  } catch (error) {
    try { if (!tab.closed) tab.close() } catch { /* sekme kapanmış olabilir */ }
    if (error instanceof ApiError && error.status === 401) {
      // Mevcut route guard, oturum varken /login'e geçişi engeller.
      clearSession()
      closeLinksMenu()
      await router.replace('/login')
      return
    }
    if (error instanceof ApiError && error.status === 403) {
      cvErrors.value = ['Bu işlem için yetkiniz yok']
    } else if (error instanceof ApiError && error.status === 404) {
      cvErrors.value = ['CV bulunamadı']
    } else {
      // 400: sunucunun düz metin hata mesajını değiştirmeden göster.
      cvErrors.value = getErrorMessages(error)
    }
  } finally {
    cvLoadingApplicationID.value = null
  }
}

async function loadApplications() {
  closeLinksMenu()
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

onMounted(() => {
  loadApplications()
  document.addEventListener('pointerdown', handleLinksOutsidePointer)
  document.addEventListener('keydown', handleLinksKeyDown)
  // Menü açıkken sayfa/tablo kaydırılırsa eski koordinatta kalmasını önle.
  window.addEventListener('scroll', handleLinksScroll, true)
  window.addEventListener('resize', handleLinksScroll)
})

onBeforeUnmount(() => {
  document.removeEventListener('pointerdown', handleLinksOutsidePointer)
  document.removeEventListener('keydown', handleLinksKeyDown)
  window.removeEventListener('scroll', handleLinksScroll, true)
  window.removeEventListener('resize', handleLinksScroll)
})
</script>

<template>
  <div>
    <h4 class="mb-4">Staj Başvuruları</h4>

    <AlertMessages v-if="notice" :type="notice.type" :messages="notice.text" role="status" />
    <AlertMessages v-if="cvErrors.length" type="danger" :messages="cvErrors" />
    <AlertMessages v-if="!isLoading && missingHasCV" type="warning" messages="API'de hasCV eksik" />
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
              <th>Üniversite</th>
              <th>Bölüm</th>
              <th>Sınıf</th>
              <th>Başvuru Tarihi</th>
              <th>Durum</th>
              <th>Bağlantılar</th>
              <th>İşlem</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="application in visibleApplications" :key="application.applicationID">
              <td>{{ getFullName(application) }}</td>
              <td>{{ application.university }}</td>
              <td>{{ application.department }}</td>
              <td>{{ application.classYear }}</td>
              <td>{{ formatDate(application.applicationDate) }}</td>
              <td>
                <span class="badge application-status-pill" :class="getStatusInfo(application.status).badge">
                  {{ getStatusInfo(application.status).text }}
                </span>
              </td>
              <td class="links-cell">
                <button
                  type="button"
                  class="links-trigger"
                  :class="{ 'is-open': openLinksApplicationID === application.applicationID }"
                  :aria-expanded="openLinksApplicationID === application.applicationID"
                  aria-haspopup="menu"
                  aria-controls="application-links-menu"
                  :aria-label="`${getFullName(application)} için bağlantıları aç`"
                  @click="toggleLinksMenu($event, application)"
                >
                  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
                    <circle cx="6" cy="12" r="2"/><circle cx="12" cy="12" r="2"/><circle cx="18" cy="12" r="2"/>
                  </svg>
                  Bağlantılar
                  <svg class="links-chevron" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="m6 9 6 6 6-6"/></svg>
                </button>
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

    <!-- Yatay kaydırmalı tablo içinde kesilmemesi için menü sayfa gövdesine taşınır. -->
    <Teleport to="body">
      <div
        v-if="selectedLinksApplication"
        id="application-links-menu"
        ref="linksMenu"
        class="app-links-menu"
        role="menu"
        :aria-label="`${getFullName(selectedLinksApplication)} için bağlantılar`"
        :style="{ top: `${linksMenuPosition.top}px`, left: `${linksMenuPosition.left}px` }"
      >
        <div class="app-links-menu-title">Hızlı erişim</div>

        <button
          type="button"
          class="app-links-menu-item is-cv"
          role="menuitem"
          :title="hasCV(selectedLinksApplication) ? 'CV dosyasını yeni sekmede görüntüle' : 'Bu başvuruda CV bulunmuyor'"
          :disabled="!hasCV(selectedLinksApplication) || cvLoadingApplicationID !== null"
          :aria-busy="cvLoadingApplicationID === selectedLinksApplication.applicationID"
          @click="viewApplicationCV(selectedLinksApplication)"
        >
          <span class="app-links-menu-icon" aria-hidden="true">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round">
              <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><path d="M14 2v6h6"/><path d="M8 14h8M8 18h5"/>
            </svg>
          </span>
          <span class="app-links-menu-copy">
            <strong>{{ cvLoadingApplicationID === selectedLinksApplication.applicationID ? 'CV açılıyor...' : 'CV görüntüle' }}</strong>
            <small>{{ cvLoadingApplicationID === selectedLinksApplication.applicationID ? 'PDF yükleniyor' : hasCV(selectedLinksApplication) ? 'Tarayıcının PDF görüntüleyicisinde aç' : 'CV bulunmuyor' }}</small>
          </span>
          <span v-if="cvLoadingApplicationID === selectedLinksApplication.applicationID" class="app-cv-loading" aria-label="Yükleniyor"></span>
        </button>

        <a
          v-if="getLinkHref(selectedLinksApplication.linkedinURL ?? selectedLinksApplication.linkedInURL)"
          class="app-links-menu-item is-linkedin"
          role="menuitem"
          :href="getLinkHref(selectedLinksApplication.linkedinURL ?? selectedLinksApplication.linkedInURL)"
          target="_blank"
          rel="noopener noreferrer"
          @click="closeLinksMenu()"
        >
          <span class="app-links-menu-icon" aria-hidden="true">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="currentColor"><path d="M6.94 8.5H3.56V20h3.38V8.5ZM5.25 3A2 2 0 1 0 5.3 7a2 2 0 0 0-.05-4ZM20.44 12.76c0-3.06-1.63-4.48-3.8-4.48a3.29 3.29 0 0 0-2.97 1.63V8.5h-3.38c.04.94 0 11.5 0 11.5h3.38v-6.42c0-.34.02-.68.13-.93.27-.68.9-1.38 1.95-1.38 1.38 0 1.93 1.04 1.93 2.58V20H21v-7.24Z"/></svg>
          </span>
          <span class="app-links-menu-copy"><strong>LinkedIn</strong><small>Profili görüntüle</small></span>
          <span class="app-links-outbound" aria-hidden="true">↗</span>
        </a>
        <button v-else type="button" class="app-links-menu-item is-linkedin" role="menuitem" disabled>
          <span class="app-links-menu-icon" aria-hidden="true">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="currentColor"><path d="M6.94 8.5H3.56V20h3.38V8.5ZM5.25 3A2 2 0 1 0 5.3 7a2 2 0 0 0-.05-4ZM20.44 12.76c0-3.06-1.63-4.48-3.8-4.48a3.29 3.29 0 0 0-2.97 1.63V8.5h-3.38c.04.94 0 11.5 0 11.5h3.38v-6.42c0-.34.02-.68.13-.93.27-.68.9-1.38 1.95-1.38 1.38 0 1.93 1.04 1.93 2.58V20H21v-7.24Z"/></svg>
          </span>
          <span class="app-links-menu-copy"><strong>LinkedIn</strong><small>Bağlantı bulunmuyor</small></span>
        </button>

        <a
          v-if="getLinkHref(selectedLinksApplication.gitHubURL ?? selectedLinksApplication.githubURL)"
          class="app-links-menu-item is-github"
          role="menuitem"
          :href="getLinkHref(selectedLinksApplication.gitHubURL ?? selectedLinksApplication.githubURL)"
          target="_blank"
          rel="noopener noreferrer"
          @click="closeLinksMenu()"
        >
          <span class="app-links-menu-icon" aria-hidden="true">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="currentColor"><path d="M12 .5C5.65.5.5 5.68.5 12.07c0 5.1 3.3 9.42 7.88 10.95.58.1.79-.25.79-.57v-2.2c-3.2.7-3.88-1.38-3.88-1.38-.52-1.34-1.28-1.7-1.28-1.7-1.04-.72.08-.7.08-.7 1.15.08 1.76 1.19 1.76 1.19 1.02 1.77 2.68 1.26 3.33.96.1-.75.4-1.26.72-1.55-2.56-.3-5.25-1.3-5.25-5.76 0-1.27.45-2.3 1.18-3.12-.12-.3-.51-1.52.11-3.17 0 0 .96-.31 3.14 1.19a10.84 10.84 0 0 1 5.72 0c2.17-1.5 3.13-1.19 3.13-1.19.63 1.65.24 2.87.12 3.17.74.82 1.18 1.85 1.18 3.12 0 4.47-2.69 5.46-5.26 5.75.42.36.79 1.06.79 2.14v3.17c0 .32.2.68.8.57 4.57-1.53 7.86-5.85 7.86-10.95C23.5 5.68 18.35.5 12 .5Z"/></svg>
          </span>
          <span class="app-links-menu-copy"><strong>GitHub</strong><small>Profili görüntüle</small></span>
          <span class="app-links-outbound" aria-hidden="true">↗</span>
        </a>
        <button v-else type="button" class="app-links-menu-item is-github" role="menuitem" disabled>
          <span class="app-links-menu-icon" aria-hidden="true">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="currentColor"><path d="M12 .5C5.65.5.5 5.68.5 12.07c0 5.1 3.3 9.42 7.88 10.95.58.1.79-.25.79-.57v-2.2c-3.2.7-3.88-1.38-3.88-1.38-.52-1.34-1.28-1.7-1.28-1.7-1.04-.72.08-.7.08-.7 1.15.08 1.76 1.19 1.76 1.19 1.02 1.77 2.68 1.26 3.33.96.1-.75.4-1.26.72-1.55-2.56-.3-5.25-1.3-5.25-5.76 0-1.27.45-2.3 1.18-3.12-.12-.3-.51-1.52.11-3.17 0 0 .96-.31 3.14 1.19a10.84 10.84 0 0 1 5.72 0c2.17-1.5 3.13-1.19 3.13-1.19.63 1.65.24 2.87.12 3.17.74.82 1.18 1.85 1.18 3.12 0 4.47-2.69 5.46-5.26 5.75.42.36.79 1.06.79 2.14v3.17c0 .32.2.68.8.57 4.57-1.53 7.86-5.85 7.86-10.95C23.5 5.68 18.35.5 12 .5Z"/></svg>
          </span>
          <span class="app-links-menu-copy"><strong>GitHub</strong><small>Bağlantı bulunmuyor</small></span>
        </button>

        <div class="app-links-menu-divider"></div>

        <a
          v-if="getGmailComposeUrl(selectedLinksApplication)"
          class="app-links-menu-item is-email"
          role="menuitem"
          :href="getGmailComposeUrl(selectedLinksApplication)"
          target="_blank"
          rel="noopener noreferrer"
          @click="closeLinksMenu()"
        >
          <span class="app-links-menu-icon" aria-hidden="true">
            <svg width="22" height="22" viewBox="0 0 24 24" aria-hidden="true">
              <path fill="#4285F4" d="M2.8 19.5h3.7V10l-3.7-2.8v12.3Z"/>
              <path fill="#34A853" d="M17.5 19.5h3.7V7.2L17.5 10v9.5Z"/>
              <path fill="#EA4335" d="m2.8 7.2 9.2 7 9.2-7V4.8l-9.2 7-9.2-7v2.4Z"/>
              <path fill="#C5221F" d="M2.8 4.8V7.2L6.5 10V6.2L4 4.3a.75.75 0 0 0-1.2.5Z"/>
              <path fill="#FBBC04" d="M17.5 6.2V10l3.7-2.8V4.8a.75.75 0 0 0-1.2-.5l-2.5 1.9Z"/>
            </svg>
          </span>
          <span class="app-links-menu-copy"><strong>Gmail ile e-posta gönder</strong><small>Gmail'de yeni ileti oluştur</small></span>
          <span class="app-links-outbound" aria-hidden="true">↗</span>
        </a>
        <button v-else type="button" class="app-links-menu-item is-email" role="menuitem" disabled>
          <span class="app-links-menu-icon" aria-hidden="true">
            <svg width="22" height="22" viewBox="0 0 24 24" aria-hidden="true">
              <path fill="#4285F4" d="M2.8 19.5h3.7V10l-3.7-2.8v12.3Z"/>
              <path fill="#34A853" d="M17.5 19.5h3.7V7.2L17.5 10v9.5Z"/>
              <path fill="#EA4335" d="m2.8 7.2 9.2 7 9.2-7V4.8l-9.2 7-9.2-7v2.4Z"/>
              <path fill="#C5221F" d="M2.8 4.8V7.2L6.5 10V6.2L4 4.3a.75.75 0 0 0-1.2.5Z"/>
              <path fill="#FBBC04" d="M17.5 6.2V10l3.7-2.8V4.8a.75.75 0 0 0-1.2-.5l-2.5 1.9Z"/>
            </svg>
          </span>
          <span class="app-links-menu-copy"><strong>E-posta gönder</strong><small>E-posta bulunmuyor</small></span>
        </button>
      </div>
    </Teleport>

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
.applications-table{font-size:.86rem;min-width:970px;margin:0}
.applications-table thead th{background:#faf8f8;padding:14px 12px;white-space:nowrap;border-color:#ebdfdf}
.applications-table td{padding:14px 12px;vertical-align:middle}
.applications-table tbody tr:last-child td{border-bottom:0}
.application-status-pill{display:inline-flex;align-items:center;justify-content:center;min-width:88px;padding:.46rem .72rem;font-size:.76rem}
/* Satırdaki açılır bağlantı menüsü */
.links-cell{white-space:nowrap}
.links-trigger{display:inline-flex;align-items:center;gap:8px;padding:7px 10px;border:1px solid #d9cbcd;border-radius:8px;background:#fff;color:#822328;font-size:.79rem;font-weight:650;white-space:nowrap;transition:background .15s,border-color .15s}
.links-trigger:hover,.links-trigger.is-open{background:#f9eeee;border-color:#822328}
.links-trigger:focus-visible{outline:3px solid rgba(130,35,40,.23);outline-offset:2px}
.links-chevron{transition:transform .16s}
.links-trigger.is-open .links-chevron{transform:rotate(180deg)}
.app-links-menu{position:fixed;z-index:1050;width:256px;max-width:calc(100vw - 16px);max-height:calc(100vh - 16px);overflow-y:auto;padding:8px;background:#fff;border:1px solid #eadfe0;border-radius:13px;box-shadow:0 16px 44px rgba(29,19,21,.17)}
.app-links-menu-title{padding:7px 11px 9px;color:#807477;font-size:.7rem;font-weight:750;letter-spacing:.07em;text-transform:uppercase}
.app-links-menu-item{display:flex;align-items:center;gap:11px;width:100%;min-height:54px;padding:9px 10px;border:0;border-radius:9px;background:transparent;color:#262626;text-decoration:none;text-align:left;cursor:pointer;transition:background .15s}
.app-links-menu-item:hover:not(:disabled){background:#f6f5f5;color:#262626}
.app-links-menu-item:focus-visible{outline:2px solid #822328;outline-offset:-2px}
.app-links-menu-item:disabled{opacity:.50;cursor:not-allowed}
.app-links-menu-icon{width:34px;height:34px;flex:0 0 34px;display:flex;align-items:center;justify-content:center;border-radius:9px}
.app-links-menu-item.is-cv .app-links-menu-icon{color:#c42b36;background:#fff0f0}
.app-links-menu-item.is-linkedin .app-links-menu-icon{color:#0a66c2;background:#eaf3ff}
.app-links-menu-item.is-github .app-links-menu-icon{color:#24292f;background:#f0f1f3}
.app-links-menu-item.is-email .app-links-menu-icon{color:#d93025;background:#fff0ed}
.app-links-menu-copy{min-width:0;flex:1;display:flex;flex-direction:column;gap:1px}
.app-links-menu-copy strong{font-size:.83rem;font-weight:700}
.app-links-menu-copy small{color:#767074;font-size:.71rem}
.app-cv-loading{width:16px;height:16px;flex-shrink:0;border:2px solid #f0d0d4;border-top-color:#a4222d;border-radius:50%;animation:cv-spin .8s linear infinite}
@keyframes cv-spin{to{transform:rotate(360deg)}}
.app-links-coming-soon{padding:3px 5px;border-radius:5px;background:#f0ecec;color:#8a7c7c;font-size:.63rem;font-weight:700}
.app-links-outbound{font-size:1rem;color:#7c7175}
.app-links-menu-divider{height:1px;margin:6px 7px;background:#eee8e8}
.applications-pagination{display:flex;justify-content:space-between;align-items:center;gap:15px;flex-wrap:wrap;padding:15px 20px;border-top:1px solid #eee;font-size:.84rem;color:#555}
.app-page-btn{border:1px solid #c8b3b5;color:#822328;background:#fff}
.app-page-btn:hover:not(:disabled){background:#822328;color:#fff;border-color:#822328}
.app-page-btn:disabled{opacity:.55;color:#9b8d8f}
@media(max-width:1100px){.applications-filters{grid-template-columns:repeat(2,minmax(0,1fr))}}
@media(max-width:600px){.applications-filters{grid-template-columns:1fr;padding:15px}.applications-filter-footer{padding:5px 15px 14px}.applications-pagination{padding:15px}}
</style>
