import axios from 'axios';
import type { AxiosRequestConfig, AxiosResponse } from 'axios';
import { environment } from '../Environment/environment.js'

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

      try {
        await axiosInstance.post('/api/GatewayAuth/refresh');
        return axiosInstance.request(originalRequest);
      } catch {
        return Promise.reject(error);
      }
    }

    return Promise.reject(error);
  }
);