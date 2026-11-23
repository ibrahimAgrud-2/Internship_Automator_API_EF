<script setup>
import { ref, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { getSession, clearSession } from '@/auth/session'
import '@/assets/css/shell.css'

const INTERN_MENU = [
  { text: 'Projelerim', to: '/intern/projects' },
  { text: 'Görevlerim', to: '/intern/tasks' },
  { text: 'Mesajlar', to: '/intern/messages' },
  { text: 'Duyurular', to: '/intern/announcements' },
]

const STAFF_MENU = [
  { text: 'Başvurular', to: '/staff/applications' },
  { text: 'Stajyerler', to: '/staff/interns' },
  { text: 'Projeler', to: '/staff/projects' },
  { text: 'Duyurular', to: '/staff/announcements' },
  { text: 'Mesajlar', to: '/staff/messages' },
  { text: 'Kullanıcılar', to: '/staff/users' },
]

const route = useRoute()
const router = useRouter()
const session = getSession()
const isStaff = computed(() => route.path.startsWith('/staff'))
const menu = computed(() => (isStaff.value ? STAFF_MENU : INTERN_MENU))
const fullName = computed(() => `${session?.firstName ?? ''} ${session?.lastName ?? ''}`.trim() || 'Hesabım')
const isCollapsed = ref(window.innerWidth < 768)

function logout() {
  clearSession()
  router.replace('/login')
}

function closeSidebarOnMobile() {
  if (window.innerWidth < 768) isCollapsed.value = true
}
</script>

<template>
  <div class="shell" :class="{ 'sidebar-collapsed': isCollapsed }">
    <header class="shell-topbar">
      <div class="shell-topbar-start">
        <button
          type="button"
          class="shell-menu-toggle"
          aria-label="Yan menüyü aç veya kapat"
          :aria-expanded="!isCollapsed"
          @click="isCollapsed = !isCollapsed"
        >
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round">
            <path d="M4 7h16M4 12h16M4 17h16" />
          </svg>
        </button>
        <span class="shell-brand">Staj Otomasyonu</span>
      </div>

      <div class="shell-topbar-end">
        <div class="dropdown">
          <button
            id="profileMenu"
            type="button"
            class="profile-avatar-button dropdown-toggle"
            data-bs-toggle="dropdown"
            aria-expanded="false"
            aria-label="Profil menüsünü aç"
          >
            <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
              <circle cx="12" cy="8" r="4" />
              <path d="M4.5 20a7.5 7.5 0 0 1 15 0" />
            </svg>
          </button>

          <ul class="dropdown-menu dropdown-menu-end shadow-sm mt-2" aria-labelledby="profileMenu">
            <li class="dropdown-header">
              <strong class="d-block text-dark">{{ fullName }}</strong>
              {{ isStaff ? 'Personel hesabı' : 'Stajyer hesabı' }}
            </li>
            <li><hr class="dropdown-divider" /></li>
            <li><RouterLink to="/change-password" class="dropdown-item">Şifremi Güncelle</RouterLink></li>
            <li><hr class="dropdown-divider" /></li>
            <li><button type="button" class="dropdown-item" @click="logout">Çıkış Yap</button></li>
          </ul>
        </div>
      </div>
    </header>

    <div class="shell-body">
      <nav class="shell-sidebar" aria-label="Ana menü">
        <div class="shell-sidebar-inner">
          <div class="shell-sidebar-label">MENÜ</div>
          <ul class="nav nav-pills flex-column gap-1">
            <li v-for="item in menu" :key="item.text" class="nav-item">
              <RouterLink :to="item.to" class="nav-link" active-class="active" @click="closeSidebarOnMobile">{{ item.text }}</RouterLink>
            </li>
          </ul>
        </div>
      </nav>

      <main class="shell-content"><RouterView /></main>
    </div>
  </div>
</template>
