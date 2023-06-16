import useSWR from 'swr';
import axios from 'axios';

const fetcher = (url) => axios.get(url).then((res) => res.data);

const useAccountSummary = (accountId) => {
  return useSWR(`${process.env.NEXT_PUBLIC_API_URL}/account/${accountId}`, fetcher);
};

export default useAccountSummary;
