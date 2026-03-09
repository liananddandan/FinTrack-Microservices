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
          redirect: "/admin/overview",
        },
        {
          path: "admin/overview",
          name: "admin-overview",
          component: () => import("../views/OverviewView.vue"),
        },
        {
          path: "admin/members",
          name: "members",
          component: () => import("../views/MembersView.vue"),
        },
        {
          path: "admin/transactions",
          name: "transactions",
          component: () => import("../views/TransactionsView.vue"),
        },
        {
          path: "admin/transactions/:transactionPublicId",
          name: "admin-transaction-detail",
          component: () => import("../views/TransactionDetailView.vue"),
        },
        {
          path: "admin/invitations",
          name: "invitations",
          component: () => import("../views/InvitationsView.vue"),
        },
        {
          path: "admin/audit-logs",
          name: "audit-logs",
          component: () => import("../views/AuditLogsView.vue"),
        }
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