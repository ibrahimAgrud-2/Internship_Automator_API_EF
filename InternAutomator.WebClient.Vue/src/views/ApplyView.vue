<script setup>
import { reactive, ref } from 'vue'
import { submitApplication } from '@/api/applications'
import { getErrorMessages } from '@/api/http'
import AlertMessages from '@/components/AlertMessages.vue'

// Form alanları. Başarıdan sonra bu başlangıç hâline döner.
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
const message = ref(null) // { type, lines } ya da null

// Opsiyonel alan boşsa boş metin yerine null gönder
function textOrNull(value) {
  const text = value.trim()
  return text === '' ? null : text
}

// Formdaki değerleri API'nın beklediği JSON şekline çevirir
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
  // İstek sürerken ikinci gönderim yapılmasın
  if (isSubmitting.value) return

  message.value = null
  isSubmitting.value = true

  try {
    await submitApplication(buildApplication())
    message.value = { type: 'success', lines: ['Başvurunuz alındı. Teşekkür ederiz!'] }
    Object.assign(form, emptyForm())
  } catch (error) {
    message.value = { type: 'danger', lines: getErrorMessages(error) }
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <main class="container py-4" style="max-width: 640px">
    <h3 class="mb-4">Staj Başvuru Formu</h3>

    <!-- Tarayıcının kendi doğrulaması (required, e-posta biçimi) çalışır -->
    <form @submit.prevent="submit">
      <h5 class="mb-3">Kişisel Bilgiler</h5>
      <div class="row">
        <div class="col-md-6 mb-3">
          <label for="firstName" class="form-label">Ad</label>
          <input id="firstName" v-model="form.firstName" type="text" class="form-control" maxlength="20" required />
        </div>
        <div class="col-md-6 mb-3">
          <label for="lastName" class="form-label">Soyad</label>
          <input id="lastName" v-model="form.lastName" type="text" class="form-control" maxlength="20" required />
        </div>
      </div>

      <div class="mb-3">
        <label for="email" class="form-label">E-posta</label>
        <input id="email" v-model="form.email" type="email" class="form-control" maxlength="50" required />
      </div>

      <div class="mb-3">
        <label for="phone" class="form-label">Telefon</label>
        <input id="phone" v-model="form.phone" type="tel" class="form-control" maxlength="20" required />
      </div>

      <div class="mb-4">
        <label for="address" class="form-label">Adres <span class="text-muted">(opsiyonel)</span></label>
        <textarea id="address" v-model="form.address" class="form-control" rows="2" maxlength="250"></textarea>
      </div>

      <h5 class="mb-3">Eğitim Bilgileri</h5>
      <div class="mb-3">
        <label for="university" class="form-label">Üniversite</label>
        <input id="university" v-model="form.university" type="text" class="form-control" maxlength="30" required />
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

      <h5 class="mt-3 mb-3">Bağlantılar <span class="text-muted fs-6">(opsiyonel)</span></h5>
      <div class="mb-3">
        <label for="linkedinUrl" class="form-label">LinkedIn</label>
        <input id="linkedinUrl" v-model="form.linkedinUrl" type="url" class="form-control" maxlength="50" />
      </div>
      <div class="mb-4">
        <label for="githubUrl" class="form-label">GitHub</label>
        <input id="githubUrl" v-model="form.githubUrl" type="url" class="form-control" maxlength="50" />
      </div>

      <button type="submit" class="btn btn-primary" :disabled="isSubmitting">Başvur</button>

      <AlertMessages v-if="message" :type="message.type" :messages="message.lines" class="mt-3" />
    </form>
  </main>
</template>
