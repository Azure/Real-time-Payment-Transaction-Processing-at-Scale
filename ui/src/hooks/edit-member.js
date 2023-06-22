import useSWRMutation from 'swr/mutation';
import axios from 'axios';

const editMember = async (member_id, { arg }) =>
  await axios.patch(`${process.env.NEXT_PUBLIC_API_URL}/member/${member_id}`, arg);

const useEditMember = (member_id) =>
  useSWRMutation('members', (url, data) => editMember(member_id, data));

export default useEditMember;
