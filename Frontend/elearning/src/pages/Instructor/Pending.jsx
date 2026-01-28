import { useSelector } from 'react-redux';
import { useGetUnfinishedCoursesQuery } from '../../services/CourseApiSlice';
import PendingCourseCard from './components/PendingCourseCard';

const Pending = () => {
  const user = useSelector((state) => state.auth.user);

  const {
    data: courses = [],
    isLoading,
    isError,
  } = useGetUnfinishedCoursesQuery(undefined, {
    skip: !user,
  });

  if (!user) return null;

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-[60vh] text-slate-500 dark:text-slate-400">
        Loading pending courses...
      </div>
    );
  }

  if (isError) {
    return (
      <div className="flex items-center justify-center min-h-[60vh] text-red-500">
        Failed to load pending courses
      </div>
    );
  }

  // FIXED: Changed c.isPending to c.isDraft to match your API response
  const pendingCourses = courses.filter(
    (c) => !c.isDeleted && c.isDraft
  );

  return (
    <div className="w-full max-w-6xl mx-auto py-10 px-4 sm:px-6 lg:px-8">
      <div className="mb-8">
        <h2 className="text-2xl font-bold text-slate-900 dark:text-white">
          Pending Tasks
        </h2>
        <p className="mt-1 text-slate-500 dark:text-slate-400">
          Complete quizzes and publish your courses.
        </p>
      </div>

      {pendingCourses.length === 0 ? (
        <div className="flex items-center justify-center min-h-[50vh]">
          <div className="bg-white/70 dark:bg-slate-900/70 backdrop-blur border border-slate-200 dark:border-slate-800 rounded-2xl px-10 py-8 text-center shadow-sm">
            <h3 className="text-lg font-semibold text-slate-800 dark:text-slate-100">
              All caught up 🎉
            </h3>
            <p className="mt-2 text-slate-600 dark:text-slate-400">
              No pending courses right now.
            </p>
          </div>
        </div>
      ) : (
        <div className="grid gap-6">
          {pendingCourses.map((course) => (
            <PendingCourseCard key={course.id} course={course} />
          ))}
        </div>
      )}
    </div>
  );
};

export default Pending;