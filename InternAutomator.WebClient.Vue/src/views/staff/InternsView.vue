<script setup>
import { ref, computed, onMounted } from 'vue'
import { getAllInterns, separateIntern } from '@/api/interns'
import { ApiError, getErrorMessages } from '@/api/http'
import { INTERN_ACTIVE } from '@/constants'
import { formatDate, getInitials, normalizeSearchText } from '@/utils/format'
import { useNotice } from '@/composables/useNotice'
import AlertMessages from '@/components/AlertMessages.vue'
import BaseModal from '@/components/BaseModal.vue'
import InternStatusBadge from '@/components/InternStatusBadge.vue'
import '@/assets/css/interns.css'

const getFullName = (intern) => `${intern.firstName} ${intern.lastName}`.trim()
const isActive = (intern) => intern.status === INTERN_ACTIVE

// ---------- Liste ve arama ----------
const interns = ref([])
const isLoading = ref(true)
const loadErrors = ref([])
const searchText = ref('')
const { notice, pushNotice } = useNotice()

const hasInterns = computed(() => interns.value.length > 0)
const searchWords = computed(() => normalizeSearchText(searchText.value).split(' ').filter(Boolean))

// Yazılan her kelime ad soyadın içinde geçmeli; sıra önemli değil ("oruc ibrahim" de bulur)
const filteredInterns = computed(() =>
  interns.value.filter((intern) => {
    const fullName = normalizeSearchText(getFullName(intern))
    return searchWords.value.every((word) => fullName.includes(word))
  }),
)

const resultCountText = computed(() =>
  searchWords.value.length > 0
    ? `${filteredInterns.value.length} / ${interns.value.length} stajyer`
    : `${interns.value.length} stajyer`,
)

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
    <div class="d-flex flex-wrap align-items-center justify-content-between gap-3 mb-4">
      <h4 class="mb-0">Stajyerler</h4>

      <!-- Ad-soyad arama: yazdıkça listeyi süzer -->
      <div class="d-flex align-items-center gap-3">
        <span v-if="hasInterns" class="small text-muted">{{ resultCountText }}</span>
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

    <div class="intern-grid">
      <div
        v-for="intern in filteredInterns"
        :key="intern.internID"
        class="card intern-card"
        :class="isActive(intern) ? '' : 'intern-card-left opacity-75'"
      >
        <!-- Üst: avatar + ad soyad + durum rozeti -->
        <div class="d-flex align-items-center gap-3 p-3 pb-2">
          <div class="avatar-circle rounded-circle bg-secondary-subtle text-secondary-emphasis d-flex align-items-center justify-content-center fw-semibold">
            {{ getInitials(intern.firstName, intern.lastName) }}
          </div>
          <div>
            <h5 class="mb-1 text-break">{{ getFullName(intern) }}</h5>
            <InternStatusBadge :status="intern.status" />
          </div>
        </div>

        <!-- Orta: başlıklı bölümler -->
        <div class="px-3 flex-grow-1">
          <div class="mb-3">
            <div class="intern-section-title">Bilgiler</div>
            <div class="small text-break"><span class="text-muted">E-posta: </span><span>{{ intern.email || '-' }}</span></div>
            <div class="small text-break"><span class="text-muted">Telefon: </span><span>{{ intern.phone || '-' }}</span></div>
          </div>
          <div class="mb-3">
            <div class="intern-section-title">Staj</div>
            <div class="small text-break"><span class="text-muted">Başlangıç: </span><span>{{ formatDate(intern.startDate, 'Belirlenmedi') }}</span></div>
            <div class="small text-break"><span class="text-muted">Bitiş: </span><span>{{ formatDate(intern.endDate, 'Belirlenmedi') }}</span></div>
          </div>
          <div class="mb-3">
            <div class="intern-section-title">Projeler</div>
            <div class="small text-break"><span class="text-muted">Aktif proje sayısı: </span><span>{{ intern.activeProjectCount ?? 0 }}</span></div>
          </div>
        </div>

        <!-- Alt: işlemler -->
        <div class="d-flex gap-2 px-3 pb-3 pt-1">
          <RouterLink :to="`/staff/interns/${intern.internID}`" class="btn btn-sm btn-outline-primary flex-fill">Detaylar</RouterLink>
          <button v-if="isActive(intern)" type="button" class="btn btn-sm btn-outline-danger flex-fill" @click="openSeparate(intern)">Staj Bitir</button>
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
