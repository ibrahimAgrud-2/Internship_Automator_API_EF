<script setup>
import { ref, computed, onMounted, onBeforeUnmount } from 'vue'
import { Dropdown } from 'bootstrap'
import { useRoute, useRouter } from 'vue-router'
import { getSession, clearSession } from '@/auth/session'
import '@/assets/css/shell.css'

const INTERN_MENU = [
  { text: 'Projelerim', to: '/intern/projects', icon: 'M3 7h7l2 2h9v11H3z M3 7V4h7l2 3h9v2' },
  { text: 'Görevlerim', to: '/intern/tasks', icon: 'M9 11l3 3 7-7 M5 4h14v16H5z' },
  { text: 'Mesajlar', to: '/intern/messages', icon: 'M21 4H3v14h4v4l5-4h9z M7 9h10 M7 13h6' },
  { text: 'Duyurular', to: '/intern/announcements', icon: 'M3 10v4h4l11 5V5L7 10z M7 14l2 6h3' },
]

const STAFF_MENU = [
  { text: 'Başvurular', to: '/staff/applications', icon: 'M6 3h9l4 4v14H6z M14 3v5h5 M9 12h7 M9 16h7' },
  { text: 'Stajyerler', to: '/staff/interns', icon: 'M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2 M9 11a4 4 0 1 0 0-8 4 4 0 0 0 0 8 M22 21v-2a4 4 0 0 0-3-4 M16 3a4 4 0 0 1 0 8' },
  { text: 'Projeler', to: '/staff/projects', icon: 'M3 7h7l2 2h9v11H3z M3 7V4h7l2 3h9v2' },
  { text: 'Duyurular', to: '/staff/announcements', icon: 'M3 10v4h4l11 5V5L7 10z M7 14l2 6h3' },
  { text: 'Mesajlar', to: '/staff/messages', icon: 'M21 4H3v14h4v4l5-4h9z M7 9h10 M7 13h6' },
  { text: 'Kullanıcılar', to: '/staff/users', icon: 'M12 12a4 4 0 1 0 0-8 4 4 0 0 0 0 8 M4 21v-2a5 5 0 0 1 5-5h6a5 5 0 0 1 5 5v2' },
]

const route = useRoute()
const router = useRouter()
const session = getSession()
const isStaff = computed(() => route.path.startsWith('/staff'))
const menu = computed(() => (isStaff.value ? STAFF_MENU : INTERN_MENU))
const fullName = computed(() => `${session?.firstName ?? ''} ${session?.lastName ?? ''}`.trim() || 'Hesabım')
const isCollapsed = ref(true)
const desktopHover = window.matchMedia('(min-width: 768px) and (hover: hover) and (pointer: fine)')
const profileButton = ref(null)
const initials = computed(() => [session?.firstName, session?.lastName]
  .filter(Boolean).map(name => name.trim().charAt(0)).join('').toLocaleUpperCase('tr-TR') || 'H')

function hideProfile() {
  if (profileButton.value) Dropdown.getInstance(profileButton.value)?.hide()
}
function collapseSidebar() {
  hideProfile()
  isCollapsed.value = true
}
function toggleSidebar() {
  if (isCollapsed.value) isCollapsed.value = false
  else collapseSidebar()
}
function onSidebarEnter() {
  if (desktopHover.matches) isCollapsed.value = false
}
function onSidebarLeave() {
  if (desktopHover.matches) collapseSidebar()
}
function onViewportChange() { collapseSidebar() }
onMounted(() => desktopHover.addEventListener('change', onViewportChange))
onBeforeUnmount(() => {
  desktopHover.removeEventListener('change', onViewportChange)
  if (profileButton.value) Dropdown.getInstance(profileButton.value)?.dispose()
})

function logout() {
  clearSession()
  router.replace('/login')
}

function closeSidebarOnMobile() {
  if (window.innerWidth < 768) collapseSidebar()
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
          @click="toggleSidebar"
        >
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round">
            <path d="M4 7h16M4 12h16M4 17h16" />
          </svg>
        </button>
        <span class="shell-brand">Staj Otomasyonu</span>
      </div>

    </header>

    <div class="shell-body">
      <nav class="shell-sidebar" aria-label="Ana menü" @mouseenter="onSidebarEnter" @mouseleave="onSidebarLeave">
        <div class="shell-sidebar-inner">
          <div class="shell-sidebar-label">MENÜ</div>
          <ul class="nav nav-pills flex-column gap-1">
            <li v-for="item in menu" :key="item.text" class="nav-item">
              <RouterLink :to="item.to" class="nav-link" active-class="active" :aria-label="item.text" :title="isCollapsed ? item.text : undefined" @click="closeSidebarOnMobile">
                <svg class="shell-nav-icon" width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path :d="item.icon" /></svg>
                <span class="shell-nav-text">{{ item.text }}</span>
              </RouterLink>
            </li>
          </ul>
          <div class="shell-sidebar-footer">
            <div class="dropup">
              <button ref="profileButton" id="profileMenu" type="button" class="shell-profile-card dropdown-toggle" data-bs-toggle="dropdown" aria-expanded="false" aria-label="Profil menüsünü aç" @click="isCollapsed = false">
                <span class="profile-avatar-button" aria-hidden="true">{{ initials }}</span>
                <span class="shell-profile-details"><strong>{{ fullName }}</strong><small>{{ isStaff ? 'Personel hesabı' : 'Stajyer hesabı' }}</small></span>
              </button>
              <ul class="dropdown-menu shadow-sm" aria-labelledby="profileMenu">
                <li class="dropdown-header"><strong class="d-block text-dark">{{ fullName }}</strong>{{ isStaff ? 'Personel hesabı' : 'Stajyer hesabı' }}</li>
                <li><hr class="dropdown-divider" /></li>
                <li><RouterLink to="/change-password" class="dropdown-item" @click="hideProfile">Şifremi Güncelle</RouterLink></li>
                <li><hr class="dropdown-divider" /></li>
                <li><button type="button" class="dropdown-item" @click="logout">Çıkış Yap</button></li>
              </ul>
            </div>
            <button type="button" class="shell-logout" aria-label="Çıkış Yap" title="Çıkış Yap" @click="logout">
              <svg class="shell-nav-icon" width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M10 17l5-5-5-5 M15 12H3 M14 3h6v18h-6" /></svg><span class="shell-nav-text">Çıkış Yap</span>
            </button>
          </div>
        </div>
      </nav>

      <main class="shell-content">
        <!-- Menüden menüye geçerken yalnızca içerik alanı yumuşakça değişir; üst çubuk ve yan menü sabit kalır -->
        <RouterView v-slot="{ Component }">
          <Transition name="page" mode="out-in">
            <component :is="Component" />
          </Transition>
        </RouterView>
      </main>
    </div>
  </div>
</template>
