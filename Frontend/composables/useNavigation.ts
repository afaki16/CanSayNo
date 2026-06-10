  import type { NavigationItem } from '~/types'

  export const navigationItems: NavigationItem[] = [
    {
      title: 'sidebar.menu.dashboard',
      icon: 'mdi-view-dashboard',
      children: [
        {
           title: 'sidebar.menu.dashboard',
      icon: 'mdi-view-dashboard',
      to: '/dashboard',
        }

      ]
    },

    {
      title: 'sidebar.menu.userManagement',
      icon: 'mdi-account-group',
      children: [
        {
          title: 'sidebar.menu.users',
          icon: 'mdi-account-multiple',
          to: '/users',
          permission: 'Users.Read'
        },
        {
          title: 'sidebar.menu.tenants',
          icon: 'mdi-domain',
          to: '/tenants',
          roles: ['SuperAdmin']
        },
        {
          title: 'sidebar.menu.roles',
          icon: 'mdi-shield-account',
          to: '/roles',
          permission: 'Roles.Read'
        },
        {
          title: 'sidebar.menu.permissions',
          icon: 'mdi-key',
          to: '/permissions',
          permission: 'Permissions.Read'
        }
      ]
    },

    {
      title: 'sidebar.menu.settings',
      icon: 'mdi-cog',
      to: '/settings',
      roles: ['Admin', 'SuperAdmin']
    }
  ]

  export const filterNavigationByPermissions = (
    items: NavigationItem[],
    hasPermission: (permission: string) => boolean,
    hasRole: (role: string) => boolean
  ): NavigationItem[] => {
    return items
      .map((item): NavigationItem | null => {
        // Check permission requirement
        if (item.permission && !hasPermission(item.permission)) return null
        // Check role requirement
        if (item.roles && !item.roles.some(role => hasRole(role))) return null
        // If item has children, filter recursively and return new object (do not mutate original)
        if (item.children) {
          const filteredChildren = filterNavigationByPermissions(item.children, hasPermission, hasRole)
          if (filteredChildren.length === 0) return null
          return { ...item, children: filteredChildren }
        }
        return { ...item }
      })
      .filter((item): item is NavigationItem => item !== null)
  } 