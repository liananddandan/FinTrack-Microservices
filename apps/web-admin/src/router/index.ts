import { createRouter, createWebHistory } from "vue-router";
import { useAuthStore } from "../stores/auth";
import LoginView from "../views/LoginView.vue";
import AdminLayout from "../layouts/AdminLayout.vue";
import MembersView from "../views/MembersView.vue";
import TransactionsView from "../views/TransactionsView.vue";

const routes = [
  { path: "/login", name: "login", component: LoginView },
  {
    path: "/",
    component: AdminLayout,
    children: [
      { path: "", redirect: "/transactions" },
      { path: "members", name: "members", component: MembersView },
      { path: "transactions", name: "transactions", component: TransactionsView },
    ],
  },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

router.beforeEach((to) => {
  const auth = useAuthStore();
  const isAuthed = !!auth.accessToken;

  if (to.path !== "/login" && !isAuthed) return "/login";
  if (to.path === "/login" && isAuthed) return "/transactions";
});

export default router;