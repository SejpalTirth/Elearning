export type UserRole = 'Admin' | 'Instructor' | 'Student';

export interface MenuItem {
  label: string;
  route: string;
  roles?: UserRole[];
}
