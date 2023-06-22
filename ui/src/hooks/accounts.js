import useSWR from 'swr';
import axios from 'axios';

const fetcher = (url) => axios.get(url).then((res) => res.data);

const useAccounts = (continuationToken = null, pageSize = 10) => {
  return useSWR(
    `${process.env.NEXT_PUBLIC_API_URL}/accounts?pageSize=${pageSize}${
      continuationToken && `&continuationToken=${continuationToken}`
    }`,
    fetcher
  );
};

export default useAccounts;
