import type { ProjectItem } from "../types/project"

export const projects: ProjectItem[] = [
  {
    name: "Multi-Tenant Microservices Platform",
    subtitle: "Full-stack multi-tenant system with API gateway and event-driven architecture",
    modules: [
      {
        name: "Portal",
        description: "User-facing interface",
        href: "https://chenlis.com",
      },
      {
        name: "Admin",
        description: "Management dashboard",
        href: "https://admin.chenlis.com",
      },
      {
        name: "Swagger",
        description: "REST API documentation",
        href: "https://api.chenlis.com/swagger",
      },
    ],
    links: [
      {
        label: "Preview",
        href: "https://chenlis.com",
      },
      {
        label: "GitHub",
        href: "https://github.com/liananddandan/FinTrack-Microservices",
      },
      {
        label: "Blog",
        href: "https://dev.to/alexleeeeeeeeee",
      },
    ],
  },

  {
    name: "ToDoList API",
    subtitle: "ASP.NET Core project with Clean Architecture",
    modules: [
      {
        name: "Portal",
        description: "User-facing interface with Vue",
      },
      {
        name: "API",
        description: "REST API with layered architecture",
      },
    ],
    links: [
      {
        label: "GitHub",
        href: "https://github.com/liananddandan/To-Do-List",
      },
      {
        label: "Blog",
        href: "https://dev.to/alexleeeeeeeeee",
      },
    ],
  },

  {
    name: "AI Knowledge Copilot",
    subtitle: "Internal AI assistant with RAG",
    modules: [
      {
        name: "LLM Gateway",
        description: "ASP.NET Core service for orchestration and tool calling",
      },
      {
        name: "RAG Service",
        description: "FastAPI service for embedding, vector search, and retrieval",
      },
    ],
    links: [
      {
        label: "GitHub",
        href: "https://github.com/liananddandan/RAG_Platform",
      },
    ],
  },
]