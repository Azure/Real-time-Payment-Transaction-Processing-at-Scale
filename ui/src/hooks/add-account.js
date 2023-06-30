import axios from 'axios';
import { useMutation, useQueryClient } from 'react-query';

const addAccount = async (data) =>
  await axios.post(`${process.env.NEXT_PUBLIC_API_URL}/account`, data);

const useAddAccount = () => {
  const client = useQueryClient();
  return useMutation({
    mutationFn: (data) => addAccount(data),
    onSuccess: () => {
      client.invalidateQueries();
    }
  });
};

export default useAddAccount;
