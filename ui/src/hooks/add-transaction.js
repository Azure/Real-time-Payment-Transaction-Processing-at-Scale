import axios from 'axios';
import { useMutation } from 'react-query';

const addTransaction = async (data) =>
  await axios.post(`${process.env.NEXT_PUBLIC_API_URL}/transaction/createtbatch`, data);

const useAddTransaction = () => {
  return useMutation({
    mutationFn: (data) => addTransaction(data)
  });
};

export default useAddTransaction;
