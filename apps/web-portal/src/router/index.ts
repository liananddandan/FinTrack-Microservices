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
    {
      path: "/login",
      name: "login",
      component: LoginView,
      meta: { public: true },
    },
    {
      path: "/register-tenant",
      name: "register-tenant",
      component: RegisterTenantView,
      meta: { public: true },
    },
    {
      path: "/register-user",
      name: "register-user",
      component: RegisterUserView,
      meta: { public: true },
    },
    {
      path: "/invitations/accept",
      name: "accept-invitation",
      component: () => import("../views/AcceptInvitationView.vue"),
      meta: { public: true },
    },
    {
      path: "/waiting-membership",
      name: "waiting-membership",
      component: WaitingMembershipView,
      meta: { requiresAuth: true },
    },
  ],
});

router.beforeEach((to) => {
  const auth = useAuthStore();

  if (to.meta.requiresAuth && !auth.accessToken) {
    return "/login";
  }

  if (to.path === "/login" && auth.accessToken) {
    return "/waiting-membership";
  }

  return true;
});

export default router;