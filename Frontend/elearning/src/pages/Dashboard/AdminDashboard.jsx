import { Users, BookOpen } from 'lucide-react';
import { NavLink } from 'react-router-dom';

const AdminDashboard = () => {
  return (
    <div className="w-full max-w-7xl mx-auto py-12 px-4 sm:px-6 lg:px-8">

      {/* HEADER - Centered to match the new 2-card vibe */}
      <div className="mb-12 border-b border-slate-200 dark:border-slate-800 pb-8 text-center">
        <h1 className="text-3xl font-bold text-slate-900 dark:text-white">
          Admin Dashboard
        </h1>
        <p className="mt-2 text-slate-500 dark:text-slate-400 mx-auto max-w-xl">
          Platform administration, moderation, and access control.
        </p>
      </div>

      {/* ACTION CARDS - Re-centered 2-Column Layout */}
      <div className="grid sm:grid-cols-2 gap-8 max-w-4xl mx-auto">
        <ActionCard
          icon={<Users className="w-6 h-6 text-indigo-600" />}
          title="Manage Users"
          desc="View platform users, verify identities, and update system roles."
          to="/admin/users"
        />

        <ActionCard
          icon={<BookOpen className="w-6 h-6 text-emerald-600" />}
          title="Manage Courses"
          desc="Review submitted content, delete violations, or restore courses."
          to="/admin/courses"
        />
      </div>

      {/* INFO FOOTER */}
      <div className="mt-12 max-w-4xl mx-auto bg-slate-50 dark:bg-slate-900/50 
        border border-slate-200 dark:border-slate-800 
        rounded-2xl p-6 text-sm text-slate-600 dark:text-slate-400 text-center">
        <p>
          User deletion is intentionally restricted. Role management and 
          content moderation are the primary administrative controls.
        </p>
      </div>
    </div>
  );
};

const ActionCard = ({ icon, title, desc, to }) => (
  <NavLink
    to={to}
    className="group bg-white dark:bg-[#0f172a] 
      border border-slate-200 dark:border-slate-800 
      rounded-2xl p-8 hover:border-indigo-500 
      transition-all shadow-sm hover:shadow-xl hover:-translate-y-1"
  >
    <div className="flex flex-col items-center text-center gap-4">
      <div className="p-4 rounded-2xl bg-slate-100 dark:bg-slate-800 group-hover:bg-indigo-500/10 transition-colors">
        {icon}
      </div>
      <div>
        <h3 className="text-lg font-bold text-slate-900 dark:text-white group-hover:text-indigo-600 transition-colors">
          {title}
        </h3>
        <p className="mt-2 text-sm text-slate-600 dark:text-slate-400 leading-relaxed">
          {desc}
        </p>
      </div>
    </div>
  </NavLink>
);

export default AdminDashboard;