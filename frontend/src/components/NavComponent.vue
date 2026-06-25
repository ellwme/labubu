<script setup>
import { useAuthStore } from '@/stores/auth'
import { useRouter } from 'vue-router'

const auth = useAuthStore()
const router = useRouter()

function logout() {
  auth.logout()
  router.push('/login')
}
</script>

<template>
  <div data-aos="flip-down" class="container text-center">
    <nav class="navbar navbar-expand-sm m-3 border-0">
      <router-link class="navbar-brand d-flex flex-row" to="/home">
        <i class="fa-solid fa-paw fa-beat me-3"></i>
        <h4 class="font-monospace">labubu</h4>
      </router-link>
      <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarSupportedContent" aria-controls="navbarSupportedContent" aria-expanded="false" aria-label="Toggle navigation">
      <span class="navbar-toggler-icon"></span>
      </button>
      <div class="collapse navbar-collapse" id="navbarSupportedContent">
        <ul class="navbar-nav mx-auto mb-2 mb-lg-0">
          <li class="nav-item">
            <router-link class="nav-link" aria-current="page" to="/"><i class="fa-solid fa-house me-1"></i>Land</router-link>
          </li>
          <li class="nav-item">
            <router-link class="nav-link" aria-current="page" to="/main"><i class="fa-solid fa-star me-1"></i>BlindBox</router-link>
          </li>
        <li class="nav-item">
            <router-link class="nav-link" aria-current="page" to="/about"><i class="fa-solid fa-info-circle me-1"></i>About</router-link>
          </li>
        </ul>
        <ul class="navbar-nav mb-2 mb-lg-0">
          <template v-if="auth.isAuthenticated">
            <li class="nav-item">
              <span class="nav-link"><i class="fa-solid fa-user me-1"></i>{{ auth.user?.name || auth.user?.email }}</span>
            </li>
            <li class="nav-item">
              <a class="nav-link" href="#" role="button" @click.prevent="logout"><i class="fa-solid fa-right-from-bracket me-1"></i>Вийти</a>
            </li>
          </template>
          <template v-else>
            <li class="nav-item">
              <router-link class="nav-link" to="/login"><i class="fa-solid fa-right-to-bracket me-1"></i>Вхід</router-link>
            </li>
            <li class="nav-item">
              <router-link class="nav-link" to="/register"><i class="fa-solid fa-user-plus me-1"></i>Реєстрація</router-link>
            </li>
          </template>
        </ul>
      </div>
    </nav>
  </div>
</template>

<style scoped>

</style>
