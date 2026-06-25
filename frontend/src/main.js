import { createApp } from 'vue'
import { createPinia } from 'pinia'

import App from './App.vue'
import router from './router'
import { useAuthStore } from '@/stores/auth'
import { setAuthTokenGetter, setUnauthorizedHandler } from '@/interceptors/http.js'
import AOS from 'aos'

import 'bootswatch/dist/brite/bootstrap.min.css'
import 'bootstrap/dist/js/bootstrap.bundle.min.js'
import '@fortawesome/fontawesome-free/css/all.min.css'
import 'aos/dist/aos.css'

const app = createApp(App)
const pinia = createPinia()

app.use(AOS)
app.use(pinia)
app.use(router)

const authStore = useAuthStore(pinia)

setAuthTokenGetter(() => authStore.token)
setUnauthorizedHandler(() => {
  const currentRoute = router.currentRoute.value

  if (authStore.isAuthenticated) {
    authStore.logout()
  }

  if (currentRoute.path !== '/login' && currentRoute.path !== '/register') {
    router.push({
      path: '/login',
      query: {
        redirect: currentRoute.fullPath,
      },
    })
  }
})

AOS.init({ once: true })
await authStore.initialize()

app.mount('#app')
