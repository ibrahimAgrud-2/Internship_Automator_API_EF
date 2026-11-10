<script setup>
import { computed } from 'vue'

// Bootstrap uyarı kutusu. messages: metin ya da metin dizisi (API hataları çok satırlı olabilir).
// type: success | danger | info | secondary | warning ...
const props = defineProps({
  messages: { type: [String, Array], required: true },
  type: { type: String, default: 'danger' },
  role: { type: String, default: 'alert' },
})

const lines = computed(() => (Array.isArray(props.messages) ? props.messages : [props.messages]))
</script>

<template>
  <div class="alert" :class="`alert-${type}`" :role="role">
    <div v-for="(line, i) in lines" :key="i">{{ line }}</div>
  </div>
</template>
