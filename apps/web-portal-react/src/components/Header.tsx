import { ChevronDown, ArrowUpRight } from "lucide-react"
import { projects } from "../data/projects"

export default function Header() {


    return (
        <header className="sticky top-0 z-50 border-b border-slate-200 bg-white/95 backdrop-blur">
            <div className="mx-auto flex h-20 max-w-7xl items-center justify-between px-6 lg:px-10">
                {/* Left: avatar + brand */}
                <div className="flex items-center gap-3">
                    <div className="h-14 w-14 overflow-hidden rounded-full bg-slate-100 ring-1 ring-slate-200">
                        <img
                            src="/avatar.png"
                            alt="Logo avatar"
                            className="h-full w-full object-cover"
                        />
                    </div>

                    <div className="flex flex-col">
                        <span className="text-base font-semibold tracking-tight text-slate-900">
                            Chen Li
                        </span>
                        <span className="text-xs text-slate-500">
                            .NET Engineer
                        </span>
                    </div>
                </div>

                <nav className="flex items-center">
                    <div className="relative group">
                        <div className="flex cursor-pointer items-center gap-2 text-sm font-bold text-slate-700 transition hover:text-indigo-600">
                            <span>Explore Projects</span>
                            <ChevronDown className="h-4 w-4 transition group-hover:translate-y-[1px]" />
                        </div>

                        <div className="invisible absolute right-0 top-full w-[960px] pt-2 opacity-0 transition-all duration-150 group-hover:visible group-hover:opacity-100">
                            <div className="rounded-2xl border border-slate-200 bg-white p-3 shadow-lg shadow-slate-200/15">
                                <div className="grid grid-cols-3 divide-x divide-slate-200">
                                    {projects.slice(0, 3).map((project, index) => (
                                        <div key={index} className="px-6 py-5">
                                            <div className="text-sm font-semibold text-slate-900">
                                                {project.name}
                                            </div>

                                            <div className="mt-1 text-xs leading-5 text-slate-500">
                                                {project.subtitle}
                                            </div>

                                            {project.modules && project.modules.length > 0 && (
                                                <div className="mt-4 space-y-3">
                                                    {project.modules.map((module, moduleIndex) => (
                                                        <div key={moduleIndex}>
                                                            {module.href ? (
                                                                <a
                                                                    href={module.href}
                                                                    target="_blank"
                                                                    rel="noreferrer"
                                                                    className="text-xs font-medium text-slate-800 transition hover:text-indigo-600"
                                                                >
                                                                    {module.name}
                                                                    <ArrowUpRight className="ml-1 inline-block h-3 w-3" />
                                                                </a>
                                                            ) : (
                                                                <div className="text-xs font-medium text-slate-800">
                                                                    {module.name}
                                                                </div>
                                                            )}

                                                            <div className="mt-1 text-xs leading-5 text-slate-500">
                                                                {module.description}
                                                            </div>
                                                        </div>
                                                    ))}
                                                </div>
                                            )}

                                            <div className="mt-4 flex flex-wrap gap-2">
                                                {project.links.map((link, linkIndex) => (
                                                    <a
                                                        key={linkIndex}
                                                        href={link.href}
                                                        target="_blank"
                                                        rel="noreferrer"
                                                        className="inline-flex rounded-full bg-slate-100 px-3 py-1 text-xs font-medium text-slate-700 transition hover:bg-slate-200 hover:text-indigo-600"                                                    >
                                                        {link.label}
                                                    </a>
                                                ))}
                                            </div>
                                        </div>
                                    ))}
                                </div>
                            </div>
                        </div>
                    </div>
                </nav>
            </div>
        </header >
    )
}