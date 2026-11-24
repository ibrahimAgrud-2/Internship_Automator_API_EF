<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { login } from '@/api/auth'
import { getErrorMessages } from '@/api/http'
import { saveSession, ROLE_INTERN, ROLE_STAFF } from '@/auth/session'
import AlertMessages from '@/components/AlertMessages.vue'

const router = useRouter()
const email = ref('')
const password = ref('')
const showPassword = ref(false)
const isSubmitting = ref(false)
const errors = ref([])

function createSession(person) {
  const apiRole = String(person.role ?? '').toLowerCase()
  const base = {
    personID: person.personID,
    mustChangePassword: person.mustChangePassword === true,
    firstName: person.firstName,
    lastName: person.lastName,
    token: person.token,
  }

  if (apiRole === 'intern' && person.internID >= 1) {
    return { session: { ...base, role: ROLE_INTERN, internID: person.internID }, home: '/intern/projects' }
  }

  if (apiRole === 'user' && person.userID >= 1) {
    return { session: { ...base, role: ROLE_STAFF, userID: person.userID }, home: '/staff/applications' }
  }

  return null
}

async function submit() {
  if (isSubmitting.value) return

  errors.value = []
  isSubmitting.value = true

  try {
    const result = createSession(await login(email.value.trim(), password.value))
    if (!result) {
      password.value = ''
      errors.value = ['Hesabınıza erişilemedi.']
      return
    }

    saveSession(result.session)
    await router.replace(result.session.mustChangePassword ? '/change-password' : result.home)
  } catch (error) {
    password.value = ''
    errors.value = getErrorMessages(error)
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <main class="login-page">
    <div class="login-card">
      <section class="login-form-panel">
        <div class="login-form-content">
          <img class="company-logo" src="/company-logo.png" alt="AE Yazılım Teknoloji" />
          <p class="login-subtitle">Hesabınıza giriş yapın</p>

          <form class="login-form" @submit.prevent="submit">
            <div class="input-group-custom">
              <label for="loginEmail">E-posta Adresi</label>
              <input
                id="loginEmail"
                v-model="email"
                type="email"
                class="login-input"
                placeholder="E-posta adresiniz"
                autocomplete="username"
                required
                :disabled="isSubmitting"
              />
            </div>

            <div class="input-group-custom">
              <label for="loginPassword">Şifre</label>
              <div class="password-field">
                <input
                  id="loginPassword"
                  v-model="password"
                  :type="showPassword ? 'text' : 'password'"
                  class="login-input"
                  placeholder="Şifreniz"
                  autocomplete="current-password"
                  required
                  :disabled="isSubmitting"
                />
                <button
                  type="button"
                  class="password-toggle"
                  :aria-label="showPassword ? 'Şifreyi gizle' : 'Şifreyi göster'"
                  :aria-pressed="showPassword"
                  :disabled="isSubmitting"
                  @click="showPassword = !showPassword"
                >
                  <svg v-if="!showPassword" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
                    <path d="M1 12s4-7 11-7 11 7 11 7-4 7-11 7S1 12 1 12Z" />
                    <circle cx="12" cy="12" r="3" />
                  </svg>
                  <svg v-else width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
                    <path d="m3 3 18 18" />
                    <path d="M10.58 10.58a2 2 0 0 0 2.83 2.83" />
                    <path d="M9.88 5.09A9.77 9.77 0 0 1 12 5c7 0 11 7 11 7a21.78 21.78 0 0 1-4.23 5.17" />
                    <path d="M6.61 6.61A21.2 21.2 0 0 0 1 12s4 7 11 7a10.7 10.7 0 0 0 5.39-1.61" />
                  </svg>
                </button>
              </div>
            </div>

            <AlertMessages v-if="errors.length" :messages="errors" class="mb-3" />

            <div class="login-submit-row">
              <button type="submit" class="login-submit" :disabled="isSubmitting">
                {{ isSubmitting ? 'Giriş yapılıyor...' : 'Giriş Yap' }}
              </button>
            </div>
          </form>
        </div>
      </section>

      <section class="welcome-panel">
        <div class="welcome-content">
          <div>
            <h2>Merhaba!</h2>
            <p>
              Henüz staj başvurusu yapmadınız mı?<br />
              Başvuru formuna aşağıdaki butondan ulaşabilirsiniz.
            </p>
          </div>

          <RouterLink to="/apply" class="register-button">Başvuru Yap</RouterLink>
        </div>
      </section>
    </div>

    <footer class="login-footer">© {{ new Date().getFullYear() }} Staj Otomasyonu. Tüm hakları saklıdır.</footer>
  </main>
</template>

<style scoped>
.login-page { --red:var(--brand-primary); min-height:100vh; min-height:100svh; display:flex; flex-direction:column; justify-content:center; align-items:center; background:var(--surface-canvas); padding:36px 20px 20px; color:var(--text-strong); }
.login-card { display:grid; grid-template-columns:1fr 1fr; width:100%; max-width:1080px; min-height:650px; background:#fff; overflow:hidden; border-radius:20px; box-shadow:var(--shadow-lg); animation:rise-in .7s var(--ease-out) backwards; }
 .login-form-panel,.welcome-panel { display:flex; align-items:center; justify-content:center; padding:50px; position:relative; }
.login-form-content,.welcome-content { width:100%; max-width:380px; }
.login-form-content { display:flex; flex-direction:column; align-items:center; text-align:center; }
.login-form { width:100%; display:flex; flex-direction:column; }
.company-logo { display:block; width:116px; height:116px; object-fit:contain; margin:-14px auto 4px; }
.login-subtitle { margin:0 0 30px; color:var(--text-body); font-size:16px; font-weight:600; }
.input-group-custom { margin-bottom:22px; text-align:left; }
.input-group-custom label { display:block; font-size:13px; font-weight:650; margin-bottom:8px; }
.login-input { width:100%; height:52px; padding:0 15px; border:1px solid var(--border-strong); border-radius:8px; background:var(--surface); color:var(--text-strong); outline:none; transition:border-color .15s ease, box-shadow .25s var(--ease-out), background-color .15s ease; }
.login-input:hover:not(:focus) { border-color:#C9BBB2; }
.login-input:focus { border-color:var(--brand-primary); box-shadow:0 0 0 3px var(--brand-primary-ring); }
.password-field { position:relative; }
.password-field .login-input { padding-right:48px; }
.password-toggle { position:absolute; right:8px; top:50%; transform:translateY(-50%); width:34px; height:34px; display:grid; place-items:center; border:0; background:transparent; color:var(--text-faint); transition:color .15s ease; padding:0; }
.password-toggle:hover:not(:disabled) { color:var(--red); }
.password-toggle:disabled { opacity:.6; }
.login-submit-row { position:absolute; bottom:70px; left:0; right:0; display:flex; justify-content:center; }
.login-submit { display:inline-flex; align-items:center; justify-content:center; width:190px; height:50px; border:0; border-radius:30px; background:var(--red); color:#fff; font-weight:700; transition:background-color .2s ease, box-shadow .25s var(--ease-out), transform .15s var(--ease-out); }
.login-submit:hover:not(:disabled) { background:var(--brand-primary-hover); box-shadow:0 8px 20px rgba(var(--brand-primary-rgb),.32); transform:translateY(-1px); }
.login-submit:active:not(:disabled) { transform:translateY(1px) scale(.98); }
.login-submit:disabled { opacity:.7; }
.welcome-panel { position:relative; background:linear-gradient(145deg,#6B1B1E 0%, var(--brand-primary) 55%, #B9524F 100%); color:#fff; overflow:hidden; }
.welcome-panel::before,.welcome-panel::after { content:''; position:absolute; width:350px; height:350px; border:1px solid rgba(255,255,255,.13); border-radius:50%; }
.welcome-panel::before { right:-170px; top:-170px; animation:orbit 18s ease-in-out infinite alternate; }
.welcome-panel::after { left:-160px; bottom:-160px; animation:orbit 22s ease-in-out infinite alternate-reverse; }
@keyframes orbit { from { translate:0 0; scale:1; } to { translate:22px 16px; scale:1.08; } }
.welcome-content { display:flex; flex-direction:column; align-items:center; justify-content:center; text-align:center; position:static; z-index:1; padding:0 0 45px; }
.welcome-content h2 { font-size:44px; font-weight:750; margin:0 0 25px; }
.welcome-content p { font-size:16px; line-height:1.8; margin:0; }
.register-button { position:absolute; bottom:70px; left:50%; transform:translateX(-50%); display:inline-flex; align-items:center; justify-content:center; min-width:190px; height:50px; padding:10px 24px; border:1px solid #fff; border-radius:30px; color:#fff; text-decoration:none; font-weight:700; transition:background-color .2s ease, color .2s ease, box-shadow .25s var(--ease-out); }
.register-button:hover { background:#fff; color:var(--red); box-shadow:0 8px 22px rgba(0,0,0,.2); }
.login-footer { margin-top:28px; font-size:12px; color:var(--text-muted); text-align:center; }
@media (max-width:800px) {
  .login-card { max-width:520px; grid-template-columns:1fr; min-height:0; }
  .login-form-panel { padding:40px 30px; }
  .login-submit-row { position:static; margin-top:28px; }
  .welcome-panel { padding:42px 30px; }
  .welcome-content { min-height:0; padding:0; justify-content:center; gap:24px; }
  .register-button { position:static; transform:none; }
  .welcome-content h2 { font-size:34px; margin-bottom:16px; }
  .welcome-content p { font-size:14px; }
}
@media (max-width:480px) {
  .login-page { padding:15px; }
  .login-form-panel { padding:35px 24px; }
  .company-logo { width:96px; height:96px; }
  .login-footer { line-height:1.6; }
}
</style>
