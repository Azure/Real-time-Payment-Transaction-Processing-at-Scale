import { useState, useRef } from 'react';
import { Button, Label, Spinner, Textarea, TextInput } from 'flowbite-react';

import useAnalyzeTransactions from '~/hooks/analyze-transactions';



const AnalyzeTransactionsForm = ({ accountId, setOpenModal }) => {
  const ref = useRef(null);
  const [form, setForm] = useState({
    accountId,
    query: 'Could you categorize each transaction into tax categories? Show results in a bulleted list.'
  });
  const { trigger } = useAnalyzeTransactions(accountId, form.query);
  const [isLoading, setIsLoading] = useState(false);
  const onClickCancel = () => {
    setForm({ accountId: '', query:'Could you categorize each transaction into tax categories? Show results in a bulleted list.'});
    ref.current.value = '';
    setIsLoading(false);
    setOpenModal(false);
  };

  const onSubmit = async () => {
    setIsLoading(true);
    const response = await trigger(form);

    if (response.status === 200) {
      ref.current.value = response.data;
      setIsLoading(false);
    } else {
      setIsLoading(false);
    }
  };

  const onChangeQuery = (e) => setForm({ ...form, query: e.target.value });

  return (
    <div className="space-y-6">
      <div className="mb-4">
        <div className="mb-2 block">
          <Label htmlFor="query" value="Query:" />
        </div>
        <TextInput
          id="query"
          onChange={onChangeQuery}
          placeholder="Could you categorize each transaction into tax categories? Show results in a bulleted list."
          required
          value={form.query}
        />
      </div>
      <div className="mb-4">
        <div className="mb-2 block">
          <Textarea ref={ref} id="results" name="results" />
        </div>
      </div>
      <div className="w-full flex justify-between pt-4">
        <Button color="light" onClick={onClickCancel}>
          Cancel
        </Button>
        <Button color="dark" onClick={onSubmit}>
          {isLoading ? <Spinner color="white" size="md" /> : 'Submit'}
        </Button>
      </div>
    </div>
  );
};

export default AnalyzeTransactionsForm;
