import useSWR from 'swr';
import axios from 'axios';

const fetcher = (url) => axios.get(url).then((res) => res.data);

const useMembers = (continuationToken = null, pageSize = 10) => {
  return useSWR(
    `${process.env.NEXT_PUBLIC_API_URL}/members?pageSize=${pageSize}${
      continuationToken && `&continuationToken=${continuationToken}`
    }`,
    fetcher
  );
};

export default useMembers;
