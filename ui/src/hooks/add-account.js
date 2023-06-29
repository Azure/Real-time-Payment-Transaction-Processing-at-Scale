import axios from 'axios';
import { useMutation } from 'react-query';

const addAccount = (data) => axios.post(`${process.env.NEXT_PUBLIC_API_URL}/account`, data);

const useAddAccount = () => {
  return useMutation({
    mutationFn: (data) => addAccount(data)
  });
};

export default useAddAccount;
