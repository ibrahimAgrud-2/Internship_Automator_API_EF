import { createRouter, createWebHistory } from 'vue-router'
import { getSession, ROLE_INTERN, ROLE_STAFF } from '@/auth/session'
import AppShell from '@/components/AppShell.vue'

// Sayfalar ihtiyaç olunca yüklenir (() => import(...)); açılış sayfası hafif kalır.
const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    // ---- Herkese açık sayfalar ----
    { path: '/', component: () => import('@/views/HomeView.vue'), meta: { title: 'Staj Otomasyonu' } },
    { path: '/apply', component: () => import('@/views/ApplyView.vue'), meta: { title: 'Staj Başvurusu' } },

    // meta.loginOf: bu rolün giriş sayfası (zaten girişliyse gösterilmez)
    {
      path: '/staff/login',
      component: () => import('@/views/StaffLoginView.vue'),
      meta: { loginOf: ROLE_STAFF, title: 'Personel Girişi' },
    },
    {
      path: '/intern/login',
      component: () => import('@/views/InternLoginView.vue'),
      meta: { loginOf: ROLE_INTERN, title: 'Stajyer Girişi' },
    },

    // ---- Personel: iskelet (AppShell) + sayfalar ----
    // meta.requiresRole: bu bölümü sadece o rol görebilir (alt sayfalar da bu kuralı devralır)
    {
      path: '/staff',
      component: AppShell,
      meta: { requiresRole: ROLE_STAFF },
      children: [
        { path: '', redirect: '/staff/applications' },
        { path: 'applications', component: () => import('@/views/staff/ApplicationsView.vue'), meta: { title: 'Staj Başvuruları' } },
        { path: 'interns', component: () => import('@/views/staff/InternsView.vue'), meta: { title: 'Stajyerler' } },
        { path: 'interns/:internID', component: () => import('@/views/staff/InternDetailView.vue'), meta: { title: 'Stajyer Detayı' } },
        { path: 'projects', component: () => import('@/views/staff/ProjectsView.vue'), meta: { title: 'Projeler' } },
        { path: 'announcements', component: () => import('@/views/staff/AnnouncementsView.vue'), meta: { title: 'Duyurular' } },
        { path: 'messages', component: () => import('@/views/staff/MessagesView.vue'), meta: { title: 'Mesajlar' } },
      ],
    },

    // ---- Stajyer: iskelet (AppShell) + sayfalar ----
    {
      path: '/intern',
      component: AppShell,
      meta: { requiresRole: ROLE_INTERN },
      children: [
        { path: '', redirect: '/intern/home' },
        { path: 'home', component: () => import('@/views/intern/InternHomeView.vue'), meta: { title: 'Stajyer Ana Sayfa' } },
        { path: 'projects', component: () => import('@/views/intern/InternProjectsView.vue'), meta: { title: 'Projelerim' } },
      ],
    },

    // Bilinmeyen adres: açılış sayfasına
    { path: '/:pathMatch(.*)*', redirect: '/' },
  ],
})

// Rol -> giriş sayfası ve girişten sonra açılacak sayfa
const LOGIN_PAGE = { [ROLE_STAFF]: '/staff/login', [ROLE_INTERN]: '/intern/login' }
const HOME_PAGE = { [ROLE_STAFF]: '/staff/applications', [ROLE_INTERN]: '/intern/home' }

// Rota koruması: her sayfa geçişinden önce tek yerde çalışır
router.beforeEach((to) => {
  const session = getSession()
  const requiredRole = to.meta.requiresRole

  // Giriş gerektiren bölüm: oturum yoksa ya da rol uymuyorsa o rolün giriş sayfasına
  if (requiredRole && session?.role !== requiredRole) {
    return LOGIN_PAGE[requiredRole]
  }

  // Zaten girişliyken giriş sayfasını gösterme
  if (to.meta.loginOf && session?.role === to.meta.loginOf) {
    return HOME_PAGE[to.meta.loginOf]
  }
})

router.afterEach((to) => {
  document.title = to.meta.title ?? 'Staj Otomasyonu'
})

export default router
