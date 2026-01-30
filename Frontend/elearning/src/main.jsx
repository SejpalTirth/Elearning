import React from 'react';
import ReactDOM from 'react-dom/client';
import { Provider } from 'react-redux';
import { BrowserRouter } from 'react-router-dom';

import App from './App';
import { store } from './app/store';
import AuthBootstrapper from './features/auth/AuthBootstrapper';

import './index.css';

ReactDOM.createRoot(document.getElementById('root')).render(
  <React.StrictMode>
    <Provider store={store}>
      <BrowserRouter>
        <AuthBootstrapper>
          <App />
        </AuthBootstrapper>
      </BrowserRouter>
    </Provider>
  </React.StrictMode>
);
