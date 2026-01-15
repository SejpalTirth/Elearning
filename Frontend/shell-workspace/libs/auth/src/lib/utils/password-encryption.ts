import CryptoJS from 'crypto-js';

const SECRET_KEY = 'b7f8e4a2c1d9f3a6e0b5c8d2f4a9e7c1'; 

export function encryptPassword(password: string): string {
  return CryptoJS.AES.encrypt(password, SECRET_KEY).toString();
}
