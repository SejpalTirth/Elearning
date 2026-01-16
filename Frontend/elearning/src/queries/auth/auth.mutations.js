import { getGatewayAuth } from '../../api/clients/gateway-auth';

const authApi = getGatewayAuth();


export const localLoginMutation = async ({ email, password }) => {
  const payload = { email, password };

  await authApi.postApiGatewayAuthLocalLogin(payload);
};

export const logoutMutation = async () => {
  await authApi.getApiGatewayAuthLogout();
};

export const localRegistrationMutation = async ({ email, password }) => {
  const payload = {email, password}

  await authApi.postApiGatewayAuthLocalRegister(payload);
};