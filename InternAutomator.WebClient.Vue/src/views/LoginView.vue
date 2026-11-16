<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { login } from '@/api/auth'
import { getErrorMessages } from '@/api/http'
import { saveSession, ROLE_INTERN, ROLE_STAFF } from '@/auth/session'
import AlertMessages from '@/components/AlertMessages.vue'

const router = useRouter()

const email = ref('')
const password = ref('')
const isSubmitting = ref(false)
const errors = ref([])

// API rolünü (küçük harfe çevirerek) oturum rolüne eşler.
// Rol ile ilgili ID tutarsızsa null döner, oturum yazılmaz.
function createSession(person) {
  const apiRole = String(person.role ?? '').toLowerCase()
  const base = {
    personID: person.personID,
    firstName: person.firstName,
    lastName: person.lastName,
  }

  if (apiRole === 'intern' && person.internID >= 1) {
    return { session: { ...base, role: ROLE_INTERN, internID: person.internID }, home: '/intern/projects' }
  }
  if (apiRole === 'user' && person.userID >= 1) {
    return { session: { ...base, role: ROLE_STAFF, userID: person.userID }, home: '/staff/applications' }
  }
  return null
}

async function submit() {
  // İstek sürerken ikinci gönderim yapılmasın
  if (isSubmitting.value) return

  errors.value = []
  isSubmitting.value = true

  try {
    const person = await login(email.value.trim(), password.value)
    const result = createSession(person)

    if (!result) {
      password.value = ''
      errors.value = ['Hesabınıza erişilemedi']
      return
    }

    // Şifre hiçbir yerde saklanmaz
    saveSession(result.session)
    await router.replace(result.home)
  } catch (error) {
    password.value = ''
    errors.value = getErrorMessages(error)
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <main class="container py-5" style="max-width: 420px">
    <h3 class="mb-4 text-center">Giriş</h3>

    <form @submit.prevent="submit">
      <div class="mb-3">
        <label for="email" class="form-label">E-posta</label>
        <input id="email" v-model="email" type="email" class="form-control" autocomplete="username" required />
      </div>

      <div class="mb-3">
        <label for="password" class="form-label">Şifre</label>
        <input id="password" v-model="password" type="password" class="form-control" autocomplete="current-password" required />
      </div>

      <button type="submit" class="btn btn-primary w-100" :disabled="isSubmitting">Giriş Yap</button>

      <AlertMessages v-if="errors.length" :messages="errors" class="mt-3" />
    </form>
  </main>
</template>