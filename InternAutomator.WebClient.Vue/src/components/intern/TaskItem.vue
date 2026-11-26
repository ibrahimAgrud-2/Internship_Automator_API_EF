<script setup>
import { formatDate } from '@/utils/format'

function isOverdue(task) {
  if(task.completed || !task.dueDate) return false
  const date=new Date(task.dueDate); if(Number.isNaN(date.getTime())) return false
  const today=new Date(); today.setHours(0,0,0,0)
  date.setHours(0,0,0,0)
  return date < today
}

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
  <!-- Kompakt görev satırı: bilgiler solda, işlem butonları sağda tek sütunda -->
  <div class="staff-task" :class="{ 'is-overdue': isOverdue(task), 'is-done': task.completed }">
    <div class="staff-task-main">
      <div class="staff-task-head">
        <span class="staff-task-title text-break">{{ task.title }}</span>
        <span class="staff-task-status" :class="task.completed ? 'is-done' : isOverdue(task) ? 'is-overdue' : 'is-active'">
          {{ task.completed ? 'Tamamlandı' : isOverdue(task) ? 'Süresi geçti' : 'Devam ediyor' }}
        </span>
      </div>
      <div v-if="projectTitle" class="staff-task-project text-truncate" :title="projectTitle">{{ projectTitle }}</div>
      <div v-if="task.description" class="staff-task-desc text-break">{{ task.description }}</div>
      <div class="staff-task-meta">
        <span>Atanma <strong>{{ formatDate(task.assignedDate, '-') }}</strong></span>
        <span>Teslim <strong>{{ formatDate(task.dueDate, '-') }}</strong></span>
        <span v-if="task.completed">Tamamlanma <strong>{{ formatDate(task.completedDate, '-') }}</strong></span>
      </div>
    </div>

    <div class="staff-task-actions" role="group" :aria-label="`${task.title} için işlemler`">
      <!-- "Düzenle" ve "Tamamla" sadece tamamlanmamış görevde çıkar -->
      <template v-if="!task.completed">
        <button
          type="button"
          class="staff-task-btn"
          :disabled="!canChange"
          :title="canChange ? 'Düzenle' : 'Ayrılan stajyerin görevi değiştirilemez'"
          aria-label="Görevi düzenle"
          @click="$emit('edit', task)"
        >
          <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M12 20h9" /><path d="M16.5 3.5a2.12 2.12 0 0 1 3 3L9 17l-4 1 1-4z" /></svg>
        </button>
        <button
          type="button"
          class="staff-task-btn is-success"
          :disabled="!canChange"
          :title="canChange ? 'Tamamla' : 'Ayrılan stajyerin görevi değiştirilemez'"
          aria-label="Görevi tamamla"
          @click="$emit('complete', task)"
        >
          <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M20 6 9 17l-5-5" /></svg>
        </button>
      </template>
      <button
        type="button"
        class="staff-task-btn is-danger"
        :disabled="!canChange"
        :title="canChange ? 'Sil' : 'Ayrılan stajyerin görevi değiştirilemez'"
        aria-label="Görevi sil"
        @click="$emit('delete', task)"
      >
        <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M3 6h18" /><path d="M19 6l-1 14H6L5 6m3 0V4h8v2" /></svg>
      </button>
    </div>
  </div>
</template>

<style scoped>
.staff-task{display:flex;gap:10px;padding:10px 10px 10px 13px;border:1px solid var(--border-soft);border-radius:11px;background:var(--surface);transition:box-shadow var(--dur) var(--ease-out),border-color var(--dur) ease}
.staff-task:hover{box-shadow:var(--shadow-sm);border-color:var(--brand-primary-border)}
.staff-task.is-overdue{border-left:3px solid var(--danger);background:#FFFBFA}
.staff-task.is-done .staff-task-main{opacity:.7}
.staff-task-main{flex:1;min-width:0}
.staff-task-head{display:flex;align-items:flex-start;justify-content:space-between;gap:8px}
.staff-task-title{font-size:.92rem;font-weight:650;line-height:1.35;color:var(--text-strong)}
.staff-task-status{flex-shrink:0;padding:2px 8px;border-radius:999px;font-size:.68rem;font-weight:650;white-space:nowrap}
.staff-task-status.is-active{background:var(--accent-soft);color:#7A5416}
.staff-task-status.is-overdue{background:var(--danger-soft);color:#8A2325}
.staff-task-status.is-done{background:var(--success-soft);color:var(--success)}
.staff-task-project{margin-top:1px;font-size:.75rem;color:var(--text-muted)}
.staff-task-desc{margin-top:4px;font-size:.82rem;line-height:1.45;color:var(--text-body);white-space:pre-wrap}
.staff-task-meta{display:flex;flex-wrap:wrap;gap:2px 12px;margin-top:5px;font-size:.72rem;color:var(--text-muted)}
.staff-task-meta strong{font-weight:600;color:var(--text-body)}
.staff-task-actions{display:flex;gap:4px;flex-shrink:0;align-self:flex-start}
.staff-task-btn{display:grid;place-items:center;width:28px;height:28px;padding:0;border:1px solid var(--border-soft);border-radius:8px;background:var(--surface);color:var(--text-muted);transition:background-color var(--dur-fast) ease,color var(--dur-fast) ease,border-color var(--dur-fast) ease}
.staff-task-btn:hover:not(:disabled){background:var(--brand-secondary-soft);color:var(--text-strong);border-color:var(--brand-secondary-border)}
.staff-task-btn.is-success:hover:not(:disabled){background:var(--success-soft);color:var(--success);border-color:#BCD8C7}
.staff-task-btn.is-danger:hover:not(:disabled){background:var(--danger-soft);color:var(--danger);border-color:#F0C4C2}
.staff-task-btn:disabled{opacity:.4;cursor:not-allowed}
</style>
