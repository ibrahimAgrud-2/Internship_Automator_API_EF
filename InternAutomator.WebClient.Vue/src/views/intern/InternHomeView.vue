<script setup>
import { getSession } from '@/auth/session'
import { formatDate } from '@/utils/format'

// Rota koruması sayesinde bu sayfaya oturum olmadan gelinmez
const session = getSession()
const fullName = `${session.firstName} ${session.lastName}`.trim()
</script>

<template>
  <div>
    <h4 class="mb-4">Hoş geldin, {{ fullName }}</h4>

    <div class="row g-3">
      <div class="col-sm-6 col-lg-4">
        <div class="card">
          <div class="card-body">
            <div class="text-muted small">Staj başlangıcı</div>
            <div class="fs-5">{{ formatDate(session.startDate, 'Belirlenmedi') }}</div>
          </div>
        </div>
      </div>
      <div class="col-sm-6 col-lg-4">
        <div class="card">
          <div class="card-body">
            <div class="text-muted small">Staj bitişi</div>
            <div class="fs-5">{{ formatDate(session.endDate, 'Belirlenmedi') }}</div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
