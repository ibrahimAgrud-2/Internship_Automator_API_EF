
<script setup>
import { ref, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { getSession, clearSession, ROLE_INTERN, ROLE_STAFF } from '@/auth/session'
import '@/assets/css/shell.css'

const INTERN_MENU = [
  { text: 'Projelerim', to: '/intern/projects' },
  { text: 'Görevlerim', to: '/intern/tasks' },
  { text: 'Mesaj', to: '/intern/messages' },
  { text: 'Duyurular', to: '/intern/announcements' },
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

const session = getSession()
const role = route.meta.requiresRole
const isIntern = role === ROLE_INTERN
const menu = role === ROLE_STAFF ? STAFF_MENU : INTERN_MENU
const fullName = computed(() => `${session?.firstName ?? ''} ${session?.lastName ?? ''}`.trim())

const isCollapsed = ref(window.innerWidth < 768)

function logout() {
  clearSession()
  router.replace('/login')
}
</script>

<template>
  <div class="shell" :class="{ 'sidebar-collapsed': isCollapsed }">
    <header class="navbar bg-light border-bottom px-3">
      <div class="d-flex align-items-center">
        <button
          type="button"
          class="btn btn-outline-secondary btn-sm me-3"
          aria-label="Menüyü aç/kapat"
          @click="isCollapsed = !isCollapsed"
        >
          &#9776;
        </button>
        <span class="navbar-brand mb-0 h1">Staj Otomasyonu</span>
      </div>

      <!-- Stajyer: sağ üstte profil simgesi ve açılır menü -->
      <div v-if="isIntern" class="d-flex align-items-center gap-2">
        <span class="profile-name d-none d-sm-inline">{{ fullName }}</span>

        <div class="dropdown">
          <button
            id="internProfileMenu"
            type="button"
            class="profile-avatar-button dropdown-toggle"
            data-bs-toggle="dropdown"
            aria-expanded="false"
            aria-label="Profil menüsünü aç"
            title="Profil"
          >
            <svg
              xmlns="http://www.w3.org/2000/svg"
              width="23"
              height="23"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              stroke-width="1.8"
              stroke-linecap="round"
              stroke-linejoin="round"
              aria-hidden="true"
            >
              <circle cx="12" cy="8" r="4" />
              <path d="M4.5 20a7.5 7.5 0 0 1 15 0" />
            </svg>
          </button>

          <ul class="dropdown-menu dropdown-menu-end shadow-sm mt-2" aria-labelledby="internProfileMenu">
            <li class="dropdown-header">
              <span class="d-block fw-semibold text-dark">{{ fullName }}</span>
              <span>Stajyer hesabı</span>
            </li>
            <li><hr class="dropdown-divider" /></li>

            <!-- Şifre sayfası ve API hazır olduğunda etkinleştirilecek. -->
            <li>
              <button type="button" class="dropdown-item" disabled aria-disabled="true">
                Şifremi Güncelle
              </button>
            </li>
            <li><hr class="dropdown-divider" /></li>
            <li>
              <button type="button" class="dropdown-item" @click="logout">Çıkış</button>
            </li>
          </ul>
        </div>
      </div>

      <!-- Personel: mevcut üst menü aynı kalıyor -->
      <div v-else>
        <span class="me-3">{{ fullName }}</span>
        <button type="button" class="btn btn-outline-secondary btn-sm" @click="logout">Çıkış</button>
      </div>
    </header>

    <div class="shell-body">
      <nav class="shell-sidebar border-end bg-white">
        <ul class="nav nav-pills flex-column p-3 gap-1">
          <li v-for="item in menu" :key="item.text" class="nav-item">
            <RouterLink v-if="item.to" :to="item.to" class="nav-link" active-class="active">
              {{ item.text }}
            </RouterLink>

            <span v-else class="nav-link disabled d-flex justify-content-between align-items-center" aria-disabled="true">
              <span>{{ item.text }}</span>
              <span class="badge text-bg-light border">Yakında</span>
            </span>
          </li>
        </ul>
      </nav>

      <main class="shell-content">
        <RouterView />
      </main>
    </div>
  </div>
</template>

<style scoped>
.profile-name {
  font-size: 0.875rem;
  font-weight: 500;
  color: #3b4655;
}

.profile-avatar-button {
  display: inline-flex;
  width: 42px;
  height: 42px;
  align-items: center;
  justify-content: center;
  border: 2px solid #fff;
  border-radius: 50%;
  background: #334e78;
  color: #fff;
  box-shadow: 0 0 0 1px #d8e0e9;
  cursor: pointer;
  transition: background-color 0.15s, box-shadow 0.15s;
}

.profile-avatar-button:hover,
.profile-avatar-button:focus-visible {
  background: #253c60;
  box-shadow: 0 0 0 3px #cbd8ec;
}

.profile-avatar-button::after {
  display: none;
}

.dropdown-menu {
  min-width: 210px;
}
</style>
