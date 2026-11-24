<template>
  <main class="home-gateway">
    <section class="home-card">
      <img src="/company-logo.png" alt="AE Yazılım Teknoloji" class="home-logo" />
      <h1>Staj Otomasyonu</h1>
      <div class="home-actions">
        <RouterLink to="/login" class="btn btn-primary">Giriş Yap</RouterLink>
        <RouterLink to="/apply" class="btn btn-outline-primary">Staj Başvurusu Yap</RouterLink>
      </div>
    </section>
  </main>
</template>
<style scoped>
.home-gateway{display:flex;align-items:center;justify-content:center;min-height:100vh;padding:24px;background:radial-gradient(900px 480px at 50% -8%,var(--brand-primary-soft),transparent 70%),var(--surface-canvas)}
.home-card{width:100%;max-width:470px;text-align:center;padding:46px 26px;background:var(--surface);border-radius:20px;border:1px solid var(--border-soft);box-shadow:var(--shadow-lg);animation:rise-in .7s var(--ease-out) backwards}
.home-logo{width:170px;height:170px;object-fit:contain;margin:-14px 0 4px;animation:fade-in 1s .15s ease backwards}.home-card h1{font-size:1.75rem;font-weight:750;color:var(--text-strong);margin-bottom:27px}
.home-actions{display:flex;justify-content:center;gap:10px;flex-wrap:wrap}.home-actions .btn{padding:10px 18px;border-radius:24px}
</style>
