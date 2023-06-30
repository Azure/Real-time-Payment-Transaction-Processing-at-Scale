import axios from 'axios';
import { useMutation, useQueryClient } from 'react-query';

const addTransaction = async (data) =>
  await axios.post(`${process.env.NEXT_PUBLIC_API_URL}/transaction/createtbatch`, data);

const useAddTransaction = () => {
  const client = useQueryClient();
  return useMutation({
    mutationFn: (data) => addTransaction(data),
    onSuccess: () => {
      client.invalidateQueries();
    }
  });
};

export default useAddTransaction;
