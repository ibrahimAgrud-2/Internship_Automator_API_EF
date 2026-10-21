<script setup>
import { computed } from 'vue'
import { INTERN_STATUS } from '@/constants'

const props = defineProps({ status: { type: Number, default: null } })

const info = computed(() => INTERN_STATUS[props.status] ?? { text: 'Bilinmiyor', badge: 'text-bg-light border' })
</script>

<template>
  <span class="badge" :class="info.badge">{{ info.text }}</span>
</template>
