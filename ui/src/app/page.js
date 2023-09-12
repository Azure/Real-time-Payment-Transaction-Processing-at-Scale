'use client';

import { useState } from 'react';
import { QueryClient, QueryClientProvider } from 'react-query';

import MemberDetailsSection from '~/components/sections/members/details';
import MembersSection from '~/components/sections/members/members';
import ViewAccountsSection from '~/components/sections/members/accounts';

const client = new QueryClient();

export default function Home() {
  const [member, setMember] = useState();
  const [accountsSelected, setAccountsSelected] = useState(false);
  return (
    <QueryClientProvider client={client}>
      <main className="flex min-h-screen flex-col items-center justify-between p-24 overflow-hidden p-6">
        <MembersSection setMember={setMember} setAccountsSelected={setAccountsSelected} />
        {member && !accountsSelected && <MemberDetailsSection member={member} setMember={setMember} />}
        {member && accountsSelected && <ViewAccountsSection member={member} setMember={setMember} />}
      </main>
    </QueryClientProvider>
  );
}
