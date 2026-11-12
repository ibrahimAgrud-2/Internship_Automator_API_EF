<script setup>
import { ref, computed, onMounted } from 'vue'
import { getInternConversations } from '@/api/conversations'
import { getErrorMessages } from '@/api/http'
import { getSession } from '@/auth/session'
import AlertMessages from '@/components/AlertMessages.vue'
import '@/assets/css/messages.css'

const session = getSession()

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
    conversations.value = (await getInternConversations(session.internID)) ?? []
  } catch (error) {
    loadError.value = getErrorMessages(error)
  } finally {
    isLoading.value = false
  }
}

function selectConversation(conversationID) {
  selectedConversationID.value = conversationID
}

onMounted(loadConversations)
</script>

<template>
  <div>
    <div class="chat-layout card">
      <!-- Sol: stajyerin dahil olduğu sohbetler -->
      <aside class="chat-list-panel border-end">
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
            @click="selectConversation(conversation.conversationID)"
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

      <!-- Sağ: seçili sohbet (mesajlar bir sonraki adımda) -->
      <section class="chat-panel">
        <div v-if="selectedConversation" class="d-flex align-items-center gap-3 px-3 py-2 border-bottom">
          <span
            class="avatar-circle rounded-circle text-white d-flex align-items-center justify-content-center fw-semibold"
            :class="selectedConversation.isGroup ? 'bg-success' : 'bg-primary'"
          >
            {{ getConversationInitials(selectedConversation.conversationName) }}
          </span>
          <div class="fw-semibold text-break">{{ selectedConversation.conversationName }}</div>
        </div>

        <div class="d-flex flex-grow-1 align-items-center justify-content-center text-muted p-4 text-center">
          {{ selectedConversation ? 'Mesajlar yakında burada olacak.' : 'Bir sohbet seçin.' }}
        </div>
      </section>
    </div>
  </div>
</template>