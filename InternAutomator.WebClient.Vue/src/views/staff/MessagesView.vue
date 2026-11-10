<script setup>
import '@/assets/css/messages.css'

// Mesajlaşma yeniden yazılıyor. Bu sayfa şimdilik sadece iskelet: API çağrısı yok, tüm kontroller pasif.
// Mesaj balonları, gün ayraçları ve referans kutusu için gereken stiller messages.css içinde duruyor.
</script>

<template>
  <div>
    <h4 class="mb-4">Mesajlar</h4>

    <div class="chat-layout card">
      <!-- Sol: sohbet listesi -->
      <aside class="chat-list-panel border-end">
        <div class="p-3 border-bottom d-grid gap-2">
          <button type="button" class="btn btn-outline-primary" disabled>Yeni sohbet</button>
          <button type="button" class="btn btn-outline-primary" disabled>Grup ekle</button>
        </div>

        <div class="flex-grow-1 overflow-auto">
          <p class="text-muted small text-center p-3 mb-0">Henüz sohbet yok.</p>
        </div>
      </aside>

      <!-- Sağ: sohbet alanı -->
      <section class="chat-panel">
        <div class="d-flex flex-grow-1 align-items-center justify-content-center text-muted p-4 text-center">
          Mesajlaşma yakında burada olacak.
        </div>

        <!-- Mesaj yazma satırı (pasif) -->
        <form class="chat-composer border-top" autocomplete="off" novalidate @submit.prevent>
          <div class="dropup">
            <button type="button" class="btn btn-outline-secondary" aria-label="Ekle" disabled>+</button>
          </div>
          <textarea class="form-control" rows="2" placeholder="Mesajınızı yazın..." aria-label="Mesaj" disabled></textarea>
          <button type="submit" class="btn btn-outline-primary" disabled>Gönder</button>
        </form>
      </section>
    </div>
  </div>
</template>