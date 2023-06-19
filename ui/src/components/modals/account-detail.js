import { Modal } from 'flowbite-react';
import { Capitalize, USDollar } from '~/helpers';

const AccountDetailModal = ({ setOpenModal, openModal, account }) => {
  return (
    <Modal show={openModal} size="md" popup onClose={() => setOpenModal(false)}>
      {/* <Modal.Header className="items-center">
        <h3>Account Details</h3>
      </Modal.Header>
      <Modal.Body>
        <h1>Account Id: {account?.id}</h1>
        <div className="flex flex-col space-beteween text-md flex-1">
          <div className="flex flex-1 items-center mr-3">
            <caption className="font-bold mr-2">Customer Greeting Name:</caption>
            <p>{account?.customerGreetingName}</p>
          </div>
          <div className="flex flex-1 items-center">
            <caption className="font-bold mr-2">Account type:</caption>
            <p>{Capitalize(account?.accountType ?? '')}</p>
          </div>
          <div className="flex flex-1 items-center mr-3">
            <caption className="font-bold mr-2">Balance:</caption>
            <p>{USDollar.format(account?.balance)}</p>
          </div>
          <div className="flex flex-1 items-center">
            <caption className="font-bold mr-2">Overdraft Limit:</caption>
            <p>{USDollar.format(account?.overdraftLimit)}</p>
          </div>
        </div>
      </Modal.Body> */}
    </Modal>
  );
};

export default AccountDetailModal;
