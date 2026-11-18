<script setup>
import { computed } from 'vue'
import { INTERN_STATUS } from '@/constants'

const props = defineProps({ isActive: { type: Boolean, default: null } })

const info = computed(() => INTERN_STATUS[props.isActive] ?? { text: 'Bilinmiyor', badge: 'text-bg-light border' })
</script>

<template>
  <span class="badge" :class="info.badge">{{ info.text }}</span>
</template>
