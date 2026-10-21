<script setup>
import { ref, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { getSession, clearSession, ROLE_STAFF } from '@/auth/session'
import '@/assets/css/shell.css'

// Ortak iskelet: üst çubuk + sol menü. Menüler TEK yerde tanımlıdır.
// to: null olan satır henüz hazır olmayan bölümdür ("Yakında"). Sayfa hazır olunca sadece adresini yaz.
const INTERN_MENU = [
  { text: 'Ana sayfa', to: '/intern/home' },
  { text: 'Duyurular', to: null },
  { text: 'Projelerim', to: '/intern/projects' },
  { text: 'Mesaj', to: null },
  { text: 'SSS', to: null },
]

const STAFF_MENU = [
  { text: 'Başvurular', to: '/staff/applications' },
  { text: 'Stajyerler', to: '/staff/interns' },
  { text: 'Projeler', to: '/staff/projects' },
  { text: 'Duyurular', to: '/staff/announcements' },
  { text: 'Mesajlar', to: '/staff/messages' },
]

const route = useRoute()
const router = useRouter()

// Rota koruması sayesinde buraya oturum olmadan gelinmez
const session = getSession()
const role = route.meta.requiresRole

const menu = role === ROLE_STAFF ? STAFF_MENU : INTERN_MENU
const fullName = computed(() => `${session?.firstName ?? ''} ${session?.lastName ?? ''}`.trim())

// Dar ekranlarda menü kapalı başlar
const isCollapsed = ref(window.innerWidth < 768)

function logout() {
  clearSession()
  router.replace(role === ROLE_STAFF ? '/staff/login' : '/intern/login')
}
</script>

<template>
  <div class="shell" :class="{ 'sidebar-collapsed': isCollapsed }">
    <header class="navbar bg-light border-bottom px-3">
      <div class="d-flex align-items-center">
        <button type="button" class="btn btn-outline-secondary btn-sm me-3" aria-label="Menüyü aç/kapat" @click="isCollapsed = !isCollapsed">&#9776;</button>
        <span class="navbar-brand mb-0 h1">Staj Otomasyonu</span>
      </div>
      <div>
        <span class="me-3">{{ fullName }}</span>
        <button type="button" class="btn btn-outline-secondary btn-sm" @click="logout">Çıkış</button>
      </div>
    </header>

    <div class="shell-body">
      <nav class="shell-sidebar border-end bg-white">
        <ul class="nav nav-pills flex-column p-3 gap-1">
          <li v-for="item in menu" :key="item.text" class="nav-item">
            <RouterLink v-if="item.to" :to="item.to" class="nav-link" active-class="active">{{ item.text }}</RouterLink>

            <!-- Henüz hazır olmayan bölüm: tıklanamaz, soluk gösterilir -->
            <span v-else class="nav-link disabled d-flex justify-content-between align-items-center" aria-disabled="true">
              <span>{{ item.text }}</span>
              <span class="badge text-bg-light border">Yakında</span>
            </span>
          </li>
        </ul>
      </nav>

      <!-- Sayfa içeriği: router, adrese göre doğru sayfayı buraya koyar -->
      <main class="shell-content">
        <RouterView />
      </main>
    </div>
  </div>
</template>
