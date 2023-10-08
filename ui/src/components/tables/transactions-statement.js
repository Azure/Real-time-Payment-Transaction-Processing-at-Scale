'use client';

import { Card, Spinner } from 'flowbite-react';
import { useCallback, useEffect, useState } from 'react';

import Datatable from '~/components/tables/datatable';
import { Capitalize, USDollar, FormatDate } from '~/helpers';
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

const TransactionsStatementTable = ({ accountId, newTransaction }) => {
  const [continuationToken, setContinuationToken] = useState('');
  const [nextToken, setNextToken] = useState('');
  const [rows, setRows] = useState([]);
  const [page, setPage] = useState(0);
  const { data, isLoading, mutate } = useTransactionsStatement(accountId, continuationToken);

  const onClickLoadMore = useCallback(() => {
    setPage(page + 1);
    setContinuationToken(nextToken);
  }, [page, nextToken]);

  const onClickGoToTop = useCallback(() => {
    setPage(0);
    setRows([]);
    setContinuationToken('');
  }, []);

  useEffect(() => {
    if (data) {
      setRows((currRows) =>
        _.orderBy(_.unionBy([...currRows, ...data.page], 'id'), ['timestamp'], ['desc'])
      );
      setNextToken(data.continuationToken);
    }
  }, [data]);

  useEffect(() => {
    mutate();
  }, [newTransaction]);

  const formattedData = rows.map((row) => {
    return {
      ...row,
      type: Capitalize(row.type),
      amount: USDollar.format(row.amount),
      timestamp: FormatDate(row.timestamp)
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
        <div className="tables">
          <Datatable
            headers={headers}
            data={formattedData}
            onClickLoadMore={onClickLoadMore}
            continuationToken={data?.continuationToken}
            onClickGoToTop={onClickGoToTop}
          />
        </div>
      )}
    </Card>
  );
};

export default TransactionsStatementTable;
