import { useAuth } from '../../context/auth/AuthContext';
import { BookOpen, PlayCircle, Info } from 'lucide-react';
import { NavLink } from 'react-router-dom';

const StudentDashboard = () => {
  const { user } = useAuth();

  return (
    <div className="w-full max-w-7xl mx-auto py-10 px-4 sm:px-6 lg:px-8">

      {/* HEADER */}
      <div className="mb-10">
        <h1 className="text-3xl font-bold text-slate-900 dark:text-white">
          Welcome back, {user?.name}
        </h1>
        <p className="mt-2 text-slate-500 dark:text-slate-400 max-w-2xl">
          This is your learning dashboard. From here, you can explore courses
          and continue your learning journey.
        </p>
      </div>

      {/* ACTIONS */}
      <div className="grid sm:grid-cols-2 gap-6 mb-12">
        <ActionCard
          icon={<BookOpen className="w-6 h-6 text-indigo-600" />}
          title="Browse Courses"
          desc="Explore available courses and enroll."
          to="/courses"
        />

        <ActionCard
          icon={<PlayCircle className="w-6 h-6 text-emerald-600" />}
          title="Continue Learning"
          desc="Resume your enrolled courses and modules."
          to="/courses"
        />
      </div>

      {/* STATUS */}
      <section className="mb-10">
        <h2 className="text-xl font-bold text-slate-800 dark:text-white mb-4">
          Your Learning Status
        </h2>

        <div className="bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-2xl p-6">
          <p className="text-slate-600 dark:text-slate-400">
            Your progress is tracked automatically as you complete modules
            and quizzes. Detailed analytics will be introduced in future updates.
          </p>
        </div>
      </section>

      {/* PLATFORM INFO */}
      <div className="flex items-start gap-4 bg-indigo-50 dark:bg-indigo-950/40
        border border-indigo-100 dark:border-indigo-900 rounded-2xl p-6">
        <Info className="w-6 h-6 text-indigo-600 dark:text-indigo-400 mt-1" />
        <p className="text-sm text-slate-600 dark:text-slate-400">
          This platform is actively evolving. New learning features will be
          added incrementally without disrupting your progress.
        </p>
      </div>
    </div>
  );
};

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
    <p className="text-sm text-slate-600 dark:text-slate-400">
      {desc}
    </p>
  </NavLink>
);

export default StudentDashboard;
