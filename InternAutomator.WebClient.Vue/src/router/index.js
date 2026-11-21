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

{
  path: '/change-password',
  component: () => import('@/views/ChangePasswordView.vue'),
  meta: { title: 'Şifremi Güncelle' }
},

  // Tek giriş sayfası: zaten girişliyse gösterilmez (beforeEach'te yönlendirilir)
    {
      path: '/login',
      component: () => import('@/views/LoginView.vue'),
      meta: { isLoginPage: true, title: 'Giriş' },
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
        { path: 'users', component: () => import('@/views/staff/UsersView.vue'), meta: { title: 'Kullanıcılar' } },
      ],
    },

    // ---- Stajyer: iskelet (AppShell) + sayfalar ----
    {
      path: '/intern',
      component: AppShell,
      meta: { requiresRole: ROLE_INTERN },
children: [
        // Stajyerin ana sayfası Projelerim'dir (eski /intern/home adresi de buraya yönlenir)
        { path: '', redirect: '/intern/projects' },
        { path: 'home', redirect: '/intern/projects' },
        { path: 'projects', component: () => import('@/views/intern/InternProjectsView.vue'), meta: { title: 'Projelerim' } },
        { path: 'tasks', component: () => import('@/views/intern/InternTasksView.vue'), meta: { title: 'Görevlerim' } },
       { path: 'announcements', component: () => import('@/views/intern/InternAnnouncementsView.vue'), meta: { title: 'Duyurular' } },
               { path: 'messages', component: () => import('@/views/intern/InternMessagesView.vue'), meta: { title: 'Mesaj' } },
       
      ],
    },

    // Bilinmeyen adres: açılış sayfasına
    { path: '/:pathMatch(.*)*', redirect: '/' },
  ],
})

// Rol -> giriş sayfası ve girişten sonra açılacak sayfa
const LOGIN_PAGE = { [ROLE_STAFF]: '/login', [ROLE_INTERN]: '/login' }
const HOME_PAGE = { [ROLE_STAFF]: '/staff/applications', [ROLE_INTERN]: '/intern/projects' }
// Rota koruması: her sayfa geçişinden önce tek yerde çalışır

router.beforeEach((to) => {
  const session = getSession()

  // Zorunlu şifre değişikliği
  if (
    session?.mustChangePassword === true &&
    to.path !== '/change-password'
  ) {
    return '/change-password'
  }

  // Şifre sayfasına yalnızca giriş yapanlar erişebilir
  if (to.path === '/change-password' && !session?.role) {
    return '/login'
  }

  // Mevcut rol kontrolü
  const requiredRole = to.meta.requiresRole

  if (requiredRole && session?.role !== requiredRole) {
    return LOGIN_PAGE[requiredRole]
  }

  // Giriş yapmış kullanıcı tekrar login sayfasına gidemez
  if (to.meta.isLoginPage && session?.role) {
    return HOME_PAGE[session.role] ?? '/login'
  }
})

router.afterEach((to) => {
  document.title = to.meta.title ?? 'Staj Otomasyonu'
})

export default router
