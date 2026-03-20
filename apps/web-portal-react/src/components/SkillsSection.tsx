import { skills } from "../data/skills"

export default function SkillsSection() {
  return (
    <section className="bg-white px-6 pt-6 lg:px-10">
      <div className="mx-auto max-w-6xl">
        <div className="mb-12">
          <p className="text-sm font-medium uppercase tracking-[0.2em] text-slate-400">
            Capabilities
          </p>
          <h2 className="mt-3 text-3xl font-semibold tracking-tight text-slate-900">
            Tech Stack
          </h2>
        </div>

        <div className="space-y-14">
          {skills.map((group) => (
            <div
              key={group.title}
              className="grid grid-cols-1 gap-6 lg:grid-cols-[160px_1fr]"
            >
              <div>
                <h3 className="text-sm font-semibold uppercase tracking-[0.18em] text-slate-500">
                  {group.title}
                </h3>
              </div>

              <div className="grid grid-cols-1 gap-5 sm:grid-cols-2">
                {group.items.map((item) => (
                  <div
                    key={item.name}
                    className="flex items-start gap-4"
                  >
                    <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-md bg-slate-100 text-lg text-slate-600 transition hover:text-indigo-600">
                      {item.icon}
                    </div>

                    <div>
                      <div className="text-sm font-medium text-slate-900">
                        {item.name}
                      </div>
                      <div className="mt-1 text-sm leading-6 text-slate-500">
                        {item.description}
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          ))}
        </div>
      </div>
    </section>
  )
}