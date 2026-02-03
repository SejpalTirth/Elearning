import { AuthRole } from "./auth-role";

export interface AuthUser {
  userId: string;
  email: string;
  name: string;
  role: AuthRole;
  isNewUser?: boolean;
}