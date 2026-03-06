import { createRouter, createWebHistory } from "vue-router";
import LoginView from "../views/LoginView.vue";
import RegisterTenantView from "../views/RegisterTenantView.vue";
import RegisterUserView from "../views/RegisterUserView.vue";
import WaitingMembershipView from "../views/WaitingMembershipView.vue";
import { useAuthStore } from "../stores/auth";

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: "/", redirect: "/login" },
    { path: "/login", name: "login", component: LoginView },
    { path: "/register-tenant", name: "register-tenant", component: RegisterTenantView },
    { path: "/register-user", name: "register-user", component: RegisterUserView },
    { path: "/waiting-membership", name: "waiting-membership", component: WaitingMembershipView },
  ],
});

router.beforeEach((to) => {
  const auth = useAuthStore();

  if (to.path === "/waiting-membership" && !auth.accessToken) {
    return "/login";
  }

  if (to.path === "/login" && auth.accessToken) {
    return "/waiting-membership";
  }
});

export default router;