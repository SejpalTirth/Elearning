import React, { useState, useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useMutation } from '@tanstack/react-query';
import { UserCircle, ShieldCheck } from 'lucide-react';

const ROLE_MAP = {
  1: 'Admin',
  2: 'Instructor',
  3: 'Student'
};

const CompleteProfile = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();

  const [name, setName] = useState('');
  const [roleId, setRoleId] = useState(3);
  const userId = searchParams.get('userId');

  useEffect(() => {
    if (!userId) {
      navigate('/login');
    }
  }, [userId, navigate]);

  const completeProfileMutation = useMutation({
    mutationFn: async (payload) => {
      console.log("Saving Profile:", payload);
    },
    onSuccess: () => {
      navigate('/login');
    },
    onError: () => {
    }
  });

  const handleSave = (e) => {
    e.preventDefault();
    if (!name.trim()) {
      return;
    }

    completeProfileMutation.mutate({
      userId,
      name,
      role: ROLE_MAP[roleId]
    });
  };

  return (
    <div className="min-h-screen flex items-center justify-center
      bg-gradient-to-br from-indigo-50 to-slate-100
      dark:from-slate-900 dark:to-slate-800 transition-colors px-4">
      
      <div className="w-full max-w-md bg-white dark:bg-slate-900 rounded-2xl shadow-xl p-8 border border-slate-100 dark:border-slate-800">
        
        <div className="text-center mb-8">
          <div className="inline-flex items-center justify-center w-16 h-16 bg-indigo-100 dark:bg-indigo-900/30 rounded-full mb-4">
            <UserCircle className="w-10 h-10 text-indigo-600 dark:text-indigo-400" />
          </div>
          <h1 className="text-2xl font-bold text-slate-800 dark:text-white">Complete Your Profile</h1>
          <p className="text-sm text-slate-500 dark:text-slate-400 mt-2">
            Just a few more details to get you started
          </p>
        </div>

        <form onSubmit={handleSave} className="space-y-6">
          <div>
            <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">
              Full Name
            </label>
            <input
              type="text"
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="John Doe"
              className="w-full px-4 py-2 rounded-lg border border-slate-300 dark:border-slate-700 
                bg-white dark:bg-slate-800 text-slate-800 dark:text-white
                focus:ring-2 focus:ring-indigo-500 transition outline-none"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-2">
              Select Your Role
            </label>
            <div className="grid grid-cols-1 gap-3">
              {[2, 3].map((id) => (
                <button
                  key={id}
                  type="button"
                  onClick={() => setRoleId(id)}
                  className={`flex items-center justify-between px-4 py-3 rounded-xl border transition-all
                    ${roleId === id 
                      ? 'border-indigo-600 bg-indigo-50 dark:bg-indigo-900/20 text-indigo-700 dark:text-indigo-300' 
                      : 'border-slate-200 dark:border-slate-700 hover:bg-slate-50 dark:hover:bg-slate-800 text-slate-600 dark:text-slate-400'
                    }`}
                >
                  <span className="font-medium">{ROLE_MAP[id]}</span>
                  {roleId === id && <ShieldCheck className="w-5 h-5" />}
                </button>
              ))}
            </div>
            <p className="text-[10px] text-slate-400 mt-2 text-center uppercase tracking-wider">
              Note: Admins are assigned internally
            </p>
          </div>

          <button
            type="submit"
            className="w-full bg-indigo-600 hover:bg-indigo-700 text-white font-semibold py-3 rounded-xl
              shadow-lg shadow-indigo-200 dark:shadow-none transition-all hover:scale-[1.02]"
          >
            Finish Setup
          </button>
        </form>
      </div>
    </div>
  );
};

export default CompleteProfile;