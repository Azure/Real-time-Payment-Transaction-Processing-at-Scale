'use client';

import React, { useState } from 'react';
import { Sidebar } from 'flowbite-react';
import { useRouter } from 'next/navigation';
import { UsersIcon, BuildingLibraryIcon, Bars3Icon, XMarkIcon } from '@heroicons/react/24/outline';

const CustomSidebar = () => {
  const [expandSidebar, setExpandSidebar] = useState(true);
  const [exported, setExported] = useState(false);
  const [linkElement, setLinkElement] = useState(null);
  const router = useRouter();

  const onClickHambuger = () => {
    setExpandSidebar(!expandSidebar);
    if (!exported) {
      const newCss = `
        .sidebar-margin {
          margin-left: 4rem !important;
        }
      `;

      const blob = new Blob([newCss], { type: 'text/css' });
      const url = URL.createObjectURL(blob);

      const link = document.createElement('link');
      link.href = url;
      link.rel = 'stylesheet';

      document.head.appendChild(link);

      setLinkElement(link);
      setExported(true);
    } else {
      if (linkElement) {
        document.head.removeChild(linkElement);
        setLinkElement(null);
      }

      setExported(false);
    }
  };

  return (
    <Sidebar className="h-full fixed z-50" style={!expandSidebar ? {width: "4rem"} : {}}>
      <Sidebar.Items>
        <Sidebar.ItemGroup>
          <Sidebar.Item>
            <div className="flex items-center justify-left cursor-pointer" onClick={onClickHambuger}>
              {expandSidebar ? ( <XMarkIcon className="h-6 w-6 text-gray-500 text-black" /> ) : ( <Bars3Icon className="h-6 w-6 text-gray-500 text-black" /> )}
              {expandSidebar ? <div className="ml-3">@ Statements</div> : null}
            </div>
          </Sidebar.Item>
        </Sidebar.ItemGroup>
        <Sidebar.ItemGroup>
          <Sidebar.Item>
            <div className="flex items-center justify-left cursor-pointer" onClick={() => {router.push('/');}}>
              <UsersIcon className="h-6 w-6 text-gray-500 text-black" />
              {expandSidebar ? <div className="ml-3">Members</div> : null}
            </div>
          </Sidebar.Item>
          <Sidebar.Item>
            <div className="flex items-center justify-left cursor-pointer" onClick={() => {router.push('accounts');}}>
              <BuildingLibraryIcon className="h-6 w-6 text-gray-500 text-black" />
              {expandSidebar ? <div className="ml-3">Accounts</div> : null}
            </div>
          </Sidebar.Item>
        </Sidebar.ItemGroup>
      </Sidebar.Items>
    </Sidebar>
  );
};

export default CustomSidebar;
