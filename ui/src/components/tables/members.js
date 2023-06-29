'use client';

import { useCallback, useEffect, useState } from 'react';
import { Card, Pagination, Spinner } from 'flowbite-react';
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
  }
];

const MembersTable = ({ setMember, showFormModal, setShowFormModal }) => {
  const [continuationToken, setContinuationToken] = useState('');
  const [history, setHistory] = useState([]);
  const [page, setPage] = useState(0);
  const { data, isLoading, mutate, isValidating } = useMembers(continuationToken);

  const onClickDetails = useCallback(
    (memberId) => {
      const member = data?.page.find((member) => member.id === memberId);
      setMember(member);
    },
    [data?.page, setMember]
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
    return {
      ...row,
      name: `${row.firstName} ${row.lastName}`,
      details: (
        <p className="underline cursor-pointer" onClick={() => onClickDetails(row.id)}>
          Details
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
      <FormModal header={modalHeader} setOpenModal={setShowFormModal} openModal={showFormModal}>
        <NewMemberForm setOpenModal={setShowFormModal} />
      </FormModal>
    </Card>
  );
};

export default MembersTable;
