import axios from 'axios';
import { useMutation, useQueryClient } from 'react-query';

const editMember = async (member_id, data) =>
  await axios.patch(`${process.env.NEXT_PUBLIC_API_URL}/member/${member_id}`, data);

const useEditMember = (member_id) => {
  const client = useQueryClient();
  return useMutation({
    mutationFn: (data) => editMember(member_id, data),
    onSuccess: async () => {
      await client.invalidateQueries();
    }
  });
};

export default useEditMember;
