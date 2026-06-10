<template>
  <div class="error-page">
    <v-container>
      <v-row justify="center" align="center" style="min-height: 100vh;">
        <v-col cols="12" md="6" class="text-center">
          <v-icon 
            :size="120" 
            :color="errorColor" 
            class="mb-4"
          >
            {{ errorIcon }}
          </v-icon>
          
          <h1 class="text-h2 font-weight-bold mb-4" :class="errorColor + '--text'">
            {{ error.statusCode }}
          </h1>
          
          <h2 class="text-h4 mb-4">
            {{ errorTitle }}
          </h2>
          
          <p class="text-body-1 text-grey-600 mb-6">
            {{ errorMessage }}
          </p>
          
          <div class="d-flex justify-center gap-4 flex-wrap">
            <v-btn
              color="primary"
              size="large"
              @click="handleError"
            >
              <v-icon start>mdi-refresh</v-icon>
              Try Again
            </v-btn>
            
            <v-btn
              variant="outlined"
              color="primary"
              size="large"
              @click="$router.push('/')"
            >
              <v-icon start>mdi-home</v-icon>
              Go Home
            </v-btn>
          </div>
          
          <!-- Additional help for 403 errors -->
          <div v-if="error.statusCode === 403" class="mt-6">
            <v-alert
              type="info"
              variant="tonal"
              class="text-left"
            >
              <v-alert-title>Need Access?</v-alert-title>
              <p class="mb-2">
                If you believe you should have access to this resource, please contact your administrator.
              </p>
              <v-btn
                variant="text"
                size="small"
                color="info"
                @click="contactSupport"
              >
                Contact Support
              </v-btn>
            </v-alert>
          </div>
        </v-col>
      </v-row>
    </v-container>
  </div>
</template>

<script setup lang="ts">
// Props
const props = defineProps<{
  error: {
    statusCode: number
    statusMessage: string
  }
}>()

// Computed
const errorColor = computed(() => {
  switch (props.error.statusCode) {
    case 403: return 'warning'
    case 404: return 'info'
    case 500: return 'error'
    default: return 'error'
  }
})

const errorIcon = computed(() => {
  switch (props.error.statusCode) {
    case 403: return 'mdi-lock'
    case 404: return 'mdi-file-search'
    case 500: return 'mdi-server-network-off'
    default: return 'mdi-alert-circle'
  }
})

const errorTitle = computed(() => {
  switch (props.error.statusCode) {
    case 403: return 'Access Denied'
    case 404: return 'Page Not Found'
    case 500: return 'Server Error'
    default: return 'Something went wrong'
  }
})

const errorMessage = computed(() => {
  switch (props.error.statusCode) {
    case 403: 
      return 'You don\'t have permission to access this resource. Please check your credentials or contact an administrator.'
    case 404: 
      return 'The page you\'re looking for doesn\'t exist. It might have been moved, deleted, or you entered the wrong URL.'
    case 500: 
      return 'An internal server error occurred. Our team has been notified and is working to fix this issue.'
    default: 
      return props.error.statusMessage || 'An unexpected error occurred. Please try again later.'
  }
})

// Methods
const handleError = () => {
  clearError({ redirect: '/' })
}

const contactSupport = () => {
  // You can implement email mailto or support ticket system
  window.location.href = 'mailto:support@jtwbaseauth.com?subject=Access Request&body=I need access to a restricted resource.'
}


</script>

<style scoped>
.error-page {
  background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
  min-height: 100vh;
}

.gap-4 {
  gap: 1rem;
}
</style> 