import { Routes, Route, Navigate } from 'react-router-dom';
import Layout from './components/Layout/Layout';
import Login from './pages/auth/login';
import Signup from './pages/auth/Signup';
import CompleteProfile from './pages/auth/CompleteProfile';
import AuthCallback from './pages/auth/AuthCallback';
import Home from './pages/home';
import Dashboard from './pages/Dashboard/Dashboard';

function App() {
  return (
    <Layout>
      <Routes>
        <Route path="/login" element={<Login />} />
        <Route path="/sign-up" element={<Signup />} />
        <Route path="/auth/callback" element={<AuthCallback />} />
        <Route path='/complete-profile' element={<CompleteProfile />} />
        <Route path="/" element={<Home />} />
        <Route path='/dashboard' element={<Dashboard />} />

        <Route path="*" element={<Navigate to="/login" replace />} />
      </Routes>
    </Layout>
  );
}

export default App;
