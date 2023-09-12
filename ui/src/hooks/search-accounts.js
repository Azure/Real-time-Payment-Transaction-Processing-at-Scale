import axios from 'axios';
import { useQuery } from 'react-query';

const findAccount = async ({ queryKey }) => {
    const [_key, { stringSearch }] = queryKey;
    if (!stringSearch) return Promise.resolve({});
    return await axios
        .get(`${process.env.NEXT_PUBLIC_API_URL}/account/find?s=${stringSearch}`)
        .then((res) => {
            return res.data;
        })
        .catch((err) => err);
};

const FindAccountMatch = (stringSearch = null) =>
    useQuery(['findAccount', { stringSearch }], findAccount);

export default FindAccountMatch;