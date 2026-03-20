import {
    SiDotnet,
    SiRedis,
    SiRabbitmq,
    SiMysql,
    SiReact,
    SiVuedotjs,
    SiDocker,
    SiGithub,
    SiTypescript,
} from "react-icons/si"
import type { SkillGroup } from "../types/skill"

export const skills: SkillGroup[] = [
    {
        title: "Backend",
        items: [
            {
                name: ".NET",
                icon: <SiDotnet />,
                description: "Built APIs and microservices with clean architecture",
            },
            {
                name: "Redis",
                icon: <SiRedis />,
                description: "Used for caching and token version control",
            },
            {
                name: "RabbitMQ",
                icon: <SiRabbitmq />,
                description: "Used for event-driven communication between services",
            },
            {
                name: "MySQL / PostgreSQL",
                icon: <SiMysql />,
                description: "Designed relational data models and persistence layers",
            },
            {
                name: "Docker",
                icon: <SiDocker />,
                description: "Containerized services for local development and deployment",
            },
            {
                name: "CI/CD",
                icon: <SiGithub />,
                description: "Automated build and test workflows using GitHub Actions",
            }
        ],
    },
    {
        title: "Frontend",
        items: [
            {
                name: "React",
                icon: <SiReact />,
                description: "Built user interfaces and dashboards",
            },
            {
                name: "Vue",
                icon: <SiVuedotjs />,
                description: "Developed frontend applications",
            },
            {
                name: "TypeScript",
                icon: <SiTypescript />,
                description: "Used for type-safe frontend development and API integration",
            }
        ],
    },
]