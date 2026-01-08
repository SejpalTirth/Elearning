import { MenuItem } from '@frontend/ui';

export const ADMIN_MENU: MenuItem[] = [
  {
    label: 'Home',
    route: '',
    roles: ['Admin']
  },
  {
    label: 'Courses',
    route: '/courses',
    roles: ['Admin']
  },
  {
    label: 'My learning',
    route: '/my-learning',
    roles: ['Admin']
  },
  {
    label: 'Manage Courses',
    route: '/manage-courses',
    roles: ['Admin']
  },
  {
    label: 'Manage Users',
    route: '/manage-users',
    roles: ['Admin']
  }
];
