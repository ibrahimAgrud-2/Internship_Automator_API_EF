<script setup>
import { ref, computed, onMounted } from 'vue'
import { getAllInterns, separateIntern } from '@/api/interns'
import { ApiError, getErrorMessages } from '@/api/http'
import { INTERN_ACTIVE } from '@/constants'
import { formatDate, getInitials, normalizeSearchText, getGmailComposeUrl } from '@/utils/format'
import { useNotice } from '@/composables/useNotice'
import AlertMessages from '@/components/AlertMessages.vue'
import BaseModal from '@/components/BaseModal.vue'
import InternStatusBadge from '@/components/InternStatusBadge.vue'
import '@/assets/css/interns.css'

const getFullName = (intern) => `${intern.firstName} ${intern.lastName}`.trim()
const isActive = (intern) => intern.isActive === INTERN_ACTIVE

// ---------- Liste ve arama ----------
const interns = ref([])
const isLoading = ref(true)
const loadErrors = ref([])
const searchText = ref('')
const statusFilter = ref('active')
const { notice, pushNotice } = useNotice()

const hasInterns = computed(() => interns.value.length > 0)
const searchWords = computed(() => normalizeSearchText(searchText.value).split(' ').filter(Boolean))

// Yazılan her kelime ad soyadın içinde geçmeli; sıra önemli değil ("oruc ibrahim" de bulur)
const filteredInterns = computed(() =>
  interns.value.filter((intern) => {
    const fullName = normalizeSearchText(getFullName(intern) + ' ' + (intern.email ?? ''))
    const matchesStatus = statusFilter.value === 'all' || (statusFilter.value === 'active' ? isActive(intern) : !isActive(intern))
    return matchesStatus && searchWords.value.every((word) => fullName.includes(word))
  }),
)

const resultCountText = computed(() => `${filteredInterns.value.length} / ${interns.value.length} stajyer`)

async function loadInterns() {
  isLoading.value = true
  loadErrors.value = []
  interns.value = []

  try {
    // Sıralama API'dan geldiği gibi gösterilir
    interns.value = await getAllInterns()
  } catch (error) {
    loadErrors.value = getErrorMessages(error)
  } finally {
    isLoading.value = false
  }
}

// ---------- Staj bitirme penceresi ----------
// Büyük/küçük harf ve fazla boşluk farkı önemsenmez
const normalizeName = (text) => (text ?? '').trim().replace(/\s+/g, ' ').toLocaleLowerCase('tr-TR')

const isSeparateOpen = ref(false)
const pendingIntern = ref(null)
const confirmText = ref('')
const isSaving = ref(false)
const separateErrors = ref([])
const confirmInput = ref(null)

const isConfirmationValid = computed(
  () => pendingIntern.value !== null && normalizeName(confirmText.value) === normalizeName(getFullName(pendingIntern.value)),
)

function openSeparate(intern) {
  if (!isActive(intern)) return

  pendingIntern.value = intern
  confirmText.value = ''
  separateErrors.value = []
  isSaving.value = false
  isSeparateOpen.value = true
}

async function submitSeparate() {
  if (isSaving.value || !isConfirmationValid.value) return // Enter ile gönderimi de engeller

  const intern = pendingIntern.value

  separateErrors.value = []
  isSaving.value = true

  try {
    await separateIntern(intern.internID)

    isSeparateOpen.value = false
    pushNotice(`${getFullName(intern)} adlı stajyerin stajı sona erdi.`)
    await loadInterns() // ekranda her zaman sunucudaki gerçek hal görünsün
  } catch (error) {
    separateErrors.value = getErrorMessages(error)

    // İstek sunucuya ulaştıysa liste eskimiş olabilir (örn. stajyer zaten ayrılmış)
    if (error instanceof ApiError && error.status > 0) {
      loadInterns()
    }
  } finally {
    isSaving.value = false
  }
}

onMounted(loadInterns)
</script>

