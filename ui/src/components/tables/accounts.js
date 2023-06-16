'use client';

import { Card, Pagination, Spinner } from 'flowbite-react';
import { useState } from 'react';

import Datatable from '~/components/tables/datatable';
import { Capitalize, USDollar } from '~/helpers';
import useTransactionsStatement from '~/hooks/transaction-statements';

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

const AccountsTable = ({ accountId }) => {
  const [continuationToken, setContinuationToken] = useState('');
  const [page, setPage] = useState(1);
  const { data, isLoading } = useTransactionsStatement(accountId, continuationToken);

  const formattedData = data?.page.map((row) => {
    console.log(row);
    return {
      ...row,
      viewDetails: <a href="">View Details</a>,
      viewTransactions: <a href="">View Transactions</a>
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
    </Card>
  );
};

export default AccountsTable;
