'use client';

import { Card, Pagination, Spinner } from 'flowbite-react';
import { useCallback, useState } from 'react';
import AccountDetailModal from '~/components/modals/account-detail';

import Datatable from '~/components/tables/datatable';
import useAccounts from '~/hooks/accounts';

const headers = [
  {
    key: 'accountId',
    name: 'Account Id'
  },
  {
    key: 'customerGreetingName',
    name: 'Customer Greeting Name'
  },
  {
    key: 'accountType',
    name: 'Account Type'
  },
  {
    key: 'balance',
    name: 'Balance'
  },
  {
    key: 'overdraftLimit',
    name: 'Overdraft Limit'
  },
  {
    key: 'viewDetails',
    name: ''
  },
  {
    key: 'viewTransactions',
    name: ''
  }
];

const AccountsTable = ({ setAccountId }) => {
  const [continuationToken, setContinuationToken] = useState('');
  const [page, setPage] = useState(1);
  const [account, setAccount] = useState();
  const [showDetailModal, setShowDetailModal] = useState(false);
  const { data, isLoading } = useAccounts(continuationToken);

  const onClickDetails = useCallback(
    (accountId) => {
      const account = data?.page.find((account) => account.id === accountId);
      setAccount(account);
      setShowDetailModal(true);
    },
    [data?.page]
  );
  const onClickTransactions = (accountId) => setAccountId(accountId);

  const formattedData = data?.page.map((row) => {
    return {
      ...row,
      viewDetails: (
        <p className="underline cursor-pointer" onClick={() => onClickDetails(row.id)}>
          View Details
        </p>
      ),
      viewTransactions: (
        <p className="underline cursor-pointer" onClick={() => onClickTransactions(row.id)}>
          View Transactions
        </p>
      )
    };
  });

  return (
    <Card className="card w-full justify-center items-center">
      <h3 className="p-6 font-bold">Accounts</h3>
      {isLoading ? (
        <div className="text-center p-6">
          <Spinner aria-label="Loading..." />
        </div>
      ) : (
        <Datatable headers={headers} data={formattedData} />
      )}
      <Pagination
        className="p-6 self-center"
        currentPage={page}
        layout="navigation"
        onPageChange={(page) => {
          setPage(page);
          setContinuationToken(data.continuationToken);
        }}
        totalPages={100}
      />
      <AccountDetailModal
        openModal={showDetailModal}
        setOpenModal={setShowDetailModal}
        account={account}
      />
    </Card>
  );
};

export default AccountsTable;
