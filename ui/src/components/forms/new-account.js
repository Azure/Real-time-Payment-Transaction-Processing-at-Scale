import { Button, Dropdown, Label, TextInput } from 'flowbite-react';
import { useState } from 'react';

const NewAccountForm = () => {
  const [form, setForm] = useState({
    accountId: '',
    accountType: '',
    balance: '',
    customerGreetingName: '',
    overdraftLimit: ''
  });

  const onChangeAccountType = (accountType) => {
    console.log(accountType);
    setForm({ ...form, accountType });
  };
  const onChangeCustomerGreetingName = (e) =>
    setForm({ ...form, customerGreetingName: e.target.value });
  const onChangeOverdraftLimit = (e) => setForm({ ...form, overdraftLimit: e.target.value });
  const onChangeBalance = (e) => setForm({ ...form, balance: e.target.value });

  return (
    <div className="space-y-6">
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
        <Dropdown color="light" label="Select" id="accountType" required>
          <Dropdown.Item>Checking</Dropdown.Item>
          <Dropdown.Item>Savings</Dropdown.Item>
        </Dropdown>
      </div>
      <div className="mb-4">
        <div className="mb-2 block">
          <Label htmlFor="overdraftLimit" value="Overdraft Limit:" />
        </div>
        <TextInput
          id="overdraftLimit"
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
          onChange={onChangeBalance}
          placeholder="Balance"
          value={form.balance}
          required
        />
      </div>
      <div className="w-full flex justify-between pt-4">
        <Button color="light">Cancel</Button>
        <Button color="dark">Save</Button>
      </div>
    </div>
  );
};

export default NewAccountForm;
