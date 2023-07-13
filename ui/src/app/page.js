'use client';

import { useState } from 'react';
import { QueryClient, QueryClientProvider } from 'react-query';

import MemberDetailsSection from '~/components/sections/members/details';
import MembersSection from '~/components/sections/members/members';

const client = new QueryClient();

export default function Home() {
  const [member, setMember] = useState();
  return (
    <QueryClientProvider client={client}>
      <main className="flex min-h-screen flex-col items-center justify-between p-24 overflow-hidden p-6">
        <MembersSection setMember={setMember} />
        {member && <MemberDetailsSection member={member} setMember={setMember} />}
      </main>
    </QueryClientProvider>
  );
}
