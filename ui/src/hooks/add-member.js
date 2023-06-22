import useSWRMutation from 'swr/mutation';
import axios from 'axios';

const addMember = async (url, { arg }) => await axios.post(url, arg);

const useAddMember = () => useSWRMutation(`${process.env.NEXT_PUBLIC_API_URL}/member`, addMember);

export default useAddMember;
