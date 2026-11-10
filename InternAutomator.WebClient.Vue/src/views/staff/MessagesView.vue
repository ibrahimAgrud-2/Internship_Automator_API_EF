<script setup>
import { ref, computed, onMounted } from 'vue'
import { getAllConversations } from '@/api/conversations'
import { getErrorMessages } from '@/api/http'
import AlertMessages from '@/components/AlertMessages.vue'
import '@/assets/css/messages.css'

const conversations = ref([])
const isLoading = ref(true)
const loadError = ref([])
const selectedConversationID = ref(null)

const selectedConversation = computed(
  () => conversations.value.find((conversation) => conversation.conversationID === selectedConversationID.value) ?? null,
)

// "stajyerler Ortak Grup" -> "SO"
function getConversationInitials(name) {
  return (name ?? '')
    .trim()
    .split(/\s+/)
    .slice(0, 2)
    .map((word) => word.charAt(0))
    .join('')
    .toLocaleUpperCase('tr-TR')
}

async function loadConversations() {
  isLoading.value = true
  loadError.value = []

  try {
    conversations.value = (await getAllConversations()) ?? []
  } catch (error) {
    loadError.value = getErrorMessages(error)
  } finally {
    isLoading.value = false
  }
}

onMounted(loadConversations)
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
          <AlertMessages v-if="isLoading" class="m-3" type="info" messages="Sohbetler yükleniyor..." />
          <AlertMessages v-else-if="loadError.length" class="m-3" :messages="loadError" />
          <p v-else-if="conversations.length === 0" class="text-muted small text-center p-3 mb-0">Henüz sohbet yok.</p>

          <button
            v-for="conversation in conversations"
            :key="conversation.conversationID"
            type="button"
            class="chat-list-item"
            :class="{ active: conversation.conversationID === selectedConversationID }"
            @click="selectedConversationID = conversation.conversationID"
          >
            <span
              class="avatar-circle rounded-circle text-white d-flex align-items-center justify-content-center fw-semibold"
              :class="conversation.isGroup ? 'bg-success' : 'bg-primary'"
            >
              {{ getConversationInitials(conversation.conversationName) }}
            </span>
            <span class="chat-list-text">
              <span class="fw-semibold text-truncate d-block">{{ conversation.conversationName }}</span>
              <span class="chat-preview d-block small text-muted">{{ conversation.isGroup ? 'Grup sohbeti' : 'Birebir sohbet' }}</span>
            </span>
          </button>
        </div>
      </aside>

      <!-- Sağ: seçili sohbet -->
      <section class="chat-panel">
        <div v-if="selectedConversation" class="d-flex align-items-center gap-3 p-3 border-bottom">
          <span
            class="avatar-circle rounded-circle text-white d-flex align-items-center justify-content-center fw-semibold"
            :class="selectedConversation.isGroup ? 'bg-success' : 'bg-primary'"
          >
            {{ getConversationInitials(selectedConversation.conversationName) }}
          </span>
          <div class="fw-semibold text-break">{{ selectedConversation.conversationName }}</div>
        </div>

        <div class="d-flex flex-grow-1 align-items-center justify-content-center text-muted p-4 text-center">
          {{ selectedConversation ? 'Mesajlar bir sonraki adımda burada görünecek.' : 'Soldan bir sohbet seçin.' }}
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