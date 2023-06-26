import useSWRMutation from 'swr/mutation';
import axios from 'axios';

const addAccount = async (url, { arg }) =>
  await axios.post(`${process.env.NEXT_PUBLIC_API_URL}/account`, arg);

const useAddAccount = () => useSWRMutation('accounts', addAccount);

export default useAddAccount;
