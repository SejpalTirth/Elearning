import { createApi, fakeBaseQuery } from '@reduxjs/toolkit/query/react';
import { getGatewayCourse } from '../api/clients/gateway-course';

/**
 * @typedef {import('../models').GatewayContractsCourseCourse} Course
 * @typedef {import('../models').GatewayContractsCourseCourseIdRequest} CourseIdReq
 * @typedef {import('../models').GatewayContractsCourseUpdateCourseRequest} UpdateCourseReq
 * @typedef {import('../models').GatewayContractsCourseEnrollRequest} EnrollReq
 * @typedef {import('../models').GatewayContractsCourseContinueCourseRequest} ContinueReq
 * @typedef {import('../models').GatewayContractsCourseModuleIdRequest} ModuleIdReq
 */

const courseApi = getGatewayCourse();

export const courseApiSlice = createApi({
  reducerPath: 'courseApi',
  baseQuery: fakeBaseQuery(),
  tagTypes: ['Course', 'Module', 'Category'],
  endpoints: (builder) => ({
    
    // --- QUERIES ---

    getAllCourses: builder.query({
      async queryFn() {
        try {
          const res = await courseApi.postApiCourseAll();
          return { data: res.data };
        } catch (error) {
          return { error };
        }
      },
      providesTags: ['Course'],
    }),

    getCourseById: builder.query({
      /** @param {CourseIdReq} arg */
      async queryFn(arg) {
        try {
          const res = await courseApi.postApiCourseById(arg);
          return { data: res.data };
        } catch (error) {
          return { error };
        }
      },
      providesTags: (result, error, arg) => [{ type: 'Course', id: arg.courseId }],
    }),

    getInstructorCourses: builder.query({
      async queryFn() {
        try {
          const res = await courseApi.postApiCourseInstructor();
          return { data: res.data };
        } catch (error) {
          return { error };
        }
      },
      providesTags: ['Course'],
    }),

    getUnfinishedCourses: builder.query({
      async queryFn() {
        try {
          const res = await courseApi.postApiCourseUnfinished();
          return { data: res.data };
        } catch (error) {
          return { error };
        }
      },
      providesTags: ['Course'],
    }),

    getEnrolledCourses: builder.query({
      async queryFn() {
        try {
          const res = await courseApi.postApiCourseEnrolled();
          return { data: res.data };
        } catch (error) {
          return { error };
        }
      },
      providesTags: ['Course'],
    }),

    getCourseModules: builder.query({
      /** @param {CourseIdReq} arg */
      async queryFn(arg) {
        try {
          const res = await courseApi.postApiCourseModules(arg);
          return { data: res.data };
        } catch (error) {
          return { error };
        }
      },
      providesTags: (result, error, arg) => [{ type: 'Module', id: arg.courseId }],
    }),

    getModuleById: builder.query({
      /** @param {ModuleIdReq} arg */
      async queryFn(arg) {
        try {
          const res = await courseApi.postApiCourseModule(arg);
          return { data: res.data };
        } catch (error) {
          return { error };
        }
      },
      providesTags: (result, error, arg) => [{ type: 'Module', id: arg.moduleId }],
    }),

    getCategories: builder.query({
      async queryFn() {
        try {
          const res = await courseApi.postApiCourseCategories();
          return { data: res.data };
        } catch (error) {
          return { error };
        }
      },
      providesTags: ['Category'],
    }),

    // --- MUTATIONS ---

    createCourse: builder.mutation({
      /** @param {Course} arg */
      async queryFn(arg) {
        try {
          const res = await courseApi.postApiCourseCreate(arg);
          return { data: res.data };
        } catch (error) {
          return { error };
        }
      },
      invalidatesTags: ['Course'],
    }),

    updateCourse: builder.mutation({
      /** @param {UpdateCourseReq} arg */
      async queryFn(arg) {
        try {
          const res = await courseApi.postApiCourseUpdate(arg);
          return { data: res.data };
        } catch (error) {
          return { error };
        }
      },
      invalidatesTags: (result, error, arg) => [{ type: 'Course', id: arg.id }, 'Course'],
    }),

    deleteCourse: builder.mutation({
      /** @param {CourseIdReq} arg */
      async queryFn(arg) {
        try {
          const res = await courseApi.postApiCourseDelete(arg);
          return { data: res.data };
        } catch (error) {
          return { error };
        }
      },
      invalidatesTags: ['Course'],
    }),

    enrollInCourse: builder.mutation({
      /** @param {EnrollReq} arg */
      async queryFn(arg) {
        try {
          const res = await courseApi.postApiCourseEnroll(arg);
          return { data: res.data };
        } catch (error) {
          return { error };
        }
      },
      invalidatesTags: ['Course'],
    }),

    continueCourse: builder.mutation({
      /** @param {ContinueReq} arg */
      async queryFn(arg) {
        try {
          const res = await courseApi.postApiCourseContinue(arg);
          return { data: res.data };
        } catch (error) {
          return { error };
        }
      },
    }),

    publishCourse: builder.mutation({
      /** @param {CourseIdReq} arg */
      async queryFn(arg) {
        try {
          const res = await courseApi.postApiCoursePublish(arg);
          return { data: res.data };
        } catch (error) {
          return { error };
        }
      },
      invalidatesTags: (result, error, arg) => [{ type: 'Course', id: arg.courseId }],
    }),

    restoreCourse: builder.mutation({
      /** @param {CourseIdReq} arg */
      async queryFn(arg) {
        try {
          const res = await courseApi.postApiCourseRestore(arg);
          return { data: res.data };
        } catch (error) {
          return { error };
        }
      },
      invalidatesTags: ['Course'],
    }),
  }),
});

export const {
  useGetAllCoursesQuery,
  useGetCourseByIdQuery,
  useGetInstructorCoursesQuery,
  useGetUnfinishedCoursesQuery,
  useGetEnrolledCoursesQuery,
  useGetCourseModulesQuery,
  useGetModuleByIdQuery,
  useGetCategoriesQuery,
  useCreateCourseMutation,
  useUpdateCourseMutation,
  useDeleteCourseMutation,
  useEnrollInCourseMutation,
  useContinueCourseMutation,
  usePublishCourseMutation,
  useRestoreCourseMutation,
} = courseApiSlice;