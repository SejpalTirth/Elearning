import React, { useState } from 'react';
import { LoadingContext } from './LoadingContext';
import { Loader } from '../../components/ui/Loader';

export const LoadingProvider = ({ children }) => {
  const [activeRequests, setActiveRequests] = useState(0);

  const show = () => setActiveRequests(prev => prev + 1);
  const hide = () => setActiveRequests(prev => Math.max(0, prev - 1));

  const isLoading = activeRequests > 0;

  return (
    <LoadingContext.Provider value={{ show, hide, isLoading }}>
      {children}
      {isLoading && <Loader />}
    </LoadingContext.Provider>
  );
};