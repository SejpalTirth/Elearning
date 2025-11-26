export function decodeToken(token: string | null): any {
  if (!token) return null;

  const payload = token.split('.')[1];
  if (!payload) return null;

  try {
    return JSON.parse(atob(payload));
  } catch {
    return null;
  }
}
