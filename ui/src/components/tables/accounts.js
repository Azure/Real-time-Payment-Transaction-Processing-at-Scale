'use client';

import { Card, Spinner } from 'flowbite-react';
import { useCallback, useEffect, useState } from 'react';

import { Capitalize, USDollar } from '~/helpers';
import AccountDetailModal from '~/components/modals/account-detail';
import Datatable from '~/components/tables/datatable';
import useAccounts from '~/hooks/accounts';
import FormModal from '~/components/modals/form';
import NewAccountForm from '~/components/forms/new-account';

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

const AccountsTable = ({ setAccountId, showFormModal, setShowFormModal }) => {
  const [continuationToken, setContinuationToken] = useState('');
  const [history, setHistory] = useState([]);
  const [page, setPage] = useState(0);
  const [account, setAccount] = useState();
  const [showDetailModal, setShowDetailModal] = useState(false);

  const { data, isLoading, mutate, isValidating } = useAccounts(continuationToken);

  const onClickDetails = useCallback(
    (accountId) => {
      const account = data?.page.find((account) => account.id === accountId);
      setAccount(account);
      setShowDetailModal(true);
    },
    [data?.page]
  );
  const onClickTransactions = (accountId) => setAccountId(accountId);

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
    return {
      ...row,
      accountType: Capitalize(row.accountType),
      balance: USDollar.format(row.balance),
      overdraftLimit: USDollar.format(row.overdraftLimit),
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

  const modalHeader = <div className="text-xl p-4">New Account</div>;

  return (
    <Card className="card w-full justify-center items-center">
      <div className="text-xl p-6 font-bold">Accounts</div>
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
      <AccountDetailModal
        openModal={showDetailModal}
        setOpenModal={setShowDetailModal}
        account={account}
      />
      <FormModal header={modalHeader} setOpenModal={setShowFormModal} openModal={showFormModal}>
        <NewAccountForm setOpenModal={setShowFormModal} />
      </FormModal>
    </Card>
  );
};

export default AccountsTable;
