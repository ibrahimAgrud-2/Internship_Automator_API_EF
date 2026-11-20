<script setup>
import { reactive, ref, computed, nextTick, onMounted, onBeforeUnmount } from 'vue'
import { submitApplication } from '@/api/applications'
import { getErrorMessages } from '@/api/http'
import { universities } from '@/data/universities'
import AlertMessages from '@/components/AlertMessages.vue'

// FORM
function emptyForm() {
  return {
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    address: '',
    university: '',
    department: '',
    classYear: '',
    linkedinUrl: '',
    githubUrl: '',
  }
}

const form = reactive(emptyForm())
const isSubmitting = ref(false)
const message = ref(null)

// ÜNİVERSİTE AÇILIR LİSTESİ
const dropdownOpen = ref(false)
const universitySearch = ref('')
const highlightedIndex = ref(0)

const dropdownElement = ref(null)
const searchElement = ref(null)
const triggerElement = ref(null)
const optionsElement = ref(null)

// Türkçe harflerle aramayı kolaylaştırır.
function normalizeText(value) {
  return String(value)
    .toLocaleLowerCase('tr-TR')
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .replace(/ı/g, 'i')
}

// Boş aramada tüm üniversiteler gösterilir.
const filteredUniversities = computed(() => {
  const query = normalizeText(universitySearch.value.trim())

  if (!query) return universities

  return universities.filter(university =>
    normalizeText(university).includes(query)
  )
})

async function openUniversityDropdown() {
  if (isSubmitting.value) return

  dropdownOpen.value = true
  universitySearch.value = ''
  highlightedIndex.value = 0

  await nextTick()
  searchElement.value?.focus()
}

function closeUniversityDropdown(focusTrigger = false) {
  dropdownOpen.value = false

  if (focusTrigger) {
    nextTick(() => triggerElement.value?.focus())
  }
}

function selectUniversity(university) {
  form.university = university
  closeUniversityDropdown(true)
}

function moveHighlight(direction) {
  const count = filteredUniversities.value.length
  if (!count) return

  highlightedIndex.value =
    (highlightedIndex.value + direction + count) % count

  nextTick(() => {
    const activeOption = optionsElement.value?.querySelector(
      '[data-active="true"]'
    )

    activeOption?.scrollIntoView({ block: 'nearest' })
  })
}

function selectHighlighted() {
  const university =
    filteredUniversities.value[highlightedIndex.value]

  if (university) {
    selectUniversity(university)
  }
}

function handleOutsideClick(event) {
  if (
    dropdownOpen.value &&
    !dropdownElement.value?.contains(event.target)
  ) {
    closeUniversityDropdown()
  }
}

onMounted(() => {
  document.addEventListener('pointerdown', handleOutsideClick)
})

onBeforeUnmount(() => {
  document.removeEventListener('pointerdown', handleOutsideClick)
})

// API İŞLEMLERİ
function textOrNull(value) {
  const text = value.trim()
  return text === '' ? null : text
}

function buildApplication() {
  return {
    personInfo: {
      firstName: form.firstName.trim(),
      lastName: form.lastName.trim(),
      email: form.email.trim(),
      phone: form.phone.trim(),
      address: textOrNull(form.address),
      imagePath: '',
    },
    university: form.university.trim(),
    department: form.department.trim(),
    classYear: Number(form.classYear),
    linkedinURL: textOrNull(form.linkedinUrl),
    githubURL: textOrNull(form.githubUrl),
    notes: '',
  }
}

