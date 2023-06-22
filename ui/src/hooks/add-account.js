import useSWRMutation from 'swr/mutation';
import axios from 'axios';

const addAccount = async (url, { arg }) => await axios.post(url, arg);

const useAddAccount = () =>
  useSWRMutation(`${process.env.NEXT_PUBLIC_API_URL}/account`, addAccount);

export default useAddAccount;
