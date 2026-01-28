import {
  PlusCircle,
  BookOpen,
  AlertTriangle,
  Edit,
} from 'lucide-react';
import { NavLink } from 'react-router-dom';
import { useGetInstructorCoursesQuery } from '../../services/CourseApiSlice';

const InstructorDashboard = () => {
  const {
    data: courses = [],
    isLoading,
    isError,
  } = useGetInstructorCoursesQuery();

  if (isLoading) {
    return (
      <div className="flex justify-center items-center h-64 text-slate-500 dark:text-slate-400">
        Loading instructor dashboard...
      </div>
    );
  }

  if (isError) {
    return (
      <div className="text-center text-red-500">
        Failed to load instructor courses
      </div>
    );
  }

  /* -----------------------------
     STATS
  ----------------------------- */

  const totalCourses = courses.length;

  const publishedCount = courses.filter(
    (c) => !c.isDraft && !c.isDeleted
  ).length;

  const draftCount = courses.filter(
    (c) => c.isDraft && !c.isDeleted
  ).length;

  const deletedCount = courses.filter(
    (c) => c.isDeleted
  ).length;

  return (
    <div className="w-full max-w-7xl mx-auto py-10 px-4 sm:px-6 lg:px-8">

      {/* HERO */}
      <div className="mb-12">
        <h1 className="text-3xl font-bold text-slate-900 dark:text-white">
          Instructor Dashboard
        </h1>
        <p className="mt-2 text-slate-500 dark:text-slate-400 max-w-2xl">
          Create, manage, and review your courses.
        </p>
      </div>

      {/* QUICK ACTIONS */}
      <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-6 mb-12">

        <ActionCard
          icon={<PlusCircle className="w-6 h-6 text-indigo-600" />}
          title="Create Course"
          desc="Start building a new course."
          to="/instructor/create-course"
        />

        <ActionCard
          icon={<BookOpen className="w-6 h-6 text-emerald-600" />}
          title="Browse Courses"
          desc="Explore and enroll as a learner."
          to="/courses"
        />

        <ActionCard
          icon={<AlertTriangle className="w-6 h-6 text-amber-600" />}
          title="Pending Tasks"
          desc="Finish quizzes and publish your courses."
          to="/instructor/pending"
        />


        <div className="sm:col-span-2 lg:col-span-3 flex justify-center">
          <div className="w-full sm:w-1/2 lg:w-1/3">
            <StatCard
              label="My Courses"
              value={totalCourses}
            />
          </div>
        </div>

      </div>

      {/* MINI STATS */}
      <div className="grid sm:grid-cols-3 gap-6 mb-12">
        <MiniStat label="Published" value={publishedCount} />
        <MiniStat label="Drafts" value={draftCount} />
        <MiniStat label="Deleted" value={deletedCount} />
      </div>

      {/* COURSE TABLE */}
      <section>
        <h2 className="text-xl font-bold text-slate-800 dark:text-white mb-4">
          My Courses
        </h2>

        <div className="bg-white dark:bg-slate-900 border
          border-slate-200 dark:border-slate-800
          rounded-2xl overflow-hidden">

          {/* HEADER */}
          <div className="grid grid-cols-12 px-6 py-3 text-sm
            text-slate-500 dark:text-slate-400 border-b">
            <div className="col-span-6">Course</div>
            <div className="col-span-3">Status</div>
            <div className="col-span-3 text-right">Action</div>
          </div>

          {/* ROWS */}
          {courses.map((course) => (
            <Row key={course.id} course={course} />
          ))}

          {courses.length === 0 && (
            <div className="p-6 text-sm text-slate-500 text-center">
              No courses created yet.
            </div>
          )}
        </div>
      </section>

      {/* INFO */}
      <div className="mt-8 bg-indigo-50 dark:bg-indigo-950/40
        border border-indigo-100 dark:border-indigo-900
        rounded-2xl p-6 text-sm text-slate-600 dark:text-slate-400">
        Deleted courses cannot be published until restored by an administrator.
      </div>
    </div>
  );
};

/* ---------- COMPONENTS ---------- */

const ActionCard = ({ icon, title, desc, to }) => (
  <NavLink
    to={to}
    className="group bg-white dark:bg-slate-900
      border border-slate-200 dark:border-slate-800
      rounded-2xl p-6 hover:border-indigo-500
      transition-all shadow-sm hover:shadow-md"
  >
    <div className="flex items-center gap-4 mb-4">
      <div className="p-3 rounded-xl bg-slate-100 dark:bg-slate-800">
        {icon}
      </div>
      <h3 className="font-bold text-slate-900 dark:text-white
        group-hover:text-indigo-600">
        {title}
      </h3>
    </div>
    <p className="text-sm text-slate-600 dark:text-slate-400">
      {desc}
    </p>
  </NavLink>
);

const StatCard = ({ label, value }) => (
  <div className="bg-white dark:bg-slate-900 border
    border-slate-200 dark:border-slate-800
    rounded-2xl p-6 shadow-sm">
    <p className="text-sm text-slate-500 dark:text-slate-400">
      {label}
    </p>
    <p className="text-2xl font-bold text-slate-900 dark:text-white mt-1">
      {value}
    </p>
  </div>
);

const MiniStat = ({ label, value }) => (
  <div className="bg-slate-50 dark:bg-slate-900 border
    border-slate-200 dark:border-slate-800
    rounded-xl p-4 text-center">
    <p className="text-sm text-slate-500">{label}</p>
    <p className="text-xl font-bold text-slate-900 dark:text-white">
      {value}
    </p>
  </div>
);

const Row = ({ course }) => {
  let statusLabel = 'Published';
  let badgeClasses =
    'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/40 dark:text-emerald-300';

  if (course.isDeleted) {
    statusLabel = 'Deleted';
    badgeClasses =
      'bg-red-100 text-red-700 dark:bg-red-900/40 dark:text-red-300';
  } else if (course.isDraft) {
    statusLabel = 'Draft';
    badgeClasses =
      'bg-amber-100 text-amber-700 dark:bg-amber-900/40 dark:text-amber-300';
  }

  return (
    <div
      className={`grid grid-cols-12 px-6 py-4 items-center
        border-b border-slate-100 dark:border-slate-800
        ${course.isDeleted ? 'bg-red-50 dark:bg-red-950/30' : ''}`}
    >
      <div className="col-span-6 font-medium text-slate-900 dark:text-white">
        {course.title}
      </div>

      <div className="col-span-3">
        <span
          className={`inline-flex items-center gap-2 text-xs px-3 py-1 rounded-full ${badgeClasses}`}
        >
          {course.isDeleted && <AlertTriangle className="w-3 h-3" />}
          {statusLabel}
        </span>
      </div>

      <div className="col-span-3 text-right">
        {!course.isDeleted && (
          <NavLink
            to={`/instructor/courses/${course.id}/edit`}
            className="inline-flex items-center gap-1
              text-indigo-600 dark:text-indigo-400 hover:underline"
          >
            <Edit className="w-4 h-4" />
            Edit
          </NavLink>
        )}
      </div>
    </div>
  );
};

export default InstructorDashboard;
