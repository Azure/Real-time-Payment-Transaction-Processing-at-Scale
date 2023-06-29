'use client';

import { Table } from 'flowbite-react';

const Datatable = ({ headers = [], data = [] }) => {
  return (
    <Table className="w-full" hoverable>
      <Table.Head>
        {headers.map((header) => (
          <Table.HeadCell key={header.key} className="!p-4">
            {header.name}
          </Table.HeadCell>
        ))}
      </Table.Head>
      <Table.Body className="divide-y">
        {data.length ? (
          data.map((row) => (
            <Table.Row key={row.id} className="bg-white dark:border-gray-700 dark:bg-gray-800">
              {Object.values(headers).map((header, index) => (
                <Table.Cell key={`${row.id}-${index}`} className="!p-4">
                  {row[header.key]}
                </Table.Cell>
              ))}
            </Table.Row>
          ))
        ) : (
          <Table.Row className="bg-white dark:border-gray-700 dark:bg-gray-800">
            <Table.Cell colSpan={headers.length} className="!p-4 text-center">
              No results.
            </Table.Cell>
          </Table.Row>
        )}
      </Table.Body>
    </Table>
  );
};

export default Datatable;
