'use client';

import { Card, Spinner } from 'flowbite-react';
import { useCallback, useEffect, useState } from 'react';

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
  const [nextToken, setNextToken] = useState('');
  const [rows, setRows] = useState([]);
  const [history, setHistory] = useState([]);
  const [page, setPage] = useState(0);
  const { data, isLoading, mutate, isValidating } = useTransactionsStatement(
    accountId,
    continuationToken
  );

  const onClickLoadMore = useCallback(() => {
    setPage(page + 1);
  }, [page]);

  const onClickGoToTop = useCallback(() => {
    setPage(0);
    setRows([]);
    setNextToken('');
  }, []);

  useEffect(() => {
    if (data) {
      setRows((currRows) => [...currRows, ...data.page]);
      setNextToken(data.continuationToken);
    }
  }, [data]);

  useEffect(() => {
    setContinuationToken(nextToken);
  }, [page]);

  useEffect(() => {
    mutate();
  }, [continuationToken, mutate]);

  const formattedData = rows.map((row) => {
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
      {isLoading || isValidating ? (
        <div className="text-center p-6">
          <Spinner aria-label="Loading..." />
        </div>
      ) : (
        <div className="tables">
          <Datatable
            headers={headers}
            data={formattedData}
            onClickLoadMore={onClickLoadMore}
            continuationToken={data.continuationToken}
            onClickGoToTop={onClickGoToTop}
          />
        </div>
      )}
    </Card>
  );
};

export default TransactionsStatementTable;
