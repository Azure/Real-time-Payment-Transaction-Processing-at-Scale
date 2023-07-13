import { useState } from 'react';
import { Button, Card } from 'flowbite-react';

import FormModal from '~/components/modals/form';
import NewMemberForm from '~/components/forms/new-member';

const MemberDetailsSection = ({ member, setMember }) => {
  const [showFormModal, setShowFormModal] = useState(false);

  const onClickEdit = () => setShowFormModal(true);

  const modalHeader = <div className="text-xl p-4">Edit Member</div>;

  return (
    <Card className="w-full mt-6">
      <div className="font-bold">Member Details</div>
      <div className="flex space-between items-center mb-6">
        <div className="flex flex-col text-sm w-full">
          <div className="flex flex-1 items-center mb-6">
            <p className="font-bold mr-2">First Name:</p>
            <p>{member.firstName}</p>
          </div>
          <div className="flex flex-1 items-center mb-6">
            <p className="font-bold mr-2">Last Name:</p>
            <p>{member.lastName}</p>
          </div>
          <div className="flex flex-1 items-center mb-6">
            <p className="font-bold mr-2">Email:</p>
            <p>{member.email}</p>
          </div>
          <div className="flex flex-1 items-center mb-6">
            <p className="font-bold mr-2">Phone Number:</p>
            <p>{member.phone}</p>
          </div>
        </div>
        <div className="flex flex-col text-sm w-full">
          <div className="flex flex-1 items-center mb-6">
            <p className="font-bold mr-2">Address:</p>
            <p>{member.address}</p>
          </div>
          <div className="flex flex-1 items-center mb-6">
            <p className="font-bold mr-2">City:</p>
            <p>{member.city}</p>
          </div>
          <div className="flex flex-1 items-center mb-6">
            <p className="font-bold mr-2">State:</p>
            <p>{member.state}</p>
          </div>
          <div className="flex flex-1 items-center mb-6">
            <p className="font-bold mr-2">Zipcode:</p>
            <p>{member.zipcode}</p>
          </div>
          <div className="flex flex-1 items-center mb-6">
            <p className="font-bold mr-2">Country:</p>
            <p>{member.country}</p>
          </div>
        </div>
      </div>
      <Button onClick={onClickEdit} color="dark" className="w-36">
        Edit
      </Button>
      <FormModal header={modalHeader} setOpenModal={setShowFormModal} openModal={showFormModal}>
        <NewMemberForm setMember={setMember} setOpenModal={setShowFormModal} member={member} />
      </FormModal>
    </Card>
  );
};

export default MemberDetailsSection;
