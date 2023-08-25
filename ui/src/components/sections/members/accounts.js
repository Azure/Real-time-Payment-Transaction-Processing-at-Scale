import { useState, useEffect } from 'react';
import { Button, Card } from 'flowbite-react';

import FormModal from '~/components/modals/form';
import AccountsTable from '~/components/tables/accounts';
import TransactionsSection from '~/components/sections/accounts/transactions';

import unassignMemberAccount from '~/hooks/unassign-member-account';

const ViewAccountsSection = ({ member, setMember }) => {
  const [showFormModal, setShowFormModal] = useState(false);
  const [accountId, setAccountId] = useState();
  const [removeAccountId, setRemoveAccountId] = useState();
  const [showRemoveAccountModal, setShowRemoveAccountModal] = useState(false);
  const [confirmation, setConfirmation] = useState(false);
  const [reload, setReload] = useState(false);
  const [viewTransactions, setViewTransactions] = useState(false);
  

  const { mutate: RemoveTrigger } = unassignMemberAccount(removeAccountId, member.id);

  const onClickAssign = () => setShowFormModal(true);
  const onClickCancelRemoveAssingedAccount = () => setShowRemoveAccountModal(false);

  useEffect(() => {
    if (member.id) {
        setViewTransactions(false);
    }
  }, [member]);

  useEffect(() => {
    if (viewTransactions) {
        console.log("View Transactions");
    }
  }, [viewTransactions]);



  const onClickRemoveAssingedAccount = () => {
    setConfirmation(true);
    RemoveTrigger(confirmation, {
        onSuccess: async () => {
            setShowRemoveAccountModal(false);
            setConfirmation(false);
            setReload(true);
        },
        onError: async (error) => {
            setShowRemoveAccountModal(false);
            setConfirmation(false);
        }
    });
  };

  const modalHeader = <div className="text-xl p-4">Remove Account Assignment: {member.firstName} {member.lastName}</div>;
  const assignAccountHeader = <div className="text-xl p-4">Assign Account: {member.firstName} {member.lastName}</div>;

  return (
    <Card className="w-full mt-6">
      <div className="font-bold">Accounts: {member.firstName} {member.lastName}</div>
      <Button onClick={onClickAssign} color="dark" className="w-36">
       Assign Account
      </Button>
      <div className="flex space-between items-center mb-6 flex-col">
        <AccountsTable
            memberId={member.id}
            setAccountId={setAccountId}
            setRemoveAccountId={setRemoveAccountId}
            setShowRemoveAccountModal={setShowRemoveAccountModal}
            reload={reload}
            setReload={setReload}
            setMember={setMember}
            setViewTransactions={setViewTransactions}
        />
        {accountId && viewTransactions && <TransactionsSection accountId={accountId} />}
      </div>
      <FormModal header={modalHeader} setOpenModal={setShowRemoveAccountModal} openModal={showRemoveAccountModal}>
        <div>Are you sure you want to <span className="text-red-500">remove</span> the <span className="text-gray-500">{removeAccountId}</span> account assignment?</div>
        <div className="w-full flex justify-between pt-4">
            <Button color="light" onClick={onClickCancelRemoveAssingedAccount}>
            Cancel
            </Button>
            <Button color="dark" onClick={onClickRemoveAssingedAccount}>
            Remove
            </Button>
        </div>
      </FormModal>
      <FormModal header={assignAccountHeader} setOpenModal={setShowFormModal} openModal={showFormModal}>
        <div>Select Account:</div>
      </FormModal>
    </Card>
  );
};

export default ViewAccountsSection;
