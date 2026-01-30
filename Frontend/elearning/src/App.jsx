import { Routes, Route, Navigate, Outlet } from 'react-router-dom';
import { useSelector } from 'react-redux';

import Layout from './components/Layout/Layout';
import { Loader } from './components/ui/Loader';

import { GlobalLoader } from './components/ui/GlobalLoader';
import { GlobalToasts } from './components/ui/GlobalToasts';
import { ThemeInit } from './components/ui/ThemeInit';


import Home from './pages/home';
import Login from './pages/auth/login';
import Signup from './pages/auth/Signup';
import Dashboard from './pages/Dashboard/Dashboard';
import AuthCallback from './pages/auth/AuthCallback';
import CompleteProfile from './pages/auth/CompleteProfile';
import BrowseCourses from './pages/courses/BrowseCourses';
import CourseDetails from './pages/courses/CourseDetails';
import ModuleDetails from './pages/courses/ModuleDetails';
import CreateCourse from './pages/Instructor/CreateCourse';
import EditCourse from './pages/Instructor/EditCourse';
import MyLearningPage from './pages/courses/MyLearning';
import QuizPage from './pages/assessment/Take-quiz';
import QuizResult from './pages/assessment/QuizResult';
import Pending from './pages/Instructor/Pending';
import AddQuiz from './pages/assessment/AddQuiz';
import ManageUsers from './pages/Admin/ManageUsers';
import ManageCourses from './pages/Admin/ManageCourses';

/* ---------------------------------------
   PROTECTED ROUTE LAYOUT (DUMB + SAFE)
--------------------------------------- */
const ProtectedLayout = () => {
  const { authChecked, status } = useSelector((state) => state.auth);

  if (!authChecked) {
    return <Loader />;
  }

  if (status !== 'authenticated') {
    return <Navigate to="/login" replace />;
  }

  return <Outlet />;
};

function App() {
  return (
    <Layout>
      <ThemeInit />
      <GlobalLoader />
      <GlobalToasts />

      <Routes>
        {/* PUBLIC ROUTES */}
        <Route path="/" element={<Home />} />
        <Route path="/login" element={<Login />} />
        <Route path="/sign-up" element={<Signup />} />
        <Route path="/auth/callback" element={<AuthCallback />} />
        <Route path="/complete-profile" element={<CompleteProfile />} />

        {/* PROTECTED ROUTES */}
        <Route element={<ProtectedLayout />}>
          <Route path="/dashboard" element={<Dashboard />} />
          <Route path="/courses" element={<BrowseCourses />} />
          <Route path="/courses/:id" element={<CourseDetails />} />
          <Route
            path="/courses/:courseId/modules/:moduleId"
            element={<ModuleDetails />}
          />
          <Route path="/instructor/create-course" element={<CreateCourse />} />
          <Route
            path="/instructor/courses/:courseId/edit"
            element={<EditCourse />}
          />
          <Route path="/my-learning" element={<MyLearningPage />} />
          <Route
            path="/courses/:CourseId/modules/:ModuleId/quiz"
            element={<QuizPage />}
          />
          <Route
            path="/assessment/:CourseId/:ModuleId/result/:submissionId"
            element={<QuizResult />}
          />
          <Route path="/instructor/pending" element={<Pending />} />
          <Route
            path="/assessment/add-quiz/:courseId"
            element={<AddQuiz />}
          />
          <Route path="/admin/users" element={<ManageUsers />} />
          <Route path="/admin/courses" element={<ManageCourses />} />
        </Route>

        {/* FALLBACK */}
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </Layout>
  );
}

export default App;
