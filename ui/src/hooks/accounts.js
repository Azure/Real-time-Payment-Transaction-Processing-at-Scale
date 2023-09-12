import axios from 'axios';
import { useQuery } from 'react-query';

const getAccounts = async ({ queryKey }) => {
  const [_key, { continuationToken, pageSize }] = queryKey;
  return await axios
    .get(
      `${process.env.NEXT_PUBLIC_API_URL}/accounts?pageSize=${pageSize}${
        continuationToken ? `&continuationToken=${continuationToken}` : ''
      }`
    )
    .then((res) => res.data);
};

const useAccounts = (continuationToken = null, pageSize = 10) =>
  useQuery(['accounts', { continuationToken, pageSize }], getAccounts);

export default useAccounts;
