import axios from 'axios';
import { useQuery, useQueryClient } from 'react-query';

const getTransactions = async ({ queryKey }) => {
  const [_key, { accountId, continuationToken, pageSize }] = queryKey;
  return await axios
    .get(
      `${process.env.NEXT_PUBLIC_API_URL}/statement/${accountId}/?pageSize=${pageSize}${
        continuationToken ? `&continuationToken=${continuationToken}` : ''
      }`
    )
    .then((res) => res.data);
};

const useTransactionsStatement = (accountId, continuationToken = null, pageSize = 10) => {
  const client = useQueryClient();
  const queryKey = ['transactions', { accountId, continuationToken, pageSize }];

  return useQuery(queryKey, getTransactions, {
    onSuccess: (data) => {
      client.setQueryData(queryKey, data); // Update the query data with the fetched result
    }
  });
};

export default useTransactionsStatement;
