import { useState, useEffect } from 'react';
import { Button, Card } from 'flowbite-react';
import { PlusIcon } from '@heroicons/react/24/outline';

import FormModal from '~/components/modals/form';
import AssignAccountForm from '~/components/forms/assign-account';
import AccountsTable from '~/components/tables/accounts';
import TransactionsSection from '~/components/sections/accounts/transactions';

import UnassignMemberAccount from '~/hooks/unassign-member-account';

const ViewAccountsSection = ({ member, setMember }) => {
  const [showFormModal, setShowFormModal] = useState(false);
  const [accountId, setAccountId] = useState();
  const [removeAccountId, setRemoveAccountId] = useState();
  const [showRemoveAccountModal, setShowRemoveAccountModal] = useState(false);
  const [confirmation, setConfirmation] = useState(false);
  const [reload, setReload] = useState(false);
  const [viewTransactions, setViewTransactions] = useState(false);
  

  const { mutate: RemoveTrigger } = UnassignMemberAccount(removeAccountId, member.id);

  const onClickAssign = () => setShowFormModal(true);
  const onClickCancelRemoveAssingedAccount = () => setShowRemoveAccountModal(false);

  useEffect(() => {
    if (member.id) {
        setViewTransactions(false);
    }
  }, [member]);

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
      <div className="flex items-center justify-between">
        <h1 className="my-6 text-left font-bold">Accounts: {member.firstName} {member.lastName}</h1>
        <div className="justify-end">
          <Button color="dark" className="p-0" onClick={onClickAssign}>
            <PlusIcon className="h-6 w-6 text-gray-500 mr-3 text-white" />
            <h4>Assign Account</h4>
          </Button>
        </div>
      </div>
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
        <AssignAccountForm setOpenModal={setShowFormModal} memberId={member.id} />
      </FormModal>
    </Card>
  );
};

export default ViewAccountsSection;
