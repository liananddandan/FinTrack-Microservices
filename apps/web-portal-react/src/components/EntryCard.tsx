import { HiOutlineArrowRight } from "react-icons/hi2"
import { Link } from "react-router-dom"

type Props = {
  to?: string
  href?: string
  icon: React.ReactNode
  title: string
  description: string
}

export default function EntryCard({
  to,
  href,
  icon,
  title,
  description,
}: Props) {
  const baseClass =
    "group block rounded-2xl border border-slate-200 bg-white p-5 transition duration-200 ease-out hover:border-indigo-500 hover:bg-slate-50 hover:shadow-sm active:scale-[0.98]"

  const content = (
    <>
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <div className="flex h-11 w-11 items-center justify-center rounded-2xl bg-indigo-50 text-indigo-600">
            {icon}
          </div>

          <p className="text-sm font-semibold text-slate-800 transition group-hover:text-indigo-600">
            {title}
          </p>
        </div>

        <HiOutlineArrowRight className="h-5 w-5 text-slate-400 transition group-hover:translate-x-1 group-hover:text-indigo-600" />
      </div>

      <p className="mt-4 text-sm leading-6 text-slate-600">
        {description}
      </p>
    </>
  )

  // 内部路由
  if (to) {
    return (
      <Link to={to} className={baseClass}>
        {content}
      </Link>
    )
  }

  // 外部链接
  return (
    <a href={href} target="_blank" rel="noopener noreferrer" className={baseClass}>
      {content}
    </a>
  )
}