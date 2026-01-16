import { getGatewayAuth } from '../../api/clients/gateway-auth';

const authApi = getGatewayAuth();

export const fetchMe = async () => {
  const response = await authApi.getApiGatewayAuthMe();
  return response.data;
};
