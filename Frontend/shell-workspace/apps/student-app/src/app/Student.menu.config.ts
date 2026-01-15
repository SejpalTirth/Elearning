import { MenuItem } from '@frontend/ui';

export const Student_MENU: MenuItem[] = [
  {
    label: 'Home',
    route: '',
    roles: ['Student']
  },
  {
    label: 'Courses',
    route: '/courses',
    roles: ['Student']
  },
  {
    label: 'My learning',
    route: '/my-learning',
    roles: ['Student']
  }
];
