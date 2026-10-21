<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { staffLogin } from '@/api/auth'
import { getErrorMessages } from '@/api/http'
import { saveSession, ROLE_STAFF } from '@/auth/session'

const router = useRouter()

const userName = ref('')
const password = ref('')
const isSubmitting = ref(false)
const errors = ref([])

async function submit() {
  // İstek sürerken ikinci gönderim yapılmasın
  if (isSubmitting.value) return

  errors.value = []
  isSubmitting.value = true

  try {
    const user = await staffLogin(userName.value.trim(), password.value)

    // Şifre hiçbir yerde saklanmaz
    saveSession({
      role: ROLE_STAFF,
      userID: user.userID,
      firstName: user.firstName,
      lastName: user.lastName,
      permissionType: user.permissionType,
      isActive: user.isActive,
    })

    await router.replace('/staff')
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
    <h3 class="mb-4 text-center">Personel Girişi</h3>

    <form @submit.prevent="submit">
      <div class="mb-3">
        <label for="userName" class="form-label">Kullanıcı adı</label>
        <input id="userName" v-model="userName" type="text" class="form-control" autocomplete="username" required />
      </div>

      <div class="mb-3">
        <label for="password" class="form-label">Şifre</label>
        <input id="password" v-model="password" type="password" class="form-control" autocomplete="current-password" required />
      </div>

      <button type="submit" class="btn btn-primary w-100" :disabled="isSubmitting">Giriş Yap</button>

      <div v-if="errors.length" class="alert alert-danger mt-3">
        <div v-for="(line, i) in errors" :key="i">{{ line }}</div>
      </div>
    </form>
  </main>
</template>