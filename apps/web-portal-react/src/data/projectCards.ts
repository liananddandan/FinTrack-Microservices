import type { ProjectCard } from "../types/projectCard"

export const projectCards: ProjectCard[] = [
    {
        name: "Multi-Tenant Microservices Platform",
        subtitle: "Full-stack multi-tenant system with API gateway and event-driven architecture",
        summary:
            "Designed and built a multi-tenant backend system with clear service boundaries and scalable architecture.",
        highlights: [
            "Designed a scalable multi-tenant architecture",
            "Implemented JWT authentication via API gateway",
            "Enabled event-driven communication using RabbitMQ",
            "Deployed portal, admin panel, and backend APIs",
            "Built frontend portal and admin panel using React",
        ],
        actions: [
            {
                label: "Live App",
                href: "https://chenlis.com",
                primary: true,
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
        subtitle: "ASP.NET Core project following Clean Architecture",
        summary:
            "Built a full-stack application, covering API design, authentication, and frontend integration.",
        highlights: [
            "Structured the project using Clean Architecture",
            "Implemented JWT-based authentication",
            "RESTful API design",
            "Unit & integration testing with a TDD approach",
        ],
        actions: [
            {
                label: "View Source",
                href: "https://github.com/liananddandan/To-Do-List",
                primary: true,
            },
            {
                label: "Blog",
                href: "https://dev.to/alexleeeeeeeeee",
            },
        ],
    },
    {
        name: "AI Knowledge Copilot",
        subtitle: "RAG-based assistant with .NET backend and FastAPI service",
        summary:
            "Built an AI assistant with RAG, combining a .NET backend with a FastAPI RAG service.",
        highlights: [
            "LLM orchestration in ASP.NET Core",
            "RAG service with vector-based retrieval",
            "Enabled structured memory handling",
        ],
        actions: [
            {
                label: "View Source",
                href: "https://github.com/liananddandan/RAG_Platform",
                primary: true,
            },
        ],
    },
    {
        name: "EF Core In-Memory Provider",
        subtitle: "Built a custom EF Core in-memory provider, reimplementing the query and execution pipeline",
        summary:
            "Built a custom EF Core provider from scratch, implementing query translation, execution, and integration with EF Core tracking.",
        highlights: [
            "Implemented custom query translation pipeline",
            "Integrated with EF Core tracking and identity resolution",
        ],
        actions: [
            {
                label: "View Source",
                href: "https://github.com/liananddandan/InMemoryProviderForEFCore",
                primary: true,
            },
        ],
    },
]