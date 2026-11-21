<script setup>
import { computed, onMounted, reactive, ref } from 'vue'
import { getAllUsers, createUser } from '@/api/users'
import { getErrorMessages } from '@/api/http'
import { normalizeSearchText } from '@/utils/format'
import AlertMessages from '@/components/AlertMessages.vue'
import BaseModal from '@/components/BaseModal.vue'
import { useNotice } from '@/composables/useNotice'

// Kullanıcı listeleme ve yeni personel hesabı oluşturma.
const users = ref([])
const isLoading = ref(true)
const loadErrors = ref([])
const searchText = ref('')
const statusFilter = ref('all')
const { notice, pushNotice } = useNotice()

// Kullanıcı ekleme formu: API DTO ile aynı alanları gönderir.
const isCreateOpen = ref(false)
const isCreating = ref(false)
const createErrors = ref([])
const firstNameInput = ref(null)
// Kaydetmeden önce oluşturulacak hesabın kullanıcı adı tekrar yazılarak onaylanır.
const confirmUserName = ref('')
const isCreateConfirmed = computed(() => {
  const userName = newUser.userName.trim()
  return userName !== '' && confirmUserName.value.trim() === userName
})

function emptyUserForm() {
  return {
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    address: '',
    imagePath: '',
    userName: '',
    isActive: true,
  }
}

const newUser = reactive(emptyUserForm())

function openCreate() {
  Object.assign(newUser, emptyUserForm())
  confirmUserName.value = ''
  createErrors.value = []
  isCreateOpen.value = true
}

function closeCreate() {
  if (isCreating.value) return
  isCreateOpen.value = false
}

async function submitCreate() {
  if (isCreating.value) return

  const data = {
    firstName: newUser.firstName.trim(),
    lastName: newUser.lastName.trim(),
    userName: newUser.userName.trim(),
    email: newUser.email.trim(),
    phone: newUser.phone.trim(),
    address: newUser.address.trim(),
    imagePath: newUser.imagePath.trim(),
  }

  const errors = []
  if (!data.firstName) errors.push('Ad alanı zorunludur.')
  if (!data.lastName) errors.push('Soyad alanı zorunludur.')
  if (!data.userName) errors.push('Kullanıcı adı zorunludur.')
  if (!isCreateConfirmed.value) errors.push('Onay için kullanıcı adını aynen yazın.')
  if (data.email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(data.email)) {
    errors.push('Geçerli bir e-posta adresi girin.')
  }
  if (errors.length) {
    createErrors.value = errors
    return
  }

  createErrors.value = []
  isCreating.value = true

  try {
    await createUser({
      personInfo: {
        firstName: data.firstName,
        lastName: data.lastName,
        email: data.email,
        phone: data.phone,
        address: data.address,
        imagePath: data.imagePath,
      },
      userName: data.userName,
      isActive: Boolean(newUser.isActive),
    })
    isCreateOpen.value = false
    pushNotice('Kullanıcı eklendi.')
    await loadUsers()
  } catch (error) {
    createErrors.value = getErrorMessages(error)
  } finally {
    isCreating.value = false
  }
}

const filteredUsers = computed(() => {
  const words = normalizeSearchText(searchText.value).split(' ').filter(Boolean)

  return users.value.filter((user) => {
    if (statusFilter.value === 'active' && user.isActive !== true) return false
    if (statusFilter.value === 'inactive' && user.isActive === true) return false

    const searchable = normalizeSearchText(`${user.fullName ?? ''} ${user.userName ?? ''}`)
    return words.every((word) => searchable.includes(word))
  })
})

const hasFilter = computed(() => searchText.value.trim() !== '' || statusFilter.value !== 'all')

function clearFilters() {
  searchText.value = ''
  statusFilter.value = 'all'
}