async function submit() {
  if (isSubmitting.value) return

  message.value = null

  // Üniversite listeden seçilmiş olmalı.
  if (!form.university) {
    message.value = {
      type: 'danger',
      lines: ['Lütfen üniversitenizi seçin.'],
    }
    openUniversityDropdown()
    return
  }

  isSubmitting.value = true

  try {
    const result = await submitApplication(buildApplication())

    if (result !== true) {
      message.value = {
        type: 'danger',
        lines: [
          'Başvurunun sonucu doğrulanamadı. Lütfen tekrar göndermeden önce kontrol edin.',
        ],
      }
      return
    }

    message.value = {
      type: 'success',
      lines: ['Başvurunuz alınmıştır.'],
    }

    Object.assign(form, emptyForm())
    universitySearch.value = ''
    dropdownOpen.value = false
  } catch (error) {
    message.value = {
      type: 'danger',
      lines: getErrorMessages(error),
    }
  } finally {
    isSubmitting.value = false
  }
}
</script>
<template>
  <div class="application-page">
    <header class="application-header">
      <div class="application-header-inner">
        <div class="application-heading">
          <img src="/company-logo.jpg" alt="AE Yazılım Teknoloji" class="application-logo" />
          <h3>Staj Başvuru Formu</h3>
        </div>
        <RouterLink to="/login" class="login-link">Giriş Yap</RouterLink>
      </div>
    </header>
    <main class="container application-main">
      <section class="application-card">
    <form @submit.prevent="submit">


      <h5 class="mb-3">Kişisel Bilgiler</h5>

      <div class="row">
        <div class="col-md-6 mb-3">
          <label for="firstName" class="form-label">Ad</label>
          <input
            id="firstName"
            v-model="form.firstName"
            type="text"
            class="form-control"
            maxlength="20"
            required
          />
        </div>

        <div class="col-md-6 mb-3">
          <label for="lastName" class="form-label">Soyad</label>
          <input
            id="lastName"
            v-model="form.lastName"
            type="text"
            class="form-control"
            maxlength="20"
            required
          />
        </div>
      </div>

      <div class="mb-3">
        <label for="email" class="form-label">E-posta</label>
        <input
          id="email"
          v-model="form.email"
          type="email"
          class="form-control"
          maxlength="50"
          required
        />
      </div>

      <div class="mb-3">
        <label for="phone" class="form-label">Telefon</label>
        <input
          id="phone"
          v-model="form.phone"
          type="tel"
          class="form-control"
          maxlength="20"
          required
        />
      </div>

      <div class="mb-4">
        <label for="address" class="form-label">
          Adres <span class="optional">(opsiyonel)</span>
        </label>
        <textarea
          id="address"
          v-model="form.address"
          class="form-control"
          rows="2"
          maxlength="250"
        ></textarea>
      </div>

      <!-- EĞİTİM BİLGİLERİ -->
      <h5 class="mb-3">Eğitim Bilgileri</h5>

      <div class="mb-3">
        <label id="university-label" for="university" class="form-label">
          Üniversite
        </label>

        <div ref="dropdownElement" class="university-dropdown">
          <!-- Açılır liste butonu -->
          <button
            id="university"
            ref="triggerElement"
            type="button"
            class="university-trigger"
            :class="{ 'is-open': dropdownOpen }"
            :disabled="isSubmitting"
            :aria-expanded="dropdownOpen"
            aria-haspopup="listbox"
            aria-controls="university-options"
            aria-labelledby="university-label"
            aria-required="true"
            @click="
              dropdownOpen
                ? closeUniversityDropdown()
                : openUniversityDropdown()
            "
            @keydown.down.prevent="openUniversityDropdown"
            @keydown.up.prevent="openUniversityDropdown"
          >
            <span
              :class="{
                'university-placeholder': !form.university
              }"
            >
              {{
                form.university ||
                'Üniversitenizi arayın veya seçin...'
              }}
            </span>

            <svg
              class="dropdown-chevron"
              :class="{ rotated: dropdownOpen }"
              width="18"
              height="18"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              stroke-width="2"
              stroke-linecap="round"
              stroke-linejoin="round"
              aria-hidden="true"
            >
              <path d="m6 9 6 6 6-6" />
            </svg>
          </button>

          <!-- Açılan liste -->
          <div
            v-if="dropdownOpen"
            class="university-panel"
          >
            <div class="university-search-container">
              <input
                ref="searchElement"
                v-model="universitySearch"
                type="search"
                class="university-search"
                placeholder="Üniversite ara..."
                aria-label="Üniversite ara"
                autocomplete="off"
                @input="highlightedIndex = 0"
                @keydown.down.prevent="moveHighlight(1)"
                @keydown.up.prevent="moveHighlight(-1)"
                @keydown.enter.prevent="selectHighlighted"
                @keydown.esc.stop.prevent="closeUniversityDropdown(true)"
                @keydown.tab="closeUniversityDropdown()"
              />
            </div>

            <div
              id="university-options"
              ref="optionsElement"
              class="university-options"
              role="listbox"
              aria-labelledby="university-label"
            >
              <button
                v-for="(university, index) in filteredUniversities"
                :key="university"
                type="button"
                role="option"
                class="university-option"
                :class="{
                  highlighted: highlightedIndex === index,
                  selected: form.university === university
                }"
                :data-active="highlightedIndex === index"
                :aria-selected="form.university === university"
                @mouseenter="highlightedIndex = index"
                @click="selectUniversity(university)"
              >
                {{ university }}
              </button>

              <div
                v-if="filteredUniversities.length === 0"
                class="university-empty"
              >
                Aramanızla eşleşen üniversite bulunamadı.
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="row">
        <div class="col-md-8 mb-3">
          <label for="department" class="form-label">Bölüm</label>
          <input
            id="department"
            v-model="form.department"
            type="text"
            class="form-control"
            maxlength="30"
            required
          />
        </div>

        <div class="col-md-4 mb-3">
          <label for="classYear" class="form-label">Sınıf</label>
          <select
            id="classYear"
            v-model="form.classYear"
            class="form-select"
            required
          >
            <option value="" disabled>Seçiniz</option>
            <option value="1">1. Sınıf</option>
            <option value="2">2. Sınıf</option>
            <option value="3">3. Sınıf</option>
            <option value="4">4. Sınıf</option>
          </select>
        </div>
      </div>

      <!-- BAĞLANTILAR -->
      <h5 class="mt-3 mb-3">
        Bağlantılar
        <span class="optional fs-6">(opsiyonel)</span>
      </h5>

      <div class="mb-3">
        <label for="linkedinUrl" class="form-label">LinkedIn</label>
        <input
          id="linkedinUrl"
          v-model="form.linkedinUrl"
          type="url"
          class="form-control"
          maxlength="50"
        />
      </div>

      <div class="mb-4">
        <label for="githubUrl" class="form-label">GitHub</label>
        <input
          id="githubUrl"
          v-model="form.githubUrl"
          type="url"
          class="form-control"
          maxlength="50"
        />
      </div>

      <button
        type="submit"
        class="btn btn-primary submit-button"
        :disabled="isSubmitting"
      >
        {{
          isSubmitting
            ? 'Başvuru gönderiliyor...'
            : 'Başvur'
        }}
      </button>

      <AlertMessages
        v-if="message"
        :type="message.type"
        :messages="message.lines"
        class="mt-3"
      />
    </form>
      </section>
    </main>
  </div>
