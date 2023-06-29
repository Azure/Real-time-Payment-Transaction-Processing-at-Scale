import useSWR from 'swr';
import axios from 'axios';

const fetcher = (countinuationToken = null, pageSize) =>
  axios
    .get(
      `${process.env.NEXT_PUBLIC_API_URL}/members?pageSize=${pageSize}${
        countinuationToken ? `&countinuationToken=${countinuationToken}` : ''
      }`
    )
    .then((res) => res.data);

const useMembers = (countinuationToken = null, pageSize = 10) => {
  return useSWR('members', () => fetcher(countinuationToken, pageSize));
};

export default useMembers;
