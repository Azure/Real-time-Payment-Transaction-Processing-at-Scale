import { useState } from 'react';
import { Button, Label, Spinner, TextInput } from 'flowbite-react';

import useAddAccount from '~/hooks/add-account';

const NewAccountForm = ({ setOpenModal }) => {
  const { mutate } = useAddAccount();

  const [error, setError] = useState('');
  const [form, setForm] = useState({
    id: '',
    accountType: 'Checking',
    balance: '',
    customerGreetingName: '',
    overdraftLimit: ''
  });

  const [isLoading, setIsLoading] = useState(false);
  const onClickCancel = () => {
    setForm({
      id: '',
      accountType: '',
      balance: '',
      customerGreetingName: '',
      overdraftLimit: ''
    });
    setIsLoading(false);
    setOpenModal(false);
  };

  const onSubmit = async () => {
    setIsLoading(true);
    setError('');

    mutate(form, {
      onSuccess: async () => {
        setOpenModal(false);
        setIsLoading(false);
        setForm({
          id: '',
          accountType: 'Checking',
          balance: '',
          customerGreetingName: '',
          overdraftLimit: ''
        });
        setError('');
      },
      onError: (e) => {
        setError(e?.response?.data ?? 'There was an error creating the account');
        setIsLoading(false);
      }
    });
  };

  const onChangeAccountId = (e) => setForm({ ...form, id: e.target.value });
  const onChangeAccountType = (accountType) => setForm({ ...form, accountType });
  const onChangeCustomerGreetingName = (e) =>
    setForm({ ...form, customerGreetingName: e.target.value });
  const onChangeOverdraftLimit = (e) => setForm({ ...form, overdraftLimit: e.target.value });
  const onChangeBalance = (e) => setForm({ ...form, balance: e.target.value });

  return (
    <div className="space-y-6">
      <div className="mb-4">
        <div className="mb-2 block">
          <Label htmlFor="accountId" value="Account Id:" />
        </div>
        <TextInput
          id="accountId"
          placeholder="Account Id"
          onChange={onChangeAccountId}
          value={form.accountId}
          required
        />
      </div>
      <div className="mb-4">
        <div className="mb-2 block">
          <Label htmlFor="customerGreetingName" value="Customer Greeting Name:" />
        </div>
        <TextInput
          id="customerGreetingName"
          placeholder="Customer Greeting Name"
          onChange={onChangeCustomerGreetingName}
          value={form.customerGreetingName}
          required
        />
      </div>
      <div className="mb-4">
        <div className="mb-2 block">
          <Label htmlFor="accountType" value="Account Type:" />
        </div>
        <select onChange={onChangeAccountType} label="Select" id="type" required>
          <option>Checking</option>
          <option>Savings</option>
        </select>
      </div>
      <div className="mb-4">
        <div className="mb-2 block">
          <Label htmlFor="overdraftLimit" value="Overdraft Limit:" />
        </div>
        <TextInput
          id="overdraftLimit"
          type="number"
          onChange={onChangeOverdraftLimit}
          placeholder="Overdraft Limit"
          value={form.overdraftLimit}
          required
        />
      </div>
      <div className="mb-6">
        <div className="mb-2 block">
          <Label htmlFor="balance" value="Balance:" />
        </div>
        <TextInput
          id="balance"
          type="number"
          onChange={onChangeBalance}
          placeholder="Balance"
          value={form.balance}
          required
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

export default NewAccountForm;
