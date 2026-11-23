<script setup></script>

<template>
  <!-- Router, adrese göre doğru sayfayı buraya koyar.
       Sayfalar arasında yumuşak geçiş: eski sayfa kısa sürede solar, yenisi hafifçe yukarı süzülerek gelir.
       (Geçiş sınıfları theme.css içinde: .page-enter-active vb.) -->
  <RouterView v-slot="{ Component }">
    <Transition name="page" mode="out-in">
      <component :is="Component" />
    </Transition>
  </RouterView>
</template>
