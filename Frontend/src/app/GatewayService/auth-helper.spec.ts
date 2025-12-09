import { decodeToken } from './auth-helpers';

function createFakeJWT(payload: any): string {
  const encoded = btoa(JSON.stringify(payload));
  return `header.${encoded}.signature`;
}

describe('decodeToken()', () => {

  it('should return null when token is null', () => {
    const result = decodeToken(null);
    expect(result).toBeNull();
  });

  it('should return null when token has no payload section', () => {
    const result = decodeToken("invalidtoken");
    expect(result).toBeNull();
  });

  it('should decode valid JWT payload', () => {
    const token = createFakeJWT({ name: 'Kira', role: 'Student' });

    const result = decodeToken(token);

    expect(result).toEqual({ name: 'Kira', role: 'Student' });
  });

  it('should return null if payload is not valid Base64', () => {
    const token = "abc.!@#.xyz"; // invalid base64 content

    const result = decodeToken(token);

    expect(result).toBeNull();
  });

  it('should return null if JSON.parse throws (corrupted payload)', () => {
    // valid base64 but invalid JSON
    const badJSON = btoa("not-json");
    const token = `aaa.${badJSON}.bbb`;

    const result = decodeToken(token);

    expect(result).toBeNull();
  });

});
