<script setup>
import { computed } from 'vue'
import { splitTechnologies } from '@/utils/format'

// "C#, EF Core, ASP.NET" -> ayrı ayrı etiketler
const props = defineProps({ technologies: { type: String, default: '' } })

const items = computed(() => splitTechnologies(props.technologies))
</script>

<template>
  <div class="d-flex flex-wrap gap-1">
    <span v-for="(text, i) in items" :key="i" class="badge text-bg-secondary">{{ text }}</span>
  </div>
</template>
