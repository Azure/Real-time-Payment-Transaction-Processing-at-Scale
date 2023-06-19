import { Dropdown, Label, Textarea, TextInput } from 'flowbite-react';
import { useState } from 'react';

const NewTransactionForm = () => {
  const [form, setForm] = useState({
    accountId: '',
    type: '',
    description: '',
    merchant: '',
    amount: ''
  });

  const onChangeMerchant = (e) => setForm({ ...form, merchant: e.target.value });
  const onChangeAmount = (e) => setForm({ ...form, amount: e.target.value });
  const onChangeDescription = (e) => setForm({ ...form, description: e.target.value });

  return (
    <div className="space-y-6">
      <div className="mb-4">
        <div className="mb-2 block">
          <Label htmlFor="merchant" value="Merchant:" />
        </div>
        <TextInput
          id="merchant"
          placeholder="Merchant Name"
          onChange={onChangeMerchant}
          value={form.merchant}
          required
        />
      </div>
      <div className="mb-4">
        <div className="mb-2 block">
          <Label htmlFor="type" value="Transaction Type:" />
        </div>
        <Dropdown label="Select" id="type" value={form.type} required />
      </div>
      <div className="mb-4">
        <div className="mb-2 block">
          <Label htmlFor="amount" value="Amount:" />
        </div>
        <TextInput id="amount" onChange={onChangeAmount} value={form.amount} required />
      </div>
      <div className="mb-4">
        <div className="mb-2 block">
          <Label htmlFor="description" value="Description:" />
        </div>
        <Textarea
          id="description"
          onChange={onChangeDescription}
          value={form.description}
          required
        />
      </div>
    </div>
  );
};

export default NewTransactionForm;
