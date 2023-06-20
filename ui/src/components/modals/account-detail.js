import { Modal } from 'flowbite-react';
import { Capitalize, USDollar } from '~/helpers';

const AccountDetailModal = ({ setOpenModal, openModal, account }) => {
  return (
    <Modal show={openModal} size="lg" popup onClose={() => setOpenModal(false)}>
      <Modal.Header className="items-center">
        <div className="px-6 mt-6">Account Details</div>
      </Modal.Header>
      <Modal.Body>
        <div className="p-2">
          <div className="mb-4">Account Id: {account?.id}</div>
          <div className="flex flex-col space-beteween text-md flex-1 gap-2">
            <div className="flex flex-1 items-center mb-3">
              <div className="font-bold mr-2">Customer Greeting Name:</div>
              <div>{account?.customerGreetingName}</div>
            </div>
            <div className="flex flex-1 items-center mb-3">
              <div className="font-bold mr-2">Account type:</div>
              <div>{Capitalize(account?.accountType ?? '')}</div>
            </div>
            <div className="flex flex-1 items-center mb-3">
              <div className="font-bold mr-2">Balance:</div>
              <div>{USDollar.format(account?.balance)}</div>
            </div>
            <div className="flex flex-1 items-center mb-3">
              <div className="font-bold mr-2">Overdraft Limit:</div>
              <div>{USDollar.format(account?.overdraftLimit)}</div>
            </div>
          </div>
        </div>
      </Modal.Body>
    </Modal>
  );
};

export default AccountDetailModal;
