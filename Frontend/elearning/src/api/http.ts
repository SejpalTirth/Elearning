import axios from 'axios';
import type { AxiosRequestConfig, AxiosResponse } from 'axios';
import { environment } from '../Environment/environment.js'
import { refreshSession } from '../services/authService';

const axiosInstance = axios.create({
  baseURL: environment.baseurl,
  withCredentials: true,
});

export const http = <T = unknown>(
  config: AxiosRequestConfig
): Promise<AxiosResponse<T>> => {
  return axiosInstance.request<T>(config);
};

axiosInstance.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    if (originalRequest?.url?.includes('/GatewayAuth/refresh')) {
      return Promise.reject(error);
    }

    if (
      error.response?.status === 401 &&
      !originalRequest._retry
    ) {
      originalRequest._retry = true;

      const refreshed = await refreshSession();

      if (refreshed) {
        return axiosInstance.request(originalRequest);
      }
    }

    return Promise.reject(error);
  }
);