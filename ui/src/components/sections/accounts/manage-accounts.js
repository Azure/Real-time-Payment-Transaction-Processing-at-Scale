'use client';

import { useState } from 'react';
import { Button } from 'flowbite-react';
import { PlusIcon } from '@heroicons/react/24/outline';

import AccountsTable from '~/components/tables/accounts';

const ManageAccountsSection = ({ setAccountId, newTransaction }) => {
  const [showFormModal, setShowFormModal] = useState(false);

  const onClickAdd = () => setShowFormModal(true);

  return (
    <div className="w-full">
      <div className="flex items-center justify-between">
        <h1 className="my-6 text-left">Manage Accounts</h1>
        <div className="justify-end">
          <Button color="dark" className="p-0" onClick={onClickAdd}>
            <PlusIcon className="h-6 w-6 text-gray-500 mr-3 text-white" />
            <h4>Create Account</h4>
          </Button>
        </div>
      </div>
      <AccountsTable
        showFormModal={showFormModal}
        setShowFormModal={setShowFormModal}
        setAccountId={setAccountId}
        newTransaction={newTransaction}
      />
    </div>
  );
};

export default ManageAccountsSection;
