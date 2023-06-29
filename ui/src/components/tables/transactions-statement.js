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
  const [history, setHistory] = useState([]);
  const [page, setPage] = useState(0);
  const { data, isLoading, mutate, isValidating } = useTransactionsStatement(
    accountId,
    continuationToken
  );

  const onClickNext = useCallback(() => {
    setPage(page + 1);
  }, [page]);

  const onClickPrev = useCallback(() => {
    history.pop();
    setHistory(history);
    setPage(page - 1);
  }, [page, history]);

  useEffect(() => {
    if (data) {
      setHistory((history) => {
        if (!history.includes(data.continuationToken) && page === history.length) {
          return [...history, data.continuationToken];
        } else return history;
      });
    }
  }, [data, page]);

  useEffect(() => {
    setContinuationToken(history[page - 1]);
  }, [history, page]);

  useEffect(() => {
    mutate();
  }, [continuationToken, mutate]);

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
      {isLoading || isValidating ? (
        <div className="text-center p-6">
          <Spinner aria-label="Loading..." />
        </div>
      ) : (
        <Datatable headers={headers} data={formattedData} />
      )}
      <div className="p-6 self-center">
        <button onClick={onClickPrev} disabled={history.length <= 1} className="p-2 border rounded">
          Previous
        </button>
        <button
          onClick={onClickNext}
          disabled={history.length > 1 && history[history.length - 1] === ''}
          className="p-2 border rounded">
          Next
        </button>
      </div>
    </Card>
  );
};

export default TransactionsStatementTable;
