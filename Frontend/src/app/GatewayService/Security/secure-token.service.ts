import { Injectable } from '@angular/core';
import * as CryptoJS from 'crypto-js';

@Injectable({ providedIn: 'root' })
export class SecureTokenService {

  private encryptedToken: string | null = null;

  // Must match backend AES encryption key
  private readonly encryptionKey = 'SuperSecretEncryptionKeyValue123!';

  constructor() {
    // Load encrypted token from localStorage on app load
    const saved = localStorage.getItem('accessToken');
    if (saved) {this.encryptedToken = saved;}
  }

  // ------------------------------
  // STORE ENCRYPTED TOKEN
  // ------------------------------
  setEncryptedToken(token: string):void {
    this.encryptedToken = token;
    localStorage.setItem('accessToken', token);
  }

  clear(): void{
    this.encryptedToken = null;
    localStorage.removeItem('accessToken');
  }

  // ------------------------------
  // FIX: NORMALIZE BASE64 STRING
  // Removes spaces, newlines, padding issues
  // ------------------------------
  private normalizeBase64(str: string): string {
    return str
      .replace(/ /g, '+')   // Spaces → "+"
      .replace(/\n/g, '')   // Remove LF
      .replace(/\r/g, '')   // Remove CR
      .trim();
  }

  // ------------------------------
  // AES DECRYPT
  // ------------------------------
  private decryptToken(): string | null {
    if (!this.encryptedToken) {return null;}

    try {
      const clean = this.normalizeBase64(this.encryptedToken);

      // Convert Base64 to raw bytes
      const rawData = CryptoJS.enc.Base64.parse(clean);

      // IMPORTANT: rawData.words is an array of 32-bit words
      // AES IV = first 16 bytes = first 4 words
      const iv = CryptoJS.lib.WordArray.create(
        rawData.words.slice(0, 4),  // First 4 words → IV
        16
      );

      // Ciphertext = rest of words
      const cipher = CryptoJS.lib.WordArray.create(
        rawData.words.slice(4),     // Words after IV
        rawData.sigBytes - 16       // Remaining bytes
      );

      const key = CryptoJS.SHA256(this.encryptionKey);

      const decrypted = CryptoJS.AES.decrypt(
        { ciphertext: cipher } as any,
        key,
        {
          iv: iv,
          mode: CryptoJS.mode.CBC,
          padding: CryptoJS.pad.Pkcs7
        }
      );

      const jwt = decrypted.toString(CryptoJS.enc.Utf8);

      if (!jwt) {
        return null;
      }

      return jwt;

    } catch {
      return null;
    }
  }

  // ------------------------------
  // PARSE JWT PAYLOAD
  // ------------------------------
  getPayload(): any | null {
    const jwt = this.decryptToken();
    if (!jwt) {return null;}

    try {
      return JSON.parse(atob(jwt.split('.')[1]));
    } catch {
      return null;
    }
  }

  // ------------------------------
  // Get UserId from decrypted JWT
  // ------------------------------
  getUserId(): string | null {
    const payload = this.getPayload();
    return payload ? payload.sub : null;
  }

  // ------------------------------
  // For AuthInterceptor (attach decrypted JWT)
  // ------------------------------
  getDecryptedToken(): string | null {
    return this.decryptToken();
  }
}
