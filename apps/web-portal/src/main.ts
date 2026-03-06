import { createApp } from "vue";
import { createPinia } from "pinia";
import ElementPlus from "element-plus";
import "element-plus/dist/index.css";
import "./assets/portal.css";

import App from "./App.vue";
import router from "./router";
import { useAuthStore } from "./stores/auth";
import { getCurrentUser } from "./api/account";

const app = createApp(App);
const pinia = createPinia();

app.use(pinia);
app.use(router);
app.use(ElementPlus);
app.mount("#app");

const auth = useAuthStore();

if (auth.accessToken) {
  getCurrentUser()
    .then((profile) => {
      auth.setProfile(profile);
    })
    .catch((error) => {
      console.error("Failed to restore current user profile:", error);
      auth.logout();
    });
}