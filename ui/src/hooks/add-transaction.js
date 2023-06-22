import useSWRMutation from 'swr/mutation';
import axios from 'axios';

const addTransaction = async (url, { arg }) => await axios.post(url, arg);

const useAddTransaction = () =>
  useSWRMutation(`${process.env.NEXT_PUBLIC_API_URL}/transaction/createtbatch`, addTransaction);

export default useAddTransaction;
