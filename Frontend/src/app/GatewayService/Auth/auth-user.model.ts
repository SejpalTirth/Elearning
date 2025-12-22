export interface AuthUser {
  userId: string;
  email: string;
  name: string | null;
  role: string | null;
  provider: string;
}
