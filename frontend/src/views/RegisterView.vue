<script>
import NavComponent from '@/components/NavComponent.vue'
import HeaderComponent from '@/components/HeaderComponent.vue'
import FooterComponent from '@/components/FooterComponent.vue'
import RegisterComponent from '@/components/RegisterComponent.vue'

export default {
  name: 'RegisterView',
  components: {
    NavComponent,
    HeaderComponent,
    FooterComponent,
    RegisterComponent,
  },
}
</script>

<template>
  <NavComponent />
  <HeaderComponent />
  <main>
    <RegisterComponent />
  </main>
  <FooterComponent />
</template>
