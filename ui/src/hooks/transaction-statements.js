import useSWR from 'swr';
import axios from 'axios';

const fetcher = (accountId, continuationToken, pageSize) =>
  axios
    .get(
      `${process.env.NEXT_PUBLIC_API_URL}/statement/${accountId}/?pageSize=${pageSize}${
        continuationToken ? `&continuationToken=${continuationToken}` : ''
      }`
    )
    .then((res) => res.data);

const useTransactionsStatement = (accountId, continuationToken = null, pageSize = 10) => {
  return useSWR('transactions', () => fetcher(accountId, continuationToken, pageSize));
};

export default useTransactionsStatement;
