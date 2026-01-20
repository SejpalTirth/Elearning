import { Users, BookOpen, Shield } from 'lucide-react';
import { NavLink } from 'react-router-dom';

const AdminDashboard = () => {
  return (
    <div className="w-full max-w-7xl mx-auto py-10 px-4 sm:px-6 lg:px-8">

      {/* HEADER */}
      <div className="mb-10 border-b border-slate-200 dark:border-slate-800 pb-6">
        <h1 className="text-3xl font-bold text-slate-900 dark:text-white">
            Admin Dashboard
        </h1>
        <p className="mt-2 text-slate-500 dark:text-slate-400 max-w-xl">
            Platform administration, moderation, and access control.
        </p>
       </div>


      {/* ACTION CARDS */}
      <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-6">

        <ActionCard
          icon={<Users className="w-6 h-6 text-indigo-600" />}
          title="Manage Users"
          desc="View users and update roles."
          to="/admin/users"
        />

        <ActionCard
          icon={<BookOpen className="w-6 h-6 text-emerald-600" />}
          title="Manage Courses"
          desc="Review, delete, or restore courses."
          to="/admin/courses"
        />

        <ActionCard
          icon={<Shield className="w-6 h-6 text-amber-600" />}
          title="Platform Policies"
          desc="Content and moderation guidelines."
          to="/admin/policies"
        />

      </div>

      {/* INFO */}
      <div className="mt-10 bg-slate-50 dark:bg-slate-900
        border border-slate-200 dark:border-slate-800
        rounded-2xl p-6 text-sm text-slate-600 dark:text-slate-400">
        User deletion is intentionally restricted. Role management and
        content moderation are the primary administrative controls.
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

export default AdminDashboard;
