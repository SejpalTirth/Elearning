import React from 'react';
import {
  useGetAllCoursesQuery,
  useGetCategoriesQuery,
} from '../../services/CourseApiSlice';
import CourseCard from './CourseCard';

const BrowseCourses = () => {
  const { data: courses = [], isLoading, isError } = useGetAllCoursesQuery();
  const { data: categories = [] } = useGetCategoriesQuery();

  const categoryMap = React.useMemo(() => {
    return Object.fromEntries(
      categories.map((cat) => [cat.id, cat.name])
    );
  }, [categories]);

  // FILTER LOGIC: Only show published, non-deleted courses to students
  const visibleCourses = React.useMemo(() => {
    return courses.filter(course => !course.isDeleted && !course.isDraft);
  }, [courses]);

  if (isLoading) {
    return (
      <div className="flex justify-center items-center h-64 text-slate-500">
        <div className="animate-pulse font-bold tracking-widest text-xs uppercase">
          Loading catalog...
        </div>
      </div>
    );
  }

  if (isError) {
    return (
      <div className="text-red-600 dark:text-red-400 text-center py-20">
        Failed to load courses. Please try again later.
      </div>
    );
  }

  return (
    <div className="px-6 py-10 max-w-7xl mx-auto">
      <div className="mb-10">
        <h1 className="text-3xl font-bold text-slate-900 dark:text-white tracking-tight">
          Browse Courses
        </h1>
        <p className="text-slate-500 mt-2">Explore our library of expert-led content.</p>
      </div>

      {visibleCourses.length > 0 ? (
        <div className="grid gap-8 sm:grid-cols-2 lg:grid-cols-3">
          {visibleCourses.map((course) => (
            <CourseCard
              key={course.id}
              course={course}
              categoryName={categoryMap[course.categoryId]}
            />
          ))}
        </div>
      ) : (
        <div className="text-center py-20 bg-slate-50 dark:bg-slate-900/50 rounded-3xl border border-dashed border-slate-200 dark:border-slate-800">
          <p className="text-slate-500 font-medium">No courses available at the moment.</p>
        </div>
      )}
    </div>
  );
};

export default BrowseCourses;