async function loadUsers() {
  isLoading.value = true
  loadErrors.value = []

  try {
    const result = await getAllUsers()
    if (!Array.isArray(result)) {
      users.value = []
      loadErrors.value = ['Kullanıcı listesi beklenen biçimde alınamadı.']
      return
    }
    // Sıralama sunucudan geldiği gibi korunur.
    users.value = result
  } catch (error) {
    users.value = []
    loadErrors.value = getErrorMessages(error)
  } finally {
    isLoading.value = false
  }
}

onMounted(loadUsers)
</script>

<template>
  <section class="users-page">
    <div class="d-flex justify-content-end flex-wrap gap-2 mb-3">
      <button
        type="button"
        class="btn btn-primary d-inline-flex align-items-center gap-2"
        @click="openCreate"
      >
        <svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
          <path d="M12 5v14M5 12h14" />
        </svg>
        Kullanıcı Ekle
      </button>
      <button
        type="button"
        class="btn btn-outline-primary d-inline-flex align-items-center gap-2"
        :disabled="isLoading"
        @click="loadUsers"
      >
        <svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
          <path d="M20 11a8 8 0 1 0-2.1 5.7" />
          <path d="M20 4v7h-7" />
        </svg>
        {{ isLoading ? 'Yükleniyor...' : 'Yenile' }}
      </button>
    </div>

    <AlertMessages v-if="notice" :type="notice.type" :messages="notice.text" />

    <div class="card users-list-card">
      <div class="card-body p-3 p-md-4">
        <div class="d-flex flex-wrap align-items-end gap-3 mb-3">
          <div class="users-search-field flex-grow-1">
            <label for="userSearch" class="form-label fw-semibold small">Kullanıcı ara</label>
            <div class="input-group">
              <span class="input-group-text bg-white text-secondary" aria-hidden="true">
                <svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <circle cx="11" cy="11" r="7" />
                  <path d="m20 20-4-4" />
                </svg>
              </span>
              <input
                id="userSearch"
                v-model="searchText"
                type="search"
                class="form-control"
                placeholder="Ad soyad veya kullanıcı adı..."
                autocomplete="off"
                :disabled="isLoading || loadErrors.length > 0"
              />
            </div>
          </div>
          <div class="users-filter-field">
            <label for="userStatusFilter" class="form-label fw-semibold small">Hesap durumu</label>
            <select
              id="userStatusFilter"
              v-model="statusFilter"
              class="form-select"
              :disabled="isLoading || loadErrors.length > 0"
            >
              <option value="all">Tüm kullanıcılar</option>
              <option value="active">Aktif kullanıcılar</option>
              <option value="inactive">Pasif kullanıcılar</option>
            </select>
          </div>
        </div>

        <div class="d-flex justify-content-between align-items-center gap-2 mb-3">
          <div class="small text-secondary" aria-live="polite">
            <template v-if="!isLoading && !loadErrors.length">
              {{ filteredUsers.length }} / {{ users.length }} kullanıcı gösteriliyor
            </template>
          </div>
          <button v-if="hasFilter" type="button" class="btn btn-link btn-sm text-decoration-none p-0" @click="clearFilters">
            Filtreleri temizle
          </button>
        </div>

        <AlertMessages v-if="loadErrors.length" :messages="loadErrors" />
        <AlertMessages v-else-if="isLoading" type="info" messages="Kullanıcılar yükleniyor..." role="status" />
        <div v-else-if="!users.length" class="users-empty text-center py-5">
          <div class="fw-semibold mb-1">Henüz kullanıcı bulunmuyor</div>
        </div>
        <div v-else-if="!filteredUsers.length" class="users-empty text-center py-5">
          <div class="fw-semibold mb-1">Eşleşen kullanıcı bulunamadı</div>
          <p class="text-secondary small mb-2">Arama veya durum filtresini değiştirebilirsiniz.</p>
          <button type="button" class="btn btn-outline-secondary btn-sm" @click="clearFilters">Filtreleri temizle</button>
        </div>
        <div v-else class="table-responsive">
          <table class="table table-hover align-middle users-table mb-0">
            <thead>
              <tr>
                <th scope="col">Ad Soyad</th>
                <th scope="col">Kullanıcı Adı</th>
                <th scope="col">Durum</th>
                <th scope="col" class="text-end">İşlemler</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="user in filteredUsers" :key="user.userID">
                <td>
                  <div class="d-flex align-items-center gap-3">
                    <span class="users-avatar" aria-hidden="true">{{ (user.fullName || '?').trim().slice(0, 1).toLocaleUpperCase('tr-TR') }}</span>
                    <div class="fw-semibold text-break">{{ user.fullName || 'Ad belirtilmemiş' }}</div>
                  </div>
                </td>
                <td class="text-break">{{ user.userName || '—' }}</td>
                <td>
                  <span class="badge rounded-pill" :class="user.isActive === true ? 'text-bg-success' : 'text-bg-secondary'">
                    <span class="users-status-dot" aria-hidden="true"></span>
                    {{ user.isActive === true ? 'Aktif' : 'Pasif' }}
                  </span>
                </td>
                <td class="text-end">
                  <div class="d-inline-flex gap-1" role="group" :aria-label="`${user.fullName || user.userName || 'Kullanıcı'} için işlemler`">
                    <button
                      type="button"
                      class="btn btn-sm btn-outline-secondary users-action"
                      disabled
                      :aria-label="`${user.fullName || user.userName || 'Kullanıcı'} düzenle (yakında)`"
                      title="Düzenleme işlemi henüz aktif değil"
                    >
                      <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
                        <path d="M12 20h9" />
                        <path d="M16.5 3.5a2.12 2.12 0 0 1 3 3L9 17l-4 1 1-4z" />
                      </svg>
                    </button>
                    <button
                      type="button"
                      class="btn btn-sm btn-outline-danger users-action"
                      disabled
                      :aria-label="`${user.fullName || user.userName || 'Kullanıcı'} sil (yakında)`"
                      title="Silme işlemi henüz aktif değil"
                    >
                      <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
                        <polyline points="3 6 5 6 21 6" />
                        <path d="M19 6l-1 14H6L5 6m3 0V4h8v2" />
                        <path d="M10 10v7M14 10v7" />
                      </svg>
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <BaseModal
      v-model="isCreateOpen"
      title="Kullanıcı Ekle"
      dialog-class="modal-dialog-centered modal-lg"
      as-form
      @submit="submitCreate"
      @shown="firstNameInput?.focus()"
    >
      <AlertMessages v-if="createErrors.length" :messages="createErrors" />

      <div class="row g-3">
        <div class="col-md-6">
          <label for="newUserFirstName" class="form-label">Ad <span class="text-danger">*</span></label>
          <input id="newUserFirstName" ref="firstNameInput" v-model="newUser.firstName" type="text" class="form-control" autocomplete="given-name" :disabled="isCreating" />
        </div>
        <div class="col-md-6">
          <label for="newUserLastName" class="form-label">Soyad <span class="text-danger">*</span></label>
          <input id="newUserLastName" v-model="newUser.lastName" type="text" class="form-control" autocomplete="family-name" :disabled="isCreating" />
        </div>
        <div class="col-md-6">
          <label for="newUserUserName" class="form-label">Kullanıcı Adı <span class="text-danger">*</span></label>
          <input id="newUserUserName" v-model="newUser.userName" type="text" class="form-control" autocapitalize="none" spellcheck="false" :disabled="isCreating" />
        </div>
        <div class="col-md-6">
          <label for="newUserEmail" class="form-label">E-posta</label>
          <input id="newUserEmail" v-model="newUser.email" type="email" class="form-control" autocomplete="email" :disabled="isCreating" />
        </div>
        <div class="col-md-6">
          <label for="newUserPhone" class="form-label">Telefon</label>
          <input id="newUserPhone" v-model="newUser.phone" type="tel" class="form-control" autocomplete="tel" :disabled="isCreating" />
        </div>
        <div class="col-md-6">
          <label for="newUserImage" class="form-label">Profil görseli yolu</label>
          <input id="newUserImage" v-model="newUser.imagePath" type="text" class="form-control" :disabled="isCreating" />
        </div>
        <div class="col-12">
          <label for="newUserAddress" class="form-label">Adres</label>
          <textarea id="newUserAddress" v-model="newUser.address" rows="2" class="form-control" autocomplete="street-address" :disabled="isCreating"></textarea>
        </div>
        <div class="col-12">
          <div class="form-check form-switch">
            <input id="newUserActive" v-model="newUser.isActive" class="form-check-input" type="checkbox" role="switch" :disabled="isCreating" />
            <label for="newUserActive" class="form-check-label">Aktif hesap</label>
          </div>
        </div>
        <div class="col-12">
          <div class="border-top pt-3 mt-1">
            <label for="confirmNewUserName" class="form-label fw-semibold">Kullanıcı eklemeyi onayla <span class="text-danger">*</span></label>
            <div class="form-text mb-2">
              Onaylamak için <strong>{{ newUser.userName.trim() || 'kullanıcı adını' }}</strong> yazın.
            </div>
            <input
              id="confirmNewUserName"
              v-model="confirmUserName"
              type="text"
              class="form-control"
              :class="{ 'is-valid': isCreateConfirmed }"
              autocomplete="off"
              autocapitalize="none"
              spellcheck="false"
              placeholder="Kullanıcı adını tekrar yazın"
              :disabled="isCreating"
            />
          </div>
        </div>
      </div>

      <template #footer>
        <button type="button" class="btn btn-outline-secondary" :disabled="isCreating" @click="closeCreate">Vazgeç</button>
        <button type="submit" class="btn btn-primary" :disabled="isCreating || !isCreateConfirmed">
          <span v-if="isCreating" class="spinner-border spinner-border-sm me-2" aria-hidden="true"></span>
          {{ isCreating ? 'Kaydediliyor...' : 'Kaydet' }}
        </button>
      </template>
    </BaseModal>
  </section>
