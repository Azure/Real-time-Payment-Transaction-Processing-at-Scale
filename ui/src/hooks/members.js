import axios from 'axios';
import { useQuery } from 'react-query';

const fetcher = async ({ queryKey }) => {
  const [_key, { continuationToken, pageSize }] = queryKey;
  return await axios
    .get(
      `${process.env.NEXT_PUBLIC_API_URL}/members?pageSize=${pageSize}${
        continuationToken ? `&continuationToken=${continuationToken}` : ''
      }`
    )
    .then((res) => res.data);
};

const useMembers = (continuationToken = null, pageSize = 10) => {
  return useQuery(['members', { continuationToken, pageSize }], fetcher);
};

export default useMembers;