<template>
  <div>
    <div class="page-head">
      <h4 class="mb-0">Stajyerler</h4>

      <!-- Ad-soyad arama: yazdıkça listeyi süzer -->
      <div class="d-flex flex-wrap align-items-center gap-2">
        <span v-if="hasInterns" class="small text-muted">{{ resultCountText }}</span>
        <select v-model="statusFilter" class="form-select intern-status-filter" aria-label="Stajyer durumu">
          <option value="active">Aktif stajyerler</option>
          <option value="inactive">Ayrılanlar</option>
          <option value="all">Tümü</option>
        </select>
        <div class="input-group intern-search">
          <input
            v-model="searchText"
            type="search"
            class="form-control"
            placeholder="Ad Soyad ara..."
            aria-label="Stajyer ara"
            autocomplete="off"
            :disabled="!hasInterns"
            @keydown.esc="searchText = ''"
          />
          <button type="button" class="btn btn-outline-secondary" :disabled="searchText === ''" @click="searchText = ''">Temizle</button>
        </div>
      </div>
    </div>

    <!-- İşlem sonucu bildirimi (staj bitirildi...) -->
    <AlertMessages v-if="notice" :type="notice.type" :messages="notice.text" role="status" />

    <!-- Yükleniyor / hata / boş liste / arama sonucu yok: aynı anda yalnızca biri çizilir -->
    <AlertMessages v-if="isLoading" type="info" messages="Stajyerler yükleniyor..." />
    <AlertMessages v-else-if="loadErrors.length" :messages="loadErrors" />
    <AlertMessages v-else-if="!hasInterns" type="secondary" messages="Henüz stajyer yok." />
    <AlertMessages v-else-if="filteredInterns.length === 0" type="secondary" messages="Aramanızla eşleşen stajyer bulunamadı." />

    <div v-if="!isLoading && !loadErrors.length && filteredInterns.length" class="intern-grid">
      <div
        v-for="intern in filteredInterns"
        :key="intern.internID"
        class="card intern-card"
        :class="isActive(intern) ? '' : 'intern-card-left'"
      >
        <div class="intern-card-body">
          <div class="intern-card-identity">
            <span class="intern-avatar">{{ getInitials(intern.firstName, intern.lastName) }}</span>
            <InternStatusBadge :is-active="intern.isActive" />
          </div>
          <h5 class="intern-card-name" :title="getFullName(intern)">{{ getFullName(intern) }}</h5>
          <div class="intern-card-email">
            <span class="intern-card-email-text" :title="intern.email">{{ intern.email || 'E-posta yok' }}</span>
            <!-- Başvurular sayfasındaki gibi: Gmail'de bu adrese yeni ileti açar -->
            <a
              v-if="getGmailComposeUrl(intern.email)"
              class="intern-mail-button"
              :href="getGmailComposeUrl(intern.email)"
              target="_blank"
              rel="noopener noreferrer"
              :title="`${intern.email} adresine Gmail ile e-posta gönder`"
              :aria-label="`${getFullName(intern)} adlı stajyere Gmail ile e-posta gönder`"
            >
              <svg width="18" height="18" viewBox="0 0 24 24" aria-hidden="true">
                <path fill="#4285F4" d="M2.8 19.5h3.7V10l-3.7-2.8v12.3Z"/>
                <path fill="#34A853" d="M17.5 19.5h3.7V7.2L17.5 10v9.5Z"/>
                <path fill="#EA4335" d="m2.8 7.2 9.2 7 9.2-7V4.8l-9.2 7-9.2-7v2.4Z"/>
                <path fill="#C5221F" d="M2.8 4.8V7.2L6.5 10V6.2L4 4.3a.75.75 0 0 0-1.2.5Z"/>
                <path fill="#FBBC04" d="M17.5 6.2V10l3.7-2.8V4.8a.75.75 0 0 0-1.2-.5l-2.5 1.9Z"/>
              </svg>
            </a>
          </div>
          <div class="intern-card-meta">
            <div class="intern-card-date"><span>Başlangıç</span><strong>{{ formatDate(intern.startDate, 'Belirlenmedi') }}</strong></div>
            <div class="intern-card-date"><span>Bitiş</span><strong>{{ formatDate(intern.endDate, 'Belirlenmedi') }}</strong></div>
          </div>
        </div>
        <div class="intern-card-actions">
          <RouterLink :to="`/staff/interns/${intern.internID}`" class="btn btn-sm btn-outline-primary">Detaylar</RouterLink>
          <button v-if="isActive(intern)" type="button" class="btn btn-sm btn-outline-danger" @click="openSeparate(intern)">Staj Bitir</button>
        </div>
      </div>
    </div>

    <!-- Staj bitirme onay penceresi -->
    <BaseModal
      v-model="isSeparateOpen"
      title="Stajı Bitir"
      dialog-class=""
      header-class="bg-danger text-white"
      as-form
      @submit="submitSeparate"
      @shown="confirmInput.focus()"
    >
      <p class="fw-semibold">{{ pendingIntern ? `${getFullName(pendingIntern)} adlı stajyerin stajı bitirilecek.` : '' }}</p>
      <ul>
        <li>Stajyer <strong>ayrıldı</strong> durumuna geçer.</li>
        <li>Stajyere yeni proje veya görev atanamaz.</li>
        <li>Geçmiş proje ve görev kayıtları korunur.</li>
        <li><strong>Bu işlem geri alınamaz.</strong></li>
      </ul>

      <label for="separateConfirm" class="form-label">
        Onaylamak için stajyerin adını soyadını yazın:
        <strong>{{ pendingIntern ? getFullName(pendingIntern) : '' }}</strong>
      </label>
      <input id="separateConfirm" ref="confirmInput" v-model="confirmText" type="text" class="form-control" :disabled="isSaving" />

      <AlertMessages v-if="separateErrors.length" :messages="separateErrors" class="mt-3" />

      <template #footer>
        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal" :disabled="isSaving">Vazgeç</button>
        <button type="submit" class="btn btn-danger" :disabled="isSaving || !isConfirmationValid">Stajı Bitir</button>
      </template>
    </BaseModal>
  </div>
</template>
