export interface UserVm {
  id: string;
  name?: string;
  email?: string;
  role?: string;
}

export interface RoleVm {
  id: number;
  name: string;
}
