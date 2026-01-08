import { MenuItem } from '@frontend/ui';

export const Instructor_MENU: MenuItem[] = [
  {
    label: 'Home',
    route: '',
    roles: ['Instructor']
  },
  {
    label: 'Courses',
    route: '/courses',
    roles: ['Instructor']
  },
  {
    label: 'My learning',
    route: '/my-learning',
    roles: ['Instructor']
  },
  {
    label: 'Add Course',
    route: '/add-course',
    roles: ['Instructor']
  },
  {
    label: 'Edit Course',
    route: '/my-courses',
    roles: ['Instructor']
  },
  {
    label: 'Pending Tasks',
    route: '/pending',
    roles: ['Instructor']
  }
];
