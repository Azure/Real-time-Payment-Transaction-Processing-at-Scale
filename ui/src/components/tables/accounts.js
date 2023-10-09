'use client';

import { Card, Spinner } from 'flowbite-react';
import { useCallback, useEffect, useState } from 'react';

import { Capitalize, USDollar } from '~/helpers';
import AccountDetailModal from '~/components/modals/account-detail';
import Datatable from '~/components/tables/datatable';
import useAccounts from '~/hooks/accounts';
import MembersAccounts from '~/hooks/members-accounts';
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

const AccountsTable = ({ setAccountId, setRemoveAccountId, showFormModal, setShowFormModal, setShowRemoveAccountModal, memberId, setMember, reload, setReload, setViewTransactions = null, newTransaction }) => {
  const [continuationToken, setContinuationToken] = useState('');
  const [nextToken, setNextToken] = useState('');
  const [rows, setRows] = useState([]);
  const [page, setPage] = useState(0);
  const [account, setAccount] = useState();
  const [showDetailModal, setShowDetailModal] = useState(false);
  const [showLoadMore, setShowLoadMore] = useState(true);

  let hook = useAccounts(continuationToken);
  if (memberId) {
    hook = MembersAccounts(memberId);
  }
  const { data, isLoading, mutate } = hook;

  const onClickDetails = useCallback(
    (accountId) => {
      const account = rows.find((account) => account.id === accountId);
      setAccount(account);
      setShowDetailModal(true);
    },
    [rows]
  );
  const onClickTransactions = (accountId) => {
    setAccountId(accountId);
    if (setViewTransactions) {
      setViewTransactions(true);
    }
  };

  const onClickLoadMore = useCallback(() => {
    setPage(page + 1);
    setContinuationToken(nextToken);
  }, [page, nextToken]);

  const onClickGoToTop = useCallback(() => {
    setPage(0);
    setRows([]);
    setContinuationToken('');
  }, []);

  const onClickRemoveAccountAssignment = (accountId) => {
    setRemoveAccountId(accountId);
    setShowRemoveAccountModal(true);
  };

  useEffect(() => {
    if (memberId) {
      setRows([]);
      if(headers.find((header) => header.key === 'removeAccountAssignment') === undefined) {
        headers.splice(1, 1);
        headers.push({
          key: 'removeAccountAssignment',
          name: ''
        });
      } else {
        if (headers.length > 7) {
          headers.splice(1, 1);
        }
      }
    } else {
      if(headers.find((header) => header.key === 'removeAccountAssignment') !== undefined) {
        headers.splice(7, 1);
        headers.splice(1, 0, {
          key: 'customerGreetingName',
          name: 'Customer Greeting Name'
        });
      }
    }
  }, [memberId]);

  useEffect(() => {
    if (data) {
      if(memberId) {
        setShowLoadMore(false);
        setRows([]);
        setRows((currRows) =>
          _.orderBy(_.unionBy([...currRows, ...data], 'id'), ['timestamp'], ['desc'])
        );
      } else {
        setRows((currRows) => {
          const updatedData = _.orderBy(_.unionBy([...currRows, ...data.page], 'id'), ['timestamp'], ['desc']);
          return _.map(updatedData, (item) => {
            const updatedItem = _.find(data.page, { 'id': item.id });
            return updatedItem ? updatedItem : item;
          });
        });
        setNextToken(data.continuationToken);
      }
    }
  }, [data, memberId]);

  useEffect(() => {
    if (reload) {
      window.reload;
    }
  }, [reload, memberId]);

  useEffect(() => {
    if(!memberId) {
      mutate();
    }
  }, [newTransaction]);

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
      ),
      removeAccountAssignment: memberId ? (
        <p className="underline cursor-pointer text-red-600" onClick={() => onClickRemoveAccountAssignment(row.id)}>
          Remove Account Assignment
        </p>
      ) : (
        ''
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
            showLoadMore={showLoadMore}
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
