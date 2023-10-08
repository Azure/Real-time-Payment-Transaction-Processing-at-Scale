import axios from 'axios';
import { useQuery, useQueryClient } from 'react-query';
import useSWR from 'swr';

const fetcher = async (url) => {
  return await axios
    .get(url)
    .then((res) => res.data);
};

const useTransactionsStatement = (accountId, continuationToken = null, pageSize = 10) => {
  const url = `${process.env.NEXT_PUBLIC_API_URL}/statement/${accountId}/?pageSize=${pageSize}${continuationToken ? `&continuationToken=${continuationToken}` : ''}`;
  return useSWR(url, fetcher);
};

export default useTransactionsStatement;