import { Button, Label, Spinner, Textarea, TextInput } from 'flowbite-react';
import { useState } from 'react';

import useAddMember from '~/hooks/add-member';

const NewMemberForm = ({ setOpenModal }) => {
  const { trigger } = useAddMember();
  const [form, setForm] = useState({
    address: '',
    country: '',
    email: '',
    phone: '',
    firstName: '',
    lastName: '',
    city: '',
    state: '',
    zipcode: ''
  });

  const [isLoading, setIsLoading] = useState(false);
  const onClickCancel = () => {
    setForm({ accountId: '', type: '', description: '', merchant: '', amount: '' });
    setIsLoading(false);
    setOpenModal(false);
  };

  const onSubmit = async () => {
    setIsLoading(true);
    const response = await trigger(form);

    if (response.status === 202) {
      setOpenModal(false);
      setIsLoading(false);
    } else {
      setIsLoading(false);
    }
  };

  const onChangeAccountType = (country) => {
    setForm({ ...form, country });
  };
  const onChangeFirstName = (e) => setForm({ ...form, firstName: e.target.value });
  const onChangeLastName = (e) => setForm({ ...form, lastName: e.target.value });
  const onChangeEmail = (e) => setForm({ ...form, email: e.target.value });
  const onChangePhone = (e) => setForm({ ...form, phone: e.target.value });
  const onChangeAddress = (e) => setForm({ ...form, address: e.target.value });
  const onChangeCity = (e) => setForm({ ...form, city: e.target.value });
  const onChangeState = (e) => setForm({ ...form, state: e.target.value });
  const onChangeZipcode = (e) => setForm({ ...form, zipcode: e.target.value });

  return (
    <div>
      <div className="flex justify-between items-center mb-4">
        <div className="mb-2 block">
          <Label htmlFor="firstName" value="First Name:" />
        </div>
        <TextInput
          className="flex-1 ml-6"
          id="firstName"
          placeholder="First Name"
          onChange={onChangeFirstName}
          value={form.firstName}
          required
        />
      </div>
      <div className="flex justify-between items-center mb-4">
        <div className="mb-2 block">
          <Label htmlFor="lastName" value="Last Name:" />
        </div>
        <TextInput
          className="flex-1 ml-6"
          id="lastName"
          onChange={onChangeLastName}
          placeholder="Last Name"
          value={form.lastName}
          required
        />
      </div>
      <div className="flex justify-between items-center mb-4">
        <div className="mb-2 block">
          <Label htmlFor="email" value="Email:" />
        </div>
        <TextInput
          className="flex-1 ml-6"
          id="email"
          type="email"
          onChange={onChangeEmail}
          placeholder="Email"
          value={form.email}
          required
        />
      </div>
      <div className="flex justify-between items-center mb-4">
        <div className="mb-2 block">
          <Label htmlFor="phone" value="Phone:" />
        </div>
        <TextInput
          className="flex-1 ml-6"
          id="phone"
          type="number"
          onChange={onChangePhone}
          placeholder="Phone"
          value={form.phone}
          required
        />
      </div>
      <div className="flex justify-between items-center mb-4">
        <div className="mb-2 block">
          <Label htmlFor="address" value="Address:" />
        </div>
        <Textarea
          id="address"
          onChange={onChangeAddress}
          placeholder="Address"
          value={form.address}
          required
        />
      </div>
      <div className="flex justify-between items-center mb-4">
        <div className="mb-2 block">
          <Label htmlFor="city" value="City:" />
        </div>
        <TextInput
          className="flex-1 ml-6"
          id="city"
          onChange={onChangeCity}
          placeholder="City"
          value={form.city}
          required
        />
      </div>
      <div className="flex justify-between items-center mb-4">
        <div className="mb-2 block">
          <Label htmlFor="state" value="State/Porvince:" />
        </div>
        <TextInput
          className="flex-1 ml-6"
          id="state"
          onChange={onChangeState}
          placeholder="State/Province"
          value={form.state}
          required
        />
      </div>
      <div className="flex justify-between items-center mb-4">
        <div className="mb-2 block">
          <Label htmlFor="zipcode" value="Zipcode:" />
        </div>
        <TextInput
          className="flex-1 ml-6"
          id="zipcode"
          type="number"
          onChange={onChangeZipcode}
          placeholder="Zipcode"
          value={form.zipcode}
          required
        />
      </div>
      <div className="flex items-center mb-4">
        <div className="mb-2 block mr-3">
          <Label htmlFor="country" value="Country:" />
        </div>
        <select onChange={onChangeAccountType} label="Select" id="type" required>
          <option>US</option>
        </select>
      </div>
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

export default NewMemberForm;
