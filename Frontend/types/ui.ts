export interface NavigationItem {
  title: string
  icon: string
  to?: string
  children?: NavigationItem[]
  permission?: string
  roles?: string[]
}

export interface BreadcrumbItem {
  title: string
  to?: string
  disabled?: boolean
}

export interface TableHeader {
  title: string
  key: string
  sortable?: boolean
  align?: 'start' | 'center' | 'end'
  width?: string | number
}

export interface DataTableOptions {
  page: number
  itemsPerPage: number
  sortBy: string[]
  search?: string
}

export interface FormField {
  name: string
  label: string
  type: 'text' | 'email' | 'password' | 'select' | 'textarea' | 'checkbox' | 'switch'
  placeholder?: string
  required?: boolean
  rules?: string[]
  options?: { title: string; value: any }[]
  multiple?: boolean
  disabled?: boolean
  hint?: string
}
