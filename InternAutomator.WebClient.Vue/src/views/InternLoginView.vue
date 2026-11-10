<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { internLogin } from '@/api/auth'
import { getErrorMessages } from '@/api/http'
import { saveSession, ROLE_INTERN } from '@/auth/session'
import AlertMessages from '@/components/AlertMessages.vue'

const router = useRouter()

const email = ref('')
const password = ref('')
const isSubmitting = ref(false)
const errors = ref([])

async function submit() {
  // İstek sürerken ikinci gönderim yapılmasın
  if (isSubmitting.value) return

  errors.value = []
  isSubmitting.value = true

  try {
    const intern = await internLogin(email.value.trim(), password.value)

    // Şifre hiçbir yerde saklanmaz
    saveSession({
      role: ROLE_INTERN,
      internID: intern.internID,
      firstName: intern.firstName,
      lastName: intern.lastName,
      startDate: intern.startDate,
      endDate: intern.endDate,
      status: intern.status,
    })

    await router.replace('/intern/home')
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
    <h3 class="mb-4 text-center">Stajyer Girişi</h3>

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
