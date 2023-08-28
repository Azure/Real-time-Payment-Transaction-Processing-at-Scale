import axios from 'axios';
import { useMutation, useQueryClient } from 'react-query';

const editMember = async (accountId, memberId) =>
  await axios.post(`${process.env.NEXT_PUBLIC_API_URL}/member/${memberId}/accounts/remove/${accountId}`);

const unassignMemberAccount = (accountId = null, memberId = null) => {
  const client = useQueryClient();
  return useMutation({
    mutationFn: () => editMember(accountId, memberId),
    onSuccess: async () => {
      await client.invalidateQueries();
    }
  });
};

export default unassignMemberAccount;
