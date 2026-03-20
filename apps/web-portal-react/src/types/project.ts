export type ProjectModule = {
  name: string
  description: string
  href?: string
}

export type ProjectLink = {
  label: "GitHub" | "Preview" | "Swagger" | "Blog" | "Case Study"
  href: string
}

export type ProjectItem = {
  name: string
  subtitle: string
  modules?: ProjectModule[]
  links: ProjectLink[]
}