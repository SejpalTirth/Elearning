import { createApi, fakeBaseQuery } from '@reduxjs/toolkit/query/react';
import { getNotificationGateway } from '../api/clients/notification-gateway';

/**
 * @typedef {import('../models').GatewayContractsNotificationEmailRequest} EmailReq
 * @typedef {import('../models').GatewayContractsNotificationTemplateRequest} TemplateReq
 * @typedef {import('../models').GatewayContractsNotificationTriggerNotification} TriggerReq
 * @typedef {import('../models').GatewayContractsNotificationUserIdRequest} UserIdReq
 */

const notificationApi = getNotificationGateway();

export const notificationApiSlice = createApi({
  reducerPath: 'notificationApi',
  baseQuery: fakeBaseQuery(),
  tagTypes: ['Notification'],
  endpoints: (builder) => ({
    
    // --- QUERIES ---
    
    getUserNotifications: builder.query({
      /** @param {UserIdReq} arg */
      async queryFn(arg) {
        try {
          const res = await notificationApi.postApiNotificationUser(arg);
          return { data: res.data };
        } catch (error) {
          return { error };
        }
      },
      providesTags: (result, error, arg) => [{ type: 'Notification', id: arg.userId }],
    }),

    // --- MUTATIONS ---

    sendDirectEmail: builder.mutation({
      /** @param {EmailReq} arg */
      async queryFn(arg) {
        try {
          const res = await notificationApi.postApiNotificationSend(arg);
          return { data: res.data };
        } catch (error) {
          return { error };
        }
      },
    }),

    sendTemplateEmail: builder.mutation({
      /** @param {TemplateReq} arg */
      async queryFn(arg) {
        try {
          const res = await notificationApi.postApiNotificationTemplateSend(arg);
          return { data: res.data };
        } catch (error) {
          return { error };
        }
      },
    }),

    triggerNotification: builder.mutation({
      /** @param {TriggerReq} arg */
      async queryFn(arg) {
        try {
          const res = await notificationApi.postApiNotificationTrigger(arg);
          return { data: res.data };
        } catch (error) {
          return { error };
        }
      },
      invalidatesTags: ['Notification'],
    }),
  }),
});

export const {
  useGetUserNotificationsQuery,
  useSendDirectEmailMutation,
  useSendTemplateEmailMutation,
  useTriggerNotificationMutation,
} = notificationApiSlice;