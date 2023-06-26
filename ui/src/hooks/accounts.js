import useSWR from 'swr';
import axios from 'axios';

const fetcher = (continuationToken, pageSize) =>
  axios
    .get(
      `${process.env.NEXT_PUBLIC_API_URL}/accounts?pageSize=${pageSize}${
        continuationToken && `&continuationToken=${continuationToken}`
      }`
    )
    .then((res) => res.data);

const useAccounts = (continuationToken = null, pageSize = 10) =>
  useSWR('accounts', () => fetcher(continuationToken, pageSize));

export default useAccounts;
