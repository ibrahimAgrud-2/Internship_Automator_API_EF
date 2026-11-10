<script setup>
import { formatDate } from '@/utils/format'

// Tek görev kartı (düz listede de, projeye göre gruplu görünümde de aynı kart).
// Butonlar işlemi kendisi yapmaz; sadece üst sayfaya haber verir.
defineProps({
  task: { type: Object, required: true },
  projectTitle: { type: String, default: '' }, // görev cevabında proje adı yok; üst sayfa verir
  canChange: { type: Boolean, default: true }, // ayrılan stajyerde butonlar pasif (asıl engel API'dadır)
})

defineEmits(['edit', 'complete', 'delete'])
</script>

<template>
  <div class="border rounded p-3">
    <div class="d-flex align-items-start justify-content-between gap-2 mb-1">
      <div class="fw-semibold text-break">{{ task.title }}</div>
      <span class="badge" :class="task.completed ? 'text-bg-success' : 'text-bg-warning'">
        {{ task.completed ? 'Tamamlandı' : 'Devam ediyor' }}
      </span>
    </div>

    <div v-if="projectTitle" class="small text-muted">Proje: {{ projectTitle }}</div>

    <div v-if="task.description" class="small text-break my-2">{{ task.description }}</div>

    <div class="small text-muted">
      Atanma: {{ formatDate(task.assignedDate, '-') }} · Teslim: {{ formatDate(task.dueDate, '-') }}
      <template v-if="task.completed"> · Tamamlanma: {{ formatDate(task.completedDate, '-') }}</template>
    </div>

    <div class="mt-2 d-flex justify-content-end gap-2">
      <!-- "Düzenle" ve "Tamamla" sadece tamamlanmamış görevde çıkar -->
      <template v-if="!task.completed">
        <button
          type="button"
          class="btn btn-sm btn-outline-secondary"
          :disabled="!canChange"
          :title="canChange ? '' : 'Ayrılan stajyerin görevi değiştirilemez'"
          @click="$emit('edit', task)"
        >
          Düzenle
        </button>
        <button
          type="button"
          class="btn btn-sm btn-outline-success"
          :disabled="!canChange"
          :title="canChange ? '' : 'Ayrılan stajyerin görevi değiştirilemez'"
          @click="$emit('complete', task)"
        >
          Tamamla
        </button>
      </template>
      <button
        type="button"
        class="btn btn-sm btn-outline-danger"
        :disabled="!canChange"
        :title="canChange ? '' : 'Ayrılan stajyerin görevi değiştirilemez'"
        @click="$emit('delete', task)"
      >
        Sil
      </button>
    </div>
  </div>
</template>
