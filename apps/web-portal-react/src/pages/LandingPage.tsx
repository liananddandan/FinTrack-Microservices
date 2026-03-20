import Header from "../components/Header"
import { motion } from "motion/react"
import SkillsSection from "../components/SkillsSection"
import ProjectsSection from "../components/ProjectsSection"
import Footer from "../components/Footer"
import { Helmet } from "react-helmet-async"

const container = {
    hidden: {},
    show: {
        transition: {
            staggerChildren: 0.3,
            delayChildren: 0.2,
        },
    },
}

const item = {
    hidden: { opacity: 0, y: 20 },
    show: {
        opacity: 1,
        y: 0,
        transition: {
            duration: 1,
            ease: [0.22, 1, 0.36, 1] as const,
        },
    },
}

const motto = {
    hidden: { opacity: 0, y: 28 },
    show: {
        opacity: 1,
        y: 0,
        transition: {
            duration: 1.2,
            delay: 0.9,
            ease: [0.22, 1, 0.36, 1] as const,
        },
    },
}

export default function LandingPage() {
    return (
        <>
            <Helmet>
                <title>Chen Li | .NET Engineer</title>
                <link rel="icon" type="image/png" href="/avatar.png" />
            </Helmet>
            <div className="min-h-screen bg-white">
                <Header />

                <main>
                    <section className="bg-gradient-to-b from-slate-100 to-white px-6 pt-10 pb-12 lg:px-10">
                        <div className="mx-auto grid max-w-6xl grid-cols-1 gap-10 lg:grid-cols-[1.3fr_0.7fr] lg:gap-12">
                            <motion.div
                                variants={container}
                                initial="hidden"
                                animate="show"
                                className="mx-auto flex w-full max-w-xl flex-col justify-center"
                            >
                                <motion.h1
                                    variants={item}
                                    className="tracking-tight text-slate-900"
                                >
                                    <span className="block text-4xl font-semibold sm:text-5xl lg:text-6xl">
                                        I turn ideas into
                                    </span>
                                    <span className="mt-1 block text-3xl font-medium text-slate-700 sm:text-4xl lg:text-5xl">
                                        working software.
                                    </span>
                                </motion.h1>

                                <motion.p
                                    variants={item}
                                    className="mt-5 text-base leading-7 text-slate-500 lg:text-lg"
                                >
                                    .NET engineer focused on backend and AI-powered systems.
                                    I design scalable systems with clear boundaries, and build full-stack solutions when needed.
                                </motion.p>

                                <motion.div
                                    variants={item}
                                    className="mt-8 flex flex-wrap items-center gap-4"
                                >
                                    <a
                                        href="https://github.com/liananddandan"
                                        target="_blank"
                                        rel="noreferrer"
                                        className="text-sm font-medium text-slate-700 transition hover:text-indigo-600"
                                    >
                                        View GitHub →
                                    </a>
                                </motion.div>
                            </motion.div>

                            <motion.div variants={motto}
                                initial="hidden"
                                animate="show" className="relative hidden items-center justify-end lg:flex">
                                <div className="border-l border-slate-200 pl-5">
                                    <p className="text-[11px] uppercase tracking-[0.28em] text-slate-400">
                                        Personal Motto
                                    </p>

                                    <p className="mt-3 text-xl leading-relaxed text-slate-700">
                                        Uncompromising in the pursuit
                                        <br />
                                        of a meaningful career
                                    </p>
                                </div>
                            </motion.div>
                        </div>
                    </section>

                    <SkillsSection />

                    <ProjectsSection />
                    <Footer />
                </main>
            </div>
        </>
    )
}