</template>

<style scoped>
.users-page {
  max-width: 1200px;
  margin: 0 auto;
}

.users-list-card {
  border: 1px solid #e3e8ef;
  border-radius: 14px;
  box-shadow: 0 2px 10px rgb(28 45 70 / 4%);
}

.users-search-field {
  min-width: min(100%, 240px);
}

.users-filter-field {
  min-width: 190px;
}

.users-table {
  --bs-table-hover-bg: #f7f9fd;
}

.users-table thead th {
  color: #65748b;
  font-size: 0.8rem;
  font-weight: 600;
  border-bottom-color: #e3e8ef;
  white-space: nowrap;
  padding: 0.9rem 0.8rem;
}

.users-table tbody td {
  padding: 0.85rem 0.8rem;
  border-color: #edf1f5;
}

.users-avatar {
  width: 38px;
  height: 38px;
  display: inline-flex;
  flex: 0 0 38px;
  align-items: center;
  justify-content: center;
  background: #e7eef8;
  color: #385b86;
  border-radius: 12px;
  font-weight: 700;
}

.users-action {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 34px;
  height: 34px;
  padding: 0;
}

.users-action:disabled {
  opacity: 0.45;
}

.users-status-dot {
  display: inline-block;
  width: 6px;
  height: 6px;
  margin-right: 0.25rem;
  border-radius: 50%;
  background: currentColor;
  vertical-align: middle;
}

.users-empty {
  border: 1px dashed #d9e1eb;
  border-radius: 12px;
  background: #fafbfd;
}

@media (max-width: 575.98px) {
  .users-filter-field { flex: 1 1 100%; }
  .users-table { min-width: 620px; }
}
</style>
