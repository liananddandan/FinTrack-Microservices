import { createRouter, createWebHistory } from "vue-router";
import LoginView from "../views/LoginView.vue";
import RegisterTenantView from "../views/RegisterTenantView.vue";
import RegisterUserView from "../views/RegisterUserView.vue";
import WaitingMembershipView from "../views/WaitingMembershipView.vue";
import HomeView from "../views/HomeView.vue";
import { useAuthStore } from "../stores/auth";

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: "/",
      redirect: "/login",
    },
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
    {
      path: "/home",
      name: "home",
      component: HomeView,
      meta: { requiresAuth: true, requiresTenant: true },
    },
    {
      path: "/donate",
      name: "donate",
      component: () => import("../views/DonationView.vue"),
      meta: { requiresAuth: true, requiresTenant: true },
    },
    {
      path: "/procurements/new",
      name: "procurement-create",
      component: () => import("../views/ProcurementDraftView.vue"),
      meta: { requiresAuth: true, requiresTenant: true },
    },
    {
      path: "/my-transactions",
      name: "my-transactions",
      component: () => import("../views/MyTransactionsView.vue"),
      meta: { requiresAuth: true, requiresTenant: true },
    },
    {
      path: "/transactions/:transactionPublicId",
      name: "transaction-detail",
      component: () => import("../views/TransactionDetailView.vue"),
      meta: { requiresAuth: true, requiresTenant: true },
    },
  ],
});

router.beforeEach(async (to) => {
  const auth = useAuthStore();

  if (to.meta.public) {
    return true;
  }

  await auth.initialize();

  if (to.path === "/login" && auth.accountAccessToken) {
    return auth.tenantAccessToken ? "/home" : "/waiting-membership";
  }

  if (to.meta.requiresAuth && !auth.accountAccessToken) {
    return "/login";
  }

  if (to.meta.requiresTenant && !auth.tenantAccessToken) {
    return "/waiting-membership";
  }

  return true;
});

export default router;