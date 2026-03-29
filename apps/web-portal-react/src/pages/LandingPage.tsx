import {
    HiOutlineArrowsRightLeft,
    HiOutlineUser,
    HiOutlineCodeBracket,
    HiOutlineServerStack,
    HiOutlineCube,
    HiOutlineCircleStack,
    HiOutlineBeaker,
    HiOutlineCloudArrowUp,
    HiOutlineArrowDownTray,
    HiOutlineRocketLaunch,
    HiOutlineBuildingStorefront,
} from "react-icons/hi2"
import {
    SiDotnet,
    SiMysql,
    SiRedis,
    SiRabbitmq,
    SiDocker,
    SiGithubactions,
    SiReact,
} from "react-icons/si"

function IconBadge({ children }: { children: React.ReactNode }) {
    return (
        <div className="flex h-11 w-11 items-center justify-center rounded-xl border border-slate-200 bg-white text-indigo-600 shadow-sm">
            {children}
        </div>
    )
}

function ArchitectureNode({
    icon,
    label,
}: {
    icon: React.ReactNode
    label: string
}) {
    return (
        <div className="flex flex-col items-center">
            <IconBadge>{icon}</IconBadge>
            <p className="mt-2 text-sm text-slate-700">{label}</p>
        </div>
    )
}

function ArchitectureArrow() {
    return <span className="text-lg text-slate-300">→</span>
}

function TechItem({
    icon,
    label,
}: {
    icon: React.ReactNode
    label: string
}) {
    return (
        <div className="flex flex-col items-center gap-2">
            <IconBadge>{icon}</IconBadge>
            <p className="text-xs text-slate-600">{label}</p>
        </div>
    )
}

function PipelineStep({
    icon,
    label,
}: {
    icon: React.ReactNode
    label: string
}) {
    return (
        <div className="flex items-center gap-3">
            <IconBadge>{icon}</IconBadge>
            <span className="text-sm font-medium text-slate-700">{label}</span>
        </div>
    )
}

const APP_LINKS = {
    aucklandCoffee: {
        portal: "https://coffee.chenlis.com/portal/login",
        admin: "https://coffee.chenlis.com/admin/login",
    },
    sushiBar: {
        portal: "https://sushi.chenlis.com/portal/login",
        admin: "https://sushi.chenlis.com/admin/login",
    },
    apiDocs: "https://fintrack.chenlis.com/api/swagger",
}

