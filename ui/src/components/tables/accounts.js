'use client';

import { Card, Spinner } from 'flowbite-react';
import { useCallback, useEffect, useState } from 'react';

import { Capitalize, USDollar } from '~/helpers';
import AccountDetailModal from '~/components/modals/account-detail';
import Datatable from '~/components/tables/datatable';
import useAccounts from '~/hooks/accounts';
import FormModal from '~/components/modals/form';
import NewAccountForm from '~/components/forms/new-account';
import _ from 'lodash';

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
  const [nextToken, setNextToken] = useState('');
  const [rows, setRows] = useState([]);
  const [page, setPage] = useState(0);
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

  const formattedData = rows.map((row) => {
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
