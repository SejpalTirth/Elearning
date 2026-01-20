import { PlusCircle, BookOpen, Edit, AlertTriangle } from 'lucide-react';
import { NavLink } from 'react-router-dom';

const InstructorDashboard = () => {
  const courseCount = 5;
  const publishedCount = 3;
  const draftCount = 2;

  return (
    <div className="w-full max-w-7xl mx-auto py-10 px-4 sm:px-6 lg:px-8">

      {/* HERO */}
      <div className="mb-12">
        <h1 className="text-3xl font-bold text-slate-900 dark:text-white">
          Instructor Dashboard
        </h1>
        <p className="mt-2 text-slate-500 dark:text-slate-400 max-w-2xl">
          Create, manage, and review your courses — and explore the platform as a learner.
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
          desc="Explore and enroll in available courses."
          to="/courses"
        />

        <StatCard
          label="My Courses"
          value={courseCount ?? '--'}
        />

      </div>

      {/* COURSE OVERVIEW */}
      <div className="grid sm:grid-cols-3 gap-6 mb-12">
        <MiniStat label="Published" value={publishedCount ?? '--'} />
        <MiniStat label="Drafts" value={draftCount ?? '--'} />
        <MiniStat label="Deleted" value="1" />
      </div>

      {/* COURSE TABLE */}
      <section>
        <h2 className="text-xl font-bold text-slate-800 dark:text-white mb-4">
          My Courses
        </h2>

        <div className="bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-2xl overflow-hidden">

          <div className="grid grid-cols-12 px-6 py-3 text-sm
            text-slate-500 dark:text-slate-400 border-b">
            <div className="col-span-6">Course</div>
            <div className="col-span-3">Status</div>
            <div className="col-span-3 text-right">Action</div>
          </div>

          <Row title="React Fundamentals" status="Draft" />
          <Row title="Advanced JavaScript" status="Deleted" />
        </div>
      </section>

      {/* INFO */}
      <div className="mt-8 bg-indigo-50 dark:bg-indigo-950/40
        border border-indigo-100 dark:border-indigo-900
        rounded-2xl p-6 text-sm text-slate-600 dark:text-slate-400">
        Deleted courses can be edited for review, but cannot be published
        until restored by an administrator.
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
      <h3 className="font-bold text-slate-900 dark:text-white group-hover:text-indigo-600">
        {title}
      </h3>
    </div>
    <p className="text-sm text-slate-600 dark:text-slate-400">{desc}</p>
  </NavLink>
);

const StatCard = ({ label, value }) => (
  <div className="bg-white dark:bg-slate-900 border border-slate-200
    dark:border-slate-800 rounded-2xl p-6 shadow-sm">
    <p className="text-sm text-slate-500 dark:text-slate-400">{label}</p>
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
    <p className="text-xl font-bold text-slate-900 dark:text-white">{value}</p>
  </div>
);

const Row = ({ title, status }) => (
  <div className={`grid grid-cols-12 px-6 py-4 items-center
    border-b border-slate-100 dark:border-slate-800
    ${status === 'Deleted' ? 'bg-red-50 dark:bg-red-950/30' : ''}`}>
    <div className="col-span-6 font-medium text-slate-900 dark:text-white">
      {title}
    </div>
    <div className="col-span-3">
      {status === 'Deleted' ? (
        <span className="inline-flex items-center gap-2 text-xs
          bg-red-100 text-red-700 dark:bg-red-900/40 dark:text-red-300
          px-3 py-1 rounded-full">
          <AlertTriangle className="w-3 h-3" /> Deleted
        </span>
      ) : (
        <span className="inline-flex text-xs
          bg-amber-100 text-amber-700 dark:bg-amber-900/40 dark:text-amber-300
          px-3 py-1 rounded-full">
          Draft
        </span>
      )}
    </div>
    <div className="col-span-3 text-right text-indigo-600 dark:text-indigo-400">
      Edit
    </div>
  </div>
);

export default InstructorDashboard;
