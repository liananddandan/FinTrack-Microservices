import { FiMail, FiGithub, FiLinkedin } from "react-icons/fi"

export default function Footer() {
  return (
    <footer className="bg-white px-6 pb-10 pt-12 lg:px-10">
      <div className="mx-auto max-w-6xl border-t border-slate-200 pt-6">
        <div className="flex flex-col items-center justify-between gap-4 text-sm text-slate-500 lg:flex-row">
          <div className="flex items-center gap-5">
            <span className="text-xs uppercase tracking-wider text-slate-400">
              Connect
            </span>
            {/* Left: Icons */}
            <div className="flex items-center gap-5">
              {/* Email */}
              <a
                href="mailto:lichen.newzealand@gmail.com"
                className="transition hover:text-slate-900"
                aria-label="Email"
              >
                <FiMail size={24} />
              </a>

              {/* GitHub */}
              <a
                href="https://github.com/liananddandan"
                target="_blank"
                rel="noreferrer"
                className="transition hover:text-slate-900"
                aria-label="GitHub"
              >
                <FiGithub size={24} />
              </a>

              {/* LinkedIn */}
              <a
                href="https://www.linkedin.com/in/chenli-nz/"
                className="transition hover:text-slate-900"
                aria-label="LinkedIn"
              >
                <FiLinkedin size={24} />
              </a>
            </div>
          </div>

          {/* Right: Copyright */}
          <div className="text-center lg:text-right">
            © {new Date().getFullYear()} Chen Li. Built with React.
          </div>
        </div>
      </div>
    </footer>
  )
}