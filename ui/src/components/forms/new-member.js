import { Button, Label, Spinner, Textarea, TextInput } from 'flowbite-react';
import _ from 'lodash';
import { useEffect, useState } from 'react';
import { useQueryClient } from 'react-query';
import { DiffObjects } from '~/helpers';

import useAddMember from '~/hooks/add-member';
import useEditMember from '~/hooks/edit-member';

const NewMemberForm = ({ setOpenModal, member = null, setMember }) => {
  const client = useQueryClient();
  const { mutate: AddTrigger } = useAddMember();
  const { mutate: EditTrigger } = useEditMember(member?.id);

  const [error, setError] = useState('');
  const [form, setForm] = useState(
    member ?? {
      address: '',
      country: 'USA',
      email: '',
      phone: '',
      firstName: '',
      lastName: '',
      city: '',
      state: '',
      zipcode: ''
    }
  );

  const [isLoading, setIsLoading] = useState(false);
  const [isDisabled, setIsDisabled] = useState(false);
  const onClickCancel = () => {
    setForm({ accountId: '', type: '', description: '', merchant: '', amount: '' });
    setIsLoading(false);
    setOpenModal(false);
  };

  const onSubmit = async () => {
    setIsLoading(true);
    setError('');

    if (member) {
      const modifiedMember = DiffObjects(form, member);
      EditTrigger(
        {
          ...modifiedMember
        },
        {
          onSuccess: async () => {
            setOpenModal(false);
            setIsLoading(false);
            setMember({ ...member, ...modifiedMember });
            setIsDisabled(false);
            await client.refetchQueries('members');
          },
          onError: () => {
            setError(e?.response?.data ?? 'There was an error editing the member');
            setIsLoading(false);
            setIsDisabled(false);
          }
        }
      );
    } else {
      AddTrigger(form, {
        onSuccess: () => {
          setOpenModal(false);
          setIsLoading(false);
          setIsDisabled(false);
        },
        onError: () => {
          setError(e?.response?.data ?? 'There was an error creating the member');
          setIsLoading(false);
          setIsDisabled(false);
        }
      });
    }
  };

  const onChangeAccountType = (e) => setForm({ ...form, country: e.target.value });
  const onChangeFirstName = (e) => setForm({ ...form, firstName: e.target.value });
  const onChangeLastName = (e) => setForm({ ...form, lastName: e.target.value });
  const onChangeEmail = (e) => setForm({ ...form, email: e.target.value });
  const onChangePhone = (e) => setForm({ ...form, phone: e.target.value });
  const onChangeAddress = (e) => setForm({ ...form, address: e.target.value });
  const onChangeCity = (e) => setForm({ ...form, city: e.target.value });
  const onChangeState = (e) => setForm({ ...form, state: e.target.value });
  const onChangeZipcode = (e) => setForm({ ...form, zipcode: e.target.value });

  useEffect(() => {
    if (member) setForm(member);
  }, [member]);

  useEffect(() => {
    if (member) {
      const diffFields = DiffObjects(form, member);
      setIsDisabled(Object.keys(diffFields).length === 0);
    } else {
      setIsDisabled(Object.values(form).some((x) => x === ''));
    }
  }, [form, member]);

  return (
    <div>
      <div className="flex justify-between items-center mb-4">
        <div className="mb-2 block flex-1">
          <Label htmlFor="firstName" value="First Name:" />
        </div>
        <TextInput
          className="flex-1"
          id="firstName"
          placeholder="First Name"
          onChange={onChangeFirstName}
          value={form.firstName}
          required
        />
      </div>
      <div className="flex justify-between items-center mb-4">
        <div className="mb-2 block flex-1">
          <Label htmlFor="lastName" value="Last Name:" />
        </div>
        <TextInput
          className="flex-1  flex-1"
          id="lastName"
          onChange={onChangeLastName}
          placeholder="Last Name"
          value={form.lastName}
          required
        />
      </div>
      <div className="flex justify-between items-center mb-4">
        <div className="mb-2 block flex-1">
          <Label htmlFor="email" value="Email:" />
        </div>
        <TextInput
          className="flex-1 "
          id="email"
          type="email"
          onChange={onChangeEmail}
          placeholder="Email"
          value={form.email}
          required
        />
      </div>
      <div className="flex justify-between items-center mb-4">
        <div className="mb-2 block flex-1">
          <Label htmlFor="phone" value="Phone:" />
        </div>
        <TextInput
          className="flex-1"
          id="phone"
          onChange={onChangePhone}
          placeholder="Phone"
          value={form.phone}
          required
        />
      </div>
      <div className="flex justify-between items-center mb-4">
        <div className="mb-2 block flex-1">
          <Label htmlFor="address" value="Address:" />
        </div>
        <Textarea
          id="address"
          className="flex-1"
          onChange={onChangeAddress}
          placeholder="Address"
          value={form.address}
          required
        />
      </div>
      <div className="flex justify-between items-center mb-4">
        <div className="mb-2 block flex-1">
          <Label htmlFor="city" value="City:" />
        </div>
        <TextInput
          className="flex-1"
          id="city"
          onChange={onChangeCity}
          placeholder="City"
          value={form.city}
          required
        />
      </div>
      <div className="flex justify-between items-center mb-4">
        <div className="mb-2 block flex-1">
          <Label htmlFor="state" value="State/Province:" />
        </div>
        <TextInput
          className="flex-1  flex-1"
          id="state"
          onChange={onChangeState}
          placeholder="State/Province"
          value={form.state}
          required
        />
      </div>
      <div className="flex justify-between items-center mb-4">
        <div className="mb-2 block flex-1">
          <Label htmlFor="zipcode" value="Zipcode:" />
        </div>
        <TextInput
          className="flex-1"
          id="zipcode"
          type="number"
          onChange={onChangeZipcode}
          placeholder="Zipcode"
          value={form.zipcode}
          required
        />
      </div>
      <div className="flex justify-between items-center mb-4">
        <div className="mb-2 block flex-1">
          <Label htmlFor="country" value="Country:" />
        </div>
        <select
          className="rounded-md flex-1"
          onChange={onChangeAccountType}
          label="Select"
          id="type"
          required>
          <option>USA</option>
          <option>Other</option>
        </select>
      </div>
      <p className="text-red-500">{error}</p>
      <div className="w-full flex justify-between pt-4">
        <Button color="light" onClick={onClickCancel}>
          Cancel
        </Button>
        <Button disabled={isDisabled} color="dark" onClick={onSubmit}>
          {isLoading ? <Spinner color="white" size="md" /> : 'Save'}
        </Button>
      </div>
    </div>
  );
};

export default NewMemberForm;
