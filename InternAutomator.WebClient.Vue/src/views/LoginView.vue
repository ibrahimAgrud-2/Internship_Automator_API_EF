
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

// API'den gelen kullanıcıya göre oturum oluşturulur.
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
    return {
      session: {
        ...base,
        role: ROLE_INTERN,
        internID: person.internID,
      },
      home: '/intern/projects',
    }
  }

  if (apiRole === 'user' && person.userID >= 1) {
    return {
      session: {
        ...base,
        role: ROLE_STAFF,
        userID: person.userID,
      },
      home: '/staff/applications',
    }
  }

  return null
}

async function submit() {
  if (isSubmitting.value) return

  errors.value = []
  isSubmitting.value = true

  try {
    const person = await login(
      email.value.trim(),
      password.value
    )

    const result = createSession(person)

    if (!result) {
      password.value = ''
      errors.value = ['Hesabınıza erişilemedi.']
      return
    }

    saveSession(result.session)

    await router.replace(
      result.session.mustChangePassword
        ? '/change-password'
        : result.home
    )
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

      <!-- SOL: GİRİŞ FORMU -->
      <section class="login-form-panel">
        <div class="login-form-content">
<div class="company-logo-container">
  <img
    src="/company-logo.jpg"
    alt="Şirket Logosu"
    class="company-logo"
  />
</div>
          <h1 class="login-title">Giriş Yap</h1>
          <p class="login-subtitle">
            Hesabınıza giriş yapın
          </p>

          <form @submit.prevent="submit">

            <!-- E-posta -->
            <div class="input-group-custom">
              <label for="email">E-posta Adresi</label>

              <input
                id="email"
                v-model="email"
                type="email"
                class="login-input"
                placeholder="E-posta adresiniz"
                autocomplete="username"
                :disabled="isSubmitting"
                required
              />
            </div>

            <!-- Şifre -->
            <div class="input-group-custom">
              <label for="password">Şifre</label>

              <div class="password-field">
                <input
                  id="password"
                  v-model="password"
                  :type="showPassword ? 'text' : 'password'"
                  class="login-input"
                  placeholder="Şifreniz"
                  autocomplete="current-password"
                  :disabled="isSubmitting"
                  required
                />

                <button
                  type="button"
                  class="password-toggle"
                  :disabled="isSubmitting"
                  :aria-label="
                    showPassword
                      ? 'Şifreyi gizle'
                      : 'Şifreyi göster'
                  "
                  :aria-pressed="showPassword"
                  @click="showPassword = !showPassword"
                >
                  <!-- Göz simgesi -->
                  <svg
                    v-if="!showPassword"
                    xmlns="http://www.w3.org/2000/svg"
                    width="20"
                    height="20"
                    viewBox="0 0 24 24"
                    fill="none"
                    stroke="currentColor"
                    stroke-width="1.8"
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    aria-hidden="true"
                  >
                    <path
                      d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7S2 12 2 12Z"
                    />
                    <circle cx="12" cy="12" r="3" />
                  </svg>

                  <!-- Gizli göz simgesi -->
                  <svg
                    v-else
                    xmlns="http://www.w3.org/2000/svg"
                    width="20"
                    height="20"
                    viewBox="0 0 24 24"
                    fill="none"
                    stroke="currentColor"
                    stroke-width="1.8"
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    aria-hidden="true"
                  >
                    <path d="M3 3l18 18" />
                    <path
                      d="M10.6 5.1A11 11 0 0 1 12 5c6.5 0 10 7 10 7a17 17 0 0 1-3.1 4.1"
                    />
                    <path
                      d="M6.1 6.1C3.5 8.2 2 12 2 12s3.5 7 10 7a9.9 9.9 0 0 0 4-.8"
                    />
                  </svg>
                </button>
              </div>
            </div>

            <AlertMessages
              v-if="errors.length"
              :messages="errors"
              class="login-error"
            />

            <!-- Giriş butonu -->
            <button
              type="submit"
              class="login-submit"
              :disabled="isSubmitting"
            >
              <span
                v-if="isSubmitting"
                class="spinner-border spinner-border-sm"
                aria-hidden="true"
              ></span>

              {{
                isSubmitting
                  ? 'Giriş yapılıyor...'
                  : 'Giriş'
              }}
            </button>
          </form>

        </div>
      </section>

      <!-- SAĞ: BAŞVURU BÖLÜMÜ -->
      <section class="welcome-panel">
        <div class="welcome-content">

          <div class="welcome-decoration"></div>

          <h2>Merhaba!</h2>

          <p>
            Henüz staj başvurusu yapmadınız mı?
            Başvuru formuna aşağıdaki butondan
            ulaşabilirsiniz.
          </p>

          <RouterLink
            to="/apply"
            class="register-button"
          >
            Başvuru Yap
          </RouterLink>

        </div>
      </section>

    </div>

    <!-- ALT BİLGİ -->
    <footer class="login-footer">
      © {{ new Date().getFullYear() }} Staj Otomasyonu.
      Tüm hakları saklıdır.
    </footer>
  </main>
</template>

<style scoped>
/* ==================================
   GENEL SAYFA
================================== */

.login-page {
--primary-color: #822328;
--primary-hover: #68191e;
  --text-color: #252525;

  min-height: 100vh;
  min-height: 100svh;

  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;

  padding: 55px 24px 25px;

  background-color: #f2f2f1;
  background-image:
    radial-gradient(
      circle at 13% 25%,
      rgba(152, 43, 44, 0.055) 0,
      transparent 27%
    ),
    radial-gradient(
      circle at 88% 78%,
      rgba(152, 43, 44, 0.045) 0,
      transparent 30%
    );

  font-family: Arial, Helvetica, sans-serif;
}

/* ==================================
   ANA KART
================================== */

.login-card {
  display: grid;
  grid-template-columns: 1fr 1fr;

  width: 100%;
  max-width: 950px;
  min-height: 550px;

  border-radius: 16px;
  overflow: hidden;

  background: #ffffff;

  box-shadow:
    0 18px 50px rgba(0, 0, 0, 0.11),
    0 4px 12px rgba(0, 0, 0, 0.045);
}

/* ==================================
   SOL FORM PANELİ
================================== */

.login-form-panel {
  display: flex;
  justify-content: center;
  align-items: center;

  padding: 55px 55px;
  background: white;
}

.login-form-content {
  width: 100%;
  max-width: 330px;
}

.login-title {
  margin: 0;
  text-align: center;

  color: var(--text-color);
  font-size: 39px;
  font-weight: 750;
  letter-spacing: -1px;
}

.login-subtitle {
  margin: 12px 0 34px;

  text-align: center;
  color: #888;
  font-size: 13px;
}

/* ==================================
   INPUT ALANLARI
================================== */

.input-group-custom {
  margin-bottom: 20px;
}

.input-group-custom label {
  display: block;

  margin-bottom: 8px;

  color: #444;
  font-size: 13px;
  font-weight: 600;
}

.login-input {
  display: block;

  width: 100%;
  height: 49px;

  padding: 0 15px;

  border: 1px solid #e6e6e6;
  border-radius: 7px;

  background: #f8f8f8;
  color: #252525;

  outline: none;
  font-size: 14px;

  transition:
    border-color 0.2s ease,
    box-shadow 0.2s ease,
    background-color 0.2s ease;
}

.login-input::placeholder {
  color: #a0a0a0;
}

.login-input:focus {
  background: white;
  border-color: var(--primary-color);

  box-shadow:
    0 0 0 3px rgba(152, 43, 44, 0.1);
}

.login-input:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

/* ==================================
   ŞİFRE GÖSTER / GİZLE
================================== */

.password-field {
  position: relative;
}

.password-field .login-input {
  padding-right: 48px;
}

.password-toggle {
  position: absolute;
  top: 50%;
  right: 12px;

  transform: translateY(-50%);

  width: 32px;
  height: 32px;

  display: flex;
  align-items: center;
  justify-content: center;

  border: none;
  border-radius: 5px;

  background: transparent;
  color: #777;

  cursor: pointer;
}

.password-toggle:hover:not(:disabled) {
  color: var(--primary-color);
  background: #f0e5e5;
}

.password-toggle:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

/* ==================================
   GİRİŞ BUTONU
================================== */

.login-submit {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;

  width: 165px;
  min-height: 46px;

  margin: 30px auto 0;

  border: none;
  border-radius: 25px;

  background: var(--primary-color);
  color: white;

  font-size: 15px;
  font-weight: 650;

  cursor: pointer;

  transition:
    background-color 0.2s ease,
    transform 0.2s ease,
    box-shadow 0.2s ease;
}

.login-submit:hover:not(:disabled) {
  background: var(--primary-hover);
  transform: translateY(-1px);

  box-shadow:
    0 5px 15px rgba(152, 43, 44, 0.2);
}

.login-submit:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.login-submit:focus-visible,
.password-toggle:focus-visible,
.register-button:focus-visible {
  outline: 3px solid #e7a7a7;
  outline-offset: 3px;
}

.login-error {
  margin-top: 16px;
  margin-bottom: 0;
  font-size: 13px;
}

/* ==================================
   SAĞ PANEL
================================== */

.welcome-panel {
  position: relative;

  display: flex;
  justify-content: center;
  align-items: center;

  padding: 60px 45px;

  background: var(--primary-color);
  color: white;

  overflow: hidden;
}

.welcome-panel::before {
  content: '';

  position: absolute;
  top: -125px;
  right: -125px;

  width: 340px;
  height: 340px;

  border: 1px solid rgba(255, 255, 255, 0.12);
  border-radius: 50%;

  pointer-events: none;
}

.welcome-panel::after {
  content: '';

  position: absolute;
  bottom: -155px;
  left: -155px;

  width: 360px;
  height: 360px;

  border: 1px solid rgba(255, 255, 255, 0.12);
  border-radius: 50%;

  pointer-events: none;
}

.welcome-content {
position: static;

  width: 100%;
  max-width: 335px;

  text-align: center;
}

.welcome-decoration {
  width: 38px;
  height: 3px;

  margin: 0 auto 24px;

  border-radius: 3px;
  background: rgba(255, 255, 255, 0.65);
}

.welcome-content h2 {
  margin: 0 0 24px;

  font-size: 43px;
  font-weight: 750;
  letter-spacing: -1.5px;
}

.welcome-content p {
  margin: 0 auto 38px;

  max-width: 310px;

  color: rgba(255, 255, 255, 0.92);
  font-size: 15px;
  line-height: 1.8;
}

/* ==================================
   BAŞVURU BUTONU
================================== */

.register-button {
  display: inline-flex;
  justify-content: center;
  align-items: center;

  min-width: 190px;
  min-height: 49px;

  padding: 10px 24px;

  border: 1.5px solid white;
  border-radius: 25px;

  background: transparent;
  color: white;

  text-decoration: none;
  font-size: 15px;
  font-weight: 650;

  transition:
    background-color 0.2s ease,
    color 0.2s ease;

    position: absolute;
bottom: 68px;
left: 50%;
transform: translateX(-50%);
}

.register-button:hover {
  background: white;
  color: var(--primary-color);
}

/* ==================================
   FOOTER
================================== */

.login-footer {
  margin-top: 33px;
  padding: 0 12px;

  color: #777;
  font-size: 12px;
  text-align: center;
}

/* ==================================
   TABLET
================================== */

@media (max-width: 800px) {
  .login-card {
    max-width: 720px;
    min-height: 500px;
  }

  .login-form-panel {
    padding: 45px 30px;
  }

  .welcome-panel {
    padding: 45px 30px;
  }

  .welcome-content h2 {
    font-size: 35px;
  }
}

/* ==================================
   MOBİL
================================== */

@media (max-width: 600px) {
  .login-page {
    justify-content: flex-start;
    padding: 25px 15px;
  }
.register-button {
  position: static;
  transform: none;
}
  .login-card {
    grid-template-columns: 1fr;
    min-height: unset;
    max-width: 430px;

    border-radius: 13px;
  }

  .login-form-panel {
    padding: 45px 28px;
  }

  .login-title {
    font-size: 34px;
  }

  .login-subtitle {
    margin-bottom: 30px;
  }

  .welcome-panel {
    min-height: 250px;
    padding: 40px 25px;
  }

  .welcome-content h2 {
    margin-bottom: 15px;
    font-size: 32px;
  }

  .welcome-content p {
    margin-bottom: 24px;
    font-size: 13px;
    line-height: 1.7;
  }

  .register-button {
    min-height: 44px;
    font-size: 14px;
  }

  .login-footer {
    margin-top: 24px;
    line-height: 1.6;
  }
}

@media (prefers-reduced-motion: reduce) {
  .login-input,
  .login-submit,
  .register-button {
    transition: none;
  }

  .login-submit:hover:not(:disabled) {
    transform: none;
  }
}

.company-logo-container {
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: 20px;
  background-color: transparent;
}

.company-logo {
  display: block;
  width: 125px;
  height: auto;
  object-fit: contain;
  background-color: #ffffff;
  border-radius: 0;
  box-shadow: none;
}
</style>
