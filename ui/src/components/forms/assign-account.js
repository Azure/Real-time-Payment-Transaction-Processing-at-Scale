import React, { useState, useCallback, useEffect } from 'react';
import { Button, Label, Spinner, TextInput } from 'flowbite-react';
import { useDebounce } from '~/hooks/use-debounce';

import FindAccountMatch from '~/hooks/search-accounts';
import AssignMemberAccount from '~/hooks/assign-member-account';

const AssignAccountForm = ({ setOpenModal, memberId }) => {
    const [queryMade, setQueryMade] = useState(false);
    const [form, setForm] = useState("");
    const [account, setAccount] = useState();
    const [confirmation, setConfirmation] = useState(false);

    const { mutate: AddTrigger } = AssignMemberAccount(account, memberId);

    const onChangeQuery = (e) => {
        setAccount(null);
        setForm(e.target.value);
        if(e.target.value.length > 0) {
            setQueryMade(true);
        } else {
            setQueryMade(false);
        }
    };

    const onAccountSelected = (e) => {
        setAccount(e.target.innerHTML);
        setForm(e.target.innerHTML);
        setQueryMade(false);
    };

    const onClickCancel = () => {
        setForm("");
        setAccount(null);
        setOpenModal(false);
    };

    const onClickSave = () => {
        setConfirmation(true);
        AddTrigger(confirmation, {
            onSuccess: async () => {
                setConfirmation(false);
            },
            onError: async (error) => {
                setConfirmation(false);
            }
        });
        setOpenModal(false);
    };
    
    const searchQuery = useDebounce(form, 1000)

    const { data, isLoading } = FindAccountMatch(searchQuery);

    useEffect(() => {
        if(data?.length === 1) {
            setAccount(data[0].id);
            setQueryMade(false);
        }
    }, [data]);

    

    return (
        <div className="space-y-6">
            <div className="mb-4">
                <div className="flex flex-row">
                    <div className="mb-2 block">
                        <Label htmlFor="select account" value="Select Account:" />
                    </div>
                    <div className="flex flex-wrap flex-col border-solid border border-black w-full mx-8 rounded-md overflow-hidden">
                        <input
                        id="query"
                        onChange={onChangeQuery}
                        placeholder="Type the first few characters..."
                        required
                        value={form}
                        className="p-2 h-max"
                        />
                        {
                        isLoading ? <Spinner color="white" size="md" /> : 
                            data.length && !account ? data.map((account, index) => {
                                if (index > 10) return;
                                if (index === 10) return (
                                    <div className="p-2 m-2 border-t border-solid border-black">
                                        <p>{ data.length - 10} More</p>
                                    </div>
                                );
                                return (
                                    <div key={account.id} className="p-2 m-2 border-t border-solid border-black hover:bg-gray-100 cursor-pointer" onClick={onAccountSelected}>
                                        <p>{account.id}</p>
                                    </div>
                                );
                            })
                            : queryMade ? (
                                    <div className="p-2 m-2 border-t border-solid border-black">
                                        <p>No accounts found</p>
                                    </div>
                                ) : null
                        }
                    </div>
                </div>
                {
                    account ? (
                        <div className="flex flex-row mt-6">
                            <div className="mb-2 block">
                                <p className="">Account Selected: {account}</p>
                            </div>
                        </div>
                    ) : null
                }
            </div>
            <div className="w-full flex justify-between pt-4">
                <Button color="light" onClick={onClickCancel}>
                    Cancel
                </Button>
                <Button color="dark" onClick={onClickSave}>
                    {isLoading ? <Spinner color="white" size="md" /> : 'Save'}
                </Button>
            </div>
        </div>
    );
        
  
}

export default AssignAccountForm;