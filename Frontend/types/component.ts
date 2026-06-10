import type { User } from './user'
import type { Role } from './role'

export interface BaseDialogProps {
  modelValue: boolean
  title?: string
  maxWidth?: string | number
  persistent?: boolean
}

export interface ConfirmDialogProps extends BaseDialogProps {
  message: string
  confirmText?: string
  cancelText?: string
  color?: string
}

export interface UserDialogProps extends BaseDialogProps {
  user?: User | null
  mode: 'create' | 'edit' | 'view'
}

export interface RoleDialogProps extends BaseDialogProps {
  role?: Role | null
  mode: 'create' | 'edit' | 'view'
}
