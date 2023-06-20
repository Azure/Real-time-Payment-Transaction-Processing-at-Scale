import { useState } from 'react';
import { Button, Spinner } from 'flowbite-react';
import { PlusIcon } from '@heroicons/react/24/outline';

import { Capitalize, USDollar } from '~/helpers';
import FormModal from '~/components/modals/form';
import NewTransactionForm from '~/components/forms/new-transaction';
import TransactionsStatementTable from '~/components/tables/transactions-statement';
import useAccountSummary from '~/hooks/account-summary';
import useAddTransaction from '~/hooks/add-transaction';

const TransactionsSection = ({ accountId }) => {
  const { data, isLoading } = useAccountSummary(accountId);
  const [isOpenModal, setIsOpenModal] = useState(false);
  const { trigger } = useAddTransaction();

  const onClickAdd = () => setIsOpenModal(true);

  const onSubmit = async () => {
    const response = await trigger({
      accountId: '0909090907',
      type: 'deposit',
      description: 'Item refund',
      merchant: 'Tailspin Toys',
      amount: 38.26
    });

    if (response.status === 200) {
      setIsOpenModal(false);
    }
  };

  const modalHeader = <div className="text-xl p-4">New Transaction</div>;

  return (
    <div className="w-full mt-6">
      <h1 className="my-6">Transactions for Account Id {accountId}</h1>
      {isLoading ? (
        <div className="text-center p-6">
          <Spinner aria-label="Loading..." />
        </div>
      ) : (
        <div className="flex items-center mb-6">
          <div className="flex flex-col space-beteween text-md flex-1">
            <div className="flex text-sm w-full mb-6">
              <div className="flex flex-1 items-center mr-3">
                <caption className="font-bold mr-2">Customer Greeting Name:</caption>
                <p>{data.customerGreetingName}</p>
              </div>
              <div className="flex flex-1 items-center">
                <caption className="font-bold mr-2">Account type:</caption>
                <p>{Capitalize(data.accountType)}</p>
              </div>
            </div>
            <div className="flex text-sm w-full">
              <div className="flex flex-1 items-center mr-3">
                <caption className="font-bold mr-2">Balance:</caption>
                <p>{USDollar.format(data.balance)}</p>
              </div>
              <div className="flex flex-1 items-center">
                <caption className="font-bold mr-2">Overdraft Limit:</caption>
                <p>{USDollar.format(data.overdraftLimit)}</p>
              </div>
            </div>
          </div>
          <div className="justify-end">
            <Button color="dark" className="p-0" onClick={onClickAdd}>
              <PlusIcon className="h-6 w-6 text-gray-500 mr-3 text-white" />
              <h4>New Transaction</h4>
            </Button>
          </div>
        </div>
      )}
      <TransactionsStatementTable accountId={accountId} />
      <FormModal
        onSubmit={onSubmit}
        header={modalHeader}
        openModal={isOpenModal}
        setOpenModal={setIsOpenModal}>
        <NewTransactionForm />
      </FormModal>
    </div>
  );
};

export default TransactionsSection;
