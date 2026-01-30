import { createApi, fakeBaseQuery } from '@reduxjs/toolkit/query/react';
import { getGatewayAuth } from '../api/clients/gateway-auth';
import { encryptPassword } from '../utils/passwordEncryption';

const authApi = getGatewayAuth();

export const authApiSlice = createApi({
  reducerPath: 'authApi',
  baseQuery: fakeBaseQuery(),
  tagTypes: ['Auth'],

  endpoints: (builder) => ({
    // GET /me
    
    getMe: builder.query({
      async queryFn() {
        try {
          const res = await authApi.getApiGatewayAuthMe();
          return { data: res.data };
        } catch (error) {
            return {
              error: {
                status: error.response?.status,
                data: error.response?.data,
              },
            };
          }

      },
      providesTags: ['Auth'],
    }),

    // LOGIN

    login: builder.mutation({
      async queryFn({ email, password }) {
        try {
          const encryptedPassword = encryptPassword(password);

          const res = await authApi.postApiGatewayAuthLocalLogin({
            email,
            password: encryptedPassword,
          });

          return { data: res.data };
        } catch (error) {
            return {
              error: {
                status: error.response?.status,
                data: error.response?.data,
              },
            };
          }

      },
      invalidatesTags: ['Auth'],
    }),
    // LOGOUT

    logout: builder.mutation({
      async queryFn() {
        try {
          await authApi.getApiGatewayAuthLogout();
          return { data: true };
        } catch (error) {
            return {
              error: {
                status: error.response?.status,
                data: error.response?.data,
              },
            };
          }

      },
      invalidatesTags: ['Auth'],
    }),

    // Refresh

    refresh: builder.mutation({
      async queryFn() {
        try {
          await authApi.postApiGatewayAuthRefresh();
          return { data: true };
        } catch (error) {
            return {
              error: {
                status: error.response?.status,
                data: error.response?.data,
              },
            };
          }

      },
    }),


    // REGISTER

    register: builder.mutation({
      async queryFn({ email, password }) {
        try {

          const encrypted = encryptPassword(password)
          await authApi.postApiGatewayAuthLocalRegister({
            email,
            password : encrypted,
          });
          return { data: true };
        } catch (error) {
          return { 
            error : {
              status: error.response?.status,
              data: error.response?.data,
            }
          };
        }
      },
    }),
  }),
});

export const {
  useGetMeQuery,
  useLoginMutation,
  useLogoutMutation,
  useRegisterMutation,
} = authApiSlice;

export const authApiEndpoints = authApiSlice;