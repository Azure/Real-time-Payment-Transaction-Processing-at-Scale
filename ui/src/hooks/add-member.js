import useSWRMutation from 'swr/mutation';
import axios from 'axios';

const addMember = async (url, { arg }) =>
  await axios.post(`${process.env.NEXT_PUBLIC_API_URL}/member`, arg);

const useAddMember = () => useSWRMutation('members', addMember);

export default useAddMember;
