import { Button, Modal } from 'flowbite-react';

const FormModal = ({ children, header, setOpenModal, openModal }) => {
  const onClickCancel = () => setOpenModal(false);
  return (
    <Modal show={openModal} size="md" popup onClose={() => setOpenModal(false)}>
      <Modal.Header className="items-center">{header}</Modal.Header>
      <Modal.Body>
        {children}

        <div className="w-full flex justify-between pt-4">
          <Button color="light" onClick={onClickCancel}>
            Cancel
          </Button>
          <Button color="dark">Save</Button>
        </div>
      </Modal.Body>
    </Modal>
  );
};

export default FormModal;
