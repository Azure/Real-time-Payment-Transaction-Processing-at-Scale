import axios from 'axios';
import { useQuery } from 'react-query';

const fetcher = async ({ queryKey }) => {
  const [_key, { memberId }] = queryKey;
  return await axios
    .get(
      `${process.env.NEXT_PUBLIC_API_URL}/member/${memberId}/accounts`
    )
    .then((res) => res.data);
};

const MembersAccounts = (memberId) => {
  return useQuery(['members', { memberId }], fetcher);
};

export default MembersAccounts;
