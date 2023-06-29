import useSWRMutation from 'swr/mutation';
import axios from 'axios';

const addTransaction = async (url, { arg }) =>
  await axios.post(`${process.env.NEXT_PUBLIC_API_URL}/transaction/createtbatch`, arg);

const useAddTransaction = () => useSWRMutation('transactions', addTransaction);

export default useAddTransaction;
