import { useEffect, useState } from 'react';
import { Button, Label, Spinner, Textarea, TextInput } from 'flowbite-react';

import useAddTransaction from '~/hooks/add-transaction';

const NewTransactionForm = ({ accountId, setOpenModal }) => {
  const { trigger } = useAddTransaction();

  const [error, setError] = useState('');
  const [form, setForm] = useState({
    accountId,
    type: 'Credit',
    description: '',
    merchant: '',
    amount: ''
  });
  const [isLoading, setIsLoading] = useState(false);
  const onClickCancel = () => {
    setForm({ accountId, type: 'Credit', description: '', merchant: '', amount: '' });
    setIsLoading(false);
    setOpenModal(false);
  };

  const onSubmit = async () => {
    setIsLoading(true);
    setError('');
    try {
      const response = await trigger(form);

      if (response.status === 200) {
        setOpenModal(false);
        setIsLoading(false);
        setForm({ accountId, type: 'Credit', description: '', merchant: '', amount: '' });
      }
    } catch (e) {
      setError(e.response.data);
      setIsLoading(false);
    }
    setForm({ accountId, type: 'Credit', description: '', merchant: '', amount: '' });
  };

  const onChangeMerchant = (e) => setForm({ ...form, merchant: e.target.value });
  const onChangeType = (e) => setForm({ ...form, type: e.target.value });
  const onChangeAmount = (e) => setForm({ ...form, amount: e.target.value });
  const onChangeDescription = (e) => setForm({ ...form, description: e.target.value });

  useEffect(() => {
    setForm((form) => ({ ...form, accountId }));
  }, [accountId]);

  return (
    <div className="space-y-6">
      <div className="mb-4">
        <div className="mb-2 block">
          <Label htmlFor="merchant" value="Merchant:" />
        </div>
        <TextInput
          id="merchant"
          onChange={onChangeMerchant}
          placeholder="Merchant Name"
          required
          value={form.merchant}
        />
      </div>
      <div className="mb-4">
        <div className="mb-2 block">
          <Label htmlFor="type" value="Transaction Type:" />
        </div>
        <select onChange={onChangeType} label="Select" id="type" required>
          <option>Credit</option>
          <option>Debit</option>
        </select>
      </div>
      <div className="mb-4">
        <div className="mb-2 block">
          <Label htmlFor="amount" value="Amount:" />
        </div>
        <TextInput
          id="amount"
          type="number"
          onChange={onChangeAmount}
          placeholder="Amount"
          required
          value={form.amount}
        />
      </div>
      <div className="mb-4">
        <div className="mb-2 block">
          <Label htmlFor="description" value="Description:" />
        </div>
        <Textarea
          id="description"
          onChange={onChangeDescription}
          placeholder="Description"
          required
          value={form.description}
        />
      </div>
      <p className="text-red-500">{error}</p>
      <div className="w-full flex justify-between pt-4">
        <Button color="light" onClick={onClickCancel}>
          Cancel
        </Button>
        <Button color="dark" onClick={onSubmit}>
          {isLoading ? <Spinner color="white" size="md" /> : 'Save'}
        </Button>
      </div>
    </div>
  );
};

export default NewTransactionForm;
