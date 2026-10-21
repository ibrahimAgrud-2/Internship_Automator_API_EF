import { createRouter, createWebHistory } from 'vue-router'
import { getSession, ROLE_STAFF } from '@/auth/session'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    { path: '/', redirect: '/staff' },

    // meta.loginOf: bu rolün giriş sayfası (zaten girişliyse gösterilmez)
    {
      path: '/staff/login',
      component: () => import('@/views/StaffLoginView.vue'),
      meta: { loginOf: ROLE_STAFF },
    },

    // meta.requiresRole: bu sayfayı sadece o rol görebilir
    {
      path: '/staff',
      component: () => import('@/views/StaffHomeView.vue'),
      meta: { requiresRole: ROLE_STAFF },
    },
  ],
})

// Rota koruması: her sayfa geçişinden önce tek yerde çalışır
router.beforeEach((to) => {
  const session = getSession()

  // Giriş gerektiren sayfa: oturum yoksa ya da rol uymuyorsa giriş sayfasına
  if (to.meta.requiresRole && session?.role !== to.meta.requiresRole) {
    return '/staff/login'
  }

  // Zaten girişliyken giriş sayfasını gösterme
  if (to.meta.loginOf && session?.role === to.meta.loginOf) {
    return '/staff'
  }
})

export default router