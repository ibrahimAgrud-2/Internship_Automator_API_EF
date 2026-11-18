
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
  <main class="container py-5" style="max-width: 520px">
    <div class="card shadow-sm">
      <div class="card-body p-4">

        <h4 class="mb-3">Şifremi Güncelle</h4>

        <div v-if="isRequired" class="alert alert-info">
          Hesabınızı kullanmaya devam etmek için
          şifrenizi değiştirmeniz gerekiyor.
        </div>

        <form @submit.prevent="submit">

          <div class="mb-3">
            <label class="form-label">Mevcut şifre</label>
            <input
              v-model="form.oldPassword"
              type="password"
              class="form-control"
              autocomplete="current-password"
              required
              :disabled="isSubmitting"
            />
          </div>

          <div class="mb-3">
            <label class="form-label">Yeni şifre</label>
            <input
              v-model="form.newPassword"
              type="password"
              class="form-control"
              autocomplete="new-password"
              required
              :disabled="isSubmitting"
            />
          </div>

          <div class="mb-3">
            <label class="form-label">Yeni şifre (tekrar)</label>
            <input
              v-model="form.confirmPassword"
              type="password"
              class="form-control"
              autocomplete="new-password"
              required
              :disabled="isSubmitting"
            />
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

        <div class="text-center mt-3">
          <button
            v-if="isRequired"
            type="button"
            class="btn btn-link"
            @click="logout"
          >
            Çıkış
          </button>

          <RouterLink
            v-else
            :to="homePath(session?.role)"
            class="link-secondary"
          >
            Panele geri dön
          </RouterLink>
        </div>

      </div>
    </div>
  </main>
</template>
