import type { ReactNode } from "react"

export type SkillItem = {
  name: string
  icon: ReactNode
  description: string
}

export type SkillGroup = {
  title: string
  items: SkillItem[]
}