// import { createContext, useContext } from 'react';
// import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
// import { fetchMe } from '../../queries/auth/auth.queries';
// import {
//   localLoginMutation,
//   logoutMutation,
// } from '../../queries/auth/auth.mutations';

// const AuthContext = createContext(null);

// export const AuthProvider = ({ children }) => {
//   const queryClient = useQueryClient();

//   const {
//     data: user,
//     isLoading,
//   } = useQuery({
//     queryKey: ['auth', 'me'],
//     queryFn: fetchMe,
//     retry: false,
//   });


//   const login = useMutation({
//     mutationFn: localLoginMutation,
//     onSuccess: () => {
//       queryClient.invalidateQueries(['auth', 'me']);
//     },
//   });


//   const logout = useMutation({
//     mutationFn: logoutMutation,
//     onSuccess: () => {
//       queryClient.setQueryData(['auth', 'me'], null);
//     },
//   });

//   return (
//     <AuthContext.Provider
//       value={{
//         user,
//         isAuthenticated: !!user,
//         loading: isLoading,
//         login,
//         logout,
//       }}
//     >
//       {children}
//     </AuthContext.Provider>
//   );
// };

// export const useAuth = () => useContext(AuthContext);

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { AuthContext } from './AuthContext';
import { fetchMe } from '../../queries/auth/auth.queries';
import {
  localLoginMutation,
  logoutMutation,
} from '../../queries/auth/auth.mutations';

export const AuthProvider = ({ children }) => {
  const queryClient = useQueryClient();

  const {
    data: user,
    isLoading,
  } = useQuery({
    queryKey: ['auth', 'me'],
    queryFn: fetchMe,
    retry: false,
  });

  const login = useMutation({
    mutationFn: localLoginMutation,
    onSuccess: () => {
      queryClient.invalidateQueries(['auth', 'me']);
    },
  });

  const logout = useMutation({
    mutationFn: logoutMutation,
    onSuccess: () => {
      queryClient.setQueryData(['auth', 'me'], null);
    },
  });

  return (
    <AuthContext.Provider
      value={{
        user,
        isAuthenticated: !!user,
        loading: isLoading,
        login,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};
