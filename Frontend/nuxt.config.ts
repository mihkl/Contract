// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  compatibilityDate: "2024-04-03",
  devtools: { enabled: true },
  modules: ["@nuxt/ui", "@nuxtjs/tailwindcss"],
  runtimeConfig: {
    public: {
      apiBaseUrl: "http://localhost:8080/api/",
    },
  },
});