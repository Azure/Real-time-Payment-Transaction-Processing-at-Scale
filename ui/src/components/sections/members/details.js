import { Card } from 'flowbite-react';

const MemberDetailsSection = ({ member }) => {
  return (
    <Card className="w-full mt-6">
      <h2 className="font-bold">Member Details</h2>
      <div className="flex space-between items-center mb-6">
        <div className="flex flex-col text-sm w-full">
          <div className="flex flex-1 items-center mb-6">
            <caption className="font-bold mr-2">First Name:</caption>
            <p>{member.firstName}</p>
          </div>
          <div className="flex flex-1 items-center mb-6">
            <caption className="font-bold mr-2">Last Name:</caption>
            <p>{member.lastName}</p>
          </div>
          <div className="flex flex-1 items-center mb-6">
            <caption className="font-bold mr-2">Email:</caption>
            <p>{member.email}</p>
          </div>
          <div className="flex flex-1 items-center mb-6">
            <caption className="font-bold mr-2">Phone Number:</caption>
            <p>{member.phone}</p>
          </div>
        </div>
        <div className="flex flex-col text-sm w-full">
          <div className="flex flex-1 items-center mb-6">
            <caption className="font-bold mr-2">Address:</caption>
            <p>{member.address}</p>
          </div>
          <div className="flex flex-1 items-center mb-6">
            <caption className="font-bold mr-2">City:</caption>
            <p>{member.city}</p>
          </div>
          <div className="flex flex-1 items-center mb-6">
            <caption className="font-bold mr-2">State:</caption>
            <p>{member.state}</p>
          </div>
          <div className="flex flex-1 items-center mb-6">
            <caption className="font-bold mr-2">Zipcode:</caption>
            <p>{member.zipcode}</p>
          </div>
          <div className="flex flex-1 items-center mb-6">
            <caption className="font-bold mr-2">Country:</caption>
            <p>{member.country}</p>
          </div>
        </div>
      </div>
    </Card>
  );
};

export default MemberDetailsSection;
