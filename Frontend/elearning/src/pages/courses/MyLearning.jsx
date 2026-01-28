import React from 'react';
import CourseCard from './CourseCard';
import { useGetEnrolledCoursesQuery } from '../../services/CourseApiSlice';
import { Loader2, BookOpen } from 'lucide-react';
import { useNavigate } from 'react-router-dom';

const MyLearningPage = () => {
  const navigate = useNavigate();
  
  const { data: enrolledCourses, isLoading, isError } = useGetEnrolledCoursesQuery();

  if (isLoading) {
    return (
      <div className="flex flex-col items-center justify-center min-h-[60vh] space-y-4">
        <Loader2 className="w-10 h-10 text-indigo-600 animate-spin" />
        <p className="text-slate-500 font-medium">Loading your classroom...</p>
      </div>
    );
  }

  if (isError) {
    return (
      <div className="text-center py-20">
        <p className="text-red-500">Failed to load enrolled courses. Please try again later.</p>
      </div>
    );
  }

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-10">
      {/* Header Section */}
      <div className="mb-10">
        <h1 className="text-3xl font-bold text-slate-900 dark:text-white">
          My Learning
        </h1>
        <p className="text-slate-600 dark:text-slate-400 mt-2">
          Pick up right where you left off.
        </p>
      </div>

      {/* Empty State (User has no courses yet) */}
      {enrolledCourses?.length === 0 ? (
        <div className="flex flex-col items-center justify-center py-20 bg-slate-50 dark:bg-slate-900/50 rounded-3xl border-2 border-dashed border-slate-200 dark:border-slate-800">
          <BookOpen className="w-16 h-16 text-slate-300 mb-4" />
          <h2 className="text-xl font-semibold text-slate-800 dark:text-slate-200">No courses found</h2>
          <p className="text-slate-500 dark:text-slate-400 mb-6">You haven't enrolled in any courses yet.</p>
          <button
            onClick={() => navigate('/courses')}
            className="px-6 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition"
          >
            Browse Courses
          </button>
        </div>
      ) : (
        /* Course Grid */
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-8">
          {enrolledCourses.map((course) => (
            <CourseCard 
              key={course.id} 
              course={course} 
              categoryName={course.category?.name} 
            />
          ))}
        </div>
      )}
    </div>
  );
};

export default MyLearningPage;