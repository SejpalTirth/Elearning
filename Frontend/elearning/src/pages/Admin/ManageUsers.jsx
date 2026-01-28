import { useState } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { 
  useGetAllUsersQuery, 
  useGetAllRolesQuery, 
  useUpdateUserRoleMutation 
} from '../../services/usersapislice';
import { useLogoutMutation } from '../../services/authapislice';
import { showToast } from '../../features/ui/toastSlice';
import { Search, UserCog, Mail, X, Shield, Loader2, CheckCircle } from 'lucide-react';

const ManageUsers = () => {
  const dispatch = useDispatch();
  const { user: currentUser } = useSelector((state) => state.auth);
  
  const { data: users = [], isLoading: loadingUsers } = useGetAllUsersQuery();
  const { data: roles = [] } = useGetAllRolesQuery();
  const [updateUserRole, { isLoading: isUpdating }] = useUpdateUserRoleMutation();
  const [logout] = useLogoutMutation();

  const [searchTerm, setSearchTerm] = useState('');
  const [selectedUser, setSelectedUser] = useState(null);
  const [selectedRoleId, setSelectedRoleId] = useState('');

  const filteredUsers = users.filter(u => 
    u.name.toLowerCase().includes(searchTerm.toLowerCase()) || 
    u.email.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const handleOpenModal = (user) => {
    setSelectedUser(user);
    const currentRole = roles.find(r => r.name?.toLowerCase() === user.role?.toLowerCase());
    setSelectedRoleId(currentRole?.id || '');
  };

  const handleUpdateRole = async () => {
    if (!selectedUser || !selectedRoleId) return;
    try {
      const res = await updateUserRole({
        userId: selectedUser.id,
        roleId: Number(selectedRoleId)
      }).unwrap();

      dispatch(showToast({ message: res?.message || "Role updated successfully", type: 'success' }));

      if (selectedUser.id === currentUser?.userId) {
        dispatch(showToast({ message: "Role changed. Logging out...", type: 'info' }));
        setTimeout(() => logout(), 2000);
      } else {
        setSelectedUser(null);
      }
    } catch (err) {
      dispatch(showToast({ message: err?.data?.message || "Update failed", type: 'error' }));
    }
  };

  return (
    <div className="w-full max-w-7xl mx-auto py-10 px-6 min-h-screen transition-colors duration-300">
      {/* HEADER & SEARCH */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-6 mb-10">
        <div>
          <h1 className="text-3xl font-extrabold text-slate-900 dark:text-white tracking-tight">Manage Users</h1>
          <p className="text-slate-500 dark:text-slate-400 mt-1 font-medium">Administrative control & permission management.</p>
        </div>

        <div className="relative group">
          <Search className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400 group-focus-within:text-indigo-500 transition-colors" size={20} />
          <input 
            type="text" 
            placeholder="Search by name or email..." 
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full md:w-96 pl-12 pr-6 py-3 rounded-2xl border border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900 text-slate-900 dark:text-white focus:ring-4 focus:ring-indigo-500/10 outline-none transition-all shadow-sm"
          />
        </div>
      </div>

      {/* USERS TABLE CONTAINER */}
      <div className="bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-[2rem] overflow-hidden shadow-xl shadow-slate-200/50 dark:shadow-none">
        <div className="overflow-x-auto">
          <table className="w-full text-left border-collapse">
            <thead>
              <tr className="bg-slate-50 dark:bg-slate-950/50 border-b border-slate-100 dark:border-slate-800">
                <th className="px-8 py-5 text-[11px] font-bold uppercase tracking-widest text-slate-500 dark:text-slate-400">Member</th>
                <th className="px-8 py-5 text-[11px] font-bold uppercase tracking-widest text-slate-500 dark:text-slate-400">Access Level</th>
                <th className="px-8 py-5 text-right text-[11px] font-bold uppercase tracking-widest text-slate-500 dark:text-slate-400">Action</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100 dark:divide-slate-800/50">
              {loadingUsers ? (
                <tr>
                  <td colSpan="3" className="px-8 py-24 text-center">
                    <Loader2 className="animate-spin mx-auto mb-4 text-indigo-500" size={40} />
                    <p className="text-slate-400 font-medium animate-pulse">Synchronizing user database...</p>
                  </td>
                </tr>
              ) : filteredUsers.map((user) => (
                <tr key={user.id} className="hover:bg-slate-50 dark:hover:bg-indigo-500/5 transition-colors group">
                  <td className="px-8 py-5">
                    <div className="flex items-center gap-4">
                      <div className="h-12 w-12 rounded-2xl bg-indigo-50 dark:bg-indigo-500/10 border border-indigo-100 dark:border-indigo-500/20 flex items-center justify-center text-indigo-600 dark:text-indigo-400 font-bold text-lg shadow-sm">
                        {user.name[0]}
                      </div>
                      <div>
                        <div className="text-sm font-bold text-slate-900 dark:text-white tracking-tight">{user.name}</div>
                        <div className="text-xs text-slate-500 dark:text-slate-400 flex items-center gap-1.5 mt-0.5">
                          <Mail size={12} className="opacity-70" /> {user.email}
                        </div>
                      </div>
                    </div>
                  </td>
                  <td className="px-8 py-5">
                    <span className={`px-3 py-1 rounded-full text-[10px] font-black uppercase tracking-tighter border ${
                      user.role === 'ADMIN' ? 'bg-amber-50 border-amber-200 text-amber-600 dark:bg-amber-500/10 dark:border-amber-500/30 dark:text-amber-500' :
                      user.role === 'INSTRUCTOR' ? 'bg-emerald-50 border-emerald-200 text-emerald-600 dark:bg-emerald-500/10 dark:border-emerald-500/30 dark:text-emerald-500' :
                      'bg-slate-100 border-slate-200 text-slate-600 dark:bg-slate-800 dark:border-slate-700 dark:text-slate-400'
                    }`}>
                      {user.role}
                    </span>
                  </td>
                  <td className="px-8 py-5 text-right">
                    <button 
                      onClick={() => handleOpenModal(user)}
                      className="p-3 bg-white dark:bg-slate-800 text-slate-400 dark:text-slate-500 hover:text-indigo-600 dark:hover:text-white hover:border-indigo-200 dark:hover:border-indigo-500 border border-slate-200 dark:border-slate-700 rounded-2xl transition-all hover:shadow-md"
                    >
                      <UserCog size={20} />
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {/* ROLE MODAL */}
      {selectedUser && (
        <div className="fixed inset-0 z-50 flex items-center justify-center px-4 bg-slate-900/40 dark:bg-slate-950/80 backdrop-blur-md animate-in fade-in duration-200">
          <div className="bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 w-full max-w-md rounded-[2.5rem] p-8 shadow-2xl animate-in zoom-in-95 duration-200">
            <div className="flex justify-between items-center mb-8">
              <div className="flex items-center gap-3">
                <div className="p-2.5 bg-indigo-50 dark:bg-indigo-500/10 rounded-xl text-indigo-600 dark:text-indigo-400 border border-indigo-100 dark:border-indigo-500/20">
                  <Shield size={22} />
                </div>
                <div>
                  <h3 className="text-xl font-bold text-slate-900 dark:text-white tracking-tight">Security Access</h3>
                  <p className="text-[10px] text-slate-500 uppercase font-black tracking-widest mt-0.5">Role Management</p>
                </div>
              </div>
              <button onClick={() => setSelectedUser(null)} className="p-2 text-slate-400 hover:text-slate-900 dark:hover:text-white hover:bg-slate-100 dark:hover:bg-slate-800 rounded-full transition-colors">
                <X size={20} />
              </button>
            </div>

            <div className="mb-8">
              <p className="text-sm text-slate-600 dark:text-slate-400 mb-6 leading-relaxed">
                Modifying permissions for <span className="text-indigo-600 dark:text-white font-bold">{selectedUser.name}</span>. Select a new system role below:
              </p>

              <div className="space-y-3">
                {roles.map((role) => (
                  <label 
                    key={role.id} 
                    className={`group flex items-center justify-between p-4 rounded-2xl border-2 cursor-pointer transition-all ${
                      Number(selectedRoleId) === role.id 
                        ? 'bg-indigo-50/50 border-indigo-500 dark:bg-indigo-500/5 dark:border-indigo-500' 
                        : 'bg-white dark:bg-slate-950 border-slate-100 dark:border-slate-800 hover:border-slate-200 dark:hover:border-slate-700'
                    }`}
                  >
                    <div className="flex items-center gap-3">
                      <input 
                        type="radio" 
                        name="role" 
                        value={role.id} 
                        checked={Number(selectedRoleId) === role.id}
                        onChange={() => setSelectedRoleId(role.id)}
                        className="hidden" 
                      />
                      <div className={`w-5 h-5 rounded-full border-2 flex items-center justify-center transition-all ${
                        Number(selectedRoleId) === role.id ? 'border-indigo-500 bg-indigo-500' : 'border-slate-300 dark:border-slate-700'
                      }`}>
                        {Number(selectedRoleId) === role.id && <div className="w-2 h-2 bg-white rounded-full" />}
                      </div>
                      <span className={`text-sm font-bold transition-colors ${
                        Number(selectedRoleId) === role.id ? 'text-indigo-700 dark:text-indigo-400' : 'text-slate-500 dark:text-slate-400'
                      }`}>
                        {role.name}
                      </span>
                    </div>
                    {Number(selectedRoleId) === role.id && (
                       <span className="text-[9px] font-black bg-indigo-500 text-white px-2 py-0.5 rounded tracking-widest">SELECTED</span>
                    )}
                  </label>
                ))}
              </div>
            </div>

            <div className="flex gap-4">
              <button 
                onClick={() => setSelectedUser(null)}
                className="flex-1 px-4 py-4 rounded-2xl border border-slate-200 dark:border-slate-800 text-slate-500 dark:text-slate-400 font-bold text-xs uppercase tracking-widest hover:bg-slate-50 dark:hover:bg-slate-800 transition-all"
              >
                Cancel
              </button>
              <button 
                onClick={handleUpdateRole}
                disabled={isUpdating}
                className="flex-[2] px-4 py-4 rounded-2xl bg-indigo-600 hover:bg-indigo-500 text-white font-bold text-xs uppercase tracking-widest shadow-xl shadow-indigo-600/20 hover:shadow-indigo-600/40 transition-all disabled:opacity-50 flex items-center justify-center gap-2"
              >
                {isUpdating && <Loader2 size={16} className="animate-spin" />}
                Save Changes
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default ManageUsers;