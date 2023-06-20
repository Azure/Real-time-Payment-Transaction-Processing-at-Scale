import { useState } from 'react';
import { Button, Modal, Spinner } from 'flowbite-react';

const FormModal = ({ children, header, setOpenModal, openModal, onSubmit }) => {
  const [isLoading, setIsLoading] = useState(false);
  const onClickCancel = () => setOpenModal(false);

  const onSubmitForm = async () => {
    setIsLoading(true);
    await onSubmit();
    setIsLoading(false);
  };

  return (
    <Modal show={openModal} size="xl" popup onClose={() => setOpenModal(false)}>
      <Modal.Header className="items-center">{header}</Modal.Header>
      <Modal.Body>
        {children}
        <div className="w-full flex justify-between pt-4">
          <Button color="light" onClick={onClickCancel}>
            Cancel
          </Button>
          <Button color="dark" onClick={onSubmitForm}>
            {isLoading ? <Spinner color="white" size="md" /> : 'Save'}
          </Button>
        </div>
      </Modal.Body>
    </Modal>
  );
};

export default FormModal;