</template>
<style scoped>

/* GENEL RENKLER */

.application-page {
  --brand-red: #822328;
  --brand-red-hover: #68191e;

  color: #1c1c1c;
  background: #ffffff;
}

.application-page h3,
.application-page h5 {
  color: #1c1c1c;
  font-weight: 700;
}

.application-page .form-label {
  color: #1c1c1c;
  font-weight: 500;
}

.optional {
  color: #666666;
  font-weight: 400;
}

/* FORM ALANLARI */

.application-page .form-control,
.application-page .form-select {
  min-height: 43px;
  background: #ffffff;
  color: #1c1c1c;
  border: 1px solid #d5d5d5;
  border-radius: 7px;
}

.application-page .form-control:focus,
.application-page .form-select:focus {
  color: #1c1c1c;
  background: #ffffff;
  border-color: var(--brand-red);
  box-shadow: 0 0 0 3px rgba(130, 35, 40, 0.12);
}

.application-page .form-control::placeholder {
  color: #777777;
}

/* ÜNİVERSİTE SEÇİMİ */

.university-dropdown {
  position: relative;
  width: 100%;
}

.university-trigger {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;

  width: 100%;
  min-height: 44px;
  padding: 10px 14px;

  background: #ffffff;
  border: 1px solid #d5d5d5;
  border-radius: 7px;

  color: #1c1c1c;
  font-size: 15px;
  line-height: 1.4;
  text-align: left;

  cursor: pointer;
  transition: border-color 0.15s ease, box-shadow 0.15s ease;
}

.university-trigger:hover,
.university-trigger.is-open,
.university-trigger:focus-visible {
  border-color: var(--brand-red);
}

.university-trigger:focus-visible,
.university-trigger.is-open {
  outline: none;
  box-shadow: 0 0 0 3px rgba(130, 35, 40, 0.12);
}

.university-trigger:disabled {
  opacity: 0.65;
  cursor: not-allowed;
}

.university-placeholder {
  color: #777777;
}

.dropdown-chevron {
  flex-shrink: 0;
  color: #1c1c1c;
  transition: transform 0.2s ease;
}

.dropdown-chevron.rotated {
  transform: rotate(180deg);
}

/* AÇILIR PANEL */

.university-panel {
  position: absolute;
  top: calc(100% + 5px);
  left: 0;
  right: 0;
  z-index: 1000;

  overflow: hidden;

  background: #ffffff;
  border: 1px solid #d5d5d5;
  border-radius: 9px;
  box-shadow: 0 12px 28px rgba(0, 0, 0, 0.12);
}

