
<script setup>
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { changePassword } from '@/api/auth'
import { getErrorMessages } from '@/api/http'
import {
  getSession,
  saveSession,
  clearSession,
  ROLE_STAFF
} from '@/auth/session'
import AlertMessages from '@/components/AlertMessages.vue'

const router = useRouter()
const session = getSession()

const isRequired = ref(session?.mustChangePassword === true)
const isSubmitting = ref(false)
const errors = ref([])
const visible = reactive({ oldPassword: false, newPassword: false, confirmPassword: false })

const form = reactive({
  oldPassword: '',
  newPassword: '',
  confirmPassword: ''
})

function homePath(role) {
  return role === ROLE_STAFF
    ? '/staff/applications'
    : '/intern/projects'
}

function logout() {
  clearSession()
  router.replace('/login')
}

async function submit() {
  if (isSubmitting.value) return

  errors.value = []

  if (!form.oldPassword || !form.newPassword || !form.confirmPassword) {
    errors.value = ['Lütfen bütün şifre alanlarını doldurun.']
    return
  }

  if (form.newPassword !== form.confirmPassword) {
    errors.value = ['Yeni şifreler eşleşmiyor.']
    return
  }

  isSubmitting.value = true

  try {
    const result = await changePassword(
      form.oldPassword,
      form.newPassword
    )

    if (result !== true) {
      errors.value = ['Şifre değişikliği doğrulanamadı.']
      return
    }

    const currentSession = getSession()

    if (!currentSession) {
      await router.replace('/login')
      return
    }

    // Şifre güncellendi, zorunluluk kalktı
    saveSession({
      ...currentSession,
      mustChangePassword: false
    })

    form.oldPassword = ''
    form.newPassword = ''
    form.confirmPassword = ''

    isRequired.value = false

    await router.replace(homePath(currentSession.role))

  } catch (error) {
    errors.value = getErrorMessages(error)
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <main class="container py-5 change-password-page" style="max-width: 560px">
    <div class="card shadow-sm">
      <div class="card-body p-4">

        <div class="password-heading"><img src="/company-logo.png" alt="AE Yazılım Teknoloji"/><h4 class="mb-0">Şifremi Güncelle</h4></div>

        <div v-if="isRequired" class="alert alert-info">
          Hesabınızı kullanmaya devam etmek için
          şifrenizi değiştirmeniz gerekiyor.
        </div>

        <form @submit.prevent="submit">

          <div class="mb-3">
            <label for="oldPassword" class="form-label">Mevcut şifre</label>
            <div class="password-field">
              <input
                id="oldPassword"
                v-model="form.oldPassword"
                :type="visible.oldPassword ? 'text' : 'password'"
                class="form-control"
                autocomplete="current-password"
                required
                :disabled="isSubmitting"
              />
              <button
                type="button"
                class="password-toggle"
                :aria-label="visible.oldPassword ? 'Şifreyi gizle' : 'Şifreyi göster'"
                :aria-pressed="visible.oldPassword"
                :disabled="isSubmitting"
                @click="visible.oldPassword = !visible.oldPassword"
              >
                <svg v-if="!visible.oldPassword" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
                  <path d="M1 12s4-7 11-7 11 7 11 7-4 7-11 7S1 12 1 12Z" />
                  <circle cx="12" cy="12" r="3" />
                </svg>
                <svg v-else width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
                  <path d="m3 3 18 18" />
                  <path d="M10.58 10.58a2 2 0 0 0 2.83 2.83" />
                  <path d="M9.88 5.09A9.77 9.77 0 0 1 12 5c7 0 11 7 11 7a21.78 21.78 0 0 1-4.23 5.17" />
                  <path d="M6.61 6.61A21.2 21.2 0 0 0 1 12s4 7 11 7a10.7 10.7 0 0 0 5.39-1.61" />
                </svg>
              </button>
            </div>
          </div>

          <div class="mb-3">
            <label for="newPassword" class="form-label">Yeni şifre</label>
            <div class="password-field">
              <input
                id="newPassword"
                v-model="form.newPassword"
                :type="visible.newPassword ? 'text' : 'password'"
                class="form-control"
                autocomplete="new-password"
                required
                :disabled="isSubmitting"
              />
              <button
                type="button"
                class="password-toggle"
                :aria-label="visible.newPassword ? 'Şifreyi gizle' : 'Şifreyi göster'"
                :aria-pressed="visible.newPassword"
                :disabled="isSubmitting"
                @click="visible.newPassword = !visible.newPassword"
              >
                <svg v-if="!visible.newPassword" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
                  <path d="M1 12s4-7 11-7 11 7 11 7-4 7-11 7S1 12 1 12Z" />
                  <circle cx="12" cy="12" r="3" />
                </svg>
                <svg v-else width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
                  <path d="m3 3 18 18" />
                  <path d="M10.58 10.58a2 2 0 0 0 2.83 2.83" />
                  <path d="M9.88 5.09A9.77 9.77 0 0 1 12 5c7 0 11 7 11 7a21.78 21.78 0 0 1-4.23 5.17" />
                  <path d="M6.61 6.61A21.2 21.2 0 0 0 1 12s4 7 11 7a10.7 10.7 0 0 0 5.39-1.61" />
                </svg>
              </button>
            </div>
          </div>

          <div class="mb-3">
            <label for="confirmPassword" class="form-label">Yeni şifre (tekrar)</label>
            <div class="password-field">
              <input
                id="confirmPassword"
                v-model="form.confirmPassword"
                :type="visible.confirmPassword ? 'text' : 'password'"
                class="form-control"
                autocomplete="new-password"
                required
                :disabled="isSubmitting"
              />
              <button
                type="button"
                class="password-toggle"
                :aria-label="visible.confirmPassword ? 'Şifreyi gizle' : 'Şifreyi göster'"
                :aria-pressed="visible.confirmPassword"
                :disabled="isSubmitting"
                @click="visible.confirmPassword = !visible.confirmPassword"
              >
                <svg v-if="!visible.confirmPassword" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
                  <path d="M1 12s4-7 11-7 11 7 11 7-4 7-11 7S1 12 1 12Z" />
                  <circle cx="12" cy="12" r="3" />
                </svg>
                <svg v-else width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
                  <path d="m3 3 18 18" />
                  <path d="M10.58 10.58a2 2 0 0 0 2.83 2.83" />
                  <path d="M9.88 5.09A9.77 9.77 0 0 1 12 5c7 0 11 7 11 7a21.78 21.78 0 0 1-4.23 5.17" />
                  <path d="M6.61 6.61A21.2 21.2 0 0 0 1 12s4 7 11 7a10.7 10.7 0 0 0 5.39-1.61" />
                </svg>
              </button>
            </div>
          </div>

          <AlertMessages
            v-if="errors.length"
            :messages="errors"
            class="mb-3"
          />

          <button
            type="submit"
            class="btn btn-primary w-100"
            :disabled="isSubmitting"
          >
            {{ isSubmitting ? 'Güncelleniyor...' : 'Şifremi Güncelle' }}
          </button>

        </form>

        <div v-if="isRequired" class="text-center mt-3">
          <button
            type="button"
            class="btn btn-link"
            @click="logout"
          >
            Çıkış
          </button>
        </div>

      </div>
    </div>
  </main>
</template>

<style scoped>
.change-password-page .card{border:1px solid var(--border-soft);border-radius:18px;box-shadow:var(--shadow-md)!important;animation:rise-in .6s var(--ease-out) backwards}
.password-heading{display:flex;align-items:center;gap:16px;margin-bottom:24px}.password-heading img{height:76px;width:76px;object-fit:contain;margin:-8px 0}.password-heading h4{font-size:1.4rem;font-weight:750;color:var(--text-strong)}
.change-password-page .btn-link{color:var(--brand-primary)}
.password-field{position:relative}
.password-field .form-control{padding-right:48px}
.password-toggle{position:absolute;right:8px;top:50%;transform:translateY(-50%);width:34px;height:34px;display:grid;place-items:center;border:0;background:transparent;color:var(--text-faint);padding:0}
.password-toggle:hover:not(:disabled){color:var(--brand-primary)}
.password-toggle:disabled{opacity:.6}
</style>
