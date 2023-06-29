import useSWR from 'swr';
import axios from 'axios';

const fetcher = (continuationToken = null, pageSize) =>
  axios
    .get(
      `${process.env.NEXT_PUBLIC_API_URL}/members?pageSize=${pageSize}${
        continuationToken ? `&continuationToken=${continuationToken}` : ''
      }`
    )
    .then((res) => res.data);

const useMembers = (continuationToken = null, pageSize = 10) => {
  return useSWR('members', () => fetcher(continuationToken, pageSize));
};

export default useMembers;
