import { useEffect, useState } from 'react';
import { Button, Spinner } from 'flowbite-react';
import { PlusIcon, SparklesIcon } from '@heroicons/react/24/outline';

import { Capitalize, USDollar } from '~/helpers';
import FormModal from '~/components/modals/form';
import NewTransactionForm from '~/components/forms/new-transaction';
import AnalyzeTransactionsForm from '~/components/forms/analyze-transactions';
import TransactionsStatementTable from '~/components/tables/transactions-statement';
import useAccountSummary from '~/hooks/account-summary';

const TransactionsSection = ({ accountId }) => {
  const { data, isLoading, isValidating } = useAccountSummary(accountId);
  const [isOpenModal, setIsOpenModal] = useState(false);
  const [loading, setLoading] = useState(false);
  const [isAnalyzeModalOpen, setIsAnalyzeModalOpen] = useState(false);
  const [submittedData, setSubmittedData] = useState({});

  const onClickAdd = () => setIsOpenModal(true);
  const onClickAnalyze = () => setIsAnalyzeModalOpen(true);

  const modalHeader = <div className="text-xl p-4">New Transaction</div>;
  const analyzeModalHeader = <div className="text-xl p-4">Analyze Transactions</div>;

  useEffect(() => {
    setLoading(isLoading || isValidating);
  }, [isLoading, isValidating]);

  return (
    <div className="w-full mt-6">
      <h1 className="my-6">Transactions for Account Id {accountId}</h1>
      {loading ? (
        <div className="text-center p-6">
          <Spinner aria-label="Loading..." />
        </div>
      ) : (
        <div className="flex items-center mb-6">
          <div className="flex flex-col space-beteween text-md flex-1">
            <div className="flex text-sm w-full mb-6">
              <div className="flex flex-1 items-center mr-3">
                <p className="font-bold mr-2">Customer Greeting Name:</p>
                <p>{data?.customerGreetingName}</p>
              </div>
              <div className="flex flex-1 items-center">
                <p className="font-bold mr-2">Account type:</p>
                <p>{Capitalize(data?.accountType ?? '')}</p>
              </div>
            </div>
            <div className="flex text-sm w-full">
              <div className="flex flex-1 items-center mr-3">
                <p className="font-bold mr-2">Balance:</p>
                <p>{USDollar.format(data?.balance)}</p>
              </div>
              <div className="flex flex-1 items-center">
                <p className="font-bold mr-2">Overdraft Limit:</p>
                <p>{USDollar.format(data?.overdraftLimit)}</p>
              </div>
            </div>
          </div>
          <div className="justify-end">
            <Button color="dark" className="p-0" onClick={onClickAnalyze}>
              <SparklesIcon className="h-6 w-6 text-gray-500 mr-3 text-white" />
              <h4>Analyze Transactions</h4>
            </Button>
          </div>
        </div>
      )}

      {loading ? (
        <div className="text-center p-6">
          <Spinner aria-label="Loading..." />
        </div>
      ) : (
        <TransactionsStatementTable accountId={accountId} />
      )}

      <FormModal
        header={analyzeModalHeader}
        openModal={isAnalyzeModalOpen}
        setOpenModal={setIsAnalyzeModalOpen}>
        <AnalyzeTransactionsForm accountId={accountId} setOpenModal={setIsAnalyzeModalOpen} />
      </FormModal>

      <div className="justify-end">
        <Button color="dark" className="p-0 mt-6" onClick={onClickAdd}>
          <PlusIcon className="h-6 w-6 text-gray-500 mr-3 text-white" />
          <h4>New Transaction</h4>
        </Button>
      </div>

      <FormModal header={modalHeader} openModal={isOpenModal} setOpenModal={setIsOpenModal}>
        <NewTransactionForm accountId={accountId} setOpenModal={setIsOpenModal} setSubmittedData={setSubmittedData} />
      </FormModal>
    </div>
  );
};

export default TransactionsSection;
