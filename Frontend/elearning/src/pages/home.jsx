import { NavLink } from 'react-router-dom';
import { Users, ArrowRight, Shield, BookOpen } from 'lucide-react';
import { useSelector } from 'react-redux';
const Home = () => {
  const isAuthenticated = useSelector((state) => state.auth.isAuthenticated);

  return (
    <div className="min-h-screen bg-white dark:bg-slate-900 transition-colors duration-500">

      {/* HERO SECTION */}
      <section className="relative pt-20 pb-24 lg:pt-32 overflow-hidden">
        <div className="absolute inset-0 pointer-events-none">
          <div className="absolute -top-24 -left-24 w-96 h-96 bg-indigo-200/30 dark:bg-indigo-900/20 rounded-full blur-[120px]" />
          <div className="absolute bottom-0 right-0 w-80 h-80 bg-sky-200/30 dark:bg-sky-900/20 rounded-full blur-[100px]" />
        </div>

        <div className="relative z-10 max-w-7xl mx-auto px-6 text-center">
          <h1 className="text-4xl lg:text-6xl font-extrabold text-slate-900 dark:text-white tracking-tight">
            Learn. Build. Grow. <br />
            <span className="text-indigo-600 dark:text-indigo-400">
              At Your Own Pace
            </span>
          </h1>

          <p className="mt-6 max-w-2xl mx-auto text-lg text-slate-600 dark:text-slate-400">
            A modern learning platform focused on clarity, structure, and real skill development.
          </p>

          <div className="mt-10 flex flex-col sm:flex-row justify-center gap-4">
            <NavLink
              to={isAuthenticated ? '/dashboard' : '/login'}
              className="px-8 py-4 bg-indigo-600 hover:bg-indigo-700 text-white font-bold rounded-xl
              shadow-lg transition-all flex items-center justify-center gap-2 group"
            >
              {isAuthenticated ? 'Go to Dashboard' : 'Get Started'}
              <ArrowRight className="w-5 h-5 group-hover:translate-x-1 transition-transform" />
            </NavLink>

            <NavLink
              to="/courses"
              className="px-8 py-4 bg-white dark:bg-slate-800 text-slate-700 dark:text-slate-200
              font-bold rounded-xl border border-slate-200 dark:border-slate-700
              hover:bg-slate-50 dark:hover:bg-slate-700 transition-all"
            >
              Explore Courses
            </NavLink>
          </div>
        </div>
      </section>

      {/* WHAT YOU CAN DO */}
      <section className="py-20 bg-slate-50 dark:bg-slate-800/40">
        <div className="max-w-7xl mx-auto px-6">
          <div className="text-center mb-16">
            <h2 className="text-3xl font-bold text-slate-900 dark:text-white">
              Everything You Need to Learn
            </h2>
            <p className="mt-4 text-slate-500 dark:text-slate-400 max-w-2xl mx-auto">
              Designed to support focused learning without unnecessary distractions.
            </p>
          </div>

          <div className="grid md:grid-cols-3 gap-8">
            <FeatureCard
              icon={<BookOpen className="w-6 h-6 text-indigo-500" />}
              title="Structured Courses"
              desc="Clear modules, logical flow, and easy navigation through content."
            />
            <FeatureCard
              icon={<Users className="w-6 h-6 text-sky-500" />}
              title="Learner-Centered"
              desc="Built for students, professionals, and self-paced learners."
            />
            <FeatureCard
              icon={<Shield className="w-6 h-6 text-emerald-500" />}
              title="Secure Platform"
              desc="Authentication, protected routes, and privacy-first architecture."
            />
          </div>
        </div>
      </section>

      {/* HOW IT WORKS */}
      <section className="py-20">
        <div className="max-w-7xl mx-auto px-6">
          <div className="text-center mb-16">
            <h2 className="text-3xl font-bold text-slate-900 dark:text-white">
              How It Works
            </h2>
            <p className="mt-4 text-slate-500 dark:text-slate-400">
              A simple flow designed to keep you moving forward.
            </p>
          </div>

          <div className="grid md:grid-cols-3 gap-10">
            <StepCard
              step="01"
              title="Create an Account"
              desc="Sign up and personalize your learning experience."
            />
            <StepCard
              step="02"
              title="Explore Courses"
              desc="Browse content and enroll based on your interests."
            />
            <StepCard
              step="03"
              title="Track Your Progress"
              desc="Continue where you left off and grow consistently."
            />
          </div>
        </div>
      </section>

        {/* FINAL CTA */}
        <section className="py-24 
        bg-gradient-to-b from-indigo-500/80 to-indigo-600/70 
        dark:from-indigo-600/40 dark:to-indigo-700/30
        text-white transition-colors">

        <div className="max-w-5xl mx-auto px-6 text-center">
            <h2 className="text-4xl font-extrabold mb-6">
            Built for Real Learning
            </h2>

            <p className="text-indigo-100/90 dark:text-indigo-200/80 text-lg mb-10">
            No inflated numbers. No empty promises.  
            Just a platform designed to help you learn effectively.
            </p>

            <NavLink
            to={isAuthenticated ? '/dashboard' : '/login'}
            className="inline-flex items-center gap-3 px-10 py-4
                bg-white/95 text-indigo-700
                font-bold rounded-xl shadow-md
                hover:bg-white transition"
            >
            {isAuthenticated ? 'Continue Learning' : 'Start Learning'}
            <ArrowRight className="w-5 h-5" />
            </NavLink>
        </div>
        </section>

    </div>
  );
};

/* COMPONENTS */

const FeatureCard = ({ icon, title, desc }) => (
  <div
    className="p-8 rounded-2xl bg-white dark:bg-slate-900
    border border-slate-200 dark:border-slate-800
    hover:border-indigo-500/50 transition-all shadow-sm hover:shadow-xl"
  >
    <div className="w-12 h-12 rounded-lg bg-slate-50 dark:bg-slate-800
      flex items-center justify-center mb-6">
      {icon}
    </div>
    <h3 className="text-xl font-bold text-slate-900 dark:text-white mb-3">
      {title}
    </h3>
    <p className="text-slate-600 dark:text-slate-400 text-sm leading-relaxed">
      {desc}
    </p>
  </div>
);

const StepCard = ({ step, title, desc }) => (
  <div className="p-8 rounded-2xl bg-white dark:bg-slate-900
    border border-slate-200 dark:border-slate-800">
    <p className="text-indigo-600 dark:text-indigo-400 font-bold mb-2">
      Step {step}
    </p>
    <h3 className="text-xl font-bold text-slate-900 dark:text-white mb-3">
      {title}
    </h3>
    <p className="text-slate-600 dark:text-slate-400 text-sm">
      {desc}
    </p>
  </div>
);

export default Home;
