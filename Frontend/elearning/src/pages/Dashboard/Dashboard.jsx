import { useSelector } from 'react-redux';
import StudentDashboard from './StudentDashboard';
import InstructorDashboard from './InstructorDashboard';
import AdminDashboard from './AdminDashboard';
import { Loader } from '../../components/ui/Loader';

const Dashboard = () => {
  const user = useSelector((state) => state.auth.user);

  if (!user) {
    return <Loader />;
  }

  switch (user.role) {
    case 'Instructor':
      return <InstructorDashboard />;

    case 'Admin':
      return <AdminDashboard />;

    default:
      return <StudentDashboard />;
  }
};

export default Dashboard;
