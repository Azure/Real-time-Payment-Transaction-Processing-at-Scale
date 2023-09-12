'use client';

import { useCallback, useEffect, useState } from 'react';
import { Card, Spinner } from 'flowbite-react';

import Datatable from '~/components/tables/datatable';
import FormModal from '~/components/modals/form';
import NewMemberForm from '~/components/forms/new-member';
import useMembers from '~/hooks/members';
import _ from 'lodash';

const headers = [
  {
    key: 'name',
    name: 'Name'
  },
  {
    key: 'state',
    name: 'State/Province'
  },
  {
    key: 'country',
    name: 'Country'
  },
  {
    key: 'city',
    name: 'City'
  },
  {
    key: 'details',
    name: ''
  },
  {
    key: 'viewAccounts',
    name: ''
  }
];

const MembersTable = ({ setMember, showFormModal, setShowFormModal, setAccountsSelected }) => {
  const [continuationToken, setContinuationToken] = useState('');
  const [nextToken, setNextToken] = useState('');
  const [rows, setRows] = useState([]);
  const [page, setPage] = useState(0);
  const { data, isLoading } = useMembers(continuationToken);

  const onClickDetails = useCallback(
    (memberId) => {
      const member = rows.find((member) => member.id === memberId);
      setMember(member);
      setAccountsSelected(false);
    },
    [setMember, rows, setAccountsSelected]
  );

  const onClickAccounts = useCallback(
    (memberId) => {
      const member = rows.find((member) => member.id === memberId);
      setMember(member);
      setAccountsSelected(true);
    },
    [setMember, rows, setAccountsSelected]
  );

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
      setRows((currRows) => _.unionBy([...currRows, ...data.page], 'id'));
      setNextToken(data.continuationToken);
    }
  }, [data]);

  const formattedData = rows.map((row) => {
    return {
      ...row,
      name: `${row.firstName} ${row.lastName}`,
      details: (
        <p className="underline cursor-pointer" onClick={() => onClickDetails(row.id)}>
          Details
        </p>
      ),
      viewAccounts: (
        <p className="underline cursor-pointer" onClick={() => onClickAccounts(row.id)}>
          View Accounts
        </p>
      )
    };
  });

  const modalHeader = <div className="text-xl p-4">New Member</div>;

  return (
    <Card className="card w-full justify-center items-center">
      <div className="text-xl p-6 font-bold">Members</div>
      {isLoading ? (
        <div className="text-center p-6">
          <Spinner aria-label="Loading..." />
        </div>
      ) : (
        <div className="tables">
          <Datatable
            headers={headers}
            data={formattedData}
            continuationToken={data?.continuationToken}
            onClickLoadMore={onClickLoadMore}
            onClickGoToTop={onClickGoToTop}
          />
        </div>
      )}
      <FormModal header={modalHeader} setOpenModal={setShowFormModal} openModal={showFormModal}>
        <NewMemberForm setOpenModal={setShowFormModal} />
      </FormModal>
    </Card>
  );
};

export default MembersTable;
