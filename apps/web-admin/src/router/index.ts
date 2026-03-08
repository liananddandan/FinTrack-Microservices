import { createRouter, createWebHistory } from "vue-router";
import { useAuthStore } from "../stores/auth";

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: "/login",
      name: "admin-login",
      component: () => import("../views/LoginView.vue"),
      meta: { public: true },
    },
    {
      path: "/",
      component: () => import("../layouts/AdminLayout.vue"),
      children: [
        {
          path: "",
          redirect: "/dashboard",
        },
        {
          path: "/dashboard",
          name: "dashboard",
          component: () => import("../views/DashboardView.vue"),
        },
        {
          path: "/members",
          name: "members",
          component: () => import("../views/MembersView.vue"),
        },
        {
          path: "/transactions",
          name: "transactions",
          component: () => import("../views/TransactionsView.vue"),
        },
        {
          path: "/invitations",
          name: "invitations",
          component: () => import("../views/InvitationsView.vue"),
        },
      ],
    },
  ],
});

router.beforeEach(async (to) => {
  const auth = useAuthStore();

  if (to.meta.public) {
    return true;
  }

  await auth.initialize();

  if (!auth.isAuthenticated) {
    return "/login";
  }

  if (!auth.hasTenantContext) {
    return "/login";
  }

  if (!auth.isAdmin) {
    return "/login";
  }

  return true;
});

export default router;