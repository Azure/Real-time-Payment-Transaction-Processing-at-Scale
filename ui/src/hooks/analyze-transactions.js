import useSWRMutation from 'swr/mutation';
import axios from 'axios';

const analyzeTransactions = async (url) => await axios.get(url);

const useAnalyzeTransactions = (accountId, query) =>
  useSWRMutation(`${process.env.NEXT_PUBLIC_API_URL}/statement/${accountId}/analyze?query=${query}`, analyzeTransactions);

export default useAnalyzeTransactions;
