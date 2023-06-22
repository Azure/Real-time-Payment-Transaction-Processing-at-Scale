'use client';

import { Card, Pagination, Spinner } from 'flowbite-react';
import { useState } from 'react';

import Datatable from '~/components/tables/datatable';
import { Capitalize, USDollar } from '~/helpers';
import useTransactionsStatement from '~/hooks/transaction-statements';

const headers = [
  {
    key: 'merchant',
    name: 'Merchant'
  },
  {
    key: 'description',
    name: 'Description'
  },
  {
    key: 'type',
    name: 'Type'
  },
  {
    key: 'amount',
    name: 'Amount'
  },
  {
    key: 'timestamp',
    name: 'Timestamp'
  }
];

const TransactionsStatementTable = ({ accountId }) => {
  const [continuationToken, setContinuationToken] = useState('');
  const [page, setPage] = useState(1);
  const { data, isLoading } = useTransactionsStatement('0909090907', continuationToken);

  const formattedData = data?.page.map((row) => {
    const date = new Date(row.timestamp);
    return {
      ...row,
      type: Capitalize(row.type),
      amount: USDollar.format(row.amount),
      timestamp: `${date.toLocaleDateString()} ${date.toLocaleTimeString()}`
    };
  });

  return (
    <Card className="card w-full justify-center items-center">
      <div className="text-xl p-6 font-bold">Transactions</div>
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

export default TransactionsStatementTable;
