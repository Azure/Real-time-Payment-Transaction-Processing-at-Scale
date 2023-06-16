import { Spinner } from 'flowbite-react';

import TransactionsStatementTable from '~/components/tables/transactions-statement-table';
import { Capitalize, USDollar } from '~/helpers';
import useAccountSummary from '~/hooks/account-summary';

const TransactionsSection = ({ account }) => {
  const { data, isLoading } = useAccountSummary(account);
  return (
    <div>
      <h1 className="my-6">Transactions for Account Id {account}</h1>
      {isLoading ? (
        <div className="text-center p-6">
          <Spinner aria-label="Loading..." />
        </div>
      ) : (
        <div className="flex flex-col space-beteween text-md">
          <div className="flex text-sm w-full mb-6">
            <div className="flex flex-1">
              <caption className="font-bold mr-2">Customer Greeting Name:</caption>
              <p>{data.customerGreetingName}</p>
            </div>
            <div className="flex flex-1">
              <caption className="font-bold mr-2">Account type:</caption>
              <p>{Capitalize(data.accountType)}</p>
            </div>
          </div>
          <div className="flex text-sm w-full mb-6">
            <div className="flex flex-1">
              <caption className="font-bold mr-2">Balance:</caption>
              <p>{USDollar.format(data.balance)}</p>
            </div>
            <div className="flex flex-1">
              <caption className="font-bold mr-2">Overdraft Limit:</caption>
              <p>{USDollar.format(data.overdraftLimit)}</p>
            </div>
          </div>
        </div>
      )}
      <TransactionsStatementTable accountId={account} />
    </div>
  );
};

export default TransactionsSection;
