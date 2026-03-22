import {
    HiOutlineArrowsRightLeft,
    HiOutlineUser,
    HiOutlineShieldCheck,
    HiOutlineCodeBracket,
    HiOutlineServerStack,
    HiOutlineCube,
    HiOutlineCircleStack,
    HiOutlineBeaker,
    HiOutlineCloudArrowUp,
    HiOutlineArrowDownTray,
    HiOutlineRocketLaunch,
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
import EntryCard from "../components/EntryCard"

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

export default function LandingPage() {
    return (
        <main className="bg-white text-gray-900">
            {/* Hero */}
            <section className="px-4 py-4">
                <div className="mx-auto max-w-5xl">
                    <div className="flex items-center gap-3">
                        <IconBadge>
                            <HiOutlineArrowsRightLeft className="h-6 w-6" />
                        </IconBadge>

                        <div>
                            <p className="text-sm font-medium tracking-wide text-gray-500">
                                Transaction & Workflow Platform
                            </p>
                            <p className="text-xs text-gray-400">
                                Multi-tenant system built with microservices
                            </p>
                        </div>
                    </div>
                </div>
            </section>

            <section className="border-t border-slate-200 bg-slate-100 py-10">
                <div className="mx-auto max-w-5xl">
                    <div className="max-w-3xl">
                        <h1 className="text-4xl font-semibold leading-tight tracking-tight text-slate-800 sm:text-5xl">
                            Transaction & workflow system for multi-tenant applications
                        </h1>

                        <p className="mt-6 text-lg leading-8 text-slate-600">
                            A production-style platform for managing donations, procurements, and
                            transaction workflows, built with clean architecture and event-driven design.
                        </p>

                        <div className="mt-10 grid gap-4 sm:grid-cols-3">
                            <EntryCard
                                href="https://fintrack.chenlis.com/portal/login"
                                icon={<HiOutlineUser className="h-5 w-5" />}
                                title="Portal"
                                description="Sign in as a tenant user to manage transactions and workflows."
                            />

                            <EntryCard
                                href="https://fintrack.chenlis.com/admin/login"
                                icon={<HiOutlineShieldCheck className="h-5 w-5" />}
                                title="Admin Panel"
                                description="Review member activity, invitations, and audit logs."
                            />

                            <EntryCard
                                href="https://fintrack.chenlis.com/api/swagger"
                                icon={<HiOutlineCodeBracket className="h-5 w-5" />}
                                title="API Docs"
                                description="Explore backend endpoints and service contracts through Swagger."
                            />
                        </div>
                    </div>
                </div>
            </section>

            {/* System architecture */}
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

            {/* Delivery pipeline */}
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

            {/* Technology stack */}
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

            {/* Core capabilities */}
            <section className="bg-slate-50 px-6 py-16">
                <div className="mx-auto max-w-5xl text-start">
                    <h2 className="text-3xl font-semibold text-slate-800">
                        What you can do with the system
                    </h2>

                    <div className="mt-10 grid gap-6 text-left sm:grid-cols-2">
                        <div className="rounded-xl border border-slate-200 bg-white p-6">
                            <h3 className="font-semibold text-slate-800">
                                Manage organizations independently
                            </h3>
                            <p className="mt-2 text-slate-600">
                                Each tenant operates in isolation with its own data, users, and workflows.
                            </p>
                        </div>

                        <div className="rounded-xl border border-slate-200 bg-white p-6">
                            <h3 className="font-semibold text-slate-800">
                                Control access with clear roles
                            </h3>
                            <p className="mt-2 text-slate-600">
                                Define permissions for admins and members to ensure secure operations.
                            </p>
                        </div>

                        <div className="rounded-xl border border-slate-200 bg-white p-6">
                            <h3 className="font-semibold text-slate-800">
                                Process transactions reliably
                            </h3>
                            <p className="mt-2 text-slate-600">
                                Actions flow through services and messaging infrastructure for consistency.
                            </p>
                        </div>

                        <div className="rounded-xl border border-slate-200 bg-white p-6">
                            <h3 className="font-semibold text-slate-800">
                                Track system activity
                            </h3>
                            <p className="mt-2 text-slate-600">
                                Audit logs provide visibility into every operation across the platform.
                            </p>
                        </div>
                    </div>
                </div>
            </section>
        </main>
    )
}