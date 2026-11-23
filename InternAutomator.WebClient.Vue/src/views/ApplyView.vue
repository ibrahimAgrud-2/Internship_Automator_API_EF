<script setup>
import { reactive, ref, computed, nextTick, onMounted, onBeforeUnmount } from 'vue'
import { submitApplication } from '@/api/applications'
import { getErrorMessages } from '@/api/http'
import { universities } from '@/data/universities'
import AlertMessages from '@/components/AlertMessages.vue'

const MAX_CV_SIZE = 5 * 1024 * 1024 // 5 MB

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
// API başarıyla yanıt verdikten sonra tam ekran onay penceresi açılır.
const showSuccessDialog = ref(false)
const successDialogRef = ref(null)
const firstNameInput = ref(null)

function closeSuccessDialog() {
  if (!showSuccessDialog.value) return
  showSuccessDialog.value = false
  // Klavye kullanan ziyaretçi için odağı tekrar forma döndür.
  nextTick(() => firstNameInput.value?.focus({ preventScroll: true }))
}

// CV seçimi: doğrulama arayüzdedir, gerçek dosya kontrolü API'de yapılır.
const selectedCv = ref(null)
const cvError = ref('')
const cvInput = ref(null)
const isDraggingCv = ref(false)

const cvSizeText = computed(() => {
  if (!selectedCv.value) return ''
  const size = selectedCv.value.size / (1024 * 1024)
  return `${size.toLocaleString('tr-TR', { maximumFractionDigits: 2 })} MB`
})

function validateCv(file) {
  if (!file) return 'Lütfen CV dosyanızı seçin.'
  if (!file.name.toLowerCase().endsWith('.pdf')) {
    return 'Yalnızca PDF uzantılı dosyalar yüklenebilir.'
  }
  if (file.size === 0) return 'Boş dosya yüklenemez.'
  if (file.size > MAX_CV_SIZE) return 'CV dosyası en fazla 5 MB olabilir.'
  return ''
}

function setCv(file) {
  if (isSubmitting.value) return
  message.value = null
  cvError.value = validateCv(file)
  selectedCv.value = cvError.value ? null : file
  if (cvInput.value) cvInput.value.value = ''
}

function onCvInputChange(event) {
  setCv(event.target.files?.[0] ?? null)
}

function openCvPicker() {
  if (!isSubmitting.value) cvInput.value?.click()
}

function removeCv() {
  if (isSubmitting.value) return
  selectedCv.value = null
  cvError.value = ''
  if (cvInput.value) cvInput.value.value = ''
}

function onCvDrop(event) {
  isDraggingCv.value = false
  if (isSubmitting.value) return
  setCv(event.dataTransfer?.files?.[0] ?? null)
}

// Üniversite seçimi (mevcut aramalı liste korunmuştur).
const dropdownOpen = ref(false)
const universitySearch = ref('')
const highlightedIndex = ref(0)
const dropdownElement = ref(null)
const searchElement = ref(null)
const triggerElement = ref(null)
const optionsElement = ref(null)

function normalizeText(value) {
  return String(value)
    .toLocaleLowerCase('tr-TR')
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .replace(/ı/g, 'i')
}

const filteredUniversities = computed(() => {
  const query = normalizeText(universitySearch.value.trim())
  return query
    ? universities.filter((university) => normalizeText(university).includes(query))
    : universities
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
  if (focusTrigger) nextTick(() => triggerElement.value?.focus())
}

function selectUniversity(university) {
  form.university = university
  closeUniversityDropdown(true)
}

function moveHighlight(direction) {
  const count = filteredUniversities.value.length
  if (!count) return
  highlightedIndex.value = (highlightedIndex.value + direction + count) % count
  nextTick(() => {
    optionsElement.value?.querySelector('[data-active="true"]')?.scrollIntoView({
      block: 'nearest',
    })
  })
}

function selectHighlighted() {
  const selected = filteredUniversities.value[highlightedIndex.value]
  if (selected) selectUniversity(selected)
}

function handleOutsideClick(event) {
  if (dropdownOpen.value && !dropdownElement.value?.contains(event.target)) {
    closeUniversityDropdown()
  }
}

onMounted(() => document.addEventListener('pointerdown', handleOutsideClick))
onBeforeUnmount(() => document.removeEventListener('pointerdown', handleOutsideClick))

function textOrNull(value) {
  const text = value.trim()
  return text || null
}

function buildApplication() {
  return {
    personInfo: {
      firstName: form.firstName.trim(),
      lastName: form.lastName.trim(),
      email: form.email.trim(),
      phone: form.phone.trim(),
      address: textOrNull(form.address),
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
  cvError.value = ''

  if (!form.university) {
    message.value = { type: 'danger', lines: ['Lütfen üniversitenizi seçin.'] }
    openUniversityDropdown()
    return
  }

  cvError.value = validateCv(selectedCv.value)
  if (cvError.value) {
    cvInput.value?.focus()
    return
  }

  isSubmitting.value = true

  try {
    const result = await submitApplication(buildApplication(), selectedCv.value)

    if (result !== true) {
      message.value = {
        type: 'danger',
        lines: ['Başvurunun sonucu doğrulanamadı. Tekrar göndermeden önce kontrol edin.'],
      }
      return
    }

    Object.assign(form, emptyForm())
    universitySearch.value = ''
    dropdownOpen.value = false
    selectedCv.value = null
    cvError.value = ''
    if (cvInput.value) cvInput.value.value = ''

    showSuccessDialog.value = true
    await nextTick()
    successDialogRef.value?.focus({ preventScroll: true })
  } catch (error) {
    message.value = { type: 'danger', lines: getErrorMessages(error) }
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <main class="container py-5 application-page" style="max-width: 780px">
    <header class="application-header">
      <div class="application-heading">
        <img src="/company-logo.jpg" alt="AE Yazılım Teknoloji" class="application-logo" />
        <h3>Staj Başvuru Formu</h3>
      </div>
      <RouterLink to="/login" class="login-link">Giriş Yap</RouterLink>
    </header>

    <section class="application-card">
      <form @submit.prevent="submit">
        <h5 class="mb-3">Kişisel Bilgiler</h5>

        <div class="row">
          <div class="col-md-6 mb-3">
            <label for="firstName" class="form-label">Ad</label>
            <input id="firstName" ref="firstNameInput" v-model="form.firstName" type="text" class="form-control" maxlength="20" autocomplete="given-name" required />
          </div>
          <div class="col-md-6 mb-3">
            <label for="lastName" class="form-label">Soyad</label>
            <input id="lastName" v-model="form.lastName" type="text" class="form-control" maxlength="20" autocomplete="family-name" required />
          </div>
        </div>

        <div class="mb-3">
          <label for="email" class="form-label">E-posta</label>
          <input id="email" v-model="form.email" type="email" class="form-control" maxlength="50" autocomplete="email" required />
        </div>

        <div class="mb-3">
          <label for="phone" class="form-label">Telefon</label>
          <input id="phone" v-model="form.phone" type="tel" class="form-control" maxlength="20" autocomplete="tel" required />
        </div>

        <div class="mb-4">
          <label for="address" class="form-label">Adres <span class="optional"></span></label>
          <textarea id="address" v-model="form.address" class="form-control" rows="2" maxlength="250" autocomplete="street-address"></textarea>
        </div>

        <h5 class="mb-3">Eğitim Bilgileri</h5>

        <div class="mb-3">
          <label id="university-label" for="university" class="form-label">Üniversite</label>
          <div ref="dropdownElement" class="university-dropdown">
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
              @click="dropdownOpen ? closeUniversityDropdown() : openUniversityDropdown()"
              @keydown.down.prevent="openUniversityDropdown"
              @keydown.up.prevent="openUniversityDropdown"
            >
              <span :class="{ 'university-placeholder': !form.university }">
                {{ form.university || 'Üniversitenizi arayın veya seçin...' }}
              </span>
              <svg class="dropdown-chevron" :class="{ rotated: dropdownOpen }" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
                <path d="m6 9 6 6 6-6" />
              </svg>
            </button>

            <div v-if="dropdownOpen" class="university-panel">
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
              <div id="university-options" ref="optionsElement" class="university-options" role="listbox" aria-labelledby="university-label">
                <button
                  v-for="(university, index) in filteredUniversities"
                  :key="university"
                  type="button"
                  role="option"
                  class="university-option"
                  :class="{ highlighted: highlightedIndex === index, selected: form.university === university }"
                  :data-active="highlightedIndex === index"
                  :aria-selected="form.university === university"
                  @mouseenter="highlightedIndex = index"
                  @click="selectUniversity(university)"
                >
                  {{ university }}
                </button>
                <div v-if="filteredUniversities.length === 0" class="university-empty">
                  Aramanızla eşleşen üniversite bulunamadı.
                </div>
              </div>
            </div>
          </div>
        </div>

        <div class="row">
          <div class="col-md-8 mb-3">
            <label for="department" class="form-label">Bölüm</label>
            <input id="department" v-model="form.department" type="text" class="form-control" maxlength="30" required />
          </div>
          <div class="col-md-4 mb-3">
            <label for="classYear" class="form-label">Sınıf</label>
            <select id="classYear" v-model="form.classYear" class="form-select" required>
              <option value="" disabled>Seçiniz</option>
              <option value="1">1. Sınıf</option>
              <option value="2">2. Sınıf</option>
              <option value="3">3. Sınıf</option>
              <option value="4">4. Sınıf</option>
            </select>
          </div>
        </div>

        <h5 class="mt-3 mb-3">Bağlantılar <span class="optional fs-6">(opsiyonel)</span></h5>

        <div class="mb-3">
          <label for="linkedinUrl" class="form-label">LinkedIn</label>
          <input id="linkedinUrl" v-model="form.linkedinUrl" type="url" class="form-control" maxlength="50" />
        </div>

        <div class="mb-4">
          <label for="githubUrl" class="form-label">GitHub</label>
          <input id="githubUrl" v-model="form.githubUrl" type="url" class="form-control" maxlength="50" />
        </div>

        <div class="mb-4">
          <h5 class="mb-2">Özgeçmiş (CV) <span class="required-indicator">*</span></h5>
          <p class="cv-hint mb-3">Başvurunuz için güncel CV'nizi PDF olarak yükleyin. En fazla 5 MB.</p>

          <input
            id="cvInput"
            ref="cvInput"
            type="file"
            class="visually-hidden"
            accept=".pdf,application/pdf"
            :disabled="isSubmitting"
            aria-label="CV dosyası seçin"
            :aria-invalid="Boolean(cvError)"
            :aria-describedby="cvError ? 'cv-error' : 'cv-description'"
            @change="onCvInputChange"
          />

          <div
            class="cv-dropzone"
            :class="{ 'has-file': selectedCv, 'is-dragging': isDraggingCv, 'has-error': cvError }"
            @dragenter.prevent="isDraggingCv = true"
            @dragover.prevent="isDraggingCv = true"
            @dragleave.prevent="isDraggingCv = false"
            @drop.prevent="onCvDrop"
          >
            <template v-if="selectedCv">
              <span class="cv-file-icon" aria-hidden="true">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round">
                  <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8Z" />
                  <path d="M14 2v6h6" /><path d="M8 15h8M8 18h5" />
                </svg>
              </span>
              <div class="cv-file-meta">
                <strong class="cv-filename" :title="selectedCv.name">{{ selectedCv.name }}</strong>
                <span class="cv-file-size">{{ cvSizeText }} · PDF dosyası seçildi</span>
              </div>
              <button type="button" class="cv-remove" :disabled="isSubmitting" aria-label="Seçili CV'yi kaldır" @click="removeCv">
                Kaldır
              </button>
            </template>
            <template v-else>
              <span class="cv-upload-icon" aria-hidden="true">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round">
                  <path d="M12 16V4m0 0L7.5 8.5M12 4l4.5 4.5" />
                  <path d="M4 16v3a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2v-3" />
                </svg>
              </span>
              <div id="cv-description" class="cv-prompt">CV dosyanızı buraya sürükleyin veya</div>
              <button type="button" class="cv-pick-button" :disabled="isSubmitting" @click="openCvPicker">Dosya Seç</button>
              <span class="cv-format">PDF · Maksimum 5 MB</span>
            </template>
          </div>
          <p v-if="cvError" id="cv-error" class="cv-error" role="alert">{{ cvError }}</p>
        </div>

        <button type="submit" class="btn btn-primary submit-button" :disabled="isSubmitting">
          {{ isSubmitting ? 'Başvuru gönderiliyor...' : 'Başvur' }}
        </button>

        <AlertMessages v-if="message" :type="message.type" :messages="message.lines" class="mt-3" />
      </form>
    </section>
  </main>

  <!-- Yalnızca API başvurunun başarıyla alındığını onayladığında görünür. -->
  <Teleport to="body">
    <Transition name="application-success">
      <div
        v-if="showSuccessDialog"
        ref="successDialogRef"
        class="application-success-overlay"
        role="dialog"
        aria-modal="true"
        aria-labelledby="application-success-title"
        aria-describedby="application-success-description"
        tabindex="-1"
        @click="closeSuccessDialog"
        @keydown.esc.prevent="closeSuccessDialog"
      >
        <div class="application-success-panel">
          <span class="application-success-confetti confetti-a" aria-hidden="true">✦</span>
          <span class="application-success-confetti confetti-b" aria-hidden="true">✧</span>
          <span class="application-success-confetti confetti-c" aria-hidden="true">✦</span>
          <span class="application-success-confetti confetti-d" aria-hidden="true">✧</span>

          <div class="application-success-icon" aria-hidden="true">
            <svg viewBox="0 0 64 64" fill="none" stroke="currentColor" stroke-width="4" stroke-linecap="round" stroke-linejoin="round">
              <path d="m17 33 11 11 21-23" />
            </svg>
          </div>
          <div class="application-success-emoji" aria-hidden="true">🎉</div>
          <h2 id="application-success-title">Başvurunuz başarıyla alındı!</h2>
          <p class="application-success-thanks">Başvurunuz için teşekkürler.</p>
          <p id="application-success-description" class="application-success-description">
            En kısa sürede başvurunuzu değerlendirip sonucu tarafınıza ileteceğiz.
          </p>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<style scoped>
.application-page {
  --brand-red: #822328;
  --brand-red-hover: #68191e;
  color: #1c1c1c;
  background: #fff;
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

.optional { color: #666; font-weight: 400; }
.required-indicator { color: var(--brand-red); }

.application-page .form-control,
.application-page .form-select {
  min-height: 43px;
  background: #fff;
  color: #1c1c1c;
  border: 1px solid #d5d5d5;
  border-radius: 7px;
}

.application-page .form-control:focus,
.application-page .form-select:focus {
  color: #1c1c1c;
  background: #fff;
  border-color: var(--brand-red);
  box-shadow: 0 0 0 3px rgba(130, 35, 40, .12);
}

.application-page .form-control::placeholder { color: #777; }

/* Üniversite seçimi */
.university-dropdown { position: relative; width: 100%; }
.university-trigger {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  width: 100%;
  min-height: 44px;
  padding: 10px 14px;
  background: #fff;
  border: 1px solid #d5d5d5;
  border-radius: 7px;
  color: #1c1c1c;
  font-size: 15px;
  line-height: 1.4;
  text-align: left;
  cursor: pointer;
  transition: border-color .15s ease, box-shadow .15s ease;
}

.university-trigger:hover,
.university-trigger.is-open,
.university-trigger:focus-visible { border-color: var(--brand-red); }
.university-trigger:focus-visible,
.university-trigger.is-open { outline: none; box-shadow: 0 0 0 3px rgba(130, 35, 40, .12); }
.university-trigger:disabled { opacity: .65; cursor: not-allowed; }
.university-placeholder { color: #777; }
.dropdown-chevron { flex-shrink: 0; transition: transform .2s ease; }
.dropdown-chevron.rotated { transform: rotate(180deg); }

.university-panel {
  position: absolute;
  top: calc(100% + 5px);
  left: 0;
  right: 0;
  z-index: 1000;
  overflow: hidden;
  background: #fff;
  border: 1px solid #d5d5d5;
  border-radius: 9px;
  box-shadow: 0 12px 28px rgba(0, 0, 0, .12);
}

.university-search-container { padding: 10px; background: #fff; border-bottom: 1px solid #e4e4e4; }
.university-search {
  width: 100%;
  min-height: 40px;
  padding: 8px 12px;
  background: #fff;
  color: #1c1c1c;
  border: 1px solid #ccc;
  border-radius: 6px;
  outline: none;
  font-size: 14px;
}
.university-search:focus { border-color: var(--brand-red); box-shadow: 0 0 0 3px rgba(130, 35, 40, .1); }
.university-options { max-height: 270px; overflow-y: auto; overscroll-behavior: contain; }
.university-option {
  display: block;
  width: 100%;
  padding: 11px 14px;
  border: none;
  border-bottom: 1px solid #eee;
  background: #fff;
  color: #1c1c1c;
  font-size: 14px;
  line-height: 1.5;
  text-align: left;
  cursor: pointer;
}
.university-option:last-child { border-bottom: none; }
.university-option:hover,
.university-option.highlighted { background: var(--brand-red); color: #fff; }
.university-option.selected { font-weight: 700; }
.university-empty { padding: 18px 14px; color: #555; font-size: 14px; text-align: center; }

/* CV alanı */
.cv-hint { color: #666; font-size: 13px; line-height: 1.5; }
.cv-dropzone {
  display: flex;
  align-items: center;
  justify-content: center;
  flex-direction: column;
  gap: 9px;
  min-height: 165px;
  padding: 20px;
  background: #fbfaf9;
  border: 1.5px dashed #bdb6b5;
  border-radius: 11px;
  text-align: center;
  transition: background .15s ease, border-color .15s ease;
}
.cv-dropzone.is-dragging { background: #f9ebeb; border-color: var(--brand-red); }
.cv-dropzone.has-file { min-height: 88px; flex-direction: row; justify-content: flex-start; text-align: left; gap: 14px; background: #fff; border-style: solid; border-color: #d7cac8; }
.cv-dropzone.has-error { border-color: #b42332; }
.cv-upload-icon { display: inline-flex; color: var(--brand-red); }
.cv-upload-icon svg { width: 30px; height: 30px; }
.cv-prompt { color: #424242; font-size: 14px; }
.cv-pick-button {
  border: 1px solid var(--brand-red);
  border-radius: 18px;
  padding: 5px 20px;
  background: #fff;
  color: var(--brand-red);
  font-size: 13px;
  font-weight: 700;
}
.cv-pick-button:hover,
.cv-pick-button:focus-visible { color: #fff; background: var(--brand-red); }
.cv-pick-button:disabled { cursor: not-allowed; opacity: .5; }
.cv-format { color: #777; font-size: 12px; }
.cv-file-icon { display: inline-flex; flex-shrink: 0; width: 42px; height: 42px; padding: 8px; background: #f8ecec; color: var(--brand-red); border-radius: 9px; }
.cv-file-icon svg { width: 100%; height: 100%; }
.cv-file-meta { display: flex; flex: 1; min-width: 0; flex-direction: column; gap: 4px; }
.cv-filename { overflow: hidden; text-overflow: ellipsis; white-space: nowrap; font-size: 13px; }
.cv-file-size { color: #666; font-size: 12px; }
.cv-remove { border: 0; background: transparent; color: var(--brand-red); font-size: 13px; font-weight: 600; padding: 8px; }
.cv-remove:hover { text-decoration: underline; }
.cv-remove:disabled { opacity: .5; cursor: not-allowed; }
.cv-error { margin: 7px 0 0; color: #b42332; font-size: 13px; }

/* Başvuru butonu ve bildirimler */
.submit-button {
  min-width: 155px;
  min-height: 43px;
  padding: 9px 24px;
  border: 1px solid var(--brand-red);
  border-radius: 25px;
  background: var(--brand-red);
  color: #fff;
  font-weight: 600;
}
.submit-button:hover,
.submit-button:focus-visible,
.submit-button:active { background: var(--brand-red-hover) !important; border-color: var(--brand-red-hover) !important; color: #fff !important; }
.submit-button:disabled { background: var(--brand-red); border-color: var(--brand-red); color: #fff; opacity: .7; }
.application-page :deep(.alert) { background: #fff; color: #1c1c1c; border: 1px solid var(--brand-red); }

/* Sayfa kartı ve üst bölüm */
.application-card {
  width: 100%;
  padding: 36px 42px 42px;
  background: #fff;
  border: 1px solid rgba(130, 35, 40, .18);
  border-radius: 16px;
  box-shadow: 0 12px 35px rgba(0, 0, 0, .08);
}
.application-header { display: flex; align-items: center; justify-content: space-between; gap: 20px; margin-bottom: 25px; }
.application-heading { display: flex; align-items: center; gap: 18px; min-width: 0; }
.application-logo { width: 88px; height: 88px; flex-shrink: 0; object-fit: contain; background: #fff; }
.application-header h3 { margin: 0; font-size: 27px; line-height: 1.3; }
.login-link {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  min-height: 38px;
  padding: 8px 20px;
  border: 1px solid var(--brand-red);
  border-radius: 22px;
  background: #fff;
  color: var(--brand-red);
  font-size: 13px;
  font-weight: 600;
  text-decoration: none;
  transition: background-color .2s ease, color .2s ease;
}
.login-link:hover,
.login-link:focus-visible { background: var(--brand-red); color: #fff; }

@media (max-width: 600px) {
  .application-page { padding: 20px 12px !important; }
  .application-card { padding: 25px 20px 30px; border-radius: 12px; }
  .application-header { flex-wrap: wrap; gap: 12px; margin-bottom: 18px; }
  .application-heading { gap: 10px; }
  .application-logo { width: 58px; height: 58px; }
  .application-header h3 { font-size: 20px; }
  .login-link { min-height: 35px; padding: 7px 15px; font-size: 12px; margin-left: auto; }
  .university-options { max-height: 235px; }
  .university-option { padding: 12px; }
  .cv-dropzone.has-file { gap: 9px; padding: 12px; }
  .cv-remove { padding: 5px; }
}

@media (prefers-reduced-motion: reduce) {
  .university-trigger,
  .dropdown-chevron,
  .cv-dropzone,
  .login-link { transition: none; }
}

/* Başvurunun gönderildiğini vurgulayan ekran ortası onay penceresi */
.application-success-overlay {
  position: fixed;
  inset: 0;
  z-index: 5000;
  display: grid;
  place-items: center;
  padding: 18px;
  background: rgba(23, 19, 26, .66);
  backdrop-filter: blur(5px);
  cursor: pointer;
}
.application-success-overlay:focus { outline: none; }
.application-success-panel {
  position: relative;
  isolation: isolate;
  overflow: hidden;
  width: min(100%, 470px);
  max-height: calc(100dvh - 36px);
  overflow-y: auto;
  padding: 41px 36px 29px;
  background: #fff;
  border-radius: 24px;
  box-shadow: 0 26px 75px rgba(0, 0, 0, .27);
  text-align: center;
  color: #282629;
}
.application-success-panel::before {
  content: '';
  position: absolute;
  z-index: -1;
  inset: 0 0 auto;
  height: 8px;
  background: linear-gradient(90deg, #822328, #d15b68, #e6a15d, #822328);
}
.application-success-icon {
  display: grid;
  place-items: center;
  width: 86px;
  height: 86px;
  margin: 0 auto 13px;
  border: 8px solid #e6f5eb;
  border-radius: 50%;
  background: #d6efe0;
  color: #16834f;
  box-shadow: 0 8px 25px rgba(22, 131, 79, .13);
}
.application-success-icon svg { width: 43px; height: 43px; }
.application-success-emoji { font-size: 30px; line-height: 1.3; margin-bottom: 10px; }
.application-success-panel h2 {
  max-width: 360px;
  margin: 0 auto 14px;
  font-size: clamp(22px, 4vw, 27px);
  line-height: 1.25;
  font-weight: 800;
  color: #242124;
}
.application-success-thanks {
  margin: 0 0 7px;
  color: #822328;
  font-size: 16px;
  font-weight: 700;
}
.application-success-description {
  max-width: 365px;
  margin: 0 auto 27px;
  font-size: 15px;
  line-height: 1.65;
  color: #5d5860;
}
.application-success-button {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 100%;
  max-width: 285px;
  min-height: 46px;
  padding: 10px 22px;
  border: 0;
  border-radius: 26px;
  background: #822328;
  color: #fff;
  font-size: 15px;
  font-weight: 700;
  transition: background .18s ease, transform .18s ease;
}
.application-success-button:hover,
.application-success-button:focus-visible { background: #68191e; transform: translateY(-1px); }
.application-success-button:focus-visible { outline: 3px solid rgba(130, 35, 40, .25); outline-offset: 3px; }
.application-success-dismiss {
  margin: 18px 0 0;
  color: #928d93;
  font-size: 11px;
  line-height: 1.5;
}
.application-success-confetti {
  position: absolute;
  pointer-events: none;
  font-weight: 800;
  line-height: 1;
  animation: application-success-float 2.5s ease-in-out infinite alternate;
}
.confetti-a { top: 45px; left: 42px; font-size: 23px; color: #bd5268; }
.confetti-b { top: 96px; right: 39px; font-size: 31px; color: #e5a34a; animation-delay: .4s; }
.confetti-c { top: 173px; left: 28px; font-size: 13px; color: #91bcb6; animation-delay: .8s; }
.confetti-d { top: 222px; right: 33px; font-size: 17px; color: #bc7595; animation-delay: 1s; }
@keyframes application-success-float { from { transform: translateY(0) rotate(-7deg); } to { transform: translateY(-9px) rotate(7deg); } }
.application-success-enter-active,
.application-success-leave-active { transition: opacity .23s ease; }
.application-success-enter-active .application-success-panel,
.application-success-leave-active .application-success-panel { transition: transform .27s ease, opacity .27s ease; }
.application-success-enter-from,
.application-success-leave-to { opacity: 0; }
.application-success-enter-from .application-success-panel,
.application-success-leave-to .application-success-panel { transform: translateY(18px) scale(.95); opacity: 0; }
@media (max-width: 480px) {
  .application-success-panel { padding: 35px 23px 25px; border-radius: 20px; }
  .confetti-a { left: 17px; }
  .confetti-b { right: 14px; }
  .confetti-c { left: 12px; }
  .confetti-d { right: 12px; }
}
@media (prefers-reduced-motion: reduce) {
  .application-success-confetti { animation: none; }
  .application-success-enter-active,
  .application-success-leave-active,
  .application-success-enter-active .application-success-panel,
  .application-success-leave-active .application-success-panel,
  .application-success-button { transition: none; }
}
</style>
