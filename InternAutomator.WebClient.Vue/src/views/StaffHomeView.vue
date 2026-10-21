<script setup>
import { useRouter } from 'vue-router'
import { getSession, clearSession } from '@/auth/session'

const router = useRouter()

// Rota koruması sayesinde bu sayfaya oturum olmadan gelinmez
const session = getSession()

function logout() {
  clearSession()
  router.replace('/staff/login')
}
</script>

<template>
  <main class="container py-4">
    <!-- Geçici sayfa: kabuk (menü + üst bar) ve gerçek sayfalar gelince değişecek -->
    <h4 class="mb-3">Hoş geldiniz, {{ session.firstName }} {{ session.lastName }}</h4>
    <button type="button" class="btn btn-outline-secondary" @click="logout">Çıkış</button>
  </main>
</template>