<script>
import NavComponent from '@/components/NavComponent.vue'
import HeaderComponent from '@/components/HeaderComponent.vue'
import FooterComponent from '@/components/FooterComponent.vue'
import LoginComponent from '@/components/LoginComponent.vue'

export default {
  name: 'LoginView',
  components: {
    NavComponent,
    HeaderComponent,
    FooterComponent,
    LoginComponent,
  },
}
</script>

<template>
  <NavComponent />
  <HeaderComponent />
  <main>
    <LoginComponent />
  </main>
  <FooterComponent />
</template>
