'use client';

import { Card, Pagination, Spinner } from 'flowbite-react';
import { useCallback, useState } from 'react';

import { useRouter } from 'next/navigation';
import Datatable from '~/components/tables/datatable';
import FormModal from '~/components/modals/form';
import NewMemberForm from '~/components/forms/new-member';
import useMembers from '~/hooks/members';

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

const MembersTable = ({ setMember, showFormModal, setShowFormModal }) => {
  const router = useRouter();
  const [continuationToken, setContinuationToken] = useState('');
  const [page, setPage] = useState(1);

  const { data, isLoading } = useMembers(continuationToken);

  const onClickDetails = useCallback(
    (memberId) => {
      const member = data?.page.find((member) => member.id === memberId);
      setMember(member);
    },
    [data?.page, setMember]
  );
  const onClickAccounts = (memberId) => {
    router.push(`accounts/${memberId}`);
  };

  const formattedData = data?.page.map((row) => {
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
          View Account(s)
        </p>
      )
    };
  });

  const modalHeader = <h3 className="text-xl p-4">New Member</h3>;

  return (
    <Card className="card w-full justify-center items-center">
      <h3 className="p-6 font-bold">Members</h3>
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
      <FormModal header={modalHeader} openModal={showFormModal} setOpenModal={setShowFormModal}>
        <NewMemberForm />
      </FormModal>
    </Card>
  );
};

export default MembersTable;
