import { projectCards } from "../data/projectCards"

export default function ProjectsSection() {
    return (
        <section id="projects" className="bg-white px-6 py-16 lg:px-10">
            <div className="mx-auto max-w-6xl">
                <div className="mb-10">
                    <p className="text-sm font-medium uppercase tracking-[0.2em] text-slate-400">
                        Selected Work
                    </p>
                    <h2 className="mt-3 text-3xl font-semibold tracking-tight text-slate-900">
                        Projects
                    </h2>
                    <p className="mt-4 max-w-4xl text-slate-500">
                        A selection of projects showcasing backend systems, system design, and real-world problem solving.
                    </p>
                </div>

                <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
                    {projectCards.map((project) => {
                        const primaryAction = project.actions.find((a) => a.primary)
                        const secondaryActions = project.actions.filter((a) => !a.primary)
                        return (
                            <div
                                key={project.name}
                                className="flex h-full flex-col rounded-2xl border border-slate-200 bg-white p-6 shadow-sm shadow-slate-200/40 transition hover:-translate-y-1 hover:border-indigo-400 hover:shadow-md">
                                <div>
                                    <h3 className="text-lg font-semibold text-slate-900">
                                        {project.name}
                                    </h3>
                                    <p className="mt-2 text-sm font-medium text-slate-600">
                                        {project.subtitle}
                                    </p>
                                    <p className="mt-4 text-sm leading-7 text-slate-500">
                                        {project.summary}
                                    </p>
                                </div>

                                <div className="mt-5 space-y-2">
                                    {project.highlights.map((item) => (
                                        <div
                                            key={item}
                                            className="text-sm leading-6 text-slate-600"
                                        >
                                            • {item}
                                        </div>
                                    ))}
                                </div>

                                <div className="mt-6 flex flex-wrap items-center gap-3">
                                    {primaryAction && (
                                        <a
                                            href={primaryAction.href}
                                            target="_blank"
                                            rel="noreferrer"
                                            className="rounded-full bg-slate-900 px-4 py-2 text-sm font-medium text-white transition hover:bg-slate-800"
                                        >
                                            {primaryAction.label}
                                        </a>
                                    )}

                                    {secondaryActions.map((action) => (
                                        <a
                                            key={action.label}
                                            href={action.href}
                                            target="_blank"
                                            rel="noreferrer"
                                            className="text-sm font-medium text-slate-600 transition hover:text-indigo-600"
                                        >
                                            {action.label}
                                        </a>
                                    ))}
                                </div>
                            </div>
                        )
                    })}
                </div>
            </div>
        </section>
    )
}