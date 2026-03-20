export type ProjectAction = {
  label: string
  href: string
  primary?: boolean
}

export type ProjectCard = {
  name: string
  subtitle: string
  summary: string
  highlights: string[]
  actions: ProjectAction[]
}