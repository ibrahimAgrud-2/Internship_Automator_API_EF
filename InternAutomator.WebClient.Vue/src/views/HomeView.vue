<template>
  <main class="home-gateway">
    <section class="home-card">
      <img src="/company-logo.jpg" alt="AE Yazılım Teknoloji" class="home-logo" />
      <h1>Staj Otomasyonu</h1>
      <div class="home-actions">
        <RouterLink to="/login" class="btn btn-primary">Giriş Yap</RouterLink>
        <RouterLink to="/apply" class="btn btn-outline-primary">Staj Başvurusu Yap</RouterLink>
      </div>
    </section>
  </main>
</template>
<style scoped>
.home-gateway{display:flex;align-items:center;justify-content:center;min-height:100vh;padding:24px;background:#f6f6f6}
.home-card{width:100%;max-width:470px;text-align:center;padding:46px 26px;background:#fff;border-radius:18px;border:1px solid #efe5e5;box-shadow:0 16px 40px rgba(0,0,0,.1)}
.home-logo{width:126px;height:114px;object-fit:contain;margin-bottom:23px}.home-card h1{font-size:1.75rem;font-weight:750;color:#222;margin-bottom:27px}
.home-actions{display:flex;justify-content:center;gap:10px;flex-wrap:wrap}.home-actions .btn{padding:10px 18px;border-radius:24px}
</style>