export default function LandingPage() {
    return (
        <main className="bg-white text-gray-900">
            <section className="px-4 py-4">
                <div className="mx-auto max-w-5xl">
                    <div className="flex items-center gap-3">
                        <IconBadge>
                            <HiOutlineBuildingStorefront className="h-6 w-6" />
                        </IconBadge>

                        <div>
                            <p className="text-sm font-medium tracking-wide text-gray-500">
                                Multi-Tenant Retail Operations Platform
                            </p>
                            <p className="text-xs text-gray-400">
                                Full-stack system for orders, products, and tenant-based business operations
                            </p>
                        </div>
                    </div>
                </div>
            </section>

            <section className="border-t border-slate-200 bg-slate-100 py-10">
                <div className="mx-auto max-w-5xl">
                    <div className="max-w-3xl">
                        <h1 className="text-4xl font-semibold leading-tight tracking-tight text-slate-800 sm:text-5xl">
                            Multi-tenant retail operations system
                        </h1>

                        <p className="mt-6 text-lg leading-8 text-slate-600">
                            A production-style system for managing menu data, orders, tenant access,
                            and administrative workflows, built with clean architecture and event-driven design.
                        </p>
                    </div>

                    <div className="mt-10 grid gap-4 md:grid-cols-3">
                        <div className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm">
                            <div className="flex items-center gap-2">
                                <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-amber-100 text-amber-700">
                                    <HiOutlineBeaker className="h-4 w-4" />
                                </div>

                                <div>
                                    <p className="text-[11px] font-medium uppercase tracking-wide text-slate-400">
                                        Tenant
                                    </p>
                                    <h3 className="text-sm font-semibold text-slate-800">
                                        Auckland Coffee
                                    </h3>
                                </div>
                            </div>

                            <div className="mt-4 grid grid-cols-2 gap-2">
                                <a
                                    href={APP_LINKS.aucklandCoffee.portal}
                                    className="group rounded-xl border border-slate-200 bg-slate-50 px-4 py-3 transition hover:border-indigo-300 hover:bg-indigo-50"
                                >
                                    <div className="flex items-center justify-between">
                                        <span className="text-sm font-semibold text-slate-800 group-hover:text-indigo-700">
                                            Portal
                                        </span>
                                        <span className="text-slate-400 transition group-hover:translate-x-1 group-hover:text-indigo-500">
                                            →
                                        </span>
                                    </div>
                                </a>

                                <a
                                    href={APP_LINKS.aucklandCoffee.admin}
                                    className="group rounded-xl border border-slate-200 bg-slate-50 px-4 py-3 transition hover:border-indigo-300 hover:bg-indigo-50"
                                >
                                    <div className="flex items-center justify-between">
                                        <span className="text-sm font-semibold text-slate-800 group-hover:text-indigo-700">
                                            Admin
                                        </span>
                                        <span className="text-slate-400 transition group-hover:translate-x-1 group-hover:text-indigo-500">
                                            →
                                        </span>
                                    </div>
                                </a>
                            </div>
                        </div>

                        <div className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm">
                            <div className="flex items-center gap-2">
                                <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-rose-100 text-rose-700">
                                    <HiOutlineCube className="h-4 w-4" />
                                </div>

                                <div>
                                    <p className="text-[11px] font-medium uppercase tracking-wide text-slate-400">
                                        Tenant
                                    </p>
                                    <h3 className="text-sm font-semibold text-slate-800">
                                        Sushi Bar
                                    </h3>
                                </div>
                            </div>

                            <div className="mt-4 grid grid-cols-2 gap-2">
                                <a
                                    href={APP_LINKS.sushiBar.portal}
                                    className="group rounded-xl border border-slate-200 bg-slate-50 px-4 py-3 transition hover:border-indigo-300 hover:bg-indigo-50"
                                >
                                    <div className="flex items-center justify-between">
                                        <span className="text-sm font-semibold text-slate-800 group-hover:text-indigo-700">
                                            Portal
                                        </span>
                                        <span className="text-slate-400 transition group-hover:translate-x-1 group-hover:text-indigo-500">
                                            →
                                        </span>
                                    </div>
                                </a>

                                <a
                                    href={APP_LINKS.sushiBar.admin}
                                    className="group rounded-xl border border-slate-200 bg-slate-50 px-4 py-3 transition hover:border-indigo-300 hover:bg-indigo-50"
                                >
                                    <div className="flex items-center justify-between">
                                        <span className="text-sm font-semibold text-slate-800 group-hover:text-indigo-700">
                                            Admin
                                        </span>
                                        <span className="text-slate-400 transition group-hover:translate-x-1 group-hover:text-indigo-500">
                                            →
                                        </span>
                                    </div>
                                </a>
                            </div>
                        </div>

                        <div className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm">
                            <div className="flex items-center gap-2">
                                <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-sky-100 text-sky-700">
                                    <HiOutlineCodeBracket className="h-4 w-4" />
                                </div>

                                <div>
                                    <p className="text-[11px] font-medium uppercase tracking-wide text-slate-400">
                                        Developer
                                    </p>
                                    <h3 className="text-sm font-semibold text-slate-800">
                                        API Docs
                                    </h3>
                                </div>
                            </div>

                            <div className="mt-4">
                                <a
                                    href={APP_LINKS.apiDocs}
                                    className="group flex items-center justify-between rounded-xl border border-slate-200 bg-slate-50 px-4 py-3 transition hover:border-indigo-300 hover:bg-indigo-50"
                                >
                                    <span className="text-sm font-semibold text-slate-800 group-hover:text-indigo-700">
                                        Swagger
                                    </span>
                                    <span className="text-slate-400 transition group-hover:translate-x-1 group-hover:text-indigo-500">
                                        →
                                    </span>
                                </a>
                            </div>
                        </div>
                    </div>
                </div>
            </section>

            <section className="px-6 pt-6">
                <div className="mx-auto max-w-5xl">
                    <h2 className="text-3xl font-semibold text-slate-800">
                        System architecture
                    </h2>

                    <p className="mt-3 text-sm text-slate-500">
                        Request flow and event-driven communication across services
                    </p>

                    <div className="flex flex-wrap items-center justify-center gap-8 py-6 sm:gap-10">
                        <ArchitectureNode
                            icon={<HiOutlineUser className="h-6 w-6" />}
                            label="Client"
                        />
                        <ArchitectureArrow />

                        <ArchitectureNode
                            icon={<HiOutlineServerStack className="h-6 w-6" />}
                            label="Gateway"
                        />
                        <ArchitectureArrow />

                        <ArchitectureNode
                            icon={<HiOutlineArrowsRightLeft className="h-6 w-6" />}
                            label="Message Bus"
                        />
                        <ArchitectureArrow />

                        <ArchitectureNode
                            icon={<HiOutlineCube className="h-6 w-6" />}
                            label="Services"
                        />
                        <ArchitectureArrow />

                        <ArchitectureNode
                            icon={<HiOutlineCircleStack className="h-6 w-6" />}
                            label="Database"
                        />
                    </div>
                </div>
            </section>

            <section className="bg-slate-50 px-6 py-14">
                <div className="mx-auto max-w-5xl">
                    <h2 className="text-3xl font-semibold text-slate-800">
                        Delivery pipeline
                    </h2>

                    <p className="mt-3 text-sm text-slate-500">
                        From code validation to image-based deployment in production
                    </p>

                    <div className="mt-8 flex flex-wrap items-center justify-center gap-5 sm:gap-6">
                        <PipelineStep
                            icon={<HiOutlineBeaker className="h-5 w-5" />}
                            label="Test"
                        />

                        <span className="text-lg text-slate-300">→</span>

                        <PipelineStep
                            icon={<HiOutlineCube className="h-5 w-5" />}
                            label="Build image"
                        />

                        <span className="text-lg text-slate-300">→</span>

                        <PipelineStep
                            icon={<HiOutlineCloudArrowUp className="h-5 w-5" />}
                            label="Push to GHCR"
                        />

                        <span className="text-lg text-slate-300">→</span>

                        <PipelineStep
                            icon={<HiOutlineArrowDownTray className="h-5 w-5" />}
                            label="VPS pull"
                        />

                        <span className="text-lg text-slate-300">→</span>

                        <PipelineStep
                            icon={<HiOutlineRocketLaunch className="h-5 w-5" />}
                            label="Deploy"
                        />
                    </div>
                </div>
            </section>

            <section className="px-6 py-16">
                <div className="mx-auto max-w-5xl">
                    <h2 className="text-3xl font-semibold text-slate-800">
                        Technology stack
                    </h2>

                    <div className="mt-10 grid grid-cols-3 gap-8 sm:grid-cols-4 md:grid-cols-7">
                        <TechItem icon={<SiDotnet className="h-5 w-5" />} label=".NET" />
                        <TechItem icon={<SiReact className="h-5 w-5" />} label="React" />
                        <TechItem icon={<SiMysql className="h-5 w-5" />} label="MySQL" />
                        <TechItem icon={<SiRedis className="h-5 w-5" />} label="Redis" />
                        <TechItem icon={<SiRabbitmq className="h-5 w-5" />} label="RabbitMQ" />
                        <TechItem icon={<SiDocker className="h-5 w-5" />} label="Docker" />
                        <TechItem icon={<SiGithubactions className="h-5 w-5" />} label="CI/CD" />
                    </div>
                </div>
            </section>

            <section className="bg-slate-50 px-6 py-16">
                <div className="mx-auto max-w-5xl text-start">
                    <h2 className="text-3xl font-semibold text-slate-800">
                        Core capabilities
                    </h2>

                    <div className="mt-10 grid gap-6 text-left sm:grid-cols-2">
                        <div className="rounded-xl border border-slate-200 bg-white p-6">
                            <h3 className="font-semibold text-slate-800">
                                Separate tenant workspaces
                            </h3>
                            <p className="mt-2 text-slate-600">
                                Each tenant runs with its own menu, orders, users, and admin workspace.
                            </p>
                        </div>

                        <div className="rounded-xl border border-slate-200 bg-white p-6">
                            <h3 className="font-semibold text-slate-800">
                                Menu and order management
                            </h3>
                            <p className="mt-2 text-slate-600">
                                Support category-based menus, order creation, and order history views.
                            </p>
                        </div>

                        <div className="rounded-xl border border-slate-200 bg-white p-6">
                            <h3 className="font-semibold text-slate-800">
                                Role-based access
                            </h3>
                            <p className="mt-2 text-slate-600">
                                Admin and member roles operate with distinct responsibilities and permissions.
                            </p>
                        </div>

                        <div className="rounded-xl border border-slate-200 bg-white p-6">
                            <h3 className="font-semibold text-slate-800">
                                Audit-ready workflows
                            </h3>
                            <p className="mt-2 text-slate-600">
                                Administrative actions can be tracked through audit logging and tenant-specific operations.
                            </p>
                        </div>
                    </div>
                </div>
            </section>
        </main>
    )
}