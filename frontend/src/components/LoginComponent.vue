<script>
import { useAuthStore } from '@/stores/auth'
import { getHttpErrorMessage } from '@/interceptors/http'

export default {
  name: 'LoginComponent',
  data() {
    return {
      authStore: useAuthStore(),
      form: { email: '', password: '' },
      loading: false,
      error: '',
    }
  },
  methods: {
    async submit() {
      this.loading = true
      this.error = ''

      try {
        await this.authStore.login(this.form)
        const redirect = typeof this.$route.query.redirect === 'string' ? this.$route.query.redirect : '/main'
        this.$router.push(redirect)
      } catch (e) {
        this.error = getHttpErrorMessage(e, 'Помилка входу.')
      } finally {
        this.loading = false
      }
    },
    openRegister() {
      this.$router.replace({ path: '/register', query: { ...this.$route.query } })
    },
  },
}
</script>

<template>
  <main class="container py-5" style="max-width: 560px;">
    <div class="card shadow-sm">
      <div class="card-body p-4">
        <h3 class="mb-3 text-center">Вхід</h3>

        <form @submit.prevent="submit">
          <div class="mb-3">
            <label for="email" class="form-label">Email</label>
            <input id="email" v-model="form.email" type="email" class="form-control" required>
          </div>

          <div class="mb-3">
            <label for="password" class="form-label">Пароль</label>
            <input id="password" v-model="form.password" type="password" class="form-control" minlength="6" required>
          </div>

          <div v-if="error" class="alert alert-danger">{{ error }}</div>

          <button type="submit" class="btn btn-primary w-100" :disabled="loading">
            {{ loading ? 'Зачекайте...' : 'Увійти' }}
          </button>
        </form>

        <div class="text-center mt-3">
          <button class="btn btn-link p-0" type="button" @click="openRegister">
            Немає акаунта? Зареєструватися
          </button>
        </div>
      </div>
    </div>
  </main>
</template>

<style scoped>
</style>