.university-search-container {
  padding: 10px;
  background: #ffffff;
  border-bottom: 1px solid #e4e4e4;
}

.university-search {
  width: 100%;
  min-height: 40px;
  padding: 8px 12px;

  background: #ffffff;
  color: #1c1c1c;
  border: 1px solid #cccccc;
  border-radius: 6px;
  outline: none;
  font-size: 14px;
}

.university-search:focus {
  border-color: var(--brand-red);
  box-shadow: 0 0 0 3px rgba(130, 35, 40, 0.1);
}

.university-options {
  max-height: 270px;
  overflow-y: auto;
  overscroll-behavior: contain;
}

.university-option {
  display: block;
  width: 100%;
  padding: 11px 14px;

  border: none;
  border-bottom: 1px solid #eeeeee;
  background: #ffffff;
  color: #1c1c1c;

  font-size: 14px;
  line-height: 1.5;
  text-align: left;

  cursor: pointer;
}

.university-option:last-child {
  border-bottom: none;
}

.university-option:hover,
.university-option.highlighted {
  background: #822328;
  color: #ffffff;
}

.university-option.selected {
  font-weight: 700;
}

.university-empty {
  padding: 18px 14px;
  color: #555555;
  font-size: 14px;
  text-align: center;
}

/* BAŞVURU BUTONU */

.submit-button {
  min-width: 155px;
  min-height: 43px;
  padding: 9px 24px;

  border: 1px solid var(--brand-red);
  border-radius: 25px;

  background: var(--brand-red);
  color: #ffffff;

  font-weight: 600;
}

.submit-button:hover,
.submit-button:focus-visible,
.submit-button:active {
  background: var(--brand-red-hover) !important;
  border-color: var(--brand-red-hover) !important;
  color: #ffffff !important;
}

.submit-button:disabled {
  background: var(--brand-red);
  border-color: var(--brand-red);
  color: #ffffff;
  opacity: 0.7;
}

/* BİLDİRİM RENKLERİ */

.application-page :deep(.alert) {
  background: #ffffff;
  color: #1c1c1c;
  border: 1px solid var(--brand-red);
}

/* MOBİL */

@media (max-width: 600px) {
  .university-panel {
    max-width: 100%;
  }

  .university-options {
    max-height: 235px;
  }

  .university-option {
    padding: 12px;
    font-size: 14px;
  }
}

@media (prefers-reduced-motion: reduce) {
  .university-trigger,
  .dropdown-chevron {
    transition: none;
  }
}



.application-page{min-height:100vh;background:#f8f8f8;color:#222}
.application-header{width:100%;background:#fff;border-bottom:1px solid #e8e8e8;box-shadow:0 2px 12px rgba(0,0,0,.04)}
.application-header-inner{display:flex;justify-content:space-between;align-items:center;gap:24px;width:100%;max-width:1200px;min-height:94px;margin:auto;padding:12px 30px}
.application-heading{display:flex;align-items:center;gap:20px;min-width:0}.application-logo{width:78px;height:70px;object-fit:contain;background:#fff;flex-shrink:0}
.application-header h3{margin:0;font-weight:700;font-size:25px;color:#1c1c1c}
.login-link{display:inline-flex;align-items:center;justify-content:center;flex-shrink:0;min-height:42px;padding:10px 25px;background:#822328;border:1px solid #822328;border-radius:24px;color:#fff;font-size:14px;font-weight:600;text-decoration:none}
.login-link:hover,.login-link:focus-visible{background:#68191e;border-color:#68191e;color:#fff}
.application-main{max-width:780px;padding:46px 18px 65px}.application-card{width:100%;padding:36px 42px 42px;background:#fff;border:1px solid rgba(130,35,40,.12);border-radius:16px;box-shadow:0 18px 50px rgba(0,0,0,.14),0 4px 12px rgba(0,0,0,.06)}
@media(max-width:600px){.application-header-inner{min-height:75px;padding:12px 16px;gap:10px}.application-heading{gap:9px}.application-logo{width:50px;height:50px}.application-header h3{font-size:17px}.login-link{min-height:36px;padding:7px 13px;font-size:12px}.application-main{padding:25px 12px 45px}.application-card{padding:25px 20px 30px}}
@media(max-width:380px){.application-header h3{font-size:15px}.login-link{padding:7px 10px;font-size:11px}}
</style>
