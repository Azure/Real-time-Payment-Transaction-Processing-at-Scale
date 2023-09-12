import axios from 'axios';
import { useQuery } from 'react-query';

const checkAccountMatch = async ({ queryKey }) => {
    const [_key, { accountId }] = queryKey;
    if (!accountId) return Promise.resolve({});
    return await axios
        .get(`${process.env.NEXT_PUBLIC_API_URL}/account/${accountId}`)
        .then((res) => {
            return res.data;
        })
        .catch((err) => err);
};

const useAccountMatch = (accountId = null) =>
    useQuery(['accountMatch', { accountId }], checkAccountMatch);

export default useAccountMatch;
