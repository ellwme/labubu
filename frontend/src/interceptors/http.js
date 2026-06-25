import axios from 'axios'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5093'

export const http = axios.create({
  baseURL: API_BASE_URL,
})

let authTokenGetter = null
let unauthorizedHandler = null
let handlingUnauthorized = false

export function setAuthTokenGetter(getter) {
  authTokenGetter = typeof getter === 'function' ? getter : null
}

export function setUnauthorizedHandler(handler) {
  unauthorizedHandler = typeof handler === 'function' ? handler : null
}

http.interceptors.request.use((config) => {
  const token = authTokenGetter ? authTokenGetter() : ''

  if (token && !config.headers?.Authorization) {
    config.headers = {
      ...config.headers,
      Authorization: `Bearer ${token}`,
    }
  }

  return config
})

http.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error?.response?.status === 401 && unauthorizedHandler && !handlingUnauthorized) {
      handlingUnauthorized = true
      try {
        unauthorizedHandler()
      } finally {
        handlingUnauthorized = false
      }
    }

    return Promise.reject(error)
  }
)

export function getHttpErrorMessage(error, fallback = 'Request failed') {
  const responseData = error?.response?.data

  if (typeof responseData === 'string' && responseData.trim()) {
    return responseData
  }

  if (responseData?.message) {
    return responseData.message
  }

  return error?.message || fallback
}
