import axios from 'axios';
import useSWR from 'swr';

const fetcher = async (url) => {
  return await axios
    .get(url)
    .then((res) => res.data);
};

const useAccounts = (continuationToken = null, pageSize = 10) => {
  const url = `${process.env.NEXT_PUBLIC_API_URL}/accounts?pageSize=${pageSize}${continuationToken ? `&continuationToken=${continuationToken}` : ''}`
  return useSWR(url, fetcher);
}

export default useAccounts;
