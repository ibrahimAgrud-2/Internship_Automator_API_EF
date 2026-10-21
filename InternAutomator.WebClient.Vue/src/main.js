import { createApp } from 'vue'
import App from './App.vue'
import router from './router'

// Bootstrap: stiller ve (modal, dropdown gibi bileşenler için) JavaScript
import 'bootstrap/dist/css/bootstrap.min.css'
import 'bootstrap' // tek kopya: BaseModal'daki import { Modal } ile aynı Bootstrap örneği kullanılır

const app = createApp(App)

app.use(router)

app.mount('#app')
