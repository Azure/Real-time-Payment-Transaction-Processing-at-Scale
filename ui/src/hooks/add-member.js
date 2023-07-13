import axios from 'axios';
import { useMutation, useQueryClient } from 'react-query';

const addMember = async (data) =>
  await axios.post(`${process.env.NEXT_PUBLIC_API_URL}/member`, data);

const useAddMember = () => {
  const client = useQueryClient();
  return useMutation({
    mutationFn: (data) => addMember(data),
    onSuccess: () => {
      client.invalidateQueries();
    }
  });
};

export default useAddMember